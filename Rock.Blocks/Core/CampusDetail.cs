// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModel.Blocks.Core.CampusDetail;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;

/*
 * WORK IN PROGRESS
 * 
 * This block is a work in progress, but we needed something to start with.
 * 
 * TODO: Add interface to provide access to Rock SystemSettings (currently in Rock.Web).
 * 
 * What doesn't work:
 * Audit drawer.
 * JS phone number validation (required country code support).
 * Editing Campus Topics (JS).
 * Editing Campus Schedules (JS).
 */
namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular campus.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Campus Detail" )]
    [Category( "Obsidian > Core" )]
    [Description( "Displays the details of a particular campus." )]
    [IconCssClass( "fa fa-building" )]

    #region Block Attributes

    #endregion

    public class CampusDetail : RockObsidianBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string CampusGuid = "CampusGuid";
            public const string CampusId = "CampusId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var detailViewBag = new DetailBlockViewBag<CampusPacket, CampusDetailOptions>();

                SetBagInitialEntityState( detailViewBag, rockContext );

                detailViewBag.NavigationUrls = GetBagNavigationUrls();
                detailViewBag.Options = GetBagOptions( detailViewBag.IsEditable );

                return detailViewBag;
            }
        }

        /// <summary>
        /// Gets the bag options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private CampusDetailOptions GetBagOptions( bool isEditable )
        {
            var options = new CampusDetailOptions
            {
                IsMultiTimeZoneSupported = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.ENABLE_MULTI_TIME_ZONE_SUPPORT ).AsBoolean()
            };

            // Get all the time zone options that will be available for the
            // individual to make their selection from. This is also used
            // when viewing to render the friendly time zone name.
            if ( options.IsMultiTimeZoneSupported )
            {
                options.TimeZoneOptions = TimeZoneInfo.GetSystemTimeZones()
                    .Select( tz => new ListItemViewModel
                    {
                        Value = tz.Id,
                        Text = tz.DisplayName
                    } )
                    .ToList();
            }

            return options;
        }

        /// <summary>
        /// Parses the <see cref="Campus.ServiceTimes"/> value into a format that
        /// can be used with the view model.
        /// </summary>
        /// <param name="serviceTimes">The campus service times.</param>
        /// <returns>A collection of <see cref="ListItemViewModel"/> objects that represent the service times.</returns>
        private static List<ListItemViewModel> ConvertServiceTimesToViewModel( string serviceTimes )
        {
            if ( serviceTimes.IsNullOrWhiteSpace() )
            {
                return new List<ListItemViewModel>();
            }

            var viewModel = new List<ListItemViewModel>();

            // Format is "Day 1^Time 1|Day 2^Time 2"
            var services = serviceTimes.Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( var service in services )
            {
                var segments = service.Split( '^' );

                if (segments.Length >= 2)
                {
                    viewModel.Add( new ListItemViewModel
                    {
                        Value = segments[0],
                        Text = segments[1]
                    } );
                }
            }

            return viewModel;
        }

        /// <summary>
        /// Converts the service times from view models into the text string
        /// stored in <see cref="Campus.ServiceTimes"/>.
        /// </summary>
        /// <param name="viewModel">The view models that represent the service times.</param>
        /// <returns>A custom formatted <see cref="string"/> that contains the service times.</returns>
        private static string ConvertServiceTimesFromViewModel( List<ListItemViewModel> viewModel )
        {
            return viewModel
                .Select( s => $"{s.Value}^{s.Text}" )
                .JoinStrings( "|" );
        }

        /// <summary>
        /// Converts the campus schedules to view models to represent the custom
        /// data that needs to be included.
        /// </summary>
        /// <param name="campusSchedules">The campus schedules.</param>
        /// <returns>A collection of <see cref="CampusSchedulePacket"/> objects that represent the schedules.</returns>
        private static List<CampusSchedulePacket> ConvertCampusSchedulesToViewModel( IEnumerable<CampusSchedule> campusSchedules )
        {
            if ( campusSchedules == null )
            {
                return new List<CampusSchedulePacket>();
            }

            return campusSchedules
                .Select( cs => new CampusSchedulePacket
                {
                    Guid = cs.Guid,
                    Schedule = cs.Schedule.ToListItemViewModel(),
                    ScheduleTypeValue = cs.ScheduleTypeValue.ToListItemViewModel()
                } )
                .ToList();
        }

        /// <summary>
        /// Updates the campus schedules from the data contained in the view models.
        /// </summary>
        /// <param name="campus">The campus instance to be updated.</param>
        /// <param name="viewModels">The view models that represent the schedules.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the schedules were valid and updated; otherwise <c>false</c>.</returns>
        private bool UpdateCampusSchedulesFromViewModel( Campus campus, IEnumerable<CampusSchedulePacket> viewModels, RockContext rockContext )
        {
            if ( viewModels == null )
            {
                return false;
            }

            // Remove any CampusSchedules that were removed in the UI.
            var selectedSchedules = viewModels.Select( s => s.Guid );
            var locationsToRemove = campus.CampusSchedules.Where( s => !selectedSchedules.Contains( s.Guid ) ).ToList();

            if ( locationsToRemove.Any() )
            {
                var campusScheduleService = new CampusScheduleService( rockContext );

                foreach ( var campusSchedule in locationsToRemove )
                {
                    campus.CampusSchedules.Remove( campusSchedule );
                    campusScheduleService.Delete( campusSchedule );
                }
            }

            // Add or update any schedules that are still selected in the UI.
            int order = 0;
            foreach ( var campusScheduleViewModel in viewModels )
            {
                var scheduleId = campusScheduleViewModel.Schedule.GetEntityId<Schedule>( rockContext );

                if ( !scheduleId.HasValue )
                {
                    return false;
                }

                var campusSchedule = campus.CampusSchedules.Where( s => s.Guid == campusScheduleViewModel.Guid ).FirstOrDefault();

                if ( campusSchedule == null )
                {
                    campusSchedule = new CampusSchedule()
                    {
                        CampusId = campus.Id,
                        Guid = Guid.NewGuid()
                    };
                    campus.CampusSchedules.Add( campusSchedule );
                }

                campusSchedule.ScheduleId = scheduleId.Value;
                campusSchedule.ScheduleTypeValueId = campusScheduleViewModel.ScheduleTypeValue.GetEntityId<DefinedValue>( rockContext );
                campusSchedule.Order = order++;
            }

            return true;
        }

        /// <summary>
        /// Validates the campus for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="campus">The campus to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the campus is valid, <c>false</c> otherwise.</returns>
        private bool ValidateCampus( Campus campus, RockContext rockContext, out string errorMessage )
        {
            // Verify the location is selected and a valid location type.
            var campusLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_CAMPUS.AsGuid() );
            var location = new LocationService( rockContext ).Get( campus.LocationId ?? 0 );

            if ( location == null || campusLocationType.Id != location.LocationTypeValueId )
            {
                errorMessage = $"The named location \"{location?.Name}\" is not a 'Campus' location type.";
                return false;
            }

            // Verify the campus name is unique.
            var existingCampus = campus.Id == 0
                ? CampusCache.All( true ).Where( c => c.Name == campus.Name ).FirstOrDefault()
                : CampusCache.All( true ).Where( c => c.Name == campus.Name && c.Id != campus.Id ).FirstOrDefault();

            if ( existingCampus != null )
            {
                var activeString = existingCampus.IsActive ?? false ? "active" : "inactive";

                errorMessage = $"The campus name \"{campus.Name}\" is already in use for an existing {activeString} campus.";
                return false;
            }

            // Verify the phone number is valid.
            if ( !IsPhoneNumberValid( campus.PhoneNumber ) )
            {
                errorMessage = $"The phone number '{campus.PhoneNumber}' is not a valid phone number.";
                return false;
            }

            // Verify the campus URL is valid.
            if ( !IsUrlValid( campus.Url ) )
            {
                errorMessage = $"The URL '{campus.Url}' is not a valid URL.";
                return false;
            }

            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Determines whether the string in <paramref name="phoneNumber"/> is a valid phone number.
        /// Uses the RegEx match string attributes in the defined values for the defined type Communication Phone Country Code.
        /// If there is nothing to match (<paramref name="phoneNumber"/> is null or empty) or match with (Missing defined values or MatchRegEx attribute) then true is returned.
        /// </summary>
        /// <param name="phoneNumber">The phone number to be validated.</param>
        /// <remarks>Taken from PhoneNumberBox UI control.</remarks>
        /// <returns>
        ///   <c>true</c> if <paramref name="phoneNumber"/> is a valid phone number otherwise, <c>false</c>.
        /// </returns>
        private static bool IsPhoneNumberValid( string phoneNumber )
        {
            // No number is a valid number, let the required field validator handle this.
            if ( phoneNumber.IsNullOrWhiteSpace() )
            {
                return true;
            }

            // This is the list of valid phone number formats, it must match one of them.
            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_PHONE_COUNTRY_CODE.AsGuid() );
            if ( definedType == null )
            {
                // If there is nothing to match against then return true
                return true;
            }

            foreach ( var definedValue in definedType.DefinedValues )
            {
                string matchRegEx = definedValue.GetAttributeValue( "MatchRegEx" );
                if ( matchRegEx.IsNullOrWhiteSpace() )
                {
                    // No available pattern so move on
                    continue;
                }

                if ( System.Text.RegularExpressions.Regex.IsMatch( phoneNumber.RemoveAllNonNumericCharacters(), matchRegEx ) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the string is a valid URL.
        /// </summary>
        /// <param name="url">The URL to be validated.</param>
        /// <returns><c>true</c> if the url is valid; otherwise, <c>false</c>.</returns>
        private static bool IsUrlValid( string url )
        {
            if ( url.IsNullOrWhiteSpace() )
            {
                return true;
            }

            var urlRegex = @"^(http[s]?:\/\/)?[^\s([" + '"' + @" <,>]*\.?[^\s[" + '"' + @",><]*\/$";

            return Regex.IsMatch( url, urlRegex );
        }

        #endregion

        #region Block Actions (Generated)

        /// <summary>
        /// Gets the edit bag that will contain all the information needed to
        /// begin the edit operation.
        /// </summary>
        /// <param name="guid">The unique identifier of the entity to be edited.</param>
        /// <returns>A bag that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( Guid guid )
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = new CampusService( rockContext ).Get( guid );

                if ( entity == null || !entity.IsAuthorized( Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest();
                }

                entity.LoadAttributes( rockContext );

                var bag = new DetailBlockEditBag<CampusPacket>
                {
                    Entity = GetEntityPacketForEdit( entity )
                };

                return ActionOk( bag );
            }
        }

        /// <summary>
        /// Saves the entity contained in the save bag.
        /// </summary>
        /// <param name="saveBag">The save bag that contains all the information required to save.</param>
        /// <returns>A new packet to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockSaveBag<CampusPacket> saveBag )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new CampusService( rockContext );
                Campus entity;

                // Determine if we are editing an existing entity or creating a new one.
                if ( saveBag.Entity.Guid != Guid.Empty )
                {
                    // If editing an existing entity then load it and make sure it
                    // was found and can still be edited.
                    entity = entityService.Get( saveBag.Entity.Guid );

                    if ( entity == null )
                    {
                        return ActionBadRequest( $"{Campus.FriendlyTypeName} not found." );
                    }

                    if ( !entity.IsAuthorized( Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        return ActionBadRequest( $"Not authorized to edit ${Campus.FriendlyTypeName}." );
                    }
                }
                else
                {
                    // Create a new entity.
                    entity = new Campus();
                    entityService.Add( entity );

                    var maxOrder = entityService.Queryable()
                        .Select( t => ( int? ) t.Order )
                        .Max();

                    entity.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromSaveBag( entity, saveBag, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateCampus( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.CampusId] = entity.Id.ToString()
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );

                return ActionOk( GetEntityPacketForView( entity ) );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="guid">The unique identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( Guid guid )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new CampusService( rockContext );
                var entity = new CampusService( rockContext ).Get( guid );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{Campus.FriendlyTypeName} was not found." );
                }

                if ( !entity.IsAuthorized( Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete this ${Campus.FriendlyTypeName}." );
                }

                // Don't allow deleting the last campus.
                if ( !entityService.Queryable().Where( c => c.Id != entity.Id ).Any() )
                {
                    return ActionBadRequest( $"{entity.Name} is the only campus and cannot be deleted (Rock requires at least one campus)." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        #endregion

        #region Generated Methods

        /// <summary>
        /// Sets the initial entity state of the bag. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="bag">The bag to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBagInitialEntityState( DetailBlockViewBag<CampusPacket, CampusDetailOptions> bag, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity != null )
            {
                var isViewable = entity.IsAuthorized( Security.Authorization.VIEW, RequestContext.CurrentPerson );
                bag.IsEditable = entity.IsAuthorized( Security.Authorization.EDIT, RequestContext.CurrentPerson );

                entity.LoadAttributes( rockContext );

                if ( entity.Id != 0 )
                {
                    // Existing entity was found, prepare for view mode by default.
                    if ( isViewable )
                    {
                        bag.Entity = GetEntityPacketForView( entity );
                    }
                    else
                    {
                        bag.ErrorMessage = EditModeMessage.NotAuthorizedToView( Campus.FriendlyTypeName );
                    }
                }
                else
                {
                    // New entity is being created, prepare for edit mode by default.
                    if ( bag.IsEditable )
                    {
                        bag.Entity = GetEntityPacketForEdit( entity );
                    }
                    else
                    {
                        bag.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Campus.FriendlyTypeName );
                    }
                }
            }
            else
            {
                bag.ErrorMessage = $"The {Campus.FriendlyTypeName} was not found.";
            }
        }

        /// <summary>
        /// Gets the entity packet that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a packet.</param>
        /// <returns>A <see cref="CampusPacket"/> that represents the entity.</returns>
        private CampusPacket GetCommonEntityPacket( Campus entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new CampusPacket
            {
                CampusSchedules = ConvertCampusSchedulesToViewModel( entity.CampusSchedules ),
                CampusStatusValue = entity.CampusStatusValue.ToListItemViewModel(),
                CampusTypeValue = entity.CampusTypeValue.ToListItemViewModel(),
                Description = entity.Description,
                Guid = entity.Guid,
                IsActive = entity.IsActive,
                IsSystem = entity.IsSystem,
                LeaderPersonAlias = entity.LeaderPersonAlias.ToListItemViewModel(),
                Location = entity.Location.ToListItemViewModel(),
                Name = entity.Name,
                PhoneNumber = entity.PhoneNumber,
                ServiceTimes = ConvertServiceTimesToViewModel( entity.ServiceTimes ),
                ShortCode = entity.ShortCode,
                TimeZoneId = entity.TimeZoneId,
                Url = entity.Url
            };
        }

        /// <summary>
        /// Gets the packet for viewing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="CampusPacket"/> that represents the entity.</returns>
        private CampusPacket GetEntityPacketForView( Campus entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var packet = GetCommonEntityPacket( entity );

            packet.PopulatePublicAttributesAndValuesForView( entity, RequestContext.CurrentPerson );

            return packet;
        }

        /// <summary>
        /// Gets the packet for editing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="CampusPacket"/> that represents the entity.</returns>
        private CampusPacket GetEntityPacketForEdit( Campus entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var packet = GetCommonEntityPacket( entity );

            packet.PopulatePublicAttributesAndValuesForEdit( entity, RequestContext.CurrentPerson );

            return packet;
        }

        /// <summary>
        /// Updates the entity from the data in the save bag.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="bag">The bag containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the bag was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromSaveBag( Campus entity, DetailBlockSaveBag<CampusPacket> bag, RockContext rockContext )
        {
            if ( bag.ValidProperties == null )
            {
                return false;
            }

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.CampusSchedules ), StringComparer.OrdinalIgnoreCase ) )
            {
                if ( !UpdateCampusSchedulesFromViewModel( entity, bag.Entity.CampusSchedules, rockContext ) )
                {
                    return false;
                }
            }

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.CampusStatusValue ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.CampusStatusValueId = bag.Entity.CampusStatusValue.GetEntityId<DefinedValue>( rockContext );
            }

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.CampusTypeValue ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.CampusTypeValueId = bag.Entity.CampusTypeValue.GetEntityId<DefinedValue>( rockContext );
            }

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.Description ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.Description = bag.Entity.Description;
            }

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.IsActive ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.IsActive = bag.Entity.IsActive;
            }

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.LeaderPersonAlias ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.LeaderPersonAliasId = bag.Entity.LeaderPersonAlias.GetEntityId<PersonAlias>( rockContext );
            }

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.Location ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.LocationId = bag.Entity.Location.GetEntityId<Location>( rockContext );
            }

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.Name ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.Name = bag.Entity.Name;
            }

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.PhoneNumber ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.PhoneNumber = bag.Entity.PhoneNumber;
            }

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.ServiceTimes ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.ServiceTimes = ConvertServiceTimesFromViewModel( bag.Entity.ServiceTimes );
            }

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.ShortCode ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.ShortCode = bag.Entity.ShortCode;
            }

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.TimeZoneId ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.TimeZoneId = bag.Entity.TimeZoneId;
            }

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.Url ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.Url = bag.Entity.Url;
            }

            entity.LoadAttributes( rockContext );

            if ( bag.ValidProperties.Contains( nameof( bag.Entity.AttributeValues ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.SetPublicAttributeValues( bag.Entity.AttributeValues, RequestContext.CurrentPerson );
            }

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="Campus"/> to be viewed or edited on the page.</returns>
        private Campus GetInitialEntity( RockContext rockContext )
        {
            var guid = RequestContext.GetPageParameter( PageParameterKey.CampusGuid ).AsGuidOrNull();
            var id = RequestContext.GetPageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();

            var entityService = new CampusService( rockContext );

            // If empty guid or zero identifier are specified then create
            // a new entity.
            if ( guid == Guid.Empty || id == 0 )
            {
                return new Campus
                {
                    Id = 0,
                    IsActive = true,
                    Guid = Guid.Empty
                };
            }

            // Otherwise look for an existing one in the database.
            if ( guid.HasValue )
            {
                return entityService.GetNoTracking( guid.Value );
            }
            else if ( id.HasValue )
            {
                return entityService.GetNoTracking( id.Value );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the bag navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBagNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        #endregion

        #region Methods to Remove?

        public List<string> GetViewBagBlockAuthorizations()
        {
            var currentPerson = RequestContext.CurrentPerson;
            var authorizations = new List<string>
            {
                Rock.Security.Authorization.VIEW,
                Rock.Security.Authorization.EDIT,
                Rock.Security.Authorization.ADMINISTRATE,
            };

            foreach ( var action in BlockCache.BlockType.SecurityActions )
            {
                authorizations.Add( action.Key );
            }

            return authorizations.Where( a => BlockCache.IsAuthorized( a, currentPerson ) ).ToList();
        }

        public List<string> GetViewBagEntityAuthorizations( Rock.Security.ISecured secured )
        {
            var currentPerson = RequestContext.CurrentPerson;

            return secured.SupportedActions
                .Select( a => a.Key )
                .Where( a => BlockCache.IsAuthorized( a, currentPerson ) )
                .ToList();
        }

        #endregion
    }

    #region Code to be relocated

    public static class ViewModelExtensions
    {
        public static ListItemViewModel ToListItemViewModel( this IEntity entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var viewModel = new ListItemViewModel
            {
                Value = entity.Guid.ToString(),
                Text = entity.ToString()
            };

            return viewModel;
        }

        public static List<ListItemViewModel> ToListItemViewModelList( this IEnumerable<IEntity> entities )
        {
            if ( entities == null )
            {
                return new List<ListItemViewModel>();
            }

            return entities.Select( e => e.ToListItemViewModel() ).ToList();
        }

        public static int? GetEntityId<TEntity>( this ListItemViewModel viewModel, RockContext rockContext )
            where TEntity : IEntity
        {
            var guid = viewModel?.Value.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            var entityType = EntityTypeCache.Get<TEntity>( false, rockContext );

            if ( entityType == null )
            {
                return null;
            }

            return Rock.Reflection.GetEntityIdForEntityType( entityType.Guid, guid.Value, rockContext );
        }
    }

    public class DetailBlockViewBag<TPacket>
    {
        public TPacket Entity { get; set; }

        public string ErrorMessage { get; set; }

        public bool IsEditable { get; set; }

        public Dictionary<string, string> NavigationUrls { get; set; } = new Dictionary<string, string>();
    }

    public class DetailBlockViewBag<TPacket, TOptions> : DetailBlockViewBag<TPacket>
        where TOptions : new()
    {
        public TOptions Options { get; set; } = new TOptions();
    }

    public class DetailBlockEditBag<TPacket>
    {
        public TPacket Entity { get; set; }
    }

    public class DetailBlockEditBag<TPacket, TOptions> : DetailBlockEditBag<TPacket>
        where TOptions : new()
    {
        public TOptions Options { get; set; } = new TOptions();
    }

    public class DetailBlockSaveBag<TPacket>
    {
        public TPacket Entity { get; set; }

        public List<string> ValidProperties { get; set; }
    }

    public class DetailBlockSaveResultBag<TPacket>
    {
        public TPacket Entity { get; set; }

        public string RedirectUrl { get; set; }
    }

    #endregion
}
