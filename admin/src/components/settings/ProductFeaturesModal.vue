<script setup lang="ts">
/**
 * Dialog for editing one product's specification — the lines the storefront's plan comparison
 * table is built from.
 *
 * Each line is saved on its own, matching the API: there is no bulk endpoint, and a failure
 * part-way through a list of edits should leave the lines that did save rather than roll the whole
 * dialog back to an unknown state.
 *
 * Rows line up across plans by label, so the label of an existing line is what an operator has to
 * keep consistent between products. The hint below the list says so, because nothing in the UI
 * would otherwise reveal it.
 */
import { onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import SettingsLoading from '@/components/settings/SettingsLoading.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiIconButton from '@/components/ui/UiIconButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { Icon } from '@/lib/icons'
import { useProductFeaturesStore } from '@/stores/productFeatures'
import type { Product } from '@/types/product'
import type { ProductFeature } from '@/types/productfeature'

const { t } = useI18n()

/** Props for ProductFeaturesModal. */
const props = defineProps<{
  /** Product whose specification is being edited. */
  product: Product
}>()

const emit = defineEmits<{
  /** Emitted when the user closes the dialog. */
  close: []
}>()

/** Product features store: the lines, and the refusal sentence of the last call. */
const store = useProductFeaturesStore()

/** The product's lines, as last loaded. */
const features = ref<ProductFeature[]>([])

/** True while the lines are loading. */
const loading = ref(true)

/** Id of the line being saved or removed, null when idle. */
const busyId = ref<number | null>(null)

/** Label of the line about to be added. */
const newLabel = ref('')

/** Value of the line about to be added. */
const newValue = ref('')

/** True while a new line is being added. */
const adding = ref(false)

/** Loads the product's lines, newest state from the server. */
const load = async (): Promise<void> => {
  loading.value = true
  const lines = await store.fetchFeatures(props.product.id)
  if (lines) {
    features.value = lines
  }
  loading.value = false
}

onMounted(load)

/** Adds a line at the end of the list. */
const add = async (): Promise<void> => {
  if (!newLabel.value.trim() || !newValue.value.trim()) {
    return
  }
  adding.value = true
  const added = await store.addFeature(props.product.id, newLabel.value.trim(), newValue.value.trim(), features.value.length)
  if (added) {
    newLabel.value = ''
    newValue.value = ''
    await load()
  }
  adding.value = false
}

/**
 * Saves an edited line.
 *
 * @param feature - The line to save, with the operator's current text.
 */
const save = async (feature: ProductFeature): Promise<void> => {
  if (!feature.label.trim() || !feature.value.trim()) {
    return
  }
  busyId.value = feature.id
  if (!(await store.saveFeature(feature))) {
    await load()
  }
  busyId.value = null
}

/**
 * Removes a line.
 *
 * @param feature - The line to remove.
 */
const remove = async (feature: ProductFeature): Promise<void> => {
  busyId.value = feature.id
  if (await store.removeFeature(feature.id)) {
    await load()
  }
  busyId.value = null
}
</script>

<template>
  <UiDialog :title="t('settings.productFeatures.specification')" size="lg" @close="emit('close')">
    <UiText tone="secondary" size="md">
      {{ t('settings.productFeatures.shownInTheStorefrontS') }} {{ product.name }}.
    </UiText>

    <UiAlert v-if="store.error" variant="danger" data-testid="product-features-error">
      {{ store.error }}
    </UiAlert>

    <SettingsLoading v-if="loading" :message="t('shared.loading')" />

    <template v-else>
      <UiEmptyState
        v-if="features.length === 0"
        :title="t('settings.productFeatures.nothingSpecifiedYetTheComparison')"
        data-testid="product-features-empty"
      />

      <UiStack v-else gap="sm" data-testid="product-features-list">
        <UiStack v-for="feature in features" :key="feature.id" direction="row" align="center" gap="sm">
          <UiInput
            v-model="feature.label"
            type="text"
            size="sm"
            :placeholder="t('settings.productFeatures.disk')"
            :data-testid="`product-feature-label-${feature.id}`"
            class="w-1/3"
            @keyup.enter="save(feature)"
          />
          <UiInput
            v-model="feature.value"
            type="text"
            size="sm"
            :placeholder="t('settings.productFeatures.10Gb')"
            :data-testid="`product-feature-value-${feature.id}`"
            class="flex-1"
            @keyup.enter="save(feature)"
          />
          <UiButton
            type="button"
            variant="secondary"
            size="sm"
            :disabled="busyId === feature.id"
            :data-testid="`product-feature-save-${feature.id}`"
            @click="save(feature)"
          >
            {{ t('shared.save') }}
          </UiButton>
          <UiIconButton
            :label="t('settings.productFeatures.removeLine')"
            size="sm"
            :disabled="busyId === feature.id"
            :data-testid="`product-feature-remove-${feature.id}`"
            @click="remove(feature)"
          >
            <UiIcon :markup="Icon.Trash" size="sm" />
          </UiIconButton>
        </UiStack>
      </UiStack>

      <UiStack direction="row" align="center" gap="sm" class="border-t border-border pt-4">
        <UiInput
          v-model="newLabel"
          type="text"
          size="sm"
          :placeholder="t('settings.productFeatures.feature')"
          data-testid="product-feature-new-label"
          class="w-1/3"
          @keyup.enter="add"
        />
        <UiInput
          v-model="newValue"
          type="text"
          size="sm"
          :placeholder="t('shared.value')"
          data-testid="product-feature-new-value"
          class="flex-1"
          @keyup.enter="add"
        />
        <UiButton
          type="button"
          variant="primary"
          size="sm"
          :disabled="adding || !newLabel.trim() || !newValue.trim()"
          data-testid="product-feature-add"
          @click="add"
        >
          {{ adding ? t('settings.productFeatures.adding') : t('settings.productFeatures.add') }}
        </UiButton>
      </UiStack>

      <UiText tone="muted" size="sm">
        {{ t('settings.productFeatures.plansLineUpByFeature') }}
      </UiText>
    </template>
  </UiDialog>
</template>
