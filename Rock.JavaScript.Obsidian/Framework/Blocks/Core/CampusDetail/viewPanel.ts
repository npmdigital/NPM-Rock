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

import { computed, defineComponent, PropType, ref, watch } from "vue";
import AttributeValuesContainer from "../../../Controls/attributeValuesContainer";
import { escapeHtml } from "../../../Services/string";
import { CampusViewModel } from "./types";

export type ValueDetailListItem = {
    title: string;

    textValue?: string;

    htmlValue?: string;
};

export class ValueDetailListItems {
    private values: ValueDetailListItem[] = [];

    public addTextValue(title: string, text: string): void {
        this.values.push({
            title: title,
            textValue: text
        });
    }

    public addHtmlValue(title: string, html: string): void {
        this.values.push({
            title: title,
            htmlValue: html
        });
    }

    public getValues(): ValueDetailListItem[] {
        return [...this.values.map(v => ({ ...v }))];
    }
}

export const ValueDetailList = defineComponent({
    name: "ValueDetailList",

    props: {
        modelValue: {
            type: Object as PropType<ValueDetailListItems>,
            required: false
        }
    },

    setup(props) {
        const values = ref(props.modelValue?.getValues() ?? []);

        const hasValues = computed((): boolean => {
            return values.value.length > 0;
        });

        watch(() => props.modelValue, () => {
            values.value = props.modelValue?.getValues() ?? [];
        });

        return {
            hasValues,
            values
        };
    },

    template: `
<dl v-if="hasValues">
    <template v-for="value in values">
        <dt>{{ value.title }}</dt>
        <dd v-if="value.htmlValue" v-html="value.htmlValue"></dd>
        <dd v-else>{{ value.textValue }}</dd>
    </template>
</dl>
`
});

export default defineComponent({
    name: "Core.CampusDetail.ViewPanel",

    props: {
        modelValue: {
            type: Object as PropType<CampusViewModel>,
            required: false
        }
    },

    components: {
        AttributeValuesContainer,
        ValueDetailList
    },

    setup(props) {
        // #region Values

        const attributes = ref(props.modelValue?.attributes ?? []);
        const attributeValues = ref(props.modelValue?.attributeValues ?? {});
        const description = ref(props.modelValue?.description ?? "");

        // #endregion

        // #region Computed Values

        const leftSideValues = computed((): ValueDetailListItems => {
            const values = new ValueDetailListItems();

            if (!props.modelValue) {
                return values;
            }

            if (props.modelValue.campusStatusValue) {
                values.addTextValue("Status", props.modelValue.campusStatusValue.text);
            }

            if (props.modelValue.shortCode) {
                values.addTextValue("Code", props.modelValue.shortCode);
            }

            if (props.modelValue.leaderPersonAlias) {
                values.addTextValue("Campus Leader", props.modelValue.leaderPersonAlias.text);
            }

            if (props.modelValue.serviceTimes && props.modelValue.serviceTimes.length > 0) {
                const htmlValue = props.modelValue.serviceTimes
                    .map(s => `${escapeHtml(s.value)} ${escapeHtml(s.text)}`)
                    .join("<br>");

                values.addHtmlValue("Service Times", htmlValue);
            }

            if (props.modelValue.campusSchedules && props.modelValue.campusSchedules.length > 0) {
                values.addTextValue("Campus Schedules", props.modelValue.campusSchedules.map(s => s.schedule?.text ?? "").join(", "));
            }

            return values;
        });

        const rightSideValues = computed((): ValueDetailListItems => {
            const values = new ValueDetailListItems();

            if (!props.modelValue) {
                return values;
            }

            if (props.modelValue.campusTypeValue) {
                values.addTextValue("Type", props.modelValue.campusTypeValue.text);
            }

            if (props.modelValue.url) {
                values.addTextValue("URL", props.modelValue.url);
            }

            if (props.modelValue.phoneNumber) {
                values.addTextValue("Phone Number", props.modelValue.phoneNumber);
            }

            if (props.modelValue.location) {
                values.addTextValue("Location", props.modelValue.location.text);
            }

            return values;
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        return {
            attributes,
            attributeValues,
            description,
            leftSideValues,
            rightSideValues
        };
    },

    template: `
<fieldset>
    <p v-if="description" class="description">{{ description }}</p>

    <div class="row">
        <div class="col-md-6">
            <ValueDetailList :modelValue="leftSideValues" />
        </div>

        <div class="col-md-6">
            <ValueDetailList :modelValue="rightSideValues" />
        </div>
    </div>

    <AttributeValuesContainer :modelValue="attributeValues" :attributes="attributes" />
</fieldset>
`
});
