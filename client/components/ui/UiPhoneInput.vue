<template>
  <div class="w-full">
    <label v-if="label" :for="id" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
      {{ label }}
      <span v-if="required" class="text-primary-500">*</span>
    </label>

    <div class="relative phone-input-container" :class="{ 'has-error': error }" :data-testid="testid">
      <ClientOnly>
        <vue-tel-input
          v-bind="bindProps"
          :model-value="modelValue"
          @on-input="handleTelInput"
        />
      </ClientOnly>

      <AlertCircle
        v-if="error"
        :size="20"
        :stroke-width="2"
        class="absolute right-3 top-1/2 -translate-y-1/2 text-red-500 pointer-events-none z-10"
      />
    </div>

    <p v-if="error" class="mt-2 text-sm text-red-500 dark:text-red-400">{{ error }}</p>
    <p v-else-if="hint" class="mt-2 text-sm text-gray-500">{{ hint }}</p>
  </div>
</template>

<script setup lang="ts">
/**
 * PhoneInput (`UiPhoneInput`) - an international phone field with a country picker.
 *
 * Wraps the tel-input widget; its own DOM is restyled in `assets/css/ui.css` (the widget's markup
 * is not ours, so that is the one place classes cannot be used). It renders on the client only,
 * and emits the number in international format when it parses, otherwise the raw text.
 */
import { computed } from 'vue'
import { VueTelInput } from 'vue-tel-input'
import 'vue-tel-input/vue-tel-input.css'
import { AlertCircle } from 'lucide-vue-next'

/** The parsed phone-number object emitted by the tel input. */
interface PhoneParseResult {
  /** The number in international format, when valid. */
  number?: string
}

/** Props of PhoneInput. */
interface Props {
  /** DOM id of the inner input. */
  id?: string
  /** Label text above the field. */
  label?: string
  /** The number (v-model). */
  modelValue?: string
  /** Marks the field required and adds the asterisk. */
  required?: boolean
  /** Disables the field. */
  disabled?: boolean
  /** Error line; also switches the field to its error look. */
  error?: string
  /** Neutral help line, hidden while an error shows. */
  hint?: string
  /** `data-testid` of the field container. */
  testid?: string
}

/** Props of the component with their defaults applied. */
const props = withDefaults(defineProps<Props>(), {
  id: undefined,
  label: undefined,
  modelValue: undefined,
  required: false,
  disabled: false,
  error: undefined,
  hint: undefined,
  testid: undefined,
})

/** Emits the component raises. */
const emit = defineEmits<{
  /** The number changed. */
  'update:modelValue': [value: string]
  /** The tel input parsed a number; carries its parse result. */
  onInput: [result: PhoneParseResult]
}>()

/** Translator for the strings this component builds itself. */
const { t } = useI18n()

/**
 * Normalises the tel input event: the parsed international number when present, else the raw one.
 *
 * @param number The raw text typed into the field.
 * @param object The parsed phone object from the tel input.
 */
const handleTelInput = (number: string, object: PhoneParseResult): void => {
  const value = (object && object.number) ? object.number : number
  emit('update:modelValue', value)
  emit('onInput', object)
}

/** Options handed to the tel input widget. */
const bindProps = computed(() => ({
  mode: 'international' as const,
  disabled: props.disabled,
  placeholder: t('ui.phoneNumber'),
  required: props.required,
  dropdownOptions: {
    showSearchBox: true,
    showFlags: true,
    showDialCodeInList: true,
    showDialCodeInSelection: false,
  },
  inputOptions: {
    id: props.id,
    showDialCode: true,
    placeholder: t('ui.phoneExample'),
  },
  autoDefaultCountry: true,
  // Armenia is the operator's home market, so it is the country offered first.
  defaultCountry: 'AM',
}))
</script>
