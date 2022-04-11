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

import { IEntity } from "../entity";

export type FinancialTransaction = IEntity & {
    authorizedPersonAliasId?: number | null;
    batchId?: number | null;
    checkMicrEncrypted?: string | null;
    checkMicrHash?: string | null;
    checkMicrParts?: string | null;
    financialGatewayId?: number | null;
    financialPaymentDetailId?: number | null;
    foreignCurrencyCodeValueId?: number | null;
    futureProcessingDateTime?: string | null;
    isReconciled?: boolean | null;
    isSettled?: boolean | null;
    mICRStatus?: number | null;
    nonCashAssetTypeValueId?: number | null;
    processedByPersonAliasId?: number | null;
    processedDateTime?: string | null;
    scheduledTransactionId?: number | null;
    settledDate?: string | null;
    settledGroupId?: string | null;
    showAsAnonymous?: boolean;
    sourceTypeValueId?: number | null;
    status?: string | null;
    statusMessage?: string | null;
    summary?: string | null;
    sundayDate?: string | null;
    transactionCode?: string | null;
    transactionDateTime?: string | null;
    transactionTypeValueId?: number;
    createdDateTime?: string | null;
    modifiedDateTime?: string | null;
    createdByPersonAliasId?: number | null;
    modifiedByPersonAliasId?: number | null;
};