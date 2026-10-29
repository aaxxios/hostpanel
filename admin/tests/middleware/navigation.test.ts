/**
 * Tests for the navigation hooks, on a real memory router: a navigation that has to download its
 * screen is pending until it arrives, one whose screen is in memory never is, and a download that
 * fails for good becomes the failure.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { createMemoryHistory, createRouter } from 'vue-router'
import type { RouteLocationNormalized, Router } from 'vue-router'
import type { Component } from 'vue'
import { navigationAdmitted, navigationFailed, navigationSettled } from '@/middleware/navigation'
import { pinia } from '@/pinia'
import { useNavigationStore } from '@/stores/navigation'

/** A screen already in memory. */
const Screen: Component = { render: () => null }

/**
 * Builds a memory router with the three hooks installed, the way `router/index.ts` installs them.
 *
 * @param slow - Loader of the `/slow` screen.
 * @returns The router, already at `/`.
 */
const routerWith = async (slow: () => Promise<{ default: Component }>): Promise<Router> => {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: Screen },
      { path: '/ready', component: Screen },
      { path: '/slow', component: slow },
    ],
  })
  router.beforeEach(navigationAdmitted)
  router.afterEach(navigationSettled)
  router.onError(navigationFailed)
  await router.push('/')
  return router
}

/**
 * Records what the store held once the screens of each navigation were loaded, just before it arrives.
 *
 * @param router - The router to watch.
 * @returns The pending target seen at that moment, per navigation.
 */
const pendingBeforeArrival = (router: Router): Array<RouteLocationNormalized | null> => {
  const seen: Array<RouteLocationNormalized | null> = []
  router.beforeResolve(() => {
    seen.push(useNavigationStore(pinia).pending)
  })
  return seen
}

beforeEach(() => {
  // The hooks use the shared Pinia; start every test from an idle store.
  const store = useNavigationStore(pinia)
  store.pending = null
  store.failed = null
})

describe('navigation hooks', () => {
  it('hold a navigation as pending while its screen downloads, and end it on arrival', async () => {
    let arrive: (module: { default: Component }) => void = () => undefined
    const router = await routerWith(() => new Promise<{ default: Component }>((resolve) => { arrive = resolve }))

    const navigation = router.push('/slow')
    await flushPromises()
    expect(useNavigationStore(pinia).pending?.path).toBe('/slow')
    expect(router.currentRoute.value.path).toBe('/')

    arrive({ default: Screen })
    await navigation

    expect(router.currentRoute.value.path).toBe('/slow')
    expect(useNavigationStore(pinia).pending).toBeNull()
  })

  it('never hold a navigation whose screen is already in memory', async () => {
    const router = await routerWith(() => Promise.resolve({ default: Screen }))
    const seen = pendingBeforeArrival(router)

    await router.push('/ready')

    expect(seen).toEqual([null])
    expect(useNavigationStore(pinia).pending).toBeNull()
  })

  it('turn a download that fails for good into the failure', async () => {
    const router = await routerWith(() => Promise.reject(new Error('offline')))

    await expect(router.push('/slow')).rejects.toThrow('offline')

    const store = useNavigationStore(pinia)
    expect(store.pending).toBeNull()
    expect(store.failed?.path).toBe('/slow')
    expect(router.currentRoute.value.path).toBe('/')
  })

  it('clear the failure once the next navigation passes the guards', async () => {
    const router = await routerWith(() => Promise.reject(new Error('offline')))
    await expect(router.push('/slow')).rejects.toThrow('offline')

    await router.push('/ready')

    expect(useNavigationStore(pinia).failed).toBeNull()
  })
})
