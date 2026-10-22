/**
 * Tests for the default layout: the active template's header and footer around the page slot,
 * the floating actions and the cart drawer, and the template stamp on the document.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { defineComponent, h } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { registerPageComponents } from '~/tests/helpers/publicPage'
import DefaultLayout from '~/layouts/default.vue'

/**
 * Builds a component that renders a marked element.
 *
 * @param testid - The `data-testid` the element carries.
 * @returns The component.
 */
const marker = (testid: string) => defineComponent({ setup: () => () => h('div', { 'data-testid': testid }) })

/** Spy for `useHead`, to read what the layout stamps on the document. */
const useHead = vi.fn()

describe('layouts/default', () => {
  beforeEach(() => {
    useHead.mockReset()
    vi.stubGlobal('useHead', useHead)
    vi.stubGlobal('useTemplate', () => ({
      name: 'nova',
      slot: (slotName: string) => marker(`template-${slotName}`),
    }))
    registerPageComponents({ LayoutCartDrawer: marker('cart-drawer'), LayoutDemoBanner: marker('demo-banner') })
  })

  /**
   * Mounts the layout with a page in its slot.
   *
   * @returns The wrapper.
   */
  const mountLayout = () => mountWithKit(DefaultLayout, {
    slots: { default: '<span data-testid="page">Page</span>' },
    stubs: { UiFloatingActions: marker('floating-actions'), UiCookieBanner: marker('cookie-banner') },
  })

  it('renders the template header, the page, the template footer, the floating actions and the cart', () => {
    const wrapper = mountLayout()

    for (const id of ['demo-banner', 'template-header', 'page', 'template-footer', 'floating-actions', 'cookie-banner', 'cart-drawer']) {
      expect(wrapper.find(`[data-testid="${id}"]`).exists()).toBe(true)
    }
  })

  it('keeps the page inside main, between the header and the footer', () => {
    const wrapper = mountLayout()

    expect(wrapper.find('main [data-testid="page"]').exists()).toBe(true)
    const html = wrapper.html()
    expect(html.indexOf('template-header')).toBeLessThan(html.indexOf('data-testid="page"'))
    expect(html.indexOf('data-testid="page"')).toBeLessThan(html.indexOf('template-footer'))
  })

  it('fills the viewport height and paints the page surface', () => {
    const wrapper = mountLayout()

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['min-h-dvh', 'bg-page']))
  })

  it('stamps the active template on the html element', () => {
    mountLayout()

    expect(useHead).toHaveBeenCalledWith({ htmlAttrs: { 'data-template': 'nova' } })
  })

  it('scopes the layout root with the active template class', () => {
    const wrapper = mountLayout()

    expect(wrapper.classes()).toContain('tpl-nova')
  })
})
