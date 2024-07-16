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

/** The way the auto-select feature will operate. */
export const AutoSelectMode = {
    /**
     * When using the auto-select feature, only the person will be
     * selected as checked or unchecked by default.
     */
    PeopleOnly: 0,

    /**
     * When using the auto-select feature, the person will be selected
     * as checked or unchecked and a default area, group and location
     * will also be selected.
     */
    PeopleAndAreaGroupLocation: 1
} as const;

/** The way the auto-select feature will operate. */
export const AutoSelectModeDescription: Record<number, string> = {
    0: "People Only",

    1: "People And Area Group Location"
};

/** The way the auto-select feature will operate. */
export type AutoSelectMode = typeof AutoSelectMode[keyof typeof AutoSelectMode];