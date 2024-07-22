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

/** The type of the Web Farm Log */
export const LogType = {
    /** Availability */
    Availability: 0,

    /** Ping */
    Ping: 1,

    /** Pong */
    Pong: 2,

    /** Startup */
    Startup: 3,

    /** Shutdown */
    Shutdown: 4,

    /** Error */
    Error: 5
} as const;

/** The type of the Web Farm Log */
export const LogTypeDescription: Record<number, string> = {
    0: "Availability",

    1: "Ping",

    2: "Pong",

    3: "Startup",

    4: "Shutdown",

    5: "Error"
};

/** The type of the Web Farm Log */
export type LogType = typeof LogType[keyof typeof LogType];