/**
 * Tests for `stores/demo.ts`: the status is read once, the published customer account is picked
 * by role, and a failed read leaves the demo off instead of throwing.
 *
 * @module tests/stores/demo.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { DemoAccountRole } from '~/types/demoaccountrole'

/** The mocked status call, re-created for every test. */
const fetchDemo = vi.fn()

vi.mock('~/composables/apis/useDemoApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns Only the endpoint the store reaches.
   */
  useDemoApi: () => ({ fetchDemo })
}))

const { useDemoStore } = await import('~/stores/demo')

/** A demo status with both published accounts. */
const STATUS = {
  enabled: true,
  nextResetAt: '2030-01-01T00:00:00Z',
  accounts: [
    { role: DemoAccountRole.Admin, email: 'admin@demo.test', password: 'a' },
    { role: DemoAccountRole.Client, email: 'client@demo.test', password: 'c' }
  ]
}

describe('useDemoStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    fetchDemo.mockReset()
  })

  it('starts as not a demo', () => {
    const store = useDemoStore()

    expect(store.enabled).toBe(false)
    expect(store.clientAccount).toBeUndefined()
  })

  it('stores the status and exposes the client account', async () => {
    fetchDemo.mockResolvedValue(STATUS)
    const store = useDemoStore()

    await store.load()

    expect(store.enabled).toBe(true)
    expect(store.nextResetAt).toBe('2030-01-01T00:00:00Z')
    expect(store.clientAccount?.email).toBe('client@demo.test')
  })

  it('reads the status only once', async () => {
    fetchDemo.mockResolvedValue(STATUS)
    const store = useDemoStore()

    await store.load()
    await store.load()

    expect(fetchDemo).toHaveBeenCalledTimes(1)
  })

  it('stays off and does not throw when the read fails', async () => {
    fetchDemo.mockRejectedValue(new Error('down'))
    const store = useDemoStore()

    await expect(store.load()).resolves.toBeUndefined()

    expect(store.enabled).toBe(false)
    expect(store.accounts).toEqual([])
    expect(store.loaded).toBe(true)
  })
})
