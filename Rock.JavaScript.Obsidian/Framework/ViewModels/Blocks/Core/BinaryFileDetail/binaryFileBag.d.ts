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

import { Guid } from "@Obsidian/Types";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

export type BinaryFileBag = {
    /** Gets or sets the attributes. */
    attributes?: Record<string, PublicAttributeBag> | null;

    /** Gets or sets the attribute values. */
    attributeValues?: Record<string, string> | null;

    /** Gets or sets the id of the Rock.Model.BinaryFileType that this file belongs to. */
    binaryFileType?: ListItemBag | null;

    /** Gets or sets a user defined description of the file. */
    description?: string | null;

    /** Gets or sets the document. */
    file?: ListItemBag | null;

    /** Gets or sets the file identifier. */
    fileId?: string | null;

    /** Gets or sets the name of the file, including any extensions. This name is usually captured when the file is uploaded to Rock and this same name will be used when the file is downloaded. This property is required. */
    fileName?: string | null;

    /** Gets or sets the identifier key of this entity. */
    idKey?: string | null;

    /** Gets or sets a value indicating whether this instance is label file. */
    isLabelFile: boolean;

    /** Gets or sets the Mime Type for the file. This property is required */
    mimeType?: string | null;

    /** Gets or sets the orphaned binary file identifier list. This holds the list of uploaded binary files that were ultimately not used and can be deleted. */
    orphanedBinaryFileIdList?: Guid[] | null;

    /** Gets or sets a value indicating whether [show binary file type]. */
    showBinaryFileType: boolean;

    /** Gets or sets a value indicating whether [show workflow button]. */
    showWorkflowButton: boolean;

    /** Gets or sets the re run workflow button text. */
    workflowButtonText?: string | null;

    /** Gets or sets the workflow notification message. */
    workflowNotificationMessage?: string | null;
};
