<script setup lang="ts">
/**
 * Phone field with a country-code picker.
 *
 * Driven by the `phoneNumber` / `countryCode` pair (it never reads `modelValue`). The flag of each
 * country is a background image set through a literal utility class, so the component carries no
 * stylesheet of its own.
 */
import { computed, ref } from 'vue'

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

/** Countries offered, with dialling code and a sample number used as the hint. */
const phoneCountries = [
  { value: 'AM', label: 'Armenia (Հայաստան)', code: '+374', placeholder: '77 123456' },
  { value: 'US', label: 'United States', code: '+1', placeholder: '201-555-0123' },
  { value: 'GB', label: 'United Kingdom', code: '+44', placeholder: '7700 900000' },
  { value: 'CA', label: 'Canada', code: '+1', placeholder: '416-555-0123' },
  { value: 'FR', label: 'France', code: '+33', placeholder: '1 42 34 56 78' },
  { value: 'DE', label: 'Germany', code: '+49', placeholder: '30 123456' },
  { value: 'IT', label: 'Italy', code: '+39', placeholder: '06 1234 5678' },
  { value: 'ES', label: 'Spain', code: '+34', placeholder: '91 123 45 67' },
  { value: 'RU', label: 'Russia', code: '+7', placeholder: '495 123-45-67' },
  { value: 'JP', label: 'Japan', code: '+81', placeholder: '90-1234-5678' },
  { value: 'CN', label: 'China', code: '+86', placeholder: '10 1234 5678' },
  { value: 'IN', label: 'India', code: '+91', placeholder: '98765 43210' },
]

/** Flag image per country, as literal classes so Tailwind emits the rules. */
const flagClasses: Record<string, string> = {
  AM: 'bg-[url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/am.svg)]',
  US: 'bg-[url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/us.svg)]',
  GB: 'bg-[url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/gb.svg)]',
  CA: 'bg-[url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/ca.svg)]',
  FR: 'bg-[url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/fr.svg)]',
  DE: 'bg-[url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/de.svg)]',
  IT: 'bg-[url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/it.svg)]',
  ES: 'bg-[url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/es.svg)]',
  RU: 'bg-[url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/ru.svg)]',
  JP: 'bg-[url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/jp.svg)]',
  CN: 'bg-[url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/cn.svg)]',
  IN: 'bg-[url(https://cdn.jsdelivr.net/gh/lipis/flag-icons/flags/4x3/in.svg)]',
}

/** ISO code of the selected country (Armenia when none is given). */
const selectedCountryCode = computed<string>(() => props.countryCode || 'AM')

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
const currentPlaceholder = computed<string>(() => selectedCountry.value?.placeholder || '77 123456')

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
      class="flex items-center gap-2 px-3 py-2 bg-surface-field border border-border border-r-0 rounded-l-[9px] cursor-pointer hover:bg-surface-hover transition-colors whitespace-nowrap"
      @click="isOpen = !isOpen"
    >
      <span
        class="inline-block w-6 h-[18px] bg-contain bg-center bg-no-repeat"
        :class="flagClasses[selectedCountryCode]"
      />
      <span class="text-[0.82rem] font-medium text-text-primary">{{ selectedCountry?.code }}</span>
      <span class="text-[0.7rem] text-text-muted" aria-hidden="true">▼</span>
    </button>

    <!-- Phone number input -->
    <input
      v-model="phoneNumberModel"
      type="text"
      :placeholder="currentPlaceholder"
      data-testid="phone-number-input"
      class="flex-1 px-3 py-2 text-[0.82rem] text-text-primary bg-surface-field border border-border border-l-0 rounded-r-[9px] placeholder:text-text-muted outline-none focus:border-primary-500/40 transition-colors"
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
        :class="selectedCountryCode === country.value ? 'bg-primary-500 text-white' : 'text-text-primary hover:bg-surface-hover'"
        @click="selectCountry(country.value)"
      >
        <span
          class="inline-block w-6 h-[18px] bg-contain bg-center bg-no-repeat"
          :class="flagClasses[country.value]"
        />
        <span>{{ country.label }}</span>
        <span class="ml-auto text-[0.75rem] font-medium opacity-75">{{ country.code }}</span>
      </div>
    </div>

    <!-- Close dropdown on outside click -->
    <div v-if="isOpen" class="fixed inset-0 z-40" @click="isOpen = false" />
  </div>
</template>
