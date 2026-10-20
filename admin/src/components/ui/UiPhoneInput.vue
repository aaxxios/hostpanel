<script setup lang="ts">
/**
 * Phone field with a country-code picker.
 *
 * Driven by the `phoneNumber` / `countryCode` pair (it never reads `modelValue`). The flag of each
 * country is the emoji the geo atlas carries for it, so the component loads no image from a host.
 */
import { computed, ref } from 'vue'
import { useGeoOptions } from '@/composables/useGeoOptions'

/** Props for UiPhoneInput. */
const props = defineProps<{
  /**
   * Combined phone value.
   *
   * Optional: this component is driven by the `phoneNumber`/`countryCode` pair and never
   * reads `modelValue`. It was declared required, which made every two-field call site —
   * the only kind there is — a type error.
   */
  modelValue?: string | null
  /** The national number (two-way bound with `v-model:phone-number`). */
  phoneNumber?: string | null
  /** ISO code of the selected country (two-way bound with `v-model:country-code`). */
  countryCode?: string
  /** Not used: the hint comes from the selected country. Kept for call-site compatibility. */
  placeholder?: string
}>()

const emit = defineEmits<{
  /** Reserved for the combined value; never emitted today. */
  'update:modelValue': [value: string | null]
  /** Emitted with the national number as the user types. */
  'update:phoneNumber': [value: string | null]
  /** Emitted with the ISO code of the country the user picks. */
  'update:countryCode': [value: string]
}>()

/** Whether the country list is open. */
const isOpen = ref(false)

/** Sample national number per offered country, in the order the list shows them. */
const SAMPLE_NUMBERS: ReadonlyArray<{ value: string, placeholder: string }> = [
  { value: 'US', placeholder: '201-555-0123' },
  { value: 'AM', placeholder: '77 123456' },
  { value: 'GB', placeholder: '7700 900000' },
  { value: 'CA', placeholder: '416-555-0123' },
  { value: 'FR', placeholder: '1 42 34 56 78' },
  { value: 'DE', placeholder: '30 123456' },
  { value: 'IT', placeholder: '06 1234 5678' },
  { value: 'ES', placeholder: '91 123 45 67' },
  { value: 'RU', placeholder: '495 123-45-67' },
  { value: 'JP', placeholder: '90-1234-5678' },
  { value: 'CN', placeholder: '10 1234 5678' },
  { value: 'IN', placeholder: '98765 43210' },
]

/** Country list of the geo atlas: the name, flag and dialling code of each offered country come from here. */
const { geoCountries, resolvePhoneCode } = useGeoOptions()

/** Countries offered, with dialling code, flag and a sample number used as the hint. */
const phoneCountries = SAMPLE_NUMBERS.map((sample) => {
  const country = geoCountries.find((c) => c.iso2 === sample.value)
  return {
    value: sample.value,
    label: country?.name ?? sample.value,
    flag: country?.emoji ?? '',
    code: resolvePhoneCode(sample.value),
    placeholder: sample.placeholder,
  }
})

/** ISO code of the selected country (the first listed when none is given). */
const selectedCountryCode = computed<string>(() => props.countryCode || phoneCountries[0]!.value) // list is non-empty

/** The national number, bound to the text field. */
const phoneNumberModel = computed<string>({
  /**
   * Reads the national number.
   *
   * @returns The number shown in the text field.
   */
  get: () => props.phoneNumber || '',
  /**
   * Publishes the typed national number.
   *
   * @param value - The text typed in the field.
   */
  set: (value) => {
    emit('update:phoneNumber', value)
  },
})

/** The selected country entry. */
const selectedCountry = computed(() =>
  phoneCountries.find(c => c.value === selectedCountryCode.value),
)

/** Sample number of the selected country, shown as the field hint. */
const currentPlaceholder = computed<string>(() => selectedCountry.value?.placeholder ?? phoneCountries[0]!.placeholder) // list is non-empty

/**
 * Picks a country and closes the list.
 *
 * @param code - ISO code of the chosen country.
 */
const selectCountry = (code: string): void => {
  emit('update:countryCode', code)
  isOpen.value = false
}
</script>

<template>
  <div class="relative flex items-center gap-0 w-full">
    <!-- Country code button (dropdown trigger) -->
    <button
      type="button"
      data-testid="phone-country-trigger"
      class="flex items-center gap-2 px-3 py-2 bg-surface-field border border-border border-r-0 rounded-l-[9px] cursor-pointer hover:bg-surface-hover outline-none focus-visible:border-primary-500/50 focus-visible:ring-2 focus-visible:ring-primary-500/30 transition-colors whitespace-nowrap"
      @click="isOpen = !isOpen"
    >
      <span aria-hidden="true">{{ selectedCountry?.flag }}</span>
      <span class="text-[0.82rem] font-medium text-text-primary">{{ selectedCountry?.code }}</span>
      <span class="text-[0.7rem] text-text-secondary" aria-hidden="true">▼</span>
    </button>

    <!-- Phone number input -->
    <input
      v-model="phoneNumberModel"
      type="text"
      :placeholder="currentPlaceholder"
      data-testid="phone-number-input"
      class="flex-1 px-3 py-2 text-[0.82rem] text-text-primary bg-surface-field border border-border border-l-0 rounded-r-[9px] placeholder:text-text-muted outline-none focus:border-primary-500/50 focus:ring-2 focus:ring-primary-500/10 transition-colors"
    >

    <!-- Country dropdown -->
    <div
      v-if="isOpen"
      class="absolute top-full left-0 mt-1 z-50 bg-surface-card border border-border rounded-lg shadow-lg max-h-72 overflow-y-auto w-80"
    >
      <div
        v-for="country in phoneCountries"
        :key="country.value"
        :data-testid="`phone-country-${country.value}`"
        class="px-3 py-2.5 text-[0.82rem] cursor-pointer transition-colors flex items-center gap-2.5"
        :class="selectedCountryCode === country.value ? 'bg-primary-600 light:bg-primary-700 text-white' : 'text-text-primary hover:bg-surface-hover'"
        @click="selectCountry(country.value)"
      >
        <span aria-hidden="true">{{ country.flag }}</span>
        <span>{{ country.label }}</span>
        <span class="ml-auto text-[0.75rem] font-medium opacity-90">{{ country.code }}</span>
      </div>
    </div>

    <!-- Close dropdown on outside click -->
    <div v-if="isOpen" class="fixed inset-0 z-40" @click="isOpen = false" />
  </div>
</template>
