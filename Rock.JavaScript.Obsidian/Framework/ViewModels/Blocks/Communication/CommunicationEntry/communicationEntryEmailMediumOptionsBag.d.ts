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

import { MediumType } from "@Obsidian/Enums/Blocks/Communication/CommunicationEntry/mediumType";
import { Guid } from "@Obsidian/Types";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

/** Bag containing the Email Medium options for the Communication Entry block. */
export type CommunicationEntryEmailMediumOptionsBag = {
    /** Gets or sets the additional merge fields that can be used in merge field pickers. */
    additionalMergeFields?: string[] | null;

    /** Gets or sets the binary file type unique identifier. */
    binaryFileTypeGuid: Guid;

    /** Gets or sets the recipient threshold that, once exceeded, will automatically mark the communication as a bulk email. */
    bulkEmailThreshold?: number | null;

    /** Gets or sets the document folder root. */
    documentFolderRoot?: string | null;

    /** Gets or sets the address of the sender. */
    fromAddress?: string | null;

    /** Gets or sets the name of the sender. */
    fromName?: string | null;

    /** Gets or sets a value indicating whether this medium has an active transport. */
    hasActiveTransport: boolean;

    /** Gets or sets the image folder root. */
    imageFolderRoot?: string | null;

    /** Gets or sets a value indicating whether the attachment uploader is shown. */
    isAttachmentUploaderShown: boolean;

    /** Gets a value indicating whether this communication medium is unknown. */
    isUnknown: boolean;

    /** Gets or sets a value indicating whether the root is user specific. */
    isUserSpecificRoot: boolean;

    /** Gets or sets the medium entity type unique identifier. */
    mediumEntityTypeGuid: Guid;

    /** Gets the type of the medium. */
    mediumType: MediumType;

    /** Gets or sets the communication templates that can be selected. */
    templates?: ListItemBag[] | null;
};