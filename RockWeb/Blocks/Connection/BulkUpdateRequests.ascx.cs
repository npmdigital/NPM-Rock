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
using CSScriptLibrary;
using Humanizer;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Connection
{
    /// <summary>
    /// Block for bulk update of multiple connection requests
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    [DisplayName( "Bulk Update Requests" )]
    [Category( "Connection" )]
    [Description( "Used for updating information about several Connection Requests at once. The QueryString must have both the EntitySetId as well as the ConnectionTypeId, and all the connection requests must be for the same opportunity." )]
    public partial class BulkUpdateRequests : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        /// <summary>
        /// A defined list of page parameter keys used by this block.
        /// </summary>
        private static class PageParameterKey
        {
            public const string ConnectionTypeId = "ConnectionTypeId";
            public const string EntitySetId = "EntitySetId";
        }

        #endregion PageParameterKeys

        #region Properties

        public Dictionary<int?, RockRadioButton> CampusRadioButtonState { get; set; } = new Dictionary<int?, RockRadioButton>();
        public ConnectionOpportunity ConnectionOpportunity { get; set; }
        public List<ConnectionActivityType> ConnectionActivityTypes { get; set; } = new List<ConnectionActivityType>();

        #endregion Properties

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );


        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                GetDetails();
            }
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlOpportunity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlOpportunity_SelectedIndexChanged( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var connectionOpportunityId = ddlOpportunity.SelectedValue.AsIntegerOrNull();
            var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( connectionOpportunityId.Value );

            if ( connectionOpportunity != null )
            {
                rbBulkUpdateDefaultConnector.Text = $"Keep Default Connector for {connectionOpportunity.Name}";
            }

            RebindOpportunityConnector( connectionOpportunity, rockContext );
        }

        protected void btnBulkRequestUpdateCancel_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var connectionActivity = new ConnectionActivityType();
        }

        protected void cbAddActivity_CheckedChanged( object sender, EventArgs e )
        {
            var selectedCampusId = CampusRadioButtonState.FirstOrDefault( m => m.Value.Checked ).Key;
            var connectionOpportunityId = ddlOpportunity.SelectedValue.AsIntegerOrNull();
            var rockContext = new RockContext();
            ConnectionOpportunity = ConnectionOpportunity ?? new ConnectionOpportunityService( rockContext ).Get( connectionOpportunityId.Value );

            // Get connectors and add to dropdown list
            var connectors = GetConnectors( ConnectionOpportunity, rockContext, selectedCampusId );
            ddlActivityConnector.Items.Clear();
            ddlActivityConnector.Items.Add( new ListItem( string.Empty, string.Empty ) );
            connectors.ToList()
                .ForEach( c => ddlActivityConnector.Items.Add( new ListItem( c.Value.FullName, c.Key.ToString() ) ) );

            // Get ActivityTypes and add to dropdown
            if ( ddlActivityType.Items.Count == 0 )
            {
                ddlActivityType.Items.Add( new ListItem( string.Empty, string.Empty ) );
                GetActivityTypes( ConnectionOpportunity, rockContext )
                    .ForEach( at => ddlActivityType.Items.Add( new ListItem( at.Name, at.Id.ToString() ) ) );
            }

            dvActivity.Visible = cbAddActivity.Checked;
        }

        #endregion Events

        #region Methods

        private void GetDetails()
        {
            var connectionTypeId = PageParameter( PageParameterKey.ConnectionTypeId ).AsInteger();
            var entitySetId = PageParameter( PageParameterKey.EntitySetId ).AsInteger();

            var rockContext = new RockContext();

            var connectionType = new ConnectionTypeService( rockContext ).Queryable().AsNoTracking()
                    .Include( ct => ct.ConnectionOpportunities )
                    .FirstOrDefault( ct => ct.Id == connectionTypeId );

            BindDropdownLists( connectionType );

            BindCampusSelector( entitySetId, rockContext );
        }

        private void BindCampusSelector( int entitySetId, RockContext rockContext )
        {
            var entitySet = new EntitySetService( rockContext ).Queryable().AsNoTracking()
                .Include( m => m.Items )
                .FirstOrDefault( e => e.Id == entitySetId );

            var requestIds = entitySet.Items.Select( m => m.EntityId );

            var requestCampuses = new ConnectionRequestService( rockContext ).Queryable().AsNoTracking()
                .Include( cr => cr.Campus )
                .Where( cr => requestIds.Contains( cr.Id ) )
                .GroupBy( cr => cr.CampusId )
                .Select( cr => new ConnectionCampusCountViewModel
                {
                    CampusId = cr.Key,
                    Campus = cr.FirstOrDefault().CampusId == null ? "No Campus" : cr.FirstOrDefault().Campus.Name,
                    Count = cr.Count(),
                    OpportunityId = cr.FirstOrDefault().ConnectionOpportunityId
                } ).ToList();

            if ( requestCampuses.Count > 0 )
            {
                rcwBulkUpdateCampuses.Visible = true;

                foreach ( var item in requestCampuses )
                {
                    var campusRadioButton = new RockRadioButton();
                    campusRadioButton.Text = $"{item.Campus} ({item.Count})";
                    campusRadioButton.ID = $"rbBulkUpdateCampus_{item.CampusId}";
                    campusRadioButton.GroupName = "BulkUpdateCampusSelector";
                    campusRadioButton.Checked = true;
                    CampusRadioButtonState.Add( item.CampusId.GetValueOrDefault(), campusRadioButton );

                    rcwBulkUpdateCampuses.Controls.Add( campusRadioButton );
                }
            }

            ddlOpportunity.SetValue( requestCampuses.FirstOrDefault().OpportunityId );
            ddlOpportunity_SelectedIndexChanged( null, null );
        }

        private void BindDropdownLists( ConnectionType connectionType )
        {
            // Add Opportunites to dropdown list
            foreach ( var opportunity in connectionType.ConnectionOpportunities.OrderBy( m => m.Order ).ThenBy( m => m.Name ) )
            {
                ddlOpportunity.Items.Add( new ListItem( opportunity.Name, opportunity.Id.ToString().ToUpper() ) );
            }

            // Add Statuses to dropdown list
            ddlStatus.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( var status in connectionType.ConnectionStatuses )
            {
                ddlStatus.Items.Add( new ListItem( status.Name, status.Id.ToString() ) );
            }

            // Add States to dropdown list
            ddlState.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( var state in Enum.GetValues( typeof( ConnectionState ) ).Cast<ConnectionState>().ToList() )
            {
                ddlState.Items.Add( new ListItem( state.Humanize(), state.ToString() ) );
            }
        }

        private void RebindOpportunityConnector( ConnectionOpportunity connectionOpportunity, RockContext rockContext )
        {
            var selectedCampusId = CampusRadioButtonState.FirstOrDefault( m => m.Value.Checked ).Key;

            Dictionary<int, Person> connectors = GetConnectors( connectionOpportunity, rockContext, selectedCampusId );

            ddlBulkUpdateOpportunityConnector.Items.Clear();
            ddlBulkUpdateOpportunityConnector.Items.Add( new ListItem( string.Empty, string.Empty ) );

            // Add connectors to dropdown list
            connectors.ToList()
                .ForEach( c => ddlBulkUpdateOpportunityConnector.Items.Add( new ListItem( c.Value.FullName, c.Key.ToString() ) ) );

            // If default connector is checked set as selected value in Connector dropdown
            if ( rbBulkUpdateDefaultConnector.Checked && connectionOpportunity != null )
            {
                var defaultConnectorPersonId = connectionOpportunity.GetDefaultConnectorPersonId( selectedCampusId );
                if ( defaultConnectorPersonId.HasValue )
                {
                    ddlBulkUpdateOpportunityConnector.SetValue( defaultConnectorPersonId.Value );
                }
            }
        }

        private Dictionary<int, Person> GetConnectors( ConnectionOpportunity connectionOpportunity, RockContext rockContext, int? selectedCampusId )
        {
            var connectors = new Dictionary<int, Person>();
            var connectionOpportunityConnectorPersonList = new ConnectionOpportunityConnectorGroupService( rockContext ).Queryable()
                        .Where( a => a.ConnectionOpportunityId == connectionOpportunity.Id && ( !selectedCampusId.HasValue || !a.CampusId.HasValue || a.CampusId.Value == selectedCampusId.Value ) )
                        .SelectMany( a => a.ConnectorGroup.Members )
                        .Where( a => a.GroupMemberStatus == GroupMemberStatus.Active )
                        .Select( a => a.Person )
                        .OrderBy( p => p.LastName )
                        .ThenBy( p => p.NickName )
                        .AsNoTracking()
                        .ToList();

            connectionOpportunityConnectorPersonList.ForEach( p => connectors.AddOrIgnore( p.Id, p ) );

            // Add the current person as possible connector
            if ( CurrentPerson != null )
            {
                connectors.AddOrIgnore( CurrentPerson.Id, CurrentPerson );
            }

            return connectors;
        }

        private List<ConnectionActivityType> GetActivityTypes( ConnectionOpportunity connectionOpportunity, RockContext rockContext )
        {
            if ( ConnectionActivityTypes.Count > 0 && ConnectionActivityTypes.FirstOrDefault().ConnectionTypeId == connectionOpportunity.ConnectionTypeId )
            {
                return ConnectionActivityTypes;
            }

            var connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
            ConnectionActivityTypes = connectionActivityTypeService.Queryable()
                .AsNoTracking()
                .Where( cat =>
                    cat.ConnectionTypeId == connectionOpportunity.ConnectionTypeId &&
                    cat.IsActive )
                .OrderBy( cat => cat.Name )
                .ThenBy( cat => cat.Id )
                .ToList();

            return ConnectionActivityTypes;
        }

        #endregion Methods

        #region Helper Class

        private sealed class ConnectionCampusCountViewModel
        {
            public int? CampusId { get; set; }
            public string Campus { get; set; }
            public int Count { get; set; }
            public int OpportunityId { get; set; }
        }

        #endregion Helper Class
    }
}
