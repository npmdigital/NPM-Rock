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

using System;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.ViewModel;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// RegistrationTemplate Service class
    /// </summary>
    public partial class RegistrationTemplateService : Service<RegistrationTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationTemplateService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public RegistrationTemplateService(RockContext context) : base(context)
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
        public bool CanDelete( RegistrationTemplate item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// RegistrationTemplate View Model Helper
    /// </summary>
    [DefaultViewModelHelper( typeof( RegistrationTemplate ) )]
    public partial class RegistrationTemplateViewModelHelper : ViewModelHelper<RegistrationTemplate, Rock.ViewModel.RegistrationTemplateViewModel>
    {
        /// <summary>
        /// Converts the model to a view model.
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public override Rock.ViewModel.RegistrationTemplateViewModel CreateViewModel( RegistrationTemplate model, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( model == null )
            {
                return default;
            }

            var viewModel = new Rock.ViewModel.RegistrationTemplateViewModel
            {
                Id = model.Id,
                Guid = model.Guid,
                AddPersonNote = model.AddPersonNote,
                AllowExternalRegistrationUpdates = model.AllowExternalRegistrationUpdates,
                AllowMultipleRegistrants = model.AllowMultipleRegistrants,
                BatchNamePrefix = model.BatchNamePrefix,
                CategoryId = model.CategoryId,
                ConfirmationEmailTemplate = model.ConfirmationEmailTemplate,
                ConfirmationFromEmail = model.ConfirmationFromEmail,
                ConfirmationFromName = model.ConfirmationFromName,
                ConfirmationSubject = model.ConfirmationSubject,
                Cost = model.Cost,
                DefaultPayment = model.DefaultPayment,
                Description = model.Description,
                DiscountCodeTerm = model.DiscountCodeTerm,
                FeeTerm = model.FeeTerm,
                FinancialGatewayId = model.FinancialGatewayId,
                GroupMemberRoleId = model.GroupMemberRoleId,
                GroupMemberStatus = ( int ) model.GroupMemberStatus,
                GroupTypeId = model.GroupTypeId,
                IsActive = model.IsActive,
                IsRegistrationMeteringEnabled = model.IsRegistrationMeteringEnabled,
                LoginRequired = model.LoginRequired,
                MaxRegistrants = model.MaxRegistrants,
                MinimumInitialPayment = model.MinimumInitialPayment,
                Name = model.Name,
                Notify = ( int ) model.Notify,
                PaymentReminderEmailTemplate = model.PaymentReminderEmailTemplate,
                PaymentReminderFromEmail = model.PaymentReminderFromEmail,
                PaymentReminderFromName = model.PaymentReminderFromName,
                PaymentReminderSubject = model.PaymentReminderSubject,
                PaymentReminderTimeSpan = model.PaymentReminderTimeSpan,
                RegistrantsSameFamily = ( int ) model.RegistrantsSameFamily,
                RegistrantTerm = model.RegistrantTerm,
                RegistrantWorkflowTypeId = model.RegistrantWorkflowTypeId,
                RegistrarOption = ( int ) model.RegistrarOption,
                RegistrationAttributeTitleEnd = model.RegistrationAttributeTitleEnd,
                RegistrationAttributeTitleStart = model.RegistrationAttributeTitleStart,
                RegistrationInstructions = model.RegistrationInstructions,
                RegistrationTerm = model.RegistrationTerm,
                RegistrationWorkflowTypeId = model.RegistrationWorkflowTypeId,
                ReminderEmailTemplate = model.ReminderEmailTemplate,
                ReminderFromEmail = model.ReminderFromEmail,
                ReminderFromName = model.ReminderFromName,
                ReminderSubject = model.ReminderSubject,
                RequestEntryName = model.RequestEntryName,
                RequiredSignatureDocumentTemplateId = model.RequiredSignatureDocumentTemplateId,
                SetCostOnInstance = model.SetCostOnInstance,
                ShowCurrentFamilyMembers = model.ShowCurrentFamilyMembers,
                SignatureDocumentAction = ( int ) model.SignatureDocumentAction,
                SuccessText = model.SuccessText,
                SuccessTitle = model.SuccessTitle,
                WaitListEnabled = model.WaitListEnabled,
                WaitListTransitionEmailTemplate = model.WaitListTransitionEmailTemplate,
                WaitListTransitionFromEmail = model.WaitListTransitionFromEmail,
                WaitListTransitionFromName = model.WaitListTransitionFromName,
                WaitListTransitionSubject = model.WaitListTransitionSubject,
                CreatedDateTime = model.CreatedDateTime,
                ModifiedDateTime = model.ModifiedDateTime,
                CreatedByPersonAliasId = model.CreatedByPersonAliasId,
                ModifiedByPersonAliasId = model.ModifiedByPersonAliasId,
            };

            AddAttributesToViewModel( model, viewModel, currentPerson, loadAttributes );
            ApplyAdditionalPropertiesAndSecurityToViewModel( model, viewModel, currentPerson, loadAttributes );
            return viewModel;
        }
    }


    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class RegistrationTemplateExtensionMethods
    {
        /// <summary>
        /// Clones this RegistrationTemplate object to a new RegistrationTemplate object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static RegistrationTemplate Clone( this RegistrationTemplate source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as RegistrationTemplate;
            }
            else
            {
                var target = new RegistrationTemplate();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Clones this RegistrationTemplate object to a new RegistrationTemplate object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static RegistrationTemplate CloneWithoutIdentity( this RegistrationTemplate source )
        {
            var target = new RegistrationTemplate();
            target.CopyPropertiesFrom( source );

            target.Id = 0;
            target.Guid = Guid.NewGuid();
            target.ForeignKey = null;
            target.ForeignId = null;
            target.ForeignGuid = null;
            target.CreatedByPersonAliasId = null;
            target.CreatedDateTime = RockDateTime.Now;
            target.ModifiedByPersonAliasId = null;
            target.ModifiedDateTime = RockDateTime.Now;

            return target;
        }

        /// <summary>
        /// Copies the properties from another RegistrationTemplate object to this RegistrationTemplate object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this RegistrationTemplate target, RegistrationTemplate source )
        {
            target.Id = source.Id;
            target.AddPersonNote = source.AddPersonNote;
            target.AllowExternalRegistrationUpdates = source.AllowExternalRegistrationUpdates;
            #pragma warning disable 612, 618
            target.AllowGroupPlacement = source.AllowGroupPlacement;
            #pragma warning restore 612, 618
            target.AllowMultipleRegistrants = source.AllowMultipleRegistrants;
            target.BatchNamePrefix = source.BatchNamePrefix;
            target.CategoryId = source.CategoryId;
            target.ConfirmationEmailTemplate = source.ConfirmationEmailTemplate;
            target.ConfirmationFromEmail = source.ConfirmationFromEmail;
            target.ConfirmationFromName = source.ConfirmationFromName;
            target.ConfirmationSubject = source.ConfirmationSubject;
            target.Cost = source.Cost;
            target.DefaultPayment = source.DefaultPayment;
            target.Description = source.Description;
            target.DiscountCodeTerm = source.DiscountCodeTerm;
            target.FeeTerm = source.FeeTerm;
            target.FinancialGatewayId = source.FinancialGatewayId;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.GroupMemberRoleId = source.GroupMemberRoleId;
            target.GroupMemberStatus = source.GroupMemberStatus;
            target.GroupTypeId = source.GroupTypeId;
            target.IsActive = source.IsActive;
            target.IsRegistrationMeteringEnabled = source.IsRegistrationMeteringEnabled;
            target.LoginRequired = source.LoginRequired;
            target.MaxRegistrants = source.MaxRegistrants;
            target.MinimumInitialPayment = source.MinimumInitialPayment;
            target.Name = source.Name;
            target.Notify = source.Notify;
            target.PaymentReminderEmailTemplate = source.PaymentReminderEmailTemplate;
            target.PaymentReminderFromEmail = source.PaymentReminderFromEmail;
            target.PaymentReminderFromName = source.PaymentReminderFromName;
            target.PaymentReminderSubject = source.PaymentReminderSubject;
            target.PaymentReminderTimeSpan = source.PaymentReminderTimeSpan;
            target.RegistrantsSameFamily = source.RegistrantsSameFamily;
            target.RegistrantTerm = source.RegistrantTerm;
            target.RegistrantWorkflowTypeId = source.RegistrantWorkflowTypeId;
            target.RegistrarOption = source.RegistrarOption;
            target.RegistrationAttributeTitleEnd = source.RegistrationAttributeTitleEnd;
            target.RegistrationAttributeTitleStart = source.RegistrationAttributeTitleStart;
            target.RegistrationInstructions = source.RegistrationInstructions;
            target.RegistrationTerm = source.RegistrationTerm;
            target.RegistrationWorkflowTypeId = source.RegistrationWorkflowTypeId;
            target.ReminderEmailTemplate = source.ReminderEmailTemplate;
            target.ReminderFromEmail = source.ReminderFromEmail;
            target.ReminderFromName = source.ReminderFromName;
            target.ReminderSubject = source.ReminderSubject;
            target.RequestEntryName = source.RequestEntryName;
            target.RequiredSignatureDocumentTemplateId = source.RequiredSignatureDocumentTemplateId;
            target.SetCostOnInstance = source.SetCostOnInstance;
            target.ShowCurrentFamilyMembers = source.ShowCurrentFamilyMembers;
            target.SignatureDocumentAction = source.SignatureDocumentAction;
            target.SuccessText = source.SuccessText;
            target.SuccessTitle = source.SuccessTitle;
            target.WaitListEnabled = source.WaitListEnabled;
            target.WaitListTransitionEmailTemplate = source.WaitListTransitionEmailTemplate;
            target.WaitListTransitionFromEmail = source.WaitListTransitionFromEmail;
            target.WaitListTransitionFromName = source.WaitListTransitionFromName;
            target.WaitListTransitionSubject = source.WaitListTransitionSubject;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }

        /// <summary>
        /// Creates a view model from this entity
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson" >The currentPerson.</param>
        /// <param name="loadAttributes" >Load attributes?</param>
        public static Rock.ViewModel.RegistrationTemplateViewModel ToViewModel( this RegistrationTemplate model, Person currentPerson = null, bool loadAttributes = false )
        {
            var helper = new RegistrationTemplateViewModelHelper();
            var viewModel = helper.CreateViewModel( model, currentPerson, loadAttributes );
            return viewModel;
        }

    }

}