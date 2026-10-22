/**
 * Unit tests for UiFooterLink: route and URL links, the per-template theme colours, the sliding
 * underline and the keyboard focus state.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiFooterLink from '~/components/ui/UiFooterLink.vue'
import { UiFooterLinkVariant } from '~/utils/ui/enums'

describe('UiFooterLink', () => {
  it('links to a route and renders its label', () => {
    const wrapper = mountWithKit(UiFooterLink, { props: { to: '/terms', testid: 'link' }, slots: { default: 'Terms' } })

    expect(wrapper.attributes('href')).toBe('/terms')
    expect(wrapper.attributes('data-testid')).toBe('link')
    expect(wrapper.text()).toBe('Terms')
  })

  it('keeps mailto and tel links in the same tab and opens absolute URLs safely', () => {
    const mail = mountWithKit(UiFooterLink, { props: { href: 'mailto:a@b.test' } })
    const site = mountWithKit(UiFooterLink, { props: { href: 'https://example.com' } })

    expect(mail.attributes('target')).toBeUndefined()
    expect(site.attributes('target')).toBe('_blank')
    expect(site.attributes('rel')).toBe('noopener noreferrer')
  })

  it.each([
    [UiFooterLinkVariant.Aurora, 'text-mut', 'hover:text-ac1', 'focus-visible:text-ac1'],
    [UiFooterLinkVariant.Nova, 'text-nova-muted', 'hover:text-nova-brand', 'focus-visible:text-nova-brand'],
    [UiFooterLinkVariant.Classic, 'text-classic-muted', 'hover:text-classic-brand', 'focus-visible:text-classic-brand'],
  ])('uses the %s theme tokens for rest, hover and focus', (variant, rest, hover, focus) => {
    const classes = mountWithKit(UiFooterLink, { props: { href: '/x', variant } }).classes()

    expect(classes).toEqual(expect.arrayContaining([rest, hover, focus, 'transition-colors']))
  })

  it('colours the classic variant from the theme tokens only, so one class pair serves both modes', () => {
    const classes = mountWithKit(UiFooterLink, { props: { href: '/x', variant: UiFooterLinkVariant.Classic } }).classes()

    expect(classes).toEqual(expect.arrayContaining(['text-classic-muted', 'focus-visible:ring-classic-brand']))
    expect(classes.some((name) => name.startsWith('dark:'))).toBe(false)
  })

  it('draws a sliding underline on hover and focus unless switched off', () => {
    const on = mountWithKit(UiFooterLink, { props: { href: '/x' } }).classes()
    const off = mountWithKit(UiFooterLink, { props: { href: '/x', underline: false } }).classes()

    expect(on).toEqual(expect.arrayContaining(['after:scale-x-0', 'hover:after:scale-x-100', 'focus-visible:after:scale-x-100']))
    expect(off).not.toContain('after:scale-x-0')
  })

  it('shows a focus ring only for keyboard focus', () => {
    const classes = mountWithKit(UiFooterLink, { props: { href: '/x' } }).classes()

    expect(classes).toEqual(expect.arrayContaining(['focus:outline-none', 'focus-visible:ring-2']))
  })
})
