//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// WorkflowActionForm Service class
    /// </summary>
    public partial class WorkflowActionFormService : Service<WorkflowActionForm>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionFormService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public WorkflowActionFormService(RockContext context) : base(context)
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( WorkflowActionForm item, out string errorMessage )
        {
            errorMessage = string.Empty;
 
            if ( new Service<WorkflowActionType>( Context ).Queryable().Any( a => a.WorkflowFormId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", WorkflowActionForm.FriendlyTypeName, WorkflowActionType.FriendlyTypeName );
                return false;
            }  
            return true;
        }
    }

    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class WorkflowActionFormExtensionMethods
    {
        /// <summary>
        /// Clones this WorkflowActionForm object to a new WorkflowActionForm object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static WorkflowActionForm Clone( this WorkflowActionForm source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as WorkflowActionForm;
            }
            else
            {
                var target = new WorkflowActionForm();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from another WorkflowActionForm object to this WorkflowActionForm object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this WorkflowActionForm target, WorkflowActionForm source )
        {
            target.Id = source.Id;
            target.ActionAttributeGuid = source.ActionAttributeGuid;
            target.Actions = source.Actions;
            target.AllowNotes = source.AllowNotes;
            target.AllowPersonEntry = source.AllowPersonEntry;
            target.Footer = source.Footer;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.Header = source.Header;
            target.IncludeActionsInNotification = source.IncludeActionsInNotification;
            target.NotificationSystemCommunicationId = source.NotificationSystemCommunicationId;
            #pragma warning disable 612, 618
            target.NotificationSystemEmailId = source.NotificationSystemEmailId;
            #pragma warning restore 612, 618
            target.PersonEntryAddressEntryOption = source.PersonEntryAddressEntryOption;
            target.PersonEntryAutofillCurrentPerson = source.PersonEntryAutofillCurrentPerson;
            target.PersonEntryBirthdateEntryOption = source.PersonEntryBirthdateEntryOption;
            target.PersonEntryCampusIsVisible = source.PersonEntryCampusIsVisible;
            target.PersonEntryCampusStatusValueId = source.PersonEntryCampusStatusValueId;
            target.PersonEntryCampusTypeValueId = source.PersonEntryCampusTypeValueId;
            target.PersonEntryConnectionStatusValueId = source.PersonEntryConnectionStatusValueId;
            target.PersonEntryEmailEntryOption = source.PersonEntryEmailEntryOption;
            target.PersonEntryFamilyAttributeGuid = source.PersonEntryFamilyAttributeGuid;
            target.PersonEntryGenderEntryOption = source.PersonEntryGenderEntryOption;
            target.PersonEntryGroupLocationTypeValueId = source.PersonEntryGroupLocationTypeValueId;
            target.PersonEntryHideIfCurrentPersonKnown = source.PersonEntryHideIfCurrentPersonKnown;
            target.PersonEntryMaritalStatusEntryOption = source.PersonEntryMaritalStatusEntryOption;
            target.PersonEntryMobilePhoneEntryOption = source.PersonEntryMobilePhoneEntryOption;
            target.PersonEntryPersonAttributeGuid = source.PersonEntryPersonAttributeGuid;
            target.PersonEntryPostHtml = source.PersonEntryPostHtml;
            target.PersonEntryPreHtml = source.PersonEntryPreHtml;
            target.PersonEntryRecordStatusValueId = source.PersonEntryRecordStatusValueId;
            target.PersonEntrySpouseAttributeGuid = source.PersonEntrySpouseAttributeGuid;
            target.PersonEntrySpouseEntryOption = source.PersonEntrySpouseEntryOption;
            target.PersonEntrySpouseLabel = source.PersonEntrySpouseLabel;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }
    }
}