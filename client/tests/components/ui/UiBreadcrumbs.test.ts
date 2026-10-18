/**
 * Unit tests for UiBreadcrumbs: the Home link, linked and current steps, and the nav landmark.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiBreadcrumbs from '~/components/ui/UiBreadcrumbs.vue'

describe('UiBreadcrumbs', () => {
  beforeEach(() => {
    vi.stubGlobal('useLocalePath', () => (path: string) => '/en' + path)
  })

  it('is a labelled navigation landmark starting with a localised Home link', () => {
    const wrapper = mountWithKit(UiBreadcrumbs, { props: { items: [] } })

    expect(wrapper.element.tagName).toBe('NAV')
    expect(wrapper.attributes('aria-label')).toBe('common.breadcrumb')
    const home = wrapper.find('a')
    expect(home.text()).toBe('common.home')
    expect(home.attributes('href')).toBe('/en/')
  })

  it('links the middle steps and marks the last one as the current page', () => {
    const wrapper = mountWithKit(UiBreadcrumbs, {
      props: { items: [{ label: 'Products', to: '/products' }, { label: 'Hosting' }] },
    })

    const links = wrapper.findAll('a')
    expect(links).toHaveLength(2)
    expect(links[1]?.text()).toBe('Products')
    expect(links[1]?.attributes('href')).toBe('/products')
    const current = wrapper.find('[aria-current="page"]')
    expect(current.text()).toBe('Hosting')
    expect(current.element.tagName).toBe('SPAN')
  })

  it('does not link the last step even when it has a route', () => {
    const wrapper = mountWithKit(UiBreadcrumbs, { props: { items: [{ label: 'Last', to: '/last' }] } })

    expect(wrapper.findAll('a')).toHaveLength(1)
    expect(wrapper.find('[aria-current="page"]').text()).toBe('Last')
  })

  it('applies the container class, defaulting to a bottom margin', () => {
    expect(mountWithKit(UiBreadcrumbs, { props: { items: [] } }).classes()).toContain('mb-8')
    expect(mountWithKit(UiBreadcrumbs, { props: { items: [], containerClass: 'mb-2' } }).classes()).toContain('mb-2')
  })
})
