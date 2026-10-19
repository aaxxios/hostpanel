<script setup lang="ts">
/**
 * Editable service detail page -- displays and allows editing all service fields.
 * Supports lifecycle actions: Suspend, Unsuspend, Terminate.
 */
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useFallbackLabels } from '@/composables/useFallbackLabels'
import { useServicesStore } from '@/stores/services'
import { useServersStore } from '@/stores/servers'
import { useSettingsStore } from '@/stores/settings'
import { SERVICE_STATUS_OPTIONS, BILLING_CYCLE_OPTIONS } from '@/lib/constants'
import { BillingCycle, ServiceChangeReason, ServiceStatus } from '@/types/service'
import { toDateInputValue } from '@/lib/format'
import type { ServiceListItem } from '@/types/service'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiDatePicker from '@/components/ui/UiDatePicker.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiField from '@/components/ui/UiField.vue'
import UiForm from '@/components/ui/UiForm.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiLabel from '@/components/ui/UiLabel.vue'
import UiLink from '@/components/ui/UiLink.vue'
import UiNumberInput from '@/components/ui/UiNumberInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import UiToggle from '@/components/ui/UiToggle.vue'
import { UiButtonVariant } from '@/utils/ui/button'
import { UiAlignItems, UiJustify } from '@/utils/ui/layout'
import { UiSize } from '@/utils/ui/size'
import { UiLabelVariant, UiTextTone } from '@/utils/ui/text'
import { NOTICE_VISIBLE_MS } from '@/lib/limits'
import { ClientSection, clientItemPath, clientSectionPath } from '@/lib/routes'

const { t } = useI18n()

/** Fallback text for fields the API leaves unresolved. */
const { productLabel } = useFallbackLabels()

/** Icon markup (shape tags) of the back link. */
const BACK_ICON = '<polyline points="15 18 9 12 15 6"/>'

/** Icon markup (shape tags) of the control panel button. */
const PANEL_ICON = '<path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6" /><polyline points="15 3 21 3 21 9" /><line x1="10" y1="14" x2="21" y2="3" />'

/** Default billing cycle. */
const DEFAULT_CYCLE: string = BillingCycle.Monthly

/** Current route, source of the client and service ids. */
const route = useRoute()

/** Router, used by the service switcher. */
const router = useRouter()

/** Services store, the only caller of the service endpoints. */
const store = useServicesStore()

/** Servers store, source of the server dropdown. */
const serversStore = useServersStore()

/** Settings store, source of the product dropdown. */
const settingsStore = useSettingsStore()

/** Client ID from route params. */
const clientId = computed(() => route.params.id as string)

/** Service ID from route params. */
const serviceId = computed(() => route.params.serviceId as string)

/** Fetched service detail, null until loaded (owned by the store). */
const service = computed(() => store.current)

/** True while the initial load is in flight (owned by the store). */
const loading = computed(() => store.loading)

/** Error message from the initial load (owned by the store). */
const error = computed(() => store.error)

/** True while save is in flight. */
const saving = ref(false)

/** Success message after save. */
const saveSuccess = ref(false)

/** Timer that hides the success message. */
let savedTimer: ReturnType<typeof setTimeout> | undefined

/** Error message after save or a provisioning command (owned by the store). */
const saveError = computed(() => store.actionError)

/** True while a module command (suspend/unsuspend/terminate) is in flight. */
const commandLoading = ref(false)

/** Service domain. */
const domain = ref('')

/** Dedicated IP address. */
const dedicatedIp = ref('')

/** Control panel username. */
const username = ref('')

/** Control panel password. */
const password = ref('')

/** Recurring charge amount. */
const recurringAmount = ref(0)

/** Next due date (ISO date string). */
const nextDueDate = ref('')

/** Billing cycle. */
const billingCycle = ref(DEFAULT_CYCLE)

/** Payment method. */
const paymentMethod = ref('')

/** Override auto-suspend toggle. */
const overrideAutoSuspend = ref(false)

