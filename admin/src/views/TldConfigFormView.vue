<script setup lang="ts">
/**
 * Full-page form for creating or editing a TLD pricing configuration.
 *
 * Detects edit mode from the route params, loads the configuration when editing, and carries the
 * pricing tables for registration, transfer and renewal across the listed year periods. Asks
 * before leaving with unsaved edits.
 */
import { computed, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import SettingsBreadcrumb from '@/components/settings/SettingsBreadcrumb.vue'
import SettingsFormFooter from '@/components/settings/SettingsFormFooter.vue'
import SettingsLoading from '@/components/settings/SettingsLoading.vue'
import SettingsSection from '@/components/settings/SettingsSection.vue'
import TldPricingTable from '@/components/settings/TldPricingTable.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiField from '@/components/ui/UiField.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiSpinner from '@/components/ui/UiSpinner.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import UiToggle from '@/components/ui/UiToggle.vue'
import { useUnsavedChangesGuard } from '@/composables/useUnsavedChangesGuard'
import { CURRENCY_CODES, currencyLabel, currencySymbol } from '@/lib/money'
import { CurrencyCode } from '@/types/currency'
import { emptyPriceMap, fromApiMap, toNonZeroMap } from '@/lib/tldPricing'
import { useTldConfigsStore } from '@/stores/tldConfigs'
import { RegistrarModule } from '@/types/registrarmodule'
import type { CreateTldConfigPayload, UpdateTldConfigPayload } from '@/types/tldconfig'

const { t } = useI18n()

/** Current route; carries the TLD config id in edit mode. */
const route = useRoute()

/** Router, used to return to the list. */
const router = useRouter()

/** TLD configs store: loads, creates and updates the configuration. */
const store = useTldConfigsStore()

/** Whether we are in edit mode (TLD config ID present in the route). */
const isEditMode = computed<boolean>(() => !!route.params.id)

/** Page title based on create/edit mode. */
const pageTitle = computed<string>(() => (isEditMode.value ? t('settings.tldConfigForm.editTld') : t('settings.tldConfigForm.addTld')))

/** The trail above the form. */
const breadcrumb = computed(() => [
  { label: t('shared.settings'), to: '/settings' },
  { label: t('settings.tldConfigForm.tldPricing'), to: '/settings/tld-pricing' },
  { label: pageTitle.value },
])

/** Registrar options of the provider dropdown. */
const providerOptions = computed(() => [
  { value: RegistrarModule.NameAm as string, label: t('settings.tldConfigs.nameAm') },
  { value: RegistrarModule.Namecheap as string, label: t('settings.tldConfigs.namecheap') },
])

/** Currency options of the cost and sell currency dropdowns. */
const currencyOptions = CURRENCY_CODES.map((code) => ({ value: code as string, label: currencyLabel(code) }))

/** True while loading existing config data in edit mode. */
const loadingConfig = ref(false)

/** True while saving the form. */
const saving = ref(false)

/** Error message from save or load operations. */
const saveError = ref<string | null>(null)

/** Tracks whether the form has been modified since loading. */
const isDirty = ref(false)

/** TLD input value (e.g. "com"). */
const tld = ref('')

/** Selected registrar module. */
const registrarModule = ref<string>(RegistrarModule.NameAm)

/** Cost currency code. */
const currency = ref<string>(CurrencyCode.Amd)

/** Sell currency code. */
const sellCurrency = ref<string>(CurrencyCode.Usd)

/** Whether this TLD is enabled for sale. */
const isEnabled = ref(true)

/** Display sort order. */
const sortOrder = ref(0)

/** Categories as a comma-separated string input. */
const categoriesInput = ref('')

/** Registration cost prices keyed by period. */
const costRegister = ref<Record<string, number>>(emptyPriceMap())

/** Transfer cost prices keyed by period. */
const costTransfer = ref<Record<string, number>>(emptyPriceMap())

/** Renewal cost prices keyed by period. */
const costRenew = ref<Record<string, number>>(emptyPriceMap())

/** Registration sell prices keyed by period. */
const sellRegister = ref<Record<string, number>>(emptyPriceMap())

/** Transfer sell prices keyed by period. */
const sellTransfer = ref<Record<string, number>>(emptyPriceMap())

/** Renewal sell prices keyed by period. */
const sellRenew = ref<Record<string, number>>(emptyPriceMap())

/** Symbol of the cost currency. */
const costSymbol = computed<string>(() => currencySymbol(currency.value))

/** Symbol of the sell currency. */
const sellSymbol = computed<string>(() => currencySymbol(sellCurrency.value))

/** Parks a route change while the reader is asked about unsaved edits. */
const leaveGuard = useUnsavedChangesGuard(() => isDirty.value && !saving.value)

/** Marks the form as dirty on any change. */
watch(
  [tld, registrarModule, currency, sellCurrency, isEnabled, sortOrder, categoriesInput,
    costRegister, costTransfer, costRenew, sellRegister, sellTransfer, sellRenew],
  () => {
    isDirty.value = true
  },
  { deep: true },
)

onMounted(async () => {
  if (!isEditMode.value) {
    return
  }
  loadingConfig.value = true
  const id = Number(route.params.id)
  const config = await store.fetchById(id)
  loadingConfig.value = false
  if (!config) {
    saveError.value = t('settings.tldConfigForm.notFound', { id })
    return
  }

  tld.value = config.tld
  registrarModule.value = config.registrarModule
  currency.value = config.currency
  sellCurrency.value = config.sellCurrency
  isEnabled.value = config.isEnabled
  sortOrder.value = config.sortOrder
  categoriesInput.value = config.categories.join(', ')

  costRegister.value = fromApiMap(config.costRegister)
  costTransfer.value = fromApiMap(config.costTransfer)
  costRenew.value = fromApiMap(config.costRenew)
  sellRegister.value = fromApiMap(config.sellRegister)
  sellTransfer.value = fromApiMap(config.sellTransfer)
  sellRenew.value = fromApiMap(config.sellRenew)

  // Populating the form is not an edit.
  isDirty.value = false
})

/**
 * Keeps the TLD field to what a domain extension can contain: lower-case letters, digits, hyphens.
 *
 * @param value - The text the operator typed.
 */
const onTldInput = (value: string): void => {
  tld.value = value.toLowerCase().replace(/[^a-z0-9-]/g, '')
}

/**
 * Writes one period of a price map.
 *
 * @param map - The price map to change.
 * @param period - The registration period.
 * @param value - The new price.
 */
const setPrice = (map: Record<string, number>, period: string, value: number): void => {
  map[period] = value
}

/** Collects form data and dispatches create or update via the store; returns to the list on success. */
const handleSave = async (): Promise<void> => {
  if (!tld.value.trim()) {
    saveError.value = t('settings.tldConfigForm.tldIsRequired')
    return
  }

  saving.value = true
  saveError.value = null

  const categories = categoriesInput.value
    .split(',')
    .map((c) => c.trim())
    .filter((c) => c.length > 0)

  const payload: CreateTldConfigPayload = {
    tld: tld.value.toLowerCase().trim(),
    registrarModule: registrarModule.value,
    currency: currency.value,
    sellCurrency: sellCurrency.value,
    isEnabled: isEnabled.value,
    sortOrder: sortOrder.value,
    categories,
    costRegister: toNonZeroMap(costRegister.value),
    costTransfer: toNonZeroMap(costTransfer.value),
    costRenew: toNonZeroMap(costRenew.value),
    sellRegister: toNonZeroMap(sellRegister.value),
    sellTransfer: toNonZeroMap(sellTransfer.value),
    sellRenew: toNonZeroMap(sellRenew.value),
  }

  let saved: boolean
  if (isEditMode.value) {
    const id = Number(route.params.id)
    const update: UpdateTldConfigPayload = { ...payload, id }
    saved = await store.update(id, update)
  } else {
    saved = (await store.create(payload)) !== null
  }
  saving.value = false

  if (saved) {
    isDirty.value = false
    void router.push('/settings/tld-pricing')
  } else {
    saveError.value = store.actionError
  }
}

/** Navigates back to the TLD pricing list; the unsaved-changes guard asks first if needed. */
const handleCancel = (): void => {
  void router.push('/settings/tld-pricing')
}
</script>

<template>
  <UiStack gap="none" data-testid="tld-config-form-view">
    <SettingsBreadcrumb :items="breadcrumb" />

    <SettingsLoading v-if="loadingConfig" :message="t('settings.tldConfigForm.loadingTldConfiguration')" />

    <template v-else>
      <UiAlert v-if="saveError" variant="danger" class="mb-5" data-testid="tld-config-form-error">
        {{ saveError }}
      </UiAlert>

      <UiGrid :columns="2" class="mb-24 items-start">
        <!-- Left: general settings -->
        <SettingsSection :title="t('settings.tldConfigForm.generalSettings')">
          <UiField :label="t('settings.tldConfigForm.tld')" :hint="t('settings.tldConfigForm.domainExtensionWithoutTheDot')">
            <template #default="{ id, describedby }">
              <UiStack direction="row" align="center" gap="sm">
                <UiText tone="muted" size="lg">.</UiText>
                <UiInput
                  :id="id"
                  :model-value="tld"
                  type="text"
                  placeholder="com"
                  :disabled="isEditMode"
                  :aria-describedby="describedby"
                  data-testid="tld-config-form-tld"
                  class="flex-1 lowercase"
                  @update:model-value="onTldInput"
                />
              </UiStack>
            </template>
          </UiField>

          <UiField :label="t('settings.tldConfigForm.registrarProvider')">
            <UiSelect
              v-model="registrarModule"
              :options="providerOptions"
              :placeholder="t('settings.tldConfigForm.registrarProvider')"
              data-testid="tld-config-form-provider"
            />
          </UiField>

          <UiField :label="t('settings.tldConfigForm.costCurrency')" :hint="t('settings.tldConfigForm.currencyUsedByTheRegistrar')">
            <UiSelect
              v-model="currency"
              :options="currencyOptions"
              :placeholder="t('settings.tldConfigForm.costCurrency')"
              data-testid="tld-config-form-cost-currency"
            />
          </UiField>

          <UiField :label="t('settings.tldConfigForm.sellCurrency')" :hint="t('settings.tldConfigForm.currencyShownToClientsFor')">
            <UiSelect
              v-model="sellCurrency"
              :options="currencyOptions"
              :placeholder="t('settings.tldConfigForm.sellCurrency')"
              data-testid="tld-config-form-sell-currency"
            />
          </UiField>

          <UiStack direction="row" align="center" justify="between">
            <UiStack gap="xs">
              <UiText tone="primary" size="md">{{ t('shared.enabled') }}</UiText>
              <UiText tone="muted" size="sm">{{ t('settings.tldConfigForm.makeThisTldAvailableFor') }}</UiText>
            </UiStack>
            <UiToggle v-model="isEnabled" data-testid="tld-config-form-enabled" />
          </UiStack>

          <UiField :label="t('settings.tldConfigForm.sortOrder')">
            <UiSpinner
              v-model="sortOrder"
              :min="0"
              :step="1"
              placeholder="0"
              data-testid="tld-config-form-sort-order"
            />
          </UiField>

          <UiField :label="t('shared.categories')" :hint="t('settings.tldConfigForm.commaSeparatedCategoryTagsFor')">
            <template #default="{ id, describedby }">
              <UiInput
                :id="id"
                v-model="categoriesInput"
                type="text"
                :aria-describedby="describedby"
                :placeholder="t('settings.tldConfigForm.popularBusinessTech')"
                data-testid="tld-config-form-categories"
              />
            </template>
          </UiField>
        </SettingsSection>

        <!-- Right: pricing -->
        <UiStack gap="none">
          <TldPricingTable
            :title="t('settings.tldConfigForm.registrationPricing')"
            testid="tld-register"
            :cost-symbol="costSymbol"
            :sell-symbol="sellSymbol"
            :cost="costRegister"
            :sell="sellRegister"
            @update:cost="(period, value) => setPrice(costRegister, period, value)"
            @update:sell="(period, value) => setPrice(sellRegister, period, value)"
          />
          <TldPricingTable
            :title="t('settings.tldConfigForm.transferPricing')"
            testid="tld-transfer"
            :cost-symbol="costSymbol"
            :sell-symbol="sellSymbol"
            :cost="costTransfer"
            :sell="sellTransfer"
            @update:cost="(period, value) => setPrice(costTransfer, period, value)"
            @update:sell="(period, value) => setPrice(sellTransfer, period, value)"
          />
          <TldPricingTable
            :title="t('settings.tldConfigForm.renewalPricing')"
            testid="tld-renew"
            :cost-symbol="costSymbol"
            :sell-symbol="sellSymbol"
            :cost="costRenew"
            :sell="sellRenew"
            @update:cost="(period, value) => setPrice(costRenew, period, value)"
            @update:sell="(period, value) => setPrice(sellRenew, period, value)"
          />
        </UiStack>
      </UiGrid>

      <SettingsFormFooter
        :save-label="isEditMode ? t('shared.saveChanges') : t('settings.tldConfigForm.createTld')"
        :saving="saving"
        @cancel="handleCancel"
        @save="handleSave"
      />
    </template>

    <!-- Unsaved-changes question, asked while the route change waits -->
    <UiDialog
      v-if="leaveGuard.pending.value"
      variant="confirm"
      :title="t('settings.tldConfigForm.unsavedChanges')"
      :message="t('settings.tldConfigForm.youHaveUnsavedChangesAre')"
      :confirm-label="t('settings.tldConfigForm.leave')"
      tone="warning"
      @confirm="leaveGuard.confirmLeave"
      @close="leaveGuard.cancelLeave"
    />
  </UiStack>
</template>
