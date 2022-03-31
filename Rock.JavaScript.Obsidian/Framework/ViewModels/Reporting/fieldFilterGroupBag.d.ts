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
import { FilterExpressionType } from "../../Reporting/filterExpressionType";
import { FieldFilterRuleBag } from "./fieldFilterRuleBag";

/**
 * A group of filter rules/expressions that make up a logical comparison group.
 */
export type FieldFilterGroupBag = {
    /** The unique identifier of this filter group. */
    guid: Guid;

    /** The logic operator to use when joining all rules and child-groups in this group. */
    expressionType: FilterExpressionType;

    /** The collection of rules/expressions that make up this group. */
    rules?: FieldFilterRuleBag[] | null;

    /** The collection of child groups that make up any nested expressions in this group. */
    groups?: FieldFilterGroupBag[] | null;
};
