/**
 * Pinia store for the integration list and the per-integration config page.
 *
 * The only caller of `useIntegrationsApi` for these screens. A refusal is kept as the API's own
 * sentence in `error`; a failed connection test is reported as an unsuccessful `testResult`
 * carrying that sentence. Nothing throws.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useIntegrationsApi } from '@/composables/apis/useIntegrationsApi'
import { runAction } from '@/lib/runAction'
import { IntegrationCategory, IntegrationCategoryName } from '@/types/integration'
import type {
  IntegrationConfigPayload,
  IntegrationDetailDto,
  IntegrationDto,
  IntegrationTestResult,
} from '@/types/integration'

/** Maps backend category strings to frontend IntegrationCategory keys. */
const CATEGORY_MAP: Record<string, IntegrationCategory> = {
  [IntegrationCategoryName.PaymentGateways]: IntegrationCategory.Payments,
  [IntegrationCategoryName.DomainRegistrars]: IntegrationCategory.Registrars,
  [IntegrationCategoryName.Provisioning]: IntegrationCategory.Provisioning,
  [IntegrationCategoryName.Email]: IntegrationCategory.Email,
  [IntegrationCategoryName.FraudProtection]: IntegrationCategory.Fraud,
}

/**
 * Normalises a raw API category string to the frontend IntegrationCategory key.
 *
 * @param raw - Category string as returned by the backend.
 * @returns Mapped frontend category key, or the original string cast as IntegrationCategory.
 */
const normaliseCategory = (raw: string): IntegrationCategory => CATEGORY_MAP[raw] ?? (raw as IntegrationCategory)

/** Manages the integrations summary list and the config of the integration being edited. */
export const useIntegrationsStore = defineStore('integrations', () => {
  const api = useIntegrationsApi()

  /** All integrations summary list. */
  const integrations = ref<IntegrationDto[]>([])

  /** Currently loaded integration detail (config page). */
  const current = ref<IntegrationDetailDto | null>(null)

  /** True while any request is in flight. */
  const loading = ref(false)

  /**
   * True while a connection test is in flight. Kept apart from `loading` on purpose: that flag
   * also drives the Save button's "Saving…" label and the initial page skeleton, so reusing it
   * for the test made the Save button claim it was saving and gave the status sidebar nothing
   * to distinguish "testing now" from "not tested yet".
   */
  const testing = ref(false)

  /** Sentence for the last failed load or save, null when none. */
  const error = ref<string | null>(null)

  /** Result of the last connection test. */
  const testResult = ref<IntegrationTestResult | null>(null)

  /**
   * Fetches the summary list of all integrations.
   *
   * @returns Resolves when the request is over; a refusal is left in `error`.
   */
  const fetchAll = async (): Promise<void> => {
    loading.value = true
    const result = await runAction(error, api.listIntegrations)
    loading.value = false
    if (result.ok) {
      integrations.value = result.value.map(i => ({ ...i, category: normaliseCategory(i.category) }))
    }
  }

  /**
   * Fetches full config detail for a single integration by slug.
   *
   * @param slug - Integration slug (e.g. "stripe", "cpanel").
   * @returns Resolves when the request is over; a refusal is left in `error`.
   */
  const fetchOne = async (slug: string): Promise<void> => {
    loading.value = true
    current.value = null
    const result = await runAction(error, () => api.getIntegration(slug))
    loading.value = false
    if (result.ok) {
      current.value = result.value
    }
  }

  /**
   * Saves updated config for an integration, then re-reads it.
   *
   * The PUT answers 204 No Content, so there is no detail in its response to show. Assigning
   * it to `current` blanked the whole page: the form is rendered from `current`, and one
   * successful save replaced it with undefined. Re-reading is also the only way to see what the
   * server actually stored, since secrets come back masked and the enabled state can be refused
   * when required fields are missing.
   *
   * @param slug - Integration slug.
   * @param payload - Updated enabled state and config values.
   * @returns Resolves when the save and the re-read are over; a refusal is left in `error`.
   */
  const saveConfig = async (slug: string, payload: IntegrationConfigPayload): Promise<void> => {
    loading.value = true
    const result = await runAction(error, () => api.saveIntegration(slug, payload))
    loading.value = false
    if (result.ok) {
      await fetchOne(slug)
    }
  }

  /**
   * Runs a live connection test for an integration.
   *
   * @param slug - Integration slug.
   * @returns Resolves when the test is over; the outcome is in `testResult`.
   */
  const testConnection = async (slug: string): Promise<void> => {
    testing.value = true
    testResult.value = null
    /** Local sink for the refusal sentence, which becomes the result's message. */
    const refusal = ref<string | null>(null)
    const startedAt = performance.now()
    const result = await runAction(refusal, () => api.testIntegration(slug))
    const durationMs = Math.round(performance.now() - startedAt)
    testing.value = false
    if (!result.ok) {
      testResult.value = { success: false, message: refusal.value ?? '', durationMs }
      return
    }
    testResult.value = { ...result.value, durationMs }
    // The detail page's "Last tested" reads the persisted value, which the backend updates
    // on a successful probe. Mirror it here so the card does not keep quoting the previous
    // run until the next full reload.
    if (result.value.success && current.value && result.value.testedAt) {
      current.value = { ...current.value, lastTestedAt: result.value.testedAt }
    }
  }

  return {
    integrations,
    current,
    loading,
    testing,
    error,
    testResult,
    fetchAll,
    fetchOne,
    saveConfig,
    testConnection,
  }
})
