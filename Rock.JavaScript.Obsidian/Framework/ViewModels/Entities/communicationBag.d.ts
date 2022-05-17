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

import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

/** Communication View Model */
export type CommunicationBag = {
    /** Gets or sets the AdditionalMergeFieldsJson. */
    additionalMergeFieldsJson?: string | null;

    /** Gets or sets the BCCEmails. */
    bCCEmails?: string | null;

    /** Gets or sets the CCEmails. */
    cCEmails?: string | null;

    /** Gets or sets the CommunicationTemplateId. */
    communicationTemplateId?: number | null;

    /** Gets or sets the CommunicationType. */
    communicationType: number;

    /** Gets or sets the EnabledLavaCommands. */
    enabledLavaCommands?: string | null;

    /** Gets or sets the ExcludeDuplicateRecipientAddress. */
    excludeDuplicateRecipientAddress: boolean;

    /** Gets or sets the FromEmail. */
    fromEmail?: string | null;

    /** Gets or sets the FromName. */
    fromName?: string | null;

    /** Gets or sets the FutureSendDateTime. */
    futureSendDateTime?: string | null;

    /** Gets or sets the IsBulkCommunication. */
    isBulkCommunication: boolean;

    /** Gets or sets the ListGroupId. */
    listGroupId?: number | null;

    /** Gets or sets the Message. */
    message?: string | null;

    /** Gets or sets the MessageMetaData. */
    messageMetaData?: string | null;

    /** Gets or sets the Name. */
    name?: string | null;

    /** Gets or sets the PushData. */
    pushData?: string | null;

    /** Gets or sets the PushImageBinaryFileId. */
    pushImageBinaryFileId?: number | null;

    /** Gets or sets the PushMessage. */
    pushMessage?: string | null;

    /** Gets or sets the PushOpenAction. */
    pushOpenAction?: number | null;

    /** Gets or sets the PushOpenMessage. */
    pushOpenMessage?: string | null;

    /** Gets or sets the PushSound. */
    pushSound?: string | null;

    /** Gets or sets the PushTitle. */
    pushTitle?: string | null;

    /** Gets or sets the ReplyToEmail. */
    replyToEmail?: string | null;

    /** Gets or sets the ReviewedDateTime. */
    reviewedDateTime?: string | null;

    /** Gets or sets the ReviewerNote. */
    reviewerNote?: string | null;

    /** Gets or sets the ReviewerPersonAliasId. */
    reviewerPersonAliasId?: number | null;

    /** Gets or sets the SegmentCriteria. */
    segmentCriteria: number;

    /** Gets or sets the Segments. */
    segments?: string | null;

    /** Gets or sets the SendDateTime. */
    sendDateTime?: string | null;

    /** Gets or sets the SenderPersonAliasId. */
    senderPersonAliasId?: number | null;

    /** Gets or sets the SMSFromDefinedValueId. */
    sMSFromDefinedValueId?: number | null;

    /** Gets or sets the SMSMessage. */
    sMSMessage?: string | null;

    /** Gets or sets the Status. */
    status: number;

    /** Gets or sets the Subject. */
    subject?: string | null;

    /** Gets or sets the SystemCommunicationId. */
    systemCommunicationId?: number | null;

    /** Gets or sets the UrlReferrer. */
    urlReferrer?: string | null;

    /** Gets or sets the CreatedDateTime. */
    createdDateTime?: string | null;

    /** Gets or sets the ModifiedDateTime. */
    modifiedDateTime?: string | null;

    /** Gets or sets the CreatedByPersonAliasId. */
    createdByPersonAliasId?: number | null;

    /** Gets or sets the ModifiedByPersonAliasId. */
    modifiedByPersonAliasId?: number | null;

    /** Gets or sets the identifier key of this entity. */
    idKey?: string | null;

    /** Gets or sets the attributes. */
    attributes?: Record<string, PublicAttributeBag> | null;

    /** Gets or sets the attribute values. */
    attributeValues?: Record<string, string> | null;
};