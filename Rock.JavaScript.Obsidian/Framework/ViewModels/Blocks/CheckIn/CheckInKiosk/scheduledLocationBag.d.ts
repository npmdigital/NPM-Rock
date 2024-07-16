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

/**
 * Represents a single GroupLocation that has been scheduled. This includes
 * all the information required to display this item for modification.
 */
export type ScheduledLocationBag = {
    /** The path to the area that contains the group. */
    areaPath?: string | null;

    /** The encrypted identifier of the group location to be modified. */
    groupLocationId?: string | null;

    /**
     * The path to the group that should be scheduled. This includes
     * any parent groups in the text.
     */
    groupPath?: string | null;

    /** The name of the location. */
    locationName?: string | null;

    /** The path to the location which includes all ancestor locations. */
    locationPath?: string | null;

    /**
     * The encrypted schedule identifiers of all schedules that are currently
     * active for this group location.
     */
    scheduleIds?: string[] | null;
};