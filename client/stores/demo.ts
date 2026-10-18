/**
 * Pinia store for demo mode: whether the deployment is a public demo, when its data resets next
 * and which accounts are published.
 *
 * Loaded once per request by `plugins/demo-mode.ts`, so a server-rendered page already knows the
 * answer and `@pinia/nuxt` carries it to the browser. A failed read means "not a demo": the
 * portal must render either way.
 *
 * @module stores/demo
 */

import { defineStore } from 'pinia'
import { useDemoApi } from '~/composables/apis/useDemoApi'
import { DemoAccountRole } from '~/types/demoaccountrole'
import type { DemoAccount } from '~/types/demoaccount'

/**
 * Central store for demo mode.
 *
 * @returns The demo state and the action that loads it.
 */
export const useDemoStore = defineStore('demo', () => {
  const api = useDemoApi()

  /** Whether the deployment is a demo; false until {@link load} says otherwise. */
  const enabled = ref(false)

  /** ISO timestamp of the next data reset, or null when none is known. */
  const nextResetAt = ref<string | null>(null)

  /** The published demo accounts; empty when the demo is off. */
  const accounts = ref<DemoAccount[]>([])

  /** True once a read has finished, successfully or not, so it is not repeated. */
  const loaded = ref(false)

  /** The published customer account, or undefined when there is none. */
  const clientAccount = computed<DemoAccount | undefined>(() =>
    accounts.value.find(account => account.role === DemoAccountRole.Client)
  )

  /**
   * Reads the demo status once. Never throws: a failure leaves the demo off.
   *
   * @returns Nothing; the state is updated in place.
   */
  const load = async (): Promise<void> => {
    if (loaded.value) {
      return
    }
    try {
      const status = await api.fetchDemo()
      enabled.value = Boolean(status.enabled)
      nextResetAt.value = status.nextResetAt ?? null
      accounts.value = status.accounts ?? []
    } catch {
      enabled.value = false
      nextResetAt.value = null
      accounts.value = []
    }
    loaded.value = true
  }

  return { enabled, nextResetAt, accounts, loaded, clientAccount, load }
})
