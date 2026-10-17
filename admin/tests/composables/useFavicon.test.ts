/**
 * Tests for `useFavicon`: the operator's favicon replaces the built-in icon links once the
 * branding settings arrive, and nothing is touched when none is configured.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { defineComponent, h } from 'vue'
import { useFavicon } from '@/composables/useFavicon'
import { mountWithApp } from '../helpers/mount'
import { json, problem } from '../helpers/fetch'
import { stubRoutes } from '../helpers/routes'

/** Mounts a component whose setup calls `useFavicon`, as the root component does. */
const mountRoot = () =>
  mountWithApp(defineComponent({
    setup() {
      useFavicon()
      return () => h('div')
    },
  }))

beforeEach(() => {
  document.head.innerHTML = '<link rel="icon" href="/built-in.svg">'
})

afterEach(() => {
  vi.unstubAllGlobals()
  document.head.innerHTML = ''
})

describe('useFavicon', () => {
  it('replaces the built-in icon links with the operator favicon', async () => {
    stubRoutes({ 'GET /api/settings/public': () => json([{ key: 'portal.favicon', value: '/custom.png' }]) })

    mountRoot()
    await flushPromises()

    const links = document.head.querySelectorAll('link[rel~="icon"]')
    expect(links).toHaveLength(1)
    expect(links[0]?.getAttribute('href')).toBe('/custom.png')
  })

  it('leaves the built-in links alone when no favicon is configured', async () => {
    stubRoutes({ 'GET /api/settings/public': () => json([]) })

    mountRoot()
    await flushPromises()

    const links = document.head.querySelectorAll('link[rel~="icon"]')
    expect(links).toHaveLength(1)
    expect(links[0]?.getAttribute('href')).toBe('/built-in.svg')
  })

  it('leaves the built-in links alone when the branding cannot be loaded', async () => {
    stubRoutes({ 'GET /api/settings/public': () => problem('Unavailable', 503) })

    mountRoot()
    await flushPromises()

    expect(document.head.querySelector('link[rel~="icon"]')?.getAttribute('href')).toBe('/built-in.svg')
  })
})
