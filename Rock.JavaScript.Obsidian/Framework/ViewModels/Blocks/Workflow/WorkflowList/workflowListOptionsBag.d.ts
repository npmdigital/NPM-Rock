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

export type WorkflowListOptionsBag = {
    /** Gets or sets a value indicating whether the current user can view the workflow. */
    canView: boolean;

    /** Gets or sets a value indicating whether the grid should be visible. */
    isGridVisible: boolean;

    /** Gets or sets a value indicating whether the workflow id column should be visible. */
    isWorkflowIdColumnVisible: boolean;

    /** Gets or sets the item term. */
    itemTerm?: string | null;

    /** Gets or sets the workflow type identifier key. */
    workflowTypeIdKey?: string | null;
};