﻿// <copyright>
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
import { Select as AntSelect } from "ant-design-vue";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import RockFormField from "./rockFormField";
import { deepEqual } from "@Obsidian/Utility/util";
import { standardRockFormFieldProps, updateRefValue, useStandardRockFormFieldProps } from "@Obsidian/Utility/component";
import { areEqual, toGuidOrNull } from "@Obsidian/Utility/guid";

/** The type definition for a select option, since the ones from the library are wrong. */
type SelectOption = {
    value?: string;

    label: string;

    options?: SelectOption[];
};

/**
 * The default compare value function. This checks if both values are GUIDs
 * and if so does a case-insensitive compare, otherwise it does a case-sensitive
 * compare of the two values.
 * 
 * @param value The value selected in the UI.
 * @param itemValue The item value to be compared against.
 *
 * @returns true if the two values are considered equal; otherwise false.
 */
function defaultCompareValue(value: string, itemValue: string): boolean {
    const guidValue = toGuidOrNull(value);
    const guidItemValue = toGuidOrNull(itemValue);

    if (guidValue !== null && guidItemValue !== null) {
        return areEqual(guidValue, guidItemValue);
    }

    return value === itemValue;
}

export default defineComponent({
    name: "DropDownList",

    components: {
        AntSelect,
        RockFormField,
        VNodes: (_, { attrs }) => {
            return attrs.vnodes;
        }
    },

    props: {
        modelValue: {
            type: Object as PropType<string | string[]>,
            required: true
        },

        items: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        },

        showBlankItem: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        blankValue: {
            type: String as PropType<string>,
            default: ""
        },

        multiple: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /** No longer used. */
        formControlClasses: {
            type: String as PropType<string>,
            default: ""
        },

        enhanceForLongLists: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        grouped: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        disabled: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        loading: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        compareValue: {
            type: Function as PropType<((value: string, itemValue: string) => boolean)>,
            default: defaultCompareValue
        },

        ...standardRockFormFieldProps
    },

    emits: {
        open: () => true,
        "update:modelValue": (_value: string | string[]) => true
    },

    setup(props, { emit }) {
        // #region Values

        const internalValue = ref(props.modelValue ? props.modelValue : null);
        const controlWrapper = ref<HTMLElement | null>(null);
        const standardFieldProps = useStandardRockFormFieldProps(props);

        // #endregion

        // #region Computed Values

        /** Determines if the blank item should be used. */
        const computedShowBlankItem = computed((): boolean => {
            // Only show the blank item if requested and we are not in multiple
            // selection mode.
            return !props.multiple && props.showBlankItem;
        });

        /** The options to be used by the Ant Select box. */
        const computedOptions = computed((): SelectOption[] => {
            // If we are not showing grouped items then simply map our item bags
            // into a format that can be used by the picker.
            if (!props.grouped) {
                return props.items.map((o): SelectOption => {
                    return {
                        value: o.value ?? "",
                        label: o.text ?? ""
                    };
                });
            }

            const groupedOptions: SelectOption[] = [];

            // Loop through all the options and group everything that has a
            // category together.
            for (const o of props.items) {
                // If no category then just include it as a regular item.
                if (!o.category) {
                    groupedOptions.push({
                        value: o.value ?? "",
                        label: o.text ?? ""
                    });
                    continue;
                }

                const matchedGroups = groupedOptions.filter(g => g.label === o.category && !!g.options);

                // If we found an existing group then just add this item to
                // that group. Otherwise create a new group for this item.
                if (matchedGroups.length >= 1 && !!matchedGroups[0].options) {
                    matchedGroups[0].options.push({
                        value: o.value ?? "",
                        label: o.text ?? ""
                    });
                }
                else {
                    groupedOptions.push({
                        label: o.category,
                        options: [{
                            value: o.value ?? "",
                            label: o.text ?? ""
                        }]
                    });
                }
            }

            return groupedOptions;
        });

        /** Determines if the control is currently in a loading state. */
        const computedLoading = computed((): boolean => {
            return props.loading;
        });

        /** The mode for the Ant Select control to operate in. */
        const mode = computed((): "multiple" | undefined => {
            return props.multiple ? "multiple" : undefined;
        });

        /** Determines if we have any selected values. */
        const hasValue = computed((): boolean => {
            if (Array.isArray(internalValue.value)) {
                return internalValue.value.length > 0;
            }
            else {
                return internalValue.value !== "";
            }
        });

        /** Determines if the clear icon should be visible. */
        const isClearable = computed((): boolean => {
            return computedShowBlankItem.value && !computedLoading.value && hasValue.value;
        });

        /** Determines if the control should be in a disabled state. */
        const isDisabled = computed((): boolean => {
            return props.disabled;
        });

        // #endregion

        // #region Functions

        /**
         * Synchronizes our internal value with the modelValue and current
         * component property values.
         */
        const syncInternalValue = (): void => {
            let value: string | string[] | null = props.modelValue;

            if (props.multiple) {
                // We are in multiple mode, if our value is a single value then
                // convert it to an array of the one value.
                if (!Array.isArray(value)) {
                    value = value === "" ? [] : [value];
                }

                // Ensure they are all valid values and make sure they are the
                // correct matching value from the item rather than what was
                // originally provided.
                value = props.items
                    .filter(o => (value as string[]).some(v => props.compareValue(v, o.value ?? "")))
                    .map(o => o.value ?? "");
            }
            else {
                // We are in single mode, if our value is an array of values then
                // convert it to a single value by taking the first value.
                if (Array.isArray(value)) {
                    value = value.length === 0 ? null : value[0];
                }

                // If no value is selected, then take either the blank value
                // or the first value in the list.
                if (value === null) {
                    value = computedShowBlankItem.value
                        ? props.blankValue
                        : (props.items[0]?.value || props.blankValue);
                }

                // Ensure it is a valid value, if not then set it to either the
                // blank value or the first value in the list.
                const selectedOption = props.items.find(o => props.compareValue(value as string, o.value ?? "")) || null;

                if (!selectedOption) {
                    value = computedShowBlankItem.value
                        ? props.blankValue
                        : (props.items[0]?.value || props.blankValue);
                }
                else {
                    value = selectedOption.value ?? "";
                }
            }

            updateRefValue(internalValue, value);
        };

        /**
         * Determines if a single option should be included during a search
         * operation.
         * 
         * @param input The search string typed by the individual.
         * @param option The option to be filtered.
         *
         * @returns true if the option should be included in the list, otherwise false.
         */
        const filterItem = (input: string, option: SelectOption): boolean => {
            return (option.label || "").toLocaleLowerCase().indexOf(input.toLocaleLowerCase()) >= 0;
        };

        /**
         * Gets the element that will contain the popup. By default this is the
         * document body, but that breaks if the user is viewing the page
         * fullscreen via one of the panel fullscreen buttons.
         *
         * @returns The HTML element to place the popup into.
         */
        const getPopupContainer = (): HTMLElement => {
            return controlWrapper.value ?? document.body;
        };

        // #endregion

        // #region Event Handlers

        const onDropdownVisibleChange = (open: boolean): void => {
            if (open) {
                emit("open");
            }
        };

        // #endregion

        watch([() => props.modelValue, computedShowBlankItem, () => props.multiple, () => props.items], () => {
            syncInternalValue();
        });

        // Watch for changes to the selection made in the UI and then make
        // make sure its in the right format and valid.
        watch(internalValue, () => {
            let newValue = internalValue.value;

            if (props.multiple) {
                // We are in multiple select mode, but if we have a non-array
                // value then convert it to an array.
                if (!Array.isArray(newValue)) {
                    newValue = newValue === null ? [] : [newValue];
                }
            }
            else {
                // We are in single select mode, but if we have an array
                // value then convert it to a single item.
                if (Array.isArray(newValue)) {
                    newValue = newValue.length === 0 ? null : newValue[0];
                }

                // Ensure that single item is valid.
                if (newValue === null) {
                    newValue = computedShowBlankItem.value
                        ? props.blankValue
                        : (props.items[0]?.value || props.blankValue);
                }
            }

            // If the value hasn't changed, then emit the new value. Normally
            // we wouldn't have to do this check, but when emitting complex
            // things like an array it can sometimes cause unwanted loops if
            // we don't.
            if (!deepEqual(props.modelValue, newValue, true)) {
                emit("update:modelValue", newValue);
            }
        });

        syncInternalValue();

        return {
            computedLoading,
            computedOptions,
            controlWrapper,
            filterItem,
            internalValue,
            isClearable,
            isDisabled,
            getPopupContainer,
            mode,
            onDropdownVisibleChange,
            standardFieldProps
        };
    },

    template: `
<RockFormField
    v-bind="standardFieldProps"
    :modelValue="internalValue"
    :formGroupClasses="'rock-drop-down-list ' + formGroupClasses"
    name="dropdownlist">
    <template #default="{uniqueId, field}">
        <div ref="controlWrapper" class="control-wrapper">
            <AntSelect
                v-model:value="internalValue"
                v-bind="field"
                class="form-control"
                :allowClear="isClearable"
                :loading="computedLoading"
                :disabled="isDisabled"
                :options="computedOptions"
                :showSearch="enhanceForLongLists"
                :filterOption="filterItem"
                :mode="mode"
                :getPopupContainer="getPopupContainer"
                @dropdownVisibleChange="onDropdownVisibleChange">
                <template #clearIcon>
                    <i class="fa fa-times"></i>
                </template>

                <template #suffixIcon>
                    <i v-if="!computedLoading" class="fa fa-caret-down"></i>
                    <i v-else class="fa fa-spinner fa-spin"></i>
                </template>

                <template #dropdownRender="{ menuNode: menu }">
                    <div v-if="computedLoading" class="text-center"><i class="fa fa-spinner fa-spin"></i> Data is loading...</div>
                    <v-nodes v-else :vnodes="menu" />
                </template>
            </AntSelect>
        </div>
    </template>
</RockFormField>
`
});