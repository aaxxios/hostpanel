/**
 * Tests for the navigation store: an admitted navigation is pending only while it has screens to
 * download, it ends on settle or turns into a failure on error, and a late hook of an older
 * navigation changes nothing.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import type { RouteLocationNormalized } from 'vue-router'
import { useNavigationStore } from '@/stores/navigation'

/**
 * Builds a navigation target; only its identity matters to the store.
 *
 * @param path - The target path.
 * @returns A route cast to the router type.
 */
const target = (path: string): RouteLocationNormalized => ({ path, matched: [] }) as unknown as RouteLocationNormalized

describe('navigation store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('starts with nothing pending and nothing failed', () => {
    const store = useNavigationStore()

    expect(store.pending).toBeNull()
    expect(store.failed).toBeNull()
  })

  it('holds a navigation that still downloads its screens until it settles', () => {
    const store = useNavigationStore()
    const to = target('/clients')

    store.admit(to, true)
    expect(store.pending).toBe(to)

    store.settle(to)
    expect(store.pending).toBeNull()
  })

  it('does not hold a navigation whose screens are already in memory', () => {
    const store = useNavigationStore()

    store.admit(target('/clients'), false)

    expect(store.pending).toBeNull()
  })

  it('turns a failed download into the failure and clears it on the next admitted navigation', () => {
    const store = useNavigationStore()
    const to = target('/clients')

    store.admit(to, true)
    store.fail(to)
    expect(store.pending).toBeNull()
    expect(store.failed).toBe(to)

    store.admit(target('/dashboard'), false)
    expect(store.failed).toBeNull()
  })

  it('ignores the late hooks of a navigation a newer one replaced', () => {
    const store = useNavigationStore()
    const older = target('/clients')
    const newer = target('/billing')

    store.admit(older, true)
    store.admit(newer, true)
    store.settle(older)
    store.fail(older)

    expect(store.pending).toBe(newer)
    expect(store.failed).toBeNull()
  })
})
