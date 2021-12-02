﻿// <copyright>
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Security;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents an event calendar.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "EventCalendar" )]
    [DataContract]
    public partial class EventCalendar : Model<EventCalendar>, ISecured, IHasActiveFlag, ICacheable, ICampusFilterable
    {
        /// <summary>
        /// Gets or sets the Name of the EventCalendar. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the EventCalendar.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description of the EventCalendar.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the EventCalendar.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class name for a font vector based icon.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the CSS class name of a font based icon.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the is active.
        /// </summary>
        /// <value>
        /// The is active.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is index enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexEnabled { get; set; } = false;

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventCalendarItem">event calendar items</see>.
        /// </summary>
        /// <value>
        /// The event calendar items.
        /// </value>
        public virtual ICollection<EventCalendarItem> EventCalendarItems
        {
            get { return _eventCalenderItems ?? ( _eventCalenderItems = new Collection<EventCalendarItem>() ); }
            set { _eventCalenderItems = value; }
        }
        private ICollection<EventCalendarItem> _eventCalenderItems;

        /// <summary>
        /// Gets or sets the content channels.
        /// </summary>
        /// <value>
        /// The content channels.
        /// </value>
        public virtual ICollection<EventCalendarContentChannel> ContentChannels
        {
            get { return _contentChannels ?? ( _contentChannels = new Collection<EventCalendarContentChannel>() ); }
            set { _contentChannels = value; }
        }
        private ICollection<EventCalendarContentChannel> _contentChannels;

        #endregion

        #region Methods

        /// <summary>
        /// Returns the default authorization for a specific action.
        /// </summary>
        /// <param name="action">A <see cref="System.String"/> representing the name of the action.</param>
        /// <returns>A <see cref="System.Boolean"/> that is <c>true</c> if the specified action is allowed by default; otherwise <c>false</c>.</returns>
        public override bool IsAllowedByDefault( string action )
        {
            return false;
        }

        /// <summary>
        /// Gets the supported actions.
        /// </summary>
        /// <value>
        /// The supported actions.
        /// </value>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( Rock.Security.Authorization.APPROVE, "The roles and/or users that have access to approve calendar items." );
                return supportedActions;
            }
        }

        #endregion

        #region Index Methods

        /// <summary>
        /// Deletes the indexed documents by calendar.
        /// </summary>
        /// <param name="calendarId">The calendar identifier.</param>
        public void DeleteIndexedDocumentsByCalendarId( int calendarId )
        {
            // Ensure provided calendar is indexable
            var calendar = EventCalendarCache.Get( calendarId );
            
            if ( calendar.IsNull() || !calendar.IsIndexEnabled )
            {
                return;
            }

            // Get event items for this calendar that are ONLY on this calendar.
            // We don't want to delete items that are also on another calendar.
            var eventItems = new EventItemService( new RockContext() )
                                    .GetActiveItemsByCalendarId( calendarId )
                                    .Where( i => i.EventCalendarItems.Count() == 1 )
                                    .Select( a => a.Id ).ToList();

            int eventItemEntityTypeId = EntityTypeCache.GetId<Rock.Model.EventItem>().Value;

            foreach ( var eventItemId in eventItems )
            {
                var transaction = new DeleteIndexEntityTransaction { EntityId = eventItemId, EntityTypeId = eventItemEntityTypeId };
                transaction.Enqueue();
            }
        }


        /// <summary>
        /// Bulks the index documents by calendar.
        /// </summary>
        /// <param name="calendarId">The calendar identifier.</param>
        public void BulkIndexDocumentsByCalendar( int calendarId )
        {
            // Ensure provided calendar is indexable
            var calendar = EventCalendarCache.Get( calendarId );

            if ( calendar.IsNull() || !calendar.IsIndexEnabled )
            {
                return;
            }

            var eventItems = new EventItemService( new RockContext() )
                                    .GetActiveItemsByCalendarId( calendarId )
                                    .Select( a => a.Id ).ToList();

            int eventItemEntityTypeId = EntityTypeCache.GetId<Rock.Model.EventItem>().Value;

            foreach ( var eventItemId in eventItems )
            {
                var transaction = new IndexEntityTransaction { EntityId = eventItemId, EntityTypeId = eventItemEntityTypeId };
                transaction.Enqueue();
            }
        }
        #endregion

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="state"></param>
        public override void PreSaveChanges( Data.DbContext dbContext, EntityState state )
        {
            // Keep the indexed Event Items correct
            if ( state == EntityState.Deleted && IsIndexEnabled )
            {
                this.DeleteIndexedDocumentsByCalendarId( Id );
            }
            else if( state == EntityState.Modified )
            {
                var changeEntry = dbContext.ChangeTracker.Entries<EventCalendar>().Where( a => a.Entity == this ).FirstOrDefault();
                if ( changeEntry != null )
                {
                    var originalIndexState = ( bool ) changeEntry.OriginalValues["IsIndexEnabled"];

                    if ( originalIndexState == true && IsIndexEnabled == false )
                    {
                        // clear out index items
                        this.DeleteIndexedDocumentsByCalendarId( Id );
                    }
                    else if ( IsIndexEnabled == true )
                    {
                        // if indexing is enabled then bulk index - needed as an attribute could have changed from IsIndexed
                        BulkIndexDocumentsByCalendar( Id );
                    }
                }
            }
        }

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return EventCalendarCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            EventCalendarCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// EventCalendar Configuration class.
    /// </summary>
    public partial class EventCalendarConfiguration : EntityTypeConfiguration<EventCalendar>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventCalendarConfiguration" /> class.
        /// </summary>
        public EventCalendarConfiguration()
        {
        }
    }

    #endregion
}