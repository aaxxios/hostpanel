<script setup lang="ts">
/**
 * Dynamic config form for a single integration.
 *
 * Renders the fields the API describes in `integration.fieldDefinitions`, each as the kit control
 * its `type` asks for. Emits `save` and `test` to the parent, which owns the requests.
 */
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { INTEGRATION_META } from '@/types/integrationmeta'
import type { FieldDefinitionDto, IntegrationConfigPayload, IntegrationDetailDto, IntegrationSlug } from '@/types/integration'
import { IntegrationFieldType } from '@/types/integration'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiField from '@/components/ui/UiField.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiImage from '@/components/ui/UiImage.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiLabel from '@/components/ui/UiLabel.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import UiToggle from '@/components/ui/UiToggle.vue'

/** Translation function. */
const { t } = useI18n()

/** Props for IntegrationConfigForm. */
const props = defineProps<{
  /** Full integration detail loaded from the store. */
  integration: IntegrationDetailDto
  /** True while a save request is in flight. */
  loading: boolean
  /** True while a connection test is in flight. Separate from `loading` so Save never claims to be saving during a test. */
  testing: boolean
}>()

/** Events emitted by IntegrationConfigForm. */
const emit = defineEmits<{
  /** Emitted when the admin clicks Save Changes. */
  save: [payload: IntegrationConfigPayload]
  /** Emitted when the admin clicks Test Connection. */
  test: []
}>()

/** Static metadata for this integration (logo and tile colour). */
const meta = INTEGRATION_META[props.integration.slug as IntegrationSlug]

/** Value a boolean field holds when it is on. */
const TOGGLE_ON = 'true'

/** Value a boolean field holds when it is off. */
const TOGGLE_OFF = 'false'

/** Local copy of config values the admin is editing. */
const localConfig = ref<Record<string, string>>({ ...props.integration.config })

/** Local toggle for enabled/disabled state. */
const isEnabled = ref(props.integration.isEnabled)

// Re-sync local state when a new integration is loaded.
watch(
  () => props.integration,
  (next) => {
    localConfig.value = { ...next.config }
    isEnabled.value = next.isEnabled
  },
  { deep: true },
)

/**
 * Stores a value the admin picked in a select.
 *
 * @param key - The field key.
 * @param value - The picked option value; null clears the field.
 */
const setValue = (key: string, value: string | number | null): void => {
  localConfig.value[key] = value === null ? '' : String(value)
}

/**
 * Select options of a field.
 *
 * @param field - The field definition.
 * @returns The options in the shape the select takes.
 */
const optionsOf = (field: FieldDefinitionDto): { value: string; label: string }[] =>
  (field.options ?? []).map(option => ({ value: option, label: option }))

/**
 * Stores a boolean field as the string the API keeps.
 *
 * @param key - The field key.
 * @param on - The new state.
 */
const setToggle = (key: string, on: boolean): void => {
  localConfig.value[key] = on ? TOGGLE_ON : TOGGLE_OFF
}

/** Builds and emits the save payload. */
const handleSave = (): void => {
  emit('save', { isEnabled: isEnabled.value, config: { ...localConfig.value } })
}
</script>

<template>
  <UiCard padding="md" data-testid="integration-config-form">
    <UiStack gap="lg">
      <!-- Card header: logo + name + enabled switch -->
      <UiStack direction="row" align="center" gap="md">
        <UiStack align="center" justify="center" class="h-10 w-10 shrink-0 rounded-xl p-2" :class="meta?.color ?? 'bg-text-muted'">
          <UiImage
            v-if="meta?.logo"
            :src="meta.logo"
            :alt="integration.name"
            class="h-full w-full object-contain brightness-0 invert"
          />
        </UiStack>
        <UiStack gap="xs" class="min-w-0 flex-1">
          <UiHeading :level="2" variant="card">{{ integration.name }}</UiHeading>
          <UiText tone="muted" size="sm" class="truncate">{{ integration.description }}</UiText>
        </UiStack>
        <UiLabel variant="inline" class="shrink-0">
          <UiToggle v-model="isEnabled" data-testid="integration-enabled" />
          <UiText :tone="isEnabled ? 'success' : 'muted'" size="sm" class="font-semibold">
            {{ isEnabled ? t('shared.enabled') : t('shared.disabled') }}
          </UiText>
        </UiLabel>
      </UiStack>

      <!-- Dynamic fields -->
      <UiGrid :columns="2" gap="md">
        <template v-for="field in props.integration.fieldDefinitions ?? []" :key="field.key">
          <UiField
            :label="field.label"
            :required="field.required"
            :class="field.type === IntegrationFieldType.Textarea ? 'md:col-span-2' : ''"
          >
            <template #default="{ id }">
              <UiTextarea
                v-if="field.type === IntegrationFieldType.Textarea"
                :id="id"
                v-model="localConfig[field.key]"
                rows="3"
                :placeholder="t('integrations.integrationConfigForm.enterValue', { label: field.label })"
                :data-testid="`integration-field-${field.key}`"
              />

              <UiSelect
                v-else-if="field.type === IntegrationFieldType.Select"
                :model-value="localConfig[field.key] ?? ''"
                :options="optionsOf(field)"
                @update:model-value="setValue(field.key, $event)"
                :placeholder="t('integrations.integrationConfigForm.chooseValue', { label: field.label })"
                :data-testid="`integration-field-${field.key}`"
              />

              <!-- Boolean fields are kept as the strings "true" / "false" -->
              <UiLabel v-else-if="field.type === IntegrationFieldType.Toggle" variant="inline">
                <UiToggle
                  :model-value="localConfig[field.key] === TOGGLE_ON"
                  :data-testid="`integration-field-${field.key}`"
                  @update:model-value="setToggle(field.key, $event)"
                />
                <UiText :tone="localConfig[field.key] === TOGGLE_ON ? 'success' : 'muted'" size="sm" class="font-semibold">
                  {{ localConfig[field.key] === TOGGLE_ON ? t('shared.enabled') : t('shared.disabled') }}
                </UiText>
              </UiLabel>

              <UiInput
                v-else
                :id="id"
                v-model="localConfig[field.key]"
                :type="field.type"
                class="font-mono"
                :placeholder="t('integrations.integrationConfigForm.enterValue', { label: field.label })"
                :data-testid="`integration-field-${field.key}`"
              />
            </template>
          </UiField>
        </template>
      </UiGrid>

      <!-- Actions -->
      <UiStack direction="row" gap="sm">
        <UiButton
          type="button"
          variant="primary"
          :loading="loading"
          :disabled="testing"
          data-testid="integration-save"
          @click="handleSave"
        >
          {{ t('shared.saveChanges') }}
        </UiButton>
        <UiButton
          type="button"
          variant="secondary"
          :loading="testing"
          :disabled="loading"
          data-testid="integration-test"
          @click="emit('test')"
        >
          {{ t('shared.testConnection') }}
        </UiButton>
      </UiStack>
    </UiStack>
  </UiCard>
</template>
