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

import { IEntity, ListItem } from "../../../ViewModels";

export const enum NavigationUrlKey {
    ParentPage = "ParentPage"
}

export type CampusSchedulePacket = {
    guid?: string | null;

    schedule?: ListItem | null;

    scheduleTypeValue?: ListItem | null;
}

export type CampusPacket = IEntity & {
    campusSchedules?: CampusSchedulePacket[] | null;

    campusStatusValue?: ListItem | null;

    campusTypeValue?: ListItem | null;

    description?: string | null;

    isActive?: boolean;

    isSystem: boolean;

    leaderPersonAlias?: ListItem | null;

    location?: ListItem | null;

    name?: string | null;

    phoneNumber?: string | null;

    serviceTimes?: ListItem[] | null;

    shortCode?: string | null;

    timeZoneId?: string | null;

    url?: string | null;
};

export type CampusDetailOptions = {
    isMultiTimeZoneSupported?: boolean;

    timeZoneOptions?: ListItem[] | null;
};

// #region Core Types

export type DetailBlockViewBag<TPacket, TOptions> = {
    entity?: TPacket | null;

    isEditable?: boolean;

    errorMessage?: string | null;

    navigationUrls?: Record<string, string> | null;

    options?: TOptions | null;
};

export type DetailBlockEditBag<TPacket, TOptions> = {
    entity?: TPacket | null;

    options?: TOptions | null;
};

export type DetailBlockSaveBag<TPacket> = {
    entity?: TPacket | null;

    validProperties?: string[] | null;
};

export type DetailBlockSaveResultBag<TPacket> = {
    entity?: TPacket | null;

    redirectUrl?: string | null;
};

// #endregion
