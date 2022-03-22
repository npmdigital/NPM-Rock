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
import AttributeEditor from "../../Controls/attributeEditor";
import Modal from "../../Controls/modal";
import RockField from "../../Controls/rockField";
import RockForm from "../../Controls/rockForm";
import Alert from "../../Elements/alert";
import DropDownList from "../../Elements/dropDownList";
import RockButton from "../../Elements/rockButton";
import TextBox from "../../Elements/textBox";
import { FieldType } from "../../SystemGuids";
import PaneledDetailBlockTemplate from "../../Templates/paneledDetailBlockTemplate";
import { useConfigurationValues, useInvokeBlockAction } from "../../Util/block";
import { useVModelPassthrough } from "../../Util/component";
import { alert, confirmDelete } from "../../Util/dialogs";
import { emptyGuid, Guid, normalize as normalizeGuid } from "../../Util/guid";
import { IEntity, ListItem, PublicAttribute } from "../../ViewModels";
import { PublicEditableAttributeViewModel } from "../../ViewModels/publicEditableAttribute";
import { DetailBlockViewBag, CampusViewModel } from "./CampusDetail/types";
import ViewPanel from "./CampusDetail/viewPanel";
import EditPanel from "./CampusDetail/editPanel";

export default defineComponent({
    name: "Core.CampusDetail",

    components: {
        Alert,
        EditPanel,
        PaneledDetailBlockTemplate,
        RockButton,
        RockField,
        RockForm,
        ViewPanel
    },

    setup() {
        const config = useConfigurationValues<DetailBlockViewBag<CampusViewModel>>();
        const invokeBlockAction = useInvokeBlockAction();

        // #region Values

        const blockError = ref("");

        const campusViewModel = ref(config.entity);
        const campusEditModel = ref<CampusViewModel | null>(null);

        const isEditMode = ref(false);

        // #endregion

        // #region Computed Values

        const blockTitle = computed((): string => {
            if (campusViewModel.value?.guid === emptyGuid) {
                return "Add Campus";
            }
            else if (!isEditMode.value) {
                return campusViewModel.value?.name ?? "";
            }
            else if (campusEditModel.value?.name) {
                return `Edit ${campusEditModel.value.name}`;
            }
            else {
                return "Edit Campus";
            }
        });

        const blockLabels = computed((): ListItem[] => {
            const labels: ListItem[] = [];

            if (isEditMode.value) {
                return labels;
            }

            if (campusViewModel.value?.isActive === true) {
                labels.push({ value: "success", text: "Active" });
            }
            else {
                labels.push({ value: "danger", text: "Inactive" });
            }

            return labels;
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        const onEdit = async (): Promise<boolean> => {
            await new Promise(resolve => setTimeout(resolve, 500));

            if (campusViewModel.value) {
                campusEditModel.value = {
                    ...campusViewModel.value
                };
            }

            return true;
        };

        // #endregion

        if (!config.entity) {
            blockError.value = "The specified campus could not be viewed.";
        }
        else if (config.entity.guid === emptyGuid) {
            isEditMode.value = true;
        }

        return {
            blockError,
            blockLabels,
            blockTitle,
            campusViewModel,
            campusEditModel,
            isEditMode,
            onEdit
        };
    },

    template: `
<Alert alertType="warning">
    This is an experimental block and should not be used in production.
</Alert>

<PaneledDetailBlockTemplate v-if="!blockError"
    :title="blockTitle"
    iconClass="fa fa-building-o"
    :labels="blockLabels"
    v-model:isEditMode="isEditMode"
    @edit="onEdit">
    <EditPanel v-if="isEditMode" v-model="campusEditModel" />
    <ViewPanel v-else :modelValue="campusViewModel" />
</PaneledDetailBlockTemplate>

<Alert v-if="blockError" alertType="warning">
    {{ blockError }}
</Alert>
`
});
