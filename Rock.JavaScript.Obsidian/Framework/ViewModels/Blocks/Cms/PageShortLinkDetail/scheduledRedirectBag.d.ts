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

import { UtmSettingsBag } from "@Obsidian/ViewModels/Blocks/Cms/PageShortLinkDetail/utmSettingsBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

/**
 * The details of a single scheduled redirect for a short link when editing
 * a page short link item.
 */
export type ScheduledRedirectBag = {
    /** The iCalendar content that describes the custom schedule. */
    customCalendarContent?: string | null;

    /**
     * The selected named schedule. If not set then a custom schedule is
     * in use.
     */
    namedSchedule?: ListItemBag | null;

    /** An optional purpose key that will be used to categorize interactions. */
    purposeKey?: string | null;

    /** The text that describes the date range of this scheduled redirect. */
    scheduleRangeText?: string | null;

    /**
     * A short description of the schedule. For a named schedule this will
     * be the name.
     */
    scheduleText?: string | null;

    /** The URL to redirect the individual to during this scheduled period. */
    url?: string | null;

    /** The UTM settings to append to the URL when this schedule is used. */
    utmSettings?: UtmSettingsBag | null;
};