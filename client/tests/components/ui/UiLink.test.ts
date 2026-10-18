/**
 * Unit tests for UiLink: route and URL links, external behaviour, tones and the underline.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiLink from '~/components/ui/UiLink.vue'
import { UiLinkTone } from '~/utils/ui/enums'

describe('UiLink', () => {
  it('links to a route', () => {
    const wrapper = mountWithKit(UiLink, { props: { to: '/pricing' }, slots: { default: 'Pricing' } })

    expect(wrapper.element.tagName).toBe('A')
    expect(wrapper.attributes('href')).toBe('/pricing')
    expect(wrapper.text()).toBe('Pricing')
    expect(wrapper.attributes('target')).toBeUndefined()
  })

  it('links to a same-site URL without opening a new tab', () => {
    const wrapper = mountWithKit(UiLink, { props: { href: '/docs/guide.pdf' } })

    expect(wrapper.attributes('href')).toBe('/docs/guide.pdf')
    expect(wrapper.attributes('target')).toBeUndefined()
  })

  it('opens an absolute URL in a new tab safely', () => {
    const wrapper = mountWithKit(UiLink, { props: { href: 'https://example.com' } })

    expect(wrapper.attributes('target')).toBe('_blank')
    expect(wrapper.attributes('rel')).toBe('noopener noreferrer')
  })

  it('opens a relative URL in a new tab when external is set', () => {
    const wrapper = mountWithKit(UiLink, { props: { href: '/report', external: true } })

    expect(wrapper.attributes('target')).toBe('_blank')
    expect(wrapper.attributes('rel')).toBe('noopener noreferrer')
  })

  it.each([
    [UiLinkTone.Primary, 'text-primary-600'],
    [UiLinkTone.Muted, 'text-gray-500'],
    [UiLinkTone.Inherit, 'hover:opacity-80'],
  ])('paints the %s tone with %s', (tone, expected) => {
    const wrapper = mountWithKit(UiLink, { props: { to: '/x', tone } })

    expect(wrapper.classes()).toContain(expected)
  })

  it('underlines by default and can turn it off', () => {
    expect(mountWithKit(UiLink, { props: { to: '/x' } }).classes()).toContain('underline')
    expect(mountWithKit(UiLink, { props: { to: '/x', underline: false } }).classes()).not.toContain('underline')
  })

  it('puts the testid on the link', () => {
    const wrapper = mountWithKit(UiLink, { props: { to: '/x', testid: 'terms-link' } })

    expect(wrapper.attributes('data-testid')).toBe('terms-link')
  })
})
