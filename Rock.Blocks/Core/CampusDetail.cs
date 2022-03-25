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
using Rock.ViewModel.Blocks;
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
                var detailViewCrate = new DetailBlockViewCrate<CampusBag, CampusDetailOptions>();

                SetBagInitialEntityState( detailViewCrate, rockContext );

                detailViewCrate.NavigationUrls = GetCrateNavigationUrls();
                detailViewCrate.Options = GetCrateOptions( detailViewCrate.IsEditable );

                return detailViewCrate;
            }
        }

        /// <summary>
        /// Gets the crate options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private CampusDetailOptions GetCrateOptions( bool isEditable )
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
        /// can be used by the client.
        /// </summary>
        /// <param name="serviceTimes">The campus service times.</param>
        /// <returns>A collection of <see cref="ListItemViewModel"/> objects that represent the service times.</returns>
        private static List<ListItemViewModel> ConvertServiceTimesToPack( string serviceTimes )
        {
            if ( serviceTimes.IsNullOrWhiteSpace() )
            {
                return new List<ListItemViewModel>();
            }

            var packs = new List<ListItemViewModel>();

            // Format is "Day 1^Time 1|Day 2^Time 2"
            var services = serviceTimes.Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( var service in services )
            {
                var segments = service.Split( '^' );

                if (segments.Length >= 2)
                {
                    packs.Add( new ListItemViewModel
                    {
                        Value = segments[0],
                        Text = segments[1]
                    } );
                }
            }

            return packs;
        }

        /// <summary>
        /// Converts the service times from packs into the text string
        /// stored in <see cref="Campus.ServiceTimes"/>.
        /// </summary>
        /// <param name="packs">The packs that represent the service times.</param>
        /// <returns>A custom formatted <see cref="string"/> that contains the service times.</returns>
        private static string ConvertServiceTimesFromPacks( List<ListItemViewModel> packs )
        {
            return packs
                .Select( s => $"{s.Value}^{s.Text}" )
                .JoinStrings( "|" );
        }

        /// <summary>
        /// Converts the campus schedules to bags to represent the custom
        /// data that needs to be included.
        /// </summary>
        /// <param name="campusSchedules">The campus schedules.</param>
        /// <returns>A collection of <see cref="CampusScheduleBag"/> objects that represent the schedules.</returns>
        private static List<CampusScheduleBag> ConvertCampusSchedulesToBags( IEnumerable<CampusSchedule> campusSchedules )
        {
            if ( campusSchedules == null )
            {
                return new List<CampusScheduleBag>();
            }

            return campusSchedules
                .Select( cs => new CampusScheduleBag
                {
                    Guid = cs.Guid,
                    Schedule = cs.Schedule.ToListItemPack(),
                    ScheduleTypeValue = cs.ScheduleTypeValue.ToListItemPack()
                } )
                .ToList();
        }

        /// <summary>
        /// Updates the campus schedules from the data contained in the bags.
        /// </summary>
        /// <param name="campus">The campus instance to be updated.</param>
        /// <param name="bags">The bags that represent the schedules.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the schedules were valid and updated; otherwise <c>false</c>.</returns>
        private bool UpdateCampusSchedulesFromBags( Campus campus, IEnumerable<CampusScheduleBag> bags, RockContext rockContext )
        {
            if ( bags == null )
            {
                return false;
            }

            // Remove any CampusSchedules that were removed in the UI.
            var selectedSchedules = bags.Select( s => s.Guid );
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
            foreach ( var campusScheduleViewModel in bags )
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
        /// Gets the edit crate that will contain all the information needed to
        /// begin the edit operation.
        /// </summary>
        /// <param name="guid">The unique identifier of the entity to be edited.</param>
        /// <returns>A crate that contains the entity and any other information required.</returns>
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

                var crate = new DetailBlockEditCrate<CampusBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                return ActionOk( crate );
            }
        }

        /// <summary>
        /// Saves the entity contained in the save crate.
        /// </summary>
        /// <param name="saveCrate">The save crate that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockSaveCrate<CampusBag> saveCrate )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new CampusService( rockContext );
                Campus entity;

                // Determine if we are editing an existing entity or creating a new one.
                if ( saveCrate.Entity.Guid != Guid.Empty )
                {
                    // If editing an existing entity then load it and make sure it
                    // was found and can still be edited.
                    entity = entityService.Get( saveCrate.Entity.Guid );

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
                if ( !UpdateEntityFromSaveCrate( entity, saveCrate, rockContext ) )
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

                return ActionOk( GetEntityBagForView( entity ) );
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
        /// Sets the initial entity state of the crate. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="crate">The crate to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBagInitialEntityState( DetailBlockViewCrate<CampusBag, CampusDetailOptions> crate, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity != null )
            {
                var isViewable = entity.IsAuthorized( Security.Authorization.VIEW, RequestContext.CurrentPerson );
                crate.IsEditable = entity.IsAuthorized( Security.Authorization.EDIT, RequestContext.CurrentPerson );

                entity.LoadAttributes( rockContext );

                if ( entity.Id != 0 )
                {
                    // Existing entity was found, prepare for view mode by default.
                    if ( isViewable )
                    {
                        crate.Entity = GetEntityBagForView( entity );
                    }
                    else
                    {
                        crate.ErrorMessage = EditModeMessage.NotAuthorizedToView( Campus.FriendlyTypeName );
                    }
                }
                else
                {
                    // New entity is being created, prepare for edit mode by default.
                    if ( crate.IsEditable )
                    {
                        crate.Entity = GetEntityBagForEdit( entity );
                    }
                    else
                    {
                        crate.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Campus.FriendlyTypeName );
                    }
                }
            }
            else
            {
                crate.ErrorMessage = $"The {Campus.FriendlyTypeName} was not found.";
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a packet.</param>
        /// <returns>A <see cref="CampusBag"/> that represents the entity.</returns>
        private CampusBag GetCommonEntityBag( Campus entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new CampusBag
            {
                CampusSchedules = ConvertCampusSchedulesToBags( entity.CampusSchedules ),
                CampusStatusValue = entity.CampusStatusValue.ToListItemPack(),
                CampusTypeValue = entity.CampusTypeValue.ToListItemPack(),
                Description = entity.Description,
                Guid = entity.Guid,
                IsActive = entity.IsActive,
                IsSystem = entity.IsSystem,
                LeaderPersonAlias = entity.LeaderPersonAlias.ToListItemPack(),
                Location = entity.Location.ToListItemPack(),
                Name = entity.Name,
                PhoneNumber = entity.PhoneNumber,
                ServiceTimes = ConvertServiceTimesToPack( entity.ServiceTimes ),
                ShortCode = entity.ShortCode,
                TimeZoneId = entity.TimeZoneId,
                Url = entity.Url
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="CampusBag"/> that represents the entity.</returns>
        private CampusBag GetEntityBagForView( Campus entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var packet = GetCommonEntityBag( entity );

            packet.PopulatePublicAttributesAndValuesForView( entity, RequestContext.CurrentPerson );

            return packet;
        }

        /// <summary>
        /// Gets the bag for editing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="CampusBag"/> that represents the entity.</returns>
        private CampusBag GetEntityBagForEdit( Campus entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var packet = GetCommonEntityBag( entity );

            packet.PopulatePublicAttributesAndValuesForEdit( entity, RequestContext.CurrentPerson );

            return packet;
        }

        /// <summary>
        /// Updates the entity from the data in the save crate.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="crate">The crate containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the crate was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromSaveCrate( Campus entity, DetailBlockSaveCrate<CampusBag> crate, RockContext rockContext )
        {
            if ( crate.ValidProperties == null )
            {
                return false;
            }

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.CampusSchedules ), StringComparer.OrdinalIgnoreCase ) )
            {
                if ( !UpdateCampusSchedulesFromBags( entity, crate.Entity.CampusSchedules, rockContext ) )
                {
                    return false;
                }
            }

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.CampusStatusValue ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.CampusStatusValueId = crate.Entity.CampusStatusValue.GetEntityId<DefinedValue>( rockContext );
            }

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.CampusTypeValue ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.CampusTypeValueId = crate.Entity.CampusTypeValue.GetEntityId<DefinedValue>( rockContext );
            }

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.Description ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.Description = crate.Entity.Description;
            }

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.IsActive ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.IsActive = crate.Entity.IsActive;
            }

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.LeaderPersonAlias ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.LeaderPersonAliasId = crate.Entity.LeaderPersonAlias.GetEntityId<PersonAlias>( rockContext );
            }

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.Location ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.LocationId = crate.Entity.Location.GetEntityId<Location>( rockContext );
            }

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.Name ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.Name = crate.Entity.Name;
            }

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.PhoneNumber ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.PhoneNumber = crate.Entity.PhoneNumber;
            }

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.ServiceTimes ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.ServiceTimes = ConvertServiceTimesFromPacks( crate.Entity.ServiceTimes );
            }

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.ShortCode ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.ShortCode = crate.Entity.ShortCode;
            }

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.TimeZoneId ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.TimeZoneId = crate.Entity.TimeZoneId;
            }

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.Url ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.Url = crate.Entity.Url;
            }

            entity.LoadAttributes( rockContext );

            if ( crate.ValidProperties.Contains( nameof( crate.Entity.AttributeValues ), StringComparer.OrdinalIgnoreCase ) )
            {
                entity.SetPublicAttributeValues( crate.Entity.AttributeValues, RequestContext.CurrentPerson );
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
            var id = RequestContext.GetPageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();

            var entityService = new CampusService( rockContext );

            // If a zero identifier is specified then create a new entity.
            if ( id == 0 )
            {
                return new Campus
                {
                    Id = 0,
                    IsActive = true,
                    Guid = Guid.Empty
                };
            }

            // Otherwise look for an existing one in the database.
            if ( id.HasValue )
            {
                return entityService.GetNoTracking( id.Value );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the crate navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetCrateNavigationUrls()
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
        public static ListItemViewModel ToListItemPack( this IEntity entity )
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

        public static List<ListItemViewModel> ToListItemPackList( this IEnumerable<IEntity> entities )
        {
            if ( entities == null )
            {
                return new List<ListItemViewModel>();
            }

            return entities.Select( e => e.ToListItemPack() ).ToList();
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

    #endregion
}