/** Suspend-until date (ISO date string). */
const suspendUntil = ref('')

/** Auto-terminate at end of cycle toggle. */
const autoTerminateEndOfCycle = ref(false)

/** Auto-terminate reason. */
const autoTerminateReason = ref('')

/** Internal admin notes. */
const adminNotes = ref('')

/** Provisioning reference / server. */
const provisioningRef = ref('')

/** First payment amount. */
const firstPaymentAmount = ref(0)

/** Promotion code. */
const promotionCode = ref('')

/** Termination date (ISO date string). */
const terminatedAt = ref('')

/** Registration date (ISO date string). */
const createdAt = ref('')

/** Status (editable dropdown). */
const status = ref<string>(ServiceStatus.Active)

/** Subscription ID. */
const subscriptionId = ref('')

/** Quantity of units purchased. */
const quantity = ref(1)

/** FK to the assigned server. */
const serverId = ref<number | null>(null)

/** FK to the ordered product (for changing product). */
const productIdRef = ref<number | null>(null)

/** All services for the current client (for service switcher). */
const clientServices = ref<ServiceListItem[]>([])

/** Available servers for dropdown. */
const serverList = computed(() => serversStore.servers)

/** Available products for dropdown. */
const productList = computed(() => settingsStore.products)

/** Whether the change password modal is visible. */
const showChangePasswordModal = ref(false)

/** New password for change password modal. */
const newPassword = ref('')

/** Whether the change package modal is visible. */
const showChangePackageModal = ref(false)

/** New package name for change package modal. */
const newPackage = ref('')

/** Whether the suspend confirmation modal is visible. */
const showSuspendModal = ref(false)

/** Whether the terminate confirmation modal is visible. */
const showTerminateModal = ref(false)

/** Whether SSO URL is loading. */
const ssoLoading = ref(false)

/** Status options for dropdown. */
const statusOptions = SERVICE_STATUS_OPTIONS

/** Billing cycle options for the UiSelect dropdown. */
const billingCycleOptions = BILLING_CYCLE_OPTIONS

/** Server options for UiSelect dropdown. */
const serverOptions = computed(() => [
  { value: 0, label: t('shared.none') },
  ...serverList.value.map(s => ({
    value: s.id,
    label: `${s.name} — ${s.hostname} (${s.accountsCount ?? 0}/${s.maxAccounts ?? '∞'})`,
  })),
])

/** Product options for UiSelect dropdown. */
const productOptions = computed(() =>
  productList.value.map(p => ({ value: p.id, label: p.name })),
)

/** Service switcher options for UiSelect dropdown. */
const serviceSwitcherOptions = computed(() =>
  clientServices.value.map(s => ({
    value: s.id,
    label: `#${s.id} — ${productLabel(s.productName)}`,
  })),
)

/**
 * Populates all form refs from the fetched service detail.
 *
 * @returns Nothing.
 */
const populateForm = (): void => {
  const s = service.value
  if (!s) {
    return
  }

  domain.value = s.domain ?? ''
  dedicatedIp.value = s.dedicatedIp ?? ''
  username.value = s.username ?? ''
  password.value = s.password ?? ''
  recurringAmount.value = s.recurringAmount
  firstPaymentAmount.value = s.firstPaymentAmount
  nextDueDate.value = toDateInputValue(s.nextDueDate)
  billingCycle.value = s.billingCycle || DEFAULT_CYCLE
  paymentMethod.value = s.paymentMethod ?? ''
  provisioningRef.value = s.provisioningRef ?? ''
  promotionCode.value = s.promotionCode ?? ''
  subscriptionId.value = s.subscriptionId ?? ''
  terminatedAt.value = toDateInputValue(s.terminatedAt)
  createdAt.value = toDateInputValue(s.createdAt)
  status.value = s.status || ServiceStatus.Active
  overrideAutoSuspend.value = s.overrideAutoSuspend
  suspendUntil.value = toDateInputValue(s.suspendUntil)
  autoTerminateEndOfCycle.value = s.autoTerminateEndOfCycle
  autoTerminateReason.value = s.autoTerminateReason ?? ''
  adminNotes.value = s.adminNotes ?? ''
  quantity.value = s.quantity
  serverId.value = s.serverId ?? null
  productIdRef.value = s.productId
}

