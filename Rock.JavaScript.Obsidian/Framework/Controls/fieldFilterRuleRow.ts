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

import { defineComponent, PropType, ref, watch, computed, defineAsyncComponent } from "vue";
import { useVModelPassthrough } from "../Util/component";
import { areEqual, newGuid } from "../Util/guid";
import DropDownList from "../Elements/dropDownList";
import TextBox from "../Elements/textBox";
import RockAttributeFilter from "./rockAttributeFilter";
import { FieldFilterRule } from "../ViewModels/Reporting/fieldFilterRule";
import { FieldFilterSource } from "../ViewModels/Reporting/fieldFilterSource";
import { ListItem } from "../ViewModels";
import { getFieldType } from "../Fields";
import { PublicFilterableAttribute } from "../ViewModels/publicFilterableAttribute";

export const FieldFilterRuleRow = defineComponent({
    name: "FieldFilterRuleRow",

    components: {
        DropDownList,
        TextBox,
        RockAttributeFilter
    },

    props: {
        modelValue: {
            type: Object as PropType<FieldFilterRule>,
            required: true
        },
        sources: {
            type: Array as PropType<FieldFilterSource[]>,
            required: true
        }
    },

    emits: [
        "update:modelValue",
        "removeRule"
    ],

    setup(props, { emit }) {
        const rule = useVModelPassthrough(props, "modelValue", emit);

        // Rule Defaults
        rule.value.guid = rule.value.guid ?? newGuid();
        rule.value.comparisonType = 0;
        rule.value.attributeGuid = props.sources[0].attribute?.attributeGuid;

        // Current Selected Attribute/Property
        const currentAttribute = computed<PublicFilterableAttribute>(() => {
            const source = props.sources.find(source => {
                areEqual(rule.value.attributeGuid ?? '', source.attribute?.attributeGuid ?? '')
            }) || props.sources[0];

            return source.attribute as PublicFilterableAttribute;
        });

        // Reset the rule after a new attribute is chosen
        watch(currentAttribute, () => {
            rule.value.comparisonType = 0x0;
            rule.value.attributeGuid = currentAttribute.value.attributeGuid;
        });

        // Convert the list of sources into the options you can choose from the 
        const attributeList = computed<ListItem[]>(() => {
            return props.sources.map(source => {
                return {
                    text: source.attribute?.name as string,
                    value: source.attribute?.attributeGuid as string
                };
            });
        });

        function removeRule(): void {
            emit("removeRule", props.modelValue);
        }

        const json = computed(() => {
            return JSON.stringify({rule: rule.value, attribute: currentAttribute.value}, null, 4);
        });

        return {
            removeRule,
            rule,
            attributeList,
            currentAttribute,
            json
        };
    },

    template: `
    <div class="filter-rule row form-row">
        <div class="col-xs-10 col-sm-11">
            <div class="row form-row">
                <div class="filter-rule-comparefield col-md-4">
                    <DropDownList :options="attributeList" v-model="rule.attributeGuid" />
                </div>
                <div class="filter-rule-fieldfilter col-md-8">
                <RockAttributeFilter :attribute="currentAttribute" v-model="rule" :filter-mode="1" required />
                <!--
                    <div class="row form-row field-criteria">
                        <div class="col-md-4" v-if="isFieldLoaded">
                            <DropDownList :options="comparisonTypes" v-model="rule.comparisonType" />
                        </div>
                        <div class="col-md-8">
                            <Suspense timeout="100">
                                <component :is="fieldComponent" v-bind="currentAttribute?.componentProps" v-model="rule.comparedToValue" />
                                <template #fallback>
                                    <div class="mt-2">Loading...</div>
                                </template>
                            </Suspense>
                        </div>
                    </div>
                -->
                </div>
            </div>
        </div>
        <div class="col-xs-2 col-sm-1">
            <button class="btn btn-danger btn-square" @click.prevent="removeRule"><i class="fa fa-times"></i></button>
        </div>
    </div>
    <pre class="mt-3">{{json}}</pre>`
});
