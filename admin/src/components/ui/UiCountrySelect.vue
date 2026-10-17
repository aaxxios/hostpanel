<script setup lang="ts">
/**
 * Country picker: a `UiSelect` over a fixed list of countries.
 *
 * The model is the ISO 3166 alpha-2 code. Names are the country list's own English names.
 */
import { computed } from 'vue'
import UiSelect from '@/components/ui/UiSelect.vue'

import { useI18n } from 'vue-i18n'

const { t } = useI18n()

/** Props for UiCountrySelect. */
const props = defineProps<{
  /** ISO code of the selected country. */
  modelValue: string
  /** Text shown while nothing is selected. */
  placeholder?: string
}>()

const emit = defineEmits<{
  /** Emitted with the ISO code of the country the user picks. */
  'update:modelValue': [value: string]
}>()

/** Countries offered, by ISO code and English name. */
const countries = [
  { code: 'AF', name: 'Afghanistan' },
  { code: 'AX', name: 'Åland Islands' },
  { code: 'AL', name: 'Albania' },
  { code: 'DZ', name: 'Algeria' },
  { code: 'AS', name: 'American Samoa' },
  { code: 'AD', name: 'Andorra' },
  { code: 'AO', name: 'Angola' },
  { code: 'AI', name: 'Anguilla' },
  { code: 'AQ', name: 'Antarctica' },
  { code: 'AG', name: 'Antigua And Barbuda' },
  { code: 'AR', name: 'Argentina' },
  { code: 'AM', name: 'Armenia' },
  { code: 'AW', name: 'Aruba' },
  { code: 'AU', name: 'Australia' },
  { code: 'AT', name: 'Austria' },
  { code: 'AZ', name: 'Azerbaijan' },
  { code: 'BS', name: 'Bahamas' },
  { code: 'BH', name: 'Bahrain' },
  { code: 'BD', name: 'Bangladesh' },
  { code: 'BB', name: 'Barbados' },
  { code: 'BY', name: 'Belarus' },
  { code: 'BE', name: 'Belgium' },
  { code: 'BZ', name: 'Belize' },
  { code: 'BJ', name: 'Benin' },
  { code: 'BM', name: 'Bermuda' },
  { code: 'BT', name: 'Bhutan' },
  { code: 'BO', name: 'Bolivia' },
  { code: 'BA', name: 'Bosnia And Herzegovina' },
  { code: 'BW', name: 'Botswana' },
  { code: 'BV', name: 'Bouvet Island' },
  { code: 'BR', name: 'Brazil' },
  { code: 'GB', name: 'United Kingdom' },
  { code: 'US', name: 'United States' },
  { code: 'CA', name: 'Canada' },
  { code: 'FR', name: 'France' },
  { code: 'DE', name: 'Germany' },
  { code: 'IT', name: 'Italy' },
  { code: 'ES', name: 'Spain' },
  { code: 'RU', name: 'Russia' },
  { code: 'JP', name: 'Japan' },
  { code: 'CN', name: 'China' },
  { code: 'IN', name: 'India' },
]

/** The countries as `UiSelect` options. */
const countryOptions = computed(() =>
  countries.map(c => ({ value: c.code, label: c.name })),
)

/** Two-way bridge between the prop and the inner select. */
const selectedValue = computed({
  /**
   * Reads the bound code.
   *
   * @returns The ISO code the parent holds.
   */
  get: () => props.modelValue,
  /**
   * Publishes the picked code.
   *
   * @param value - The ISO code picked in the select.
   */
  set: (value) => {
    emit('update:modelValue', value)
  },
})
</script>

<template>
  <UiSelect
    v-model="selectedValue"
    :options="countryOptions"
    :placeholder="placeholder || t('ui.uiCountrySelect.selectCountry')"
  />
</template>
