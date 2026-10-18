<script setup lang="ts">
/**
 * Branding card — uploads the storefront's logo and favicon.
 *
 * Mirrors the slide image uploader (`SlideFormView.vue`): drag-and-drop or browse, uploaded straight
 * to disk via a dedicated endpoint that returns a URL, which is what actually gets saved to the
 * setting. The setting itself still accepts a pasted URL too, for an operator who already hosts the
 * file elsewhere and would rather not upload a copy.
 */
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import SettingsSection from '@/components/settings/SettingsSection.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiIconButton from '@/components/ui/UiIconButton.vue'
import UiImage from '@/components/ui/UiImage.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import UiUploadZone from '@/components/ui/UiUploadZone.vue'
import { useSettingsStore } from '@/stores/settings'
import type { Setting } from '@/types/setting'
import { BrandingImageKind } from '@/types/upload'

const { t } = useI18n()

/** Props for BrandingCard. */
const props = defineProps<{
  /** All system settings, as loaded from the backend. */
  settings: Setting[]
  /** True while a save is in flight. */
  saving?: boolean
}>()

const emit = defineEmits<{
  /** Emitted with the row id and the new value when the operator saves an image. */
  save: [id: number, value: string]
}>()

/** Settings store; owns the upload so the component never touches transport. */
const settingsStore = useSettingsStore()

/** One branding image field: which setting key it saves to, and upload constraints. */
interface BrandingField {
  /** Settings key the image URL is saved to. */
  key: string
  /** Route segment of the upload endpoint; the backend's BrandingKind name, lower-cased. */
  kind: BrandingImageKind
  /** Label key. */
  labelKey: string
  /** Hint key (formats and size limit). */
  hintKey: string
  /** Accepted MIME types. */
  accept: string
  /** Preview box size — the favicon and mark are square and small, the logos wide and short. */
  previewClass: string
  /**
   * Preview tile background. A light-background logo previewed on the panel's dark tile is
   * unreadable — which is the very mistake the dark variant exists to fix — so each field
   * previews on the surface it will actually sit on.
   */
  previewBg: string
}

/** MIME types accepted for the logos. */
const LOGO_ACCEPT = 'image/jpeg,image/png,image/webp,image/gif,image/bmp'

/** The four images the card manages. */
const FIELDS: BrandingField[] = [
  {
    key: 'portal.logo',
    kind: BrandingImageKind.Logo,
    labelKey: 'settings.branding.logo.label',
    hintKey: 'settings.branding.logo.hint',
    accept: LOGO_ACCEPT,
    previewClass: 'w-32 h-14',
    previewBg: 'bg-white',
  },
  {
    key: 'portal.logo.dark',
    kind: BrandingImageKind.LogoDark,
    labelKey: 'settings.branding.logoDark.label',
    hintKey: 'settings.branding.logoDark.hint',
    accept: LOGO_ACCEPT,
    previewClass: 'w-32 h-14',
    previewBg: 'bg-neutral-900',
  },
  {
    key: 'portal.logo.mark',
    kind: BrandingImageKind.LogoMark,
    labelKey: 'settings.branding.logoMark.label',
    hintKey: 'settings.branding.logoMark.hint',
    accept: LOGO_ACCEPT,
    previewClass: 'w-14 h-14',
    previewBg: 'bg-neutral-900',
  },
  {
    key: 'portal.favicon',
    kind: BrandingImageKind.Favicon,
    labelKey: 'settings.branding.favicon.label',
    hintKey: 'settings.branding.favicon.hint',
    accept: 'image/png,image/jpeg,image/webp',
    previewClass: 'w-10 h-10',
    previewBg: 'bg-neutral-900',
  },
]

/** Unsaved edit per field, keyed by setting key. */
const draft = ref<Record<string, string>>({})

/** Whether an upload is in flight, keyed by setting key. */
const uploading = ref<Record<string, boolean>>({})

/** The refusal sentence of the last failed upload, keyed by setting key. */
const uploadError = ref<Record<string, string | null>>({})

/**
 * The stored setting row for a field, if it has been seeded.
 *
 * @param field - The branding field.
 * @returns The row, or undefined when the key was never seeded.
 */