/**
 * Fetches the service detail through the store and populates the form.
 *
 * @returns Promise that resolves when the service is loaded.
 */
const fetchService = async (): Promise<void> => {
  if (await store.fetchById(serviceId.value)) {
    populateForm()
  }
}

/**
 * Fetches all services for the current client (service switcher).
 *
 * @returns Promise that resolves when services are loaded.
 */
const fetchClientServices = async (): Promise<void> => {
  clientServices.value = await store.fetchClientServices(clientId.value)
}

/**
 * Resets form to the last saved state.
 *
 * @returns Nothing.
 */
const handleCancel = (): void => {
  populateForm()
  store.actionError = null
  saveSuccess.value = false
}

/**
 * Submits all editable field changes to the API.
 *
 * @returns Promise that resolves when the save completes.
 */
const handleSave = async (): Promise<void> => {
  saveSuccess.value = false
  saving.value = true

  const saved = await store.update(serviceId.value, {
    domain: domain.value.trim() || null,
    dedicatedIp: dedicatedIp.value.trim() || null,
    username: username.value.trim() || null,
    password: password.value.trim() || null,
    provisioningRef: provisioningRef.value.trim() || null,
    recurringAmount: recurringAmount.value,
    firstPaymentAmount: firstPaymentAmount.value,
    nextRenewalAt: nextDueDate.value || null,
    createdAt: createdAt.value || null,
    terminatedAt: terminatedAt.value || null,
    billingCycle: billingCycle.value,
    paymentMethod: paymentMethod.value.trim() || null,
    promotionCode: promotionCode.value.trim() || null,
    subscriptionId: subscriptionId.value.trim() || null,
    overrideAutoSuspend: overrideAutoSuspend.value,
    suspendUntil: overrideAutoSuspend.value && suspendUntil.value ? suspendUntil.value : null,
    autoTerminateEndOfCycle: autoTerminateEndOfCycle.value,
    autoTerminateReason: autoTerminateEndOfCycle.value ? autoTerminateReason.value.trim() || null : null,
    adminNotes: adminNotes.value.trim() || null,
    status: status.value,
    quantity: quantity.value,
    serverId: serverId.value || null,
    productId: productIdRef.value,
  })
  if (saved) {
    saveSuccess.value = true
    clearTimeout(savedTimer)
    savedTimer = setTimeout(() => {
      saveSuccess.value = false
    }, NOTICE_VISIBLE_MS)
    await fetchService()
  }
  saving.value = false
}

/**
 * Opens a cPanel SSO URL in a new browser tab.
 *
 * @returns Promise that resolves when the SSO URL is retrieved.
 */
const handleLoginToPanel = async (): Promise<void> => {
  ssoLoading.value = true
  const url = await store.fetchPanelLoginUrl(serviceId.value)
  ssoLoading.value = false
  if (url !== null) {
    window.open(url, '_blank')
  }
}

/**
 * Runs a provisioning command with the shared busy flag and reloads the service when it succeeds.
 *
 * @param command - The store call to run.
 * @returns Promise that resolves with whether the command succeeded.
 */
const runCommand = async (command: () => Promise<boolean>): Promise<boolean> => {
  commandLoading.value = true
  const ok = await command()
  commandLoading.value = false
  if (ok) {
    await fetchService()
  }
  return ok
}

/**
 * Provisions a pending service on the hosting server.
 *
 * @returns Promise that resolves when provisioning completes.
 */
const handleProvision = async (): Promise<void> => {
  await runCommand(() => store.provision(serviceId.value))
}

/**
 * Changes the hosting account password via the provisioning server.
 *
 * @returns Promise that resolves when the password is changed.
 */
