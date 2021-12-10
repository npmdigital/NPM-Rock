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
    /// FinancialTransaction Service class
    /// </summary>
    public partial class FinancialTransactionService : Service<FinancialTransaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public FinancialTransactionService(RockContext context) : base(context)
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
        public bool CanDelete( FinancialTransaction item, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( new Service<FinancialTransactionAlert>( Context ).Queryable().Any( a => a.TransactionId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", FinancialTransaction.FriendlyTypeName, FinancialTransactionAlert.FriendlyTypeName );
                return false;
            }

            if ( new Service<FinancialTransactionRefund>( Context ).Queryable().Any( a => a.OriginalTransactionId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", FinancialTransaction.FriendlyTypeName, FinancialTransactionRefund.FriendlyTypeName );
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// FinancialTransaction View Model Helper
    /// </summary>
    [DefaultViewModelHelper( typeof( FinancialTransaction ) )]
    public partial class FinancialTransactionViewModelHelper : ViewModelHelper<FinancialTransaction, Rock.ViewModel.FinancialTransactionViewModel>
    {
        /// <summary>
        /// Converts the model to a view model.
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public override Rock.ViewModel.FinancialTransactionViewModel CreateViewModel( FinancialTransaction model, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( model == null )
            {
                return default;
            }

            var viewModel = new Rock.ViewModel.FinancialTransactionViewModel
            {
                Id = model.Id,
                Guid = model.Guid,
                AuthorizedPersonAliasId = model.AuthorizedPersonAliasId,
                BatchId = model.BatchId,
                CheckMicrEncrypted = model.CheckMicrEncrypted,
                CheckMicrHash = model.CheckMicrHash,
                CheckMicrParts = model.CheckMicrParts,
                FinancialGatewayId = model.FinancialGatewayId,
                FinancialPaymentDetailId = model.FinancialPaymentDetailId,
                ForeignCurrencyCodeValueId = model.ForeignCurrencyCodeValueId,
                FutureProcessingDateTime = model.FutureProcessingDateTime,
                IsReconciled = model.IsReconciled,
                IsSettled = model.IsSettled,
                MICRStatus = ( int? ) model.MICRStatus,
                NonCashAssetTypeValueId = model.NonCashAssetTypeValueId,
                ProcessedByPersonAliasId = model.ProcessedByPersonAliasId,
                ProcessedDateTime = model.ProcessedDateTime,
                ScheduledTransactionId = model.ScheduledTransactionId,
                SettledDate = model.SettledDate,
                SettledGroupId = model.SettledGroupId,
                ShowAsAnonymous = model.ShowAsAnonymous,
                SourceTypeValueId = model.SourceTypeValueId,
                Status = model.Status,
                StatusMessage = model.StatusMessage,
                Summary = model.Summary,
                SundayDate = model.SundayDate,
                TransactionCode = model.TransactionCode,
                TransactionDateTime = model.TransactionDateTime,
                TransactionTypeValueId = model.TransactionTypeValueId,
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
    public static partial class FinancialTransactionExtensionMethods
    {
        /// <summary>
        /// Clones this FinancialTransaction object to a new FinancialTransaction object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static FinancialTransaction Clone( this FinancialTransaction source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as FinancialTransaction;
            }
            else
            {
                var target = new FinancialTransaction();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Clones this FinancialTransaction object to a new FinancialTransaction object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static FinancialTransaction CloneWithoutIdentity( this FinancialTransaction source )
        {
            var target = new FinancialTransaction();
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
        /// Copies the properties from another FinancialTransaction object to this FinancialTransaction object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this FinancialTransaction target, FinancialTransaction source )
        {
            target.Id = source.Id;
            target.AuthorizedPersonAliasId = source.AuthorizedPersonAliasId;
            target.BatchId = source.BatchId;
            target.CheckMicrEncrypted = source.CheckMicrEncrypted;
            target.CheckMicrHash = source.CheckMicrHash;
            target.CheckMicrParts = source.CheckMicrParts;
            target.FinancialGatewayId = source.FinancialGatewayId;
            target.FinancialPaymentDetailId = source.FinancialPaymentDetailId;
            target.ForeignCurrencyCodeValueId = source.ForeignCurrencyCodeValueId;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.FutureProcessingDateTime = source.FutureProcessingDateTime;
            target.IsReconciled = source.IsReconciled;
            target.IsSettled = source.IsSettled;
            target.MICRStatus = source.MICRStatus;
            target.NonCashAssetTypeValueId = source.NonCashAssetTypeValueId;
            target.ProcessedByPersonAliasId = source.ProcessedByPersonAliasId;
            target.ProcessedDateTime = source.ProcessedDateTime;
            target.ScheduledTransactionId = source.ScheduledTransactionId;
            target.SettledDate = source.SettledDate;
            target.SettledGroupId = source.SettledGroupId;
            target.ShowAsAnonymous = source.ShowAsAnonymous;
            target.SourceTypeValueId = source.SourceTypeValueId;
            target.Status = source.Status;
            target.StatusMessage = source.StatusMessage;
            target.Summary = source.Summary;
            target.SundayDate = source.SundayDate;
            target.TransactionCode = source.TransactionCode;
            target.TransactionDateTime = source.TransactionDateTime;
            target.TransactionTypeValueId = source.TransactionTypeValueId;
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
        public static Rock.ViewModel.FinancialTransactionViewModel ToViewModel( this FinancialTransaction model, Person currentPerson = null, bool loadAttributes = false )
        {
            var helper = new FinancialTransactionViewModelHelper();
            var viewModel = helper.CreateViewModel( model, currentPerson, loadAttributes );
            return viewModel;
        }

    }

}