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

using Rock.Attribute;
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
 * What doesn't work:
 * ...
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
        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string CampusGuid = "CampusGuid";
            public const string CampusId = "CampusId";
        }

        #endregion PageParameterKeys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var detailViewBag = new DetailBlockViewBag<CampusViewModel, CampusDetailSettingsViewModel>
                {
                    BlockAuthorizations = GetViewBagBlockAuthorizations()
                };

                var campus = GetInitialCampus( rockContext );

                if ( campus != null && campus.IsAuthorized( Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    campus.LoadAttributes( rockContext );

                    detailViewBag.EntityAuthorizations = GetViewBagEntityAuthorizations( campus );
                    detailViewBag.Entity = GetCampusViewModelForView( campus );
                }

                return detailViewBag;
            }
        }

        private static List<ListItemViewModel> ServiceTimesToViewModel( string serviceTimes )
        {
            if ( serviceTimes.IsNullOrWhiteSpace() )
            {
                return new List<ListItemViewModel>();
            }

            var viewModel = new List<ListItemViewModel>();

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

        private static string ServiceTimesFromViewModel( List<ListItemViewModel> viewModel )
        {
            return viewModel
                .Select( s => $"{s.Value}^{s.Text}" )
                .JoinStrings( "|" );
        }

        private static List<CampusScheduleViewModel> CampusSchedulesToViewModel( IEnumerable<CampusSchedule> campusSchedules )
        {
            if ( campusSchedules == null )
            {
                return new List<CampusScheduleViewModel>();
            }

            return campusSchedules
                .Select( cs => new CampusScheduleViewModel
                {
                    Guid = cs.Guid,
                    Schedule = cs.Schedule.ToListItemViewModel(),
                    ScheduleTypeValue = cs.ScheduleTypeValue.ToListItemViewModel()
                } )
                .ToList();
        }

        #endregion

        #region Block Actions

        #endregion

        #region Generated Methods

        private CampusViewModel GetCommonCampusViewModel( Campus entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new CampusViewModel
            {
                CampusSchedules = CampusSchedulesToViewModel( entity.CampusSchedules ),
                CampusStatusValue = entity.CampusStatusValue.ToListItemViewModel(),
                CampusTypeValue = entity.CampusTypeValue.ToListItemViewModel(),
                Description = entity.Description,
                Guid = entity.Guid,
                IsActive = entity.IsActive,
                LeaderPersonAlias = entity.ToListItemViewModel(),
                Location = entity.Location.ToListItemViewModel(),
                Name = entity.Name,
                PhoneNumber = entity.PhoneNumber,
                ServiceTimes = ServiceTimesToViewModel( entity.ServiceTimes ),
                ShortCode = entity.ShortCode,
                Url = entity.Url
            };
        }

        private CampusViewModel GetCampusViewModelForView( Campus entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var viewModel = GetCommonCampusViewModel( entity );

            viewModel.PopulatePublicAttributesAndValuesForView( entity, RequestContext.CurrentPerson );

            return viewModel;
        }

        private CampusViewModel GetCampusViewModelForEdit( Campus entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var viewModel = GetCommonCampusViewModel( entity );

            viewModel.PopulatePublicAttributesAndValuesForEdit( entity, RequestContext.CurrentPerson );

            return viewModel;
        }

        private Campus GetInitialCampus( RockContext rockContext )
        {
            var campusGuid = RequestContext.GetPageParameter( PageParameterKey.CampusGuid ).AsGuidOrNull();
            var campusId = RequestContext.GetPageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();

            var campusService = new CampusService( rockContext );

            if ( campusGuid == Guid.Empty || campusId == 0 )
            {
                return new Campus
                {
                    Id = 0,
                    Guid = Guid.Empty
                };
            }

            if ( campusGuid.HasValue )
            {
                return campusService.Get( campusGuid.Value );
            }
            else if ( campusId.HasValue )
            {
                return campusService.Get( campusId.Value );
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region RockObsidianDetailBlockType Methods

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

        private class CampusDetailViewBag : DetailBlockViewBag<CampusViewModel>
        {
        }

        private class CampusDetailSettingsViewModel
        {
        }

        public abstract class DetailBlockViewBag
        {
            public List<string> BlockAuthorizations { get; set; } = new List<string>();

            public List<string> EntityAuthorizations { get; set; } = new List<string>();

            public Dictionary<string, string> NavigationUrls { get; set; } = new Dictionary<string, string>();
        }

        public class DetailBlockViewBag<TViewModel> : DetailBlockViewBag
        {
            public TViewModel Entity { get; set; }
        }

        public class DetailBlockViewBag<TEntity, TSettings> : DetailBlockViewBag<TEntity>
            where TSettings : new()
        {
            public TSettings Settings { get; set; } = new TSettings();
        }
    }

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
    }
}