const handleChangePassword = async (): Promise<void> => {
  if (!newPassword.value.trim()) {
    return
  }
  if (await runCommand(() => store.changePassword(serviceId.value, newPassword.value))) {
    showChangePasswordModal.value = false
    newPassword.value = ''
  }
}

/**
 * Changes the hosting package via the provisioning server.
 *
 * @returns Promise that resolves when the package is changed.
 */
const handleChangePackage = async (): Promise<void> => {
  if (!newPackage.value.trim()) {
    return
  }
  if (await runCommand(() => store.changePackage(serviceId.value, newPackage.value))) {
    showChangePackageModal.value = false
    newPackage.value = ''
  }
}

/**
 * Suspends the service via the API and re-fetches the detail.
 *
 * @returns Promise that resolves when the action completes.
 */
const handleSuspend = async (): Promise<void> => {
  showSuspendModal.value = false
  await runCommand(() => store.suspend(serviceId.value, ServiceChangeReason.Suspended))
}

/**
 * Unsuspends the service via the API and re-fetches the detail.
 *
 * @returns Promise that resolves when the action completes.
 */
const handleUnsuspend = async (): Promise<void> => {
  await runCommand(() => store.unsuspend(serviceId.value))
}

/**
 * Terminates the service via the API and re-fetches the detail.
 *
 * @returns Promise that resolves when the action completes.
 */
const handleTerminate = async (): Promise<void> => {
  showTerminateModal.value = false
  await runCommand(() => store.terminate(serviceId.value, ServiceChangeReason.Terminated))
}

/**
 * Opens another service of the same client from the switcher.
 *
 * @param val - The service id.
 * @returns Nothing.
 */
const switchService = (val: number): void => {
  router.push(clientItemPath(clientId.value, ClientSection.Services, val))
}

/**
 * Stores the server chosen in the dropdown; zero means "none".
 *
 * @param val - The chosen server id.
 * @returns Nothing.
 */
const setServer = (val: number): void => {
  serverId.value = val === 0 ? null : val
}

// Populate form when service data changes
watch(service, () => populateForm())

// Re-fetch service when navigating via the service switcher
watch(serviceId, () => {
  fetchService()
})

onMounted(() => {
  fetchService()
  fetchClientServices()
  serversStore.fetchServers()
  settingsStore.fetchProducts()
})

onBeforeUnmount(() => {
  clearTimeout(savedTimer)
})
</script>

