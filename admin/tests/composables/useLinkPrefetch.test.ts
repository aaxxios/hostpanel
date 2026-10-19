/**
 * Tests for prefetch-on-hover: an in-app link under the pointer, focus or finger starts its screen's
 * download; external, new-tab, download and anchor links do not; the listener goes away on unmount.
 */
import { describe, expect, it, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createMemoryHistory, createRouter, createWebHistory } from 'vue-router'
import type { Router } from 'vue-router'
import { defineComponent, h } from 'vue'
import { inAppPathOf, useLinkPrefetch } from '@/composables/useLinkPrefetch'

/** Stand-in screen. */
const Blank = { render: () => null }

/** Loader spy of the `/clients` screen; a fresh one per router, because a loader is requested only once. */
let clientsLoader = vi.fn(() => Promise.resolve(Blank))

/**
 * Builds a router with one lazy screen, and points {@link clientsLoader} at that screen's loader.
 *
 * @returns The router.
 */
const buildRouter = (): Router => {
  clientsLoader = vi.fn(() => Promise.resolve(Blank))
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: Blank },
      { path: '/clients', component: clientsLoader },
    ],
  })
}

/**
 * Mounts a host that installs the prefetch listener and renders the given anchors.
 *
 * @param router - The router to prefetch through.
 * @param anchors - Attributes of the anchors to render.
 * @returns The mounted wrapper, attached to the document.
 */
const mountHost = (router: Router, anchors: Array<Record<string, string>>) => mount(defineComponent({
  setup() {
    useLinkPrefetch(router)
    return () => h('div', anchors.map((attrs, index) => h('a', { key: index, ...attrs }, 'link')))
  },
}), { attachTo: document.body })


describe('useLinkPrefetch', () => {
  it.each(['pointerover', 'focusin', 'touchstart'])('prefetches the screen on %s', async (eventName) => {
    const wrapper = mountHost(buildRouter(), [{ href: '/clients' }])

    await wrapper.get('a').trigger(eventName)

    expect(clientsLoader).toHaveBeenCalledTimes(1)
  })

  it('ignores external, new-tab, download and in-page links', async () => {
    const wrapper = mountHost(buildRouter(), [
      { href: 'https://example.test/clients' },
      { href: '/clients', target: '_blank' },
      { href: '/clients', download: '' },
      { href: '#top' },
    ])

    for (const anchor of wrapper.findAll('a')) {
      await anchor.trigger('pointerover')
    }

    expect(clientsLoader).not.toHaveBeenCalled()
  })

  it('ignores events that are not on a link', async () => {
    const wrapper = mountHost(buildRouter(), [{ href: '/clients' }])

    await wrapper.get('div').trigger('pointerover')
    document.dispatchEvent(new Event('pointerover'))

    expect(clientsLoader).not.toHaveBeenCalled()
  })

  it('stops listening once the component is unmounted', () => {
    const wrapper = mountHost(buildRouter(), [{ href: '/clients' }])
    const anchor = wrapper.get('a').element

    wrapper.unmount()
    anchor.dispatchEvent(new Event('pointerover', { bubbles: true }))

    expect(clientsLoader).not.toHaveBeenCalled()
  })
})

describe('inAppPathOf', () => {
  /**
   * Builds a detached anchor.
   *
   * @param href - Its `href`.
   * @returns The anchor.
   */
  const anchorTo = (href: string): HTMLAnchorElement => {
    const anchor = document.createElement('a')
    anchor.setAttribute('href', href)
    return anchor
  }

  /**
   * Builds a router served under `/admin`.
   *
   * @returns The router.
   */
  const adminRouter = (): Router => createRouter({ history: createWebHistory('/admin'), routes: [{ path: '/:p(.*)*', component: Blank }] })

  it('strips the router base so the path can be resolved', () => {
    expect(inAppPathOf(anchorTo('/admin/clients?page=2#top'), adminRouter())).toBe('/clients?page=2#top')
    expect(inAppPathOf(anchorTo('/admin'), adminRouter())).toBe('/')
  })

  it('rejects a path outside the base', () => {
    expect(inAppPathOf(anchorTo('/administrator'), adminRouter())).toBeNull()
    expect(inAppPathOf(anchorTo('/client/home'), adminRouter())).toBeNull()
  })

  it('returns null for an anchor with no href', () => {
    expect(inAppPathOf(document.createElement('a'), buildRouter())).toBeNull()
  })
})
