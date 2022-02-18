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

import { PropType, defineComponent, ref, TransitionGroup, computed, watch } from "vue";
import { FieldFilterRuleRow, FieldFilterRuleItem, FieldFilterRuleAttributeOption } from "./fieldFilterRuleRow";
import DropDownList from "../Elements/dropDownList";
import { ListItem } from "../ViewModels";
import { useVModelPassthrough } from "../Util/component";
import { FieldFilterSource} from "../ViewModels/Reporting/fieldFilterSource";

type ShowHide = "Show" | "Hide";
type AllAny = "All" | "Any";

export type FilterExpressionType = 1 | 2 | 3 | 4;

// export type FieldVisibilityRules = {
//     RuleList: FieldVisibilityRuleItem[],
//     FilterExpressionType: FilterExpressionType
// }

// Maps for converting between `FilterExpressionType` and `ShowHide`/`AllAny`
const filterExpressionTypeMap: Record<ShowHide, Record<AllAny, FilterExpressionType>> = {
    Show: {
        All: 1,
        Any: 2
    },
    Hide: {
        All: 3,
        Any: 4
    }
};
const filterExpressionToShowHideMap: ShowHide[] = ["Show", "Show", "Hide", "Hide"]; // Use FilterExpressionType - 1 as index
const filterExpressionToAllAnyMap: AllAny[] = ["All", "Any", "All", "Any"]; // Use FilterExpressionType - 1 as index


export default defineComponent({
    name: "FieldVisibilityRulesEditor",

    components: {
        TransitionGroup,
        DropDownList,
        FieldFilterRuleRow
    },

    props: {
        rules: {
            type: Array as PropType<FieldFilterRuleItem[]>,
            required: true
        },
        filterExpressionType: {
            type: Number as PropType<FilterExpressionType>,
            required: true
        },
        fieldName: {
            type: String as PropType<string>,
            required: true
        }
    },

    emits: ["update:rules", "update:filterExpressionType"],

    setup(props, { emit }) {
        const rules = useVModelPassthrough(props, "rules", emit);
        const filterExpressionType = useVModelPassthrough(props, "filterExpressionType", emit);

        const showHide = ref<ShowHide>(filterExpressionToShowHideMap[filterExpressionType.value - 1]);
        const showHideOptions: ListItem[] = [
            { text: "Show", value: "Show" },
            { text: "Hide", value: "Hide" }
        ];

        const allAny = ref<AllAny>(filterExpressionToAllAnyMap[filterExpressionType.value - 1]);
        const allAnyOptions: ListItem[] = [
            { text: "All", value: "All" },
            { text: "Any", value: "Any" }
        ];

        watch([showHide, allAny], () => {
            filterExpressionType.value = filterExpressionTypeMap[showHide.value][allAny.value];
        });

        function addRule():void {
            rules.value.push({});
        }

        function removeRule(rule: FieldFilterRuleItem): void {
            rules.value = rules.value.filter((val: FieldFilterRuleItem) => val !== rule);
        }

        const attributeOptions: Record<string, FieldFilterRuleAttributeOption> = {
            "6af9bf76-9e43-49f5-ac77-b02f59c65549": {
                name: "Position Description",
                comparators: ["Equal To", "Not Equal To", "Contains", "Does Not Contain", "Is Blank", "Is Not Blank", "Starts With", "Ends With"],
                type: "Text",
                componentProps: {}
            },
            "e5ed7172-802b-4e09-b115-08d6dac354b8": {
                name: "Number of Hours",
                comparators: ["Equal To", "Not Equal To", "Is Blank", "Is Not Blank", "Greater Than", "Greater Than Or Equal To", "Less Than", "Less Than Or Equal To"],
                type: "Integer",
                componentProps: {}
            },
            "cd8a6a09-9645-42b1-99b8-5c1d5ef77499": {
                name: "Start Date",
                comparators: ["Equal To", "Not Equal To", "Is Blank", "Is Not Blank", "Greater Than", "Greater Than Or Equal To", "Less Than", "Less Than Or Equal To"],
                type: "Date",
                componentProps: {}
            },
        }

        return {
            showHide,
            showHideOptions,
            allAny,
            allAnyOptions,
            rules,
            attributeOptions,
            addRule,
            removeRule,
        };
    },

    template: `
<div class="filtervisibilityrules-container">
    <div class="filtervisibilityrules-rulesheader">
        <div class="filtervisibilityrules-type form-inline form-inline-all">
            <DropDownList v-model="showHide" :options="showHideOptions" :show-blank-item="false" formControlClasses="input-width-sm margin-r-sm" />
            <div class="form-control-static margin-r-sm">
                <span class="filtervisibilityrules-fieldname">{{ fieldName }}</span><span class="filtervisibilityrules-if"> if</span>
            </div>
            <DropDownList v-model="allAny" :options="allAnyOptions" :show-blank-item="false" formControlClasses="input-width-sm margin-r-sm" />
            <span class="form-control-static">of the following match:</span>
        </div>
    </div>

    <div class="filtervisibilityrules-ruleslist ">
        <FieldFilterRuleRow v-for="rule in rules" :key="rule.guid" v-model="rule" :attributeOptions="attributeOptions" @removeRule="removeRule" />
    </div>

    <div class="filter-actions">
        <button class="btn btn-xs btn-action add-action" @click.prevent="addRule"><i class="fa fa-filter"></i> Add Criteria</button>
    </div>
</div>
`
});