<template>
  <UiStack>
    <UiStack v-if="loading && !service" data-testid="client-service-loading">
      <UiSkeleton :lines="3" />
      <UiText :tone="UiTextTone.Secondary">{{ t('clients.clientServiceDetail.loadingService') }}</UiText>
    </UiStack>

    <UiAlert v-else-if="error && !service" variant="error" data-testid="client-service-error">
      {{ error }}
    </UiAlert>

    <template v-else-if="service">
      <UiForm stacked @submit="handleSave">
        <UiStack direction="row" wrap :justify="UiJustify.Between" :align="UiAlignItems.Center">
          <UiStack direction="row" :align="UiAlignItems.Center">
            <UiLink
              :to="clientSectionPath(clientId, ClientSection.Services)"
              :variant="UiButtonVariant.Secondary"
              data-testid="client-service-back"
            >
              <UiIcon :markup="BACK_ICON" :size="UiSize.Sm" />
              {{ t('shared.back') }}
            </UiLink>
            <UiSelect
              data-testid="client-service-switcher"
              v-if="clientServices.length > 1"
              :model-value="Number(serviceId)"
              :options="serviceSwitcherOptions"
              :placeholder="t('shared.productService')"
              searchable
              @update:model-value="switchService"
            />
            <UiHeading :level="1" variant="card">
              {{ t('clients.clientServiceDetail.serviceIdProductname', { id: service.id, productName: productLabel(service.productName) }) }}
            </UiHeading>
          </UiStack>

          <UiStack direction="row" :align="UiAlignItems.Center" gap="sm">
            <UiAlert v-if="saveSuccess" variant="success" data-testid="client-service-saved">
              {{ t('shared.changesSavedSuccessfully') }}
            </UiAlert>
            <UiAlert v-if="saveError" variant="error" data-testid="client-service-action-error">
              {{ saveError }}
            </UiAlert>
            <UiButton
              type="button"
              :variant="UiButtonVariant.Secondary"
              :disabled="ssoLoading || !service.provisioningRef"
              data-testid="client-service-panel"
              @click="handleLoginToPanel"
            >
              <UiIcon :markup="PANEL_ICON" :size="UiSize.Sm" />
              {{ ssoLoading ? t('shared.loading') : t('clients.clientServiceDetail.loginToControlPanel') }}
            </UiButton>
            <UiButton
              type="button"
              :variant="UiButtonVariant.Secondary"
              data-testid="client-service-cancel"
              @click="handleCancel"
            >
              {{ t('shared.cancel') }}
            </UiButton>
            <UiButton
              type="submit"
              :variant="UiButtonVariant.Primary"
              :disabled="saving"
              data-testid="client-service-save"
            >
              {{ saving ? t('shared.saving') : t('shared.saveChanges') }}
            </UiButton>
          </UiStack>
        </UiStack>

        <UiGrid :columns="2">
          <UiStack>
            <UiCard padding="md">
              <UiStack>
                <UiHeading :level="2" variant="card">{{ t('clients.clientServiceDetail.serviceInfo') }}</UiHeading>
                <UiGrid :columns="2" gap="sm">
                  <UiField :label="t('clients.clientServiceDetail.order')">
                    <UiText :tone="UiTextTone.Secondary">{{ service.id }}</UiText>
                  </UiField>
                  <UiField :label="t('shared.productService')">
                    <UiSelect
                      data-testid="client-service-product"
                      v-model="productIdRef"
                      :options="productOptions"
                      :placeholder="t('shared.productService')"
                      searchable
                    />
                  </UiField>
                  <UiField :label="t('clients.clientServiceDetail.server')">
                    <UiSelect
                      data-testid="client-service-server"
                      :model-value="serverId ?? 0"
                      :options="serverOptions"
                      :placeholder="t('clients.clientServiceDetail.server')"
                      searchable
                      @update:model-value="setServer"
                    />
                  </UiField>
                  <UiField :label="t('shared.domain')">
                    <template #default="{ id }">
                      <UiInput
                        :id="id"
                        v-model="domain"
                        type="text"
                        :placeholder="t('shared.examples.domain')"
                        data-testid="client-service-domain"
                      />
                    </template>
                  </UiField>
                  <UiField :label="t('clients.clientServiceDetail.provisioningRef')">
                    <template #default="{ id }">
                      <UiInput
                        :id="id"
                        v-model="provisioningRef"
                        type="text"
                        :placeholder="t('shared.examples.hostingAccount')"
                        data-testid="client-service-provisioning-ref"
                      />
                    </template>
                  </UiField>
                  <UiField :label="t('clients.clientServiceDetail.dedicatedIp')">
                    <template #default="{ id }">
                      <UiInput
                        :id="id"
                        v-model="dedicatedIp"
                        type="text"
                        :placeholder="t('shared.examples.ipAddress')"
                        data-testid="client-service-dedicated-ip"
                      />
                    </template>
                  </UiField>
                  <UiField :label="t('shared.username')">
                    <template #default="{ id }">
                      <UiInput
                        :id="id"
                        v-model="username"
                        type="text"
                        :placeholder="t('shared.examples.hostingUser')"
                        data-testid="client-service-username"
                      />
                    </template>
                  </UiField>
                  <UiField :label="t('shared.password')">
                    <template #default="{ id }">
                      <UiInput
                        :id="id"
                        v-model="password"
                        type="password"
                        placeholder="********"
                        data-testid="client-service-password"
                      />
                    </template>
                  </UiField>
                </UiGrid>
                <UiField :label="t('shared.status')">
                  <UiSelect data-testid="client-service-status" v-model="status" :options="statusOptions" :placeholder="t('shared.status')" />
                </UiField>
              </UiStack>
            </UiCard>

            <UiCard padding="md">
              <UiStack>
                <UiHeading :level="2" variant="card">{{ t('clients.clientServiceDetail.moduleCommands') }}</UiHeading>

                <UiText v-if="service.status === ServiceStatus.Terminated" :tone="UiTextTone.Muted">
                  {{ t('clients.clientServiceDetail.serviceTerminated') }}
                </UiText>

                <UiStack v-else direction="row" wrap gap="sm">
                  <template v-if="service.status === ServiceStatus.Pending">
                    <UiButton
                      type="button"
                      :variant="UiButtonVariant.Secondary"
                      :disabled="commandLoading"
                      data-testid="client-service-create"
                      @click="handleProvision"
                    >
                      {{ commandLoading ? t('shared.processing') : t('clients.clientServiceDetail.create') }}
                    </UiButton>
                    <UiButton
                      type="button"
                      :variant="UiButtonVariant.Danger"
                      :disabled="commandLoading"
                      data-testid="client-service-terminate"
                      @click="showTerminateModal = true"
                    >
                      {{ commandLoading ? t('shared.processing') : t('clients.clientServiceDetail.terminate') }}
                    </UiButton>
                  </template>

                  <template v-else-if="service.status === ServiceStatus.Active">
                    <UiButton
                      type="button"
                      :variant="UiButtonVariant.Secondary"
                      :disabled="commandLoading"
                      data-testid="client-service-suspend"
                      @click="showSuspendModal = true"
                    >
                      {{ commandLoading ? t('shared.processing') : t('clients.clientServiceDetail.suspend') }}
                    </UiButton>
                    <UiButton
                      type="button"
                      :variant="UiButtonVariant.Danger"
                      :disabled="commandLoading"
                      data-testid="client-service-terminate"
                      @click="showTerminateModal = true"
                    >
                      {{ commandLoading ? t('shared.processing') : t('clients.clientServiceDetail.terminate') }}
                    </UiButton>
                    <UiButton
                      type="button"
                      :variant="UiButtonVariant.Secondary"
                      :disabled="commandLoading || !service.provisioningRef"
                      data-testid="client-service-change-package"
                      @click="showChangePackageModal = true"
                    >
                      {{ commandLoading ? t('shared.processing') : t('clients.clientServiceDetail.changePackage') }}
                    </UiButton>
                    <UiButton
                      type="button"
                      :variant="UiButtonVariant.Secondary"
                      :disabled="commandLoading || !service.provisioningRef"
                      data-testid="client-service-change-password"
                      @click="showChangePasswordModal = true"
                    >
                      {{ commandLoading ? t('shared.processing') : t('shared.changePassword') }}
                    </UiButton>
                  </template>

                  <template v-else-if="service.status === ServiceStatus.Suspended">
                    <UiButton
                      type="button"
                      :variant="UiButtonVariant.Secondary"
                      :disabled="commandLoading"
                      data-testid="client-service-unsuspend"
                      @click="handleUnsuspend"
                    >
                      {{ commandLoading ? t('shared.processing') : t('clients.clientServiceDetail.unsuspend') }}
                    </UiButton>
                    <UiButton
                      type="button"
                      :variant="UiButtonVariant.Danger"
                      :disabled="commandLoading"
                      data-testid="client-service-terminate"
                      @click="showTerminateModal = true"
                    >
                      {{ commandLoading ? t('shared.processing') : t('clients.clientServiceDetail.terminate') }}
                    </UiButton>
                  </template>
                </UiStack>
              </UiStack>
            </UiCard>

            <UiCard padding="md">
              <UiStack>
                <UiHeading :level="2" variant="card">{{ t('clients.clientServiceDetail.addons') }}</UiHeading>
                <UiText :tone="UiTextTone.Muted">{{ t('clients.clientServiceDetail.noAddonsFound') }}</UiText>
              </UiStack>
            </UiCard>
          </UiStack>

          <UiStack>
            <UiCard padding="md">
              <UiStack>
                <UiHeading :level="2" variant="card">{{ t('clients.clientServiceDetail.billing') }}</UiHeading>
                <UiField :label="t('clients.clientServiceDetail.quantity')">
                  <UiNumberInput data-testid="client-service-quantity" v-model="quantity" :min="1" placeholder="1" />
                </UiField>
                <UiGrid :columns="2" gap="sm">
                  <UiField :label="t('shared.registrationDate')">
                    <UiDatePicker v-model="createdAt" />
                  </UiField>
                  <UiField :label="t('clients.clientServiceDetail.firstPaymentAmount')">
                    <UiNumberInput data-testid="client-service-first-payment" v-model="firstPaymentAmount" :step="0.01" :min="0" placeholder="0.00" />
                  </UiField>
                  <UiField :label="t('clients.clientServiceDetail.recurringAmount')">
                    <UiNumberInput data-testid="client-service-recurring" v-model="recurringAmount" :step="0.01" :min="0" placeholder="0.00" />
                  </UiField>
                  <UiField :label="t('shared.nextDueDate')">
                    <UiDatePicker v-model="nextDueDate" />
                  </UiField>
                  <UiField :label="t('clients.clientServiceDetail.terminationDate')">
                    <UiDatePicker v-model="terminatedAt" />
                  </UiField>
                  <UiField :label="t('shared.billingCycle')">
                    <UiSelect data-testid="client-service-billing-cycle" v-model="billingCycle" :options="billingCycleOptions" :placeholder="t('shared.billingCycle')" />
                  </UiField>
                  <UiField :label="t('shared.paymentMethod')">
                    <template #default="{ id }">
                      <UiInput
                        :id="id"
                        v-model="paymentMethod"
                        type="text"
                        :placeholder="t('clients.clientServiceDetail.stripePaypal')"
                        data-testid="client-service-payment-method"
                      />
                    </template>
                  </UiField>
                  <UiField :label="t('clients.clientServiceDetail.promotionCode')">
                    <template #default="{ id }">
                      <UiInput
                        :id="id"
                        v-model="promotionCode"
                        type="text"
                        :placeholder="t('shared.examples.promoCode')"
                        data-testid="client-service-promotion"
                      />
                    </template>
                  </UiField>
                </UiGrid>
                <UiField :label="t('clients.clientServiceDetail.subscriptionId')">
                  <template #default="{ id }">
                    <UiInput
                      :id="id"
                      v-model="subscriptionId"
                      type="text"
                      :placeholder="t('shared.examples.subscriptionId')"
                      data-testid="client-service-subscription"
                    />
                  </template>
                </UiField>
              </UiStack>
            </UiCard>

            <UiCard padding="md">
              <UiStack>
                <UiHeading :level="2" variant="card">{{ t('clients.clientServiceDetail.autoManagement') }}</UiHeading>
                <UiLabel :variant="UiLabelVariant.Inline">
                  <UiToggle v-model="overrideAutoSuspend" data-testid="client-service-override-suspend" />
                  {{ t('clients.clientServiceDetail.overrideAutoSuspend') }}
                </UiLabel>
                <UiField v-if="overrideAutoSuspend" :label="t('clients.clientServiceDetail.suspendUntil')">
                  <UiDatePicker v-model="suspendUntil" />
                </UiField>
                <UiLabel :variant="UiLabelVariant.Inline">
                  <UiToggle v-model="autoTerminateEndOfCycle" data-testid="client-service-auto-terminate" />
                  {{ t('clients.clientServiceDetail.autoTerminateEndOfCycle') }}
                </UiLabel>
                <UiField v-if="autoTerminateEndOfCycle" :label="t('clients.clientServiceDetail.terminationReason')">
                  <template #default="{ id }">
                    <UiInput
                      :id="id"
                      v-model="autoTerminateReason"
                      type="text"
                      :placeholder="t('clients.clientServiceDetail.reasonForAutoTermination')"
                      data-testid="client-service-terminate-reason"
                    />
                  </template>
                </UiField>
              </UiStack>
            </UiCard>
          </UiStack>
        </UiGrid>

        <UiCard padding="md">
          <UiStack>
            <UiHeading :level="2" variant="card">{{ t('shared.adminNotes') }}</UiHeading>
            <UiTextarea
              v-model="adminNotes"
              rows="6"
              :placeholder="t('shared.internalNotesVisibleOnlyTo')"
              data-testid="client-service-admin-notes"
            />
          </UiStack>
        </UiCard>
      </UiForm>

      <UiDialog
        v-if="showChangePasswordModal"
        :title="t('shared.changePassword')"
        @close="showChangePasswordModal = false"
      >
        <UiText :tone="UiTextTone.Secondary">{{ t('clients.clientServiceDetail.enterANewPasswordFor') }}</UiText>
        <UiField :label="t('clients.clientServiceDetail.newPassword')">
          <template #default="{ id }">
            <UiInput
              :id="id"
              v-model="newPassword"
              type="password"
              :placeholder="t('clients.clientServiceDetail.enterNewPassword')"
              data-testid="client-service-new-password"
            />
          </template>
        </UiField>
        <template #footer>
          <UiButton
            :variant="UiButtonVariant.Secondary"
            data-testid="client-service-password-cancel"
            @click="showChangePasswordModal = false"
          >
            {{ t('shared.cancel') }}
          </UiButton>
          <UiButton
            :variant="UiButtonVariant.Primary"
            :disabled="commandLoading || !newPassword.trim()"
            data-testid="client-service-password-confirm"
            @click="handleChangePassword"
          >
            {{ commandLoading ? t('shared.processing') : t('clients.clientServiceDetail.confirm') }}
          </UiButton>
        </template>
      </UiDialog>

      <UiDialog
        v-if="showChangePackageModal"
        :title="t('clients.clientServiceDetail.changePackage')"
        @close="showChangePackageModal = false"
      >
        <UiText :tone="UiTextTone.Secondary">{{ t('clients.clientServiceDetail.enterTheNewHostingPackage') }}</UiText>
        <UiField :label="t('clients.clientServiceDetail.packageName')">
          <template #default="{ id }">
            <UiInput
              :id="id"
              v-model="newPackage"
              type="text"
              :placeholder="t('clients.clientServiceDetail.eGStarterPlan')"
              data-testid="client-service-new-package"
            />
          </template>
        </UiField>
        <template #footer>
          <UiButton
            :variant="UiButtonVariant.Secondary"
            data-testid="client-service-package-cancel"
            @click="showChangePackageModal = false"
          >
            {{ t('shared.cancel') }}
          </UiButton>
          <UiButton
            :variant="UiButtonVariant.Primary"
            :disabled="commandLoading || !newPackage.trim()"
            data-testid="client-service-package-confirm"
            @click="handleChangePackage"
          >
            {{ commandLoading ? t('shared.processing') : t('clients.clientServiceDetail.confirm') }}
          </UiButton>
        </template>
      </UiDialog>

      <UiDialog
        v-if="showSuspendModal"
        variant="confirm"
        :title="t('clients.clientServiceDetail.suspendService')"
        :message="t('clients.clientServiceDetail.areYouSureYouWant')"
        :confirm-label="t('clients.clientServiceDetail.suspend')"
        :loading-label="t('shared.processing')"
        :loading="commandLoading"
        tone="warning"
        @confirm="handleSuspend"
        @close="showSuspendModal = false"
      />

      <UiDialog
        v-if="showTerminateModal"
        variant="confirm"
        :title="t('clients.clientServiceDetail.terminateService')"
        :message="t('clients.clientServiceDetail.thisActionCannotBeUndone')"
        :confirm-label="t('clients.clientServiceDetail.terminate')"
        :loading-label="t('shared.processing')"
        :loading="commandLoading"
        tone="danger"
        @confirm="handleTerminate"
        @close="showTerminateModal = false"
      />
    </template>
  </UiStack>
</template>
