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
import CheckBox from "../../../Elements/checkBox";
import TextBox from "../../../Elements/textBox";
import { CampusViewModel } from "./types";

export default defineComponent({
    name: "Core.CampusDetail.EditPanel",

    props: {
        modelValue: {
            type: Object as PropType<CampusViewModel>,
            required: false
        }
    },

    components: {
        AttributeValuesContainer,
        CheckBox,
        TextBox
    },

    setup(props) {
        // #region Values

        const attributes = ref(props.modelValue?.attributes ?? []);
        const attributeValues = ref(props.modelValue?.attributeValues ?? {});
        const campusStatusValue = ref(props.modelValue?.campusStatusValue ?? null);
        const campusTypeValue = ref(props.modelValue?.campusTypeValue ?? null);
        const description = ref(props.modelValue?.description ?? "");
        const isActive = ref(props.modelValue?.isActive ?? false);
        const leaderPersonAlias = ref(props.modelValue?.leaderPersonAlias ?? null);
        const location = ref(props.modelValue?.location ?? null);
        const name = ref(props.modelValue?.name ?? "");
        const phoneNumber = ref(props.modelValue?.phoneNumber ?? "");
        const serviceTimes = ref(props.modelValue?.serviceTimes ?? "");
        const shortCode = ref(props.modelValue?.shortCode ?? "");
        const timeZoneId = ref(props.modelValue?.timeZoneId ?? "");
        const url = ref(props.modelValue?.url ?? "");

        // #endregion

        // #region Computed Values


        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        return {
            attributes,
            attributeValues,
            campusStatusValue,
            campusTypeValue,
            description,
            isActive,
            leaderPersonAlias,
            location,
            name,
            phoneNumber,
            serviceTimes,
            shortCode,
            timeZoneId,
            url
        };
    },

    template: `
<fieldset>
    <div class="row">
        <div class="col-md-6">
            <TextBox v-model="name"
                label="Name"
                rules="required" />
        </div>

        <div class="col-md-6">
            <CheckBox v-model="isActive"
                label="Active" />
        </div>
    </div>

    <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode />
</fieldset>
`
});