const settingOf = (field: BrandingField): Setting | undefined => props.settings.find((s) => s.key === field.key)

/**
 * Current value shown for a field: the operator's unsaved edit, or what is stored.
 *
 * @param field - The branding field.
 * @returns The image URL, `''` when there is none.
 */
const valueOf = (field: BrandingField): string => draft.value[field.key] ?? settingOf(field)?.value ?? ''

/**
 * Whether a field has an edit worth saving.
 *
 * @param field - The branding field.
 * @returns True when the draft differs from what is stored.
 */
const isDirty = (field: BrandingField): boolean =>
  field.key in draft.value && draft.value[field.key] !== (settingOf(field)?.value ?? '')

/**
 * Saves one field. No-op when the setting row does not exist yet or nothing changed.
 *
 * @param field - The branding field.
 */
const save = (field: BrandingField): void => {
  const setting = settingOf(field)
  const edited = draft.value[field.key]
  if (!setting || !isDirty(field) || edited === undefined) {
    return
  }
  emit('save', setting.id, edited)
}

/**
 * Uploads a file for one field and stages the returned URL as its draft.
 *
 * The store sends it as multipart data; a refusal arrives as the API's own sentence.
 *
 * @param field - The branding field being uploaded for.
 * @param file - The image file to upload.
 */
const uploadFile = async (field: BrandingField, file: File): Promise<void> => {
  uploadError.value[field.key] = null
  uploading.value[field.key] = true
  const url = await settingsStore.uploadBrandingFile(field.kind, file)
  if (url !== null) {
    draft.value[field.key] = url
  } else {
    uploadError.value[field.key] = settingsStore.uploadError
  }
  uploading.value[field.key] = false
}
</script>

<template>
  <SettingsSection
    :title="t('settings.branding.title')"
    :description="t('settings.branding.description')"
  >
    <UiStack v-for="field in FIELDS" :key="field.key" gap="sm">
      <UiText tone="secondary" size="md">
        {{ t(field.labelKey) }}
      </UiText>

      <UiStack direction="row" align="start">
        <!-- Preview -->
        <UiStack
          v-if="valueOf(field)"
          align="center"
          justify="center"
          gap="none"
          class="group relative shrink-0 overflow-hidden rounded-lg border border-border"
          :class="[field.previewClass, field.previewBg]"
        >
          <UiImage
            :src="valueOf(field)"
            :alt="t(field.labelKey)"
            fit="contain"
            class="max-h-full max-w-full"
            :data-testid="`branding-preview-${field.kind}`"
          />
          <UiIconButton
            :label="t('settings.branding.remove')"
            size="sm"
            class="absolute right-1 top-1 opacity-0 transition-opacity group-hover:opacity-100"
            :data-testid="`branding-remove-${field.kind}`"
            @click="draft[field.key] = ''"
          >
            <svg class="h-3 w-3" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" aria-hidden="true">
              <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </UiIconButton>
        </UiStack>

        <UiStack gap="sm" class="min-w-56 flex-1">
          <UiUploadZone
            compact
            :accept="field.accept"
            :uploading="uploading[field.key] === true"
            :hint="t(field.hintKey)"
            :input-testid="`branding-file-${field.kind}`"
            @select="uploadFile(field, $event)"
          />

          <UiStack direction="row" align="center" gap="sm">
            <UiInput
              :model-value="valueOf(field)"
              type="text"
              size="sm"
              :placeholder="t('settings.branding.pasteUrl')"
              :data-testid="`branding-url-${field.kind}`"
              class="flex-1"
              @update:model-value="draft[field.key] = $event"
            />
            <UiButton
              v-if="isDirty(field)"
              type="button"
              variant="primary"
              size="sm"
              :disabled="saving"
              :data-testid="`branding-save-${field.kind}`"
              @click="save(field)"
            >
              {{ saving ? t('common.saving') : t('common.save') }}
            </UiButton>
          </UiStack>
          <UiText
            v-if="uploadError[field.key]"
            tone="danger"
            size="sm"
            :data-testid="`branding-error-${field.kind}`"
          >
            {{ uploadError[field.key] }}
          </UiText>
        </UiStack>
      </UiStack>
    </UiStack>
  </SettingsSection>
</template>
