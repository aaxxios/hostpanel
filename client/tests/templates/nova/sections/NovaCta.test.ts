/**
 * Unit tests for NovaCta: route link vs anchor, and the tone and size maps.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaCta from '~/templates/nova/sections/NovaCta.vue'
import { NovaCtaSize, NovaCtaTone } from '~/templates/nova/enums'

describe('NovaCta', () => {
  it('renders a route link for `to` and carries its text', () => {
    const wrapper = mountWithKit(NovaCta, { props: { to: '/hosting' }, slots: { default: 'Start' } })

    expect(wrapper.element.tagName).toBe('A')
    expect(wrapper.attributes('href')).toBe('/hosting')
    expect(wrapper.text()).toBe('Start')
  })

  it('renders a plain anchor for `href`', () => {
    const wrapper = mountWithKit(NovaCta, { props: { href: '/sso/start' } })

    expect(wrapper.attributes('href')).toBe('/sso/start')
  })

  it('has a default test id the caller can override', () => {
    expect(mountWithKit(NovaCta, { props: { to: '/' } }).attributes('data-testid')).toBe('nova-cta')
    expect(mountWithKit(NovaCta, { props: { to: '/' }, attrs: { 'data-testid': 'hero-cta' } }).attributes('data-testid')).toBe('hero-cta')
  })

  it.each([
    [NovaCtaTone.Accent, 'bg-nova-accent'],
    [NovaCtaTone.Outline, 'border-nova-border'],
    [NovaCtaTone.Quiet, 'text-nova-muted'],
  ])('paints the %s tone with %s', (tone, expected) => {
    expect(mountWithKit(NovaCta, { props: { to: '/', tone } }).classes()).toContain(expected)
  })

  it.each([
    [NovaCtaSize.Lg, 'min-h-[52px]'],
    [NovaCtaSize.Md, 'min-h-[48px]'],
    [NovaCtaSize.Card, 'min-h-[48px]'],
    [NovaCtaSize.Bar, 'min-h-[44px]'],
  ])('sizes the %s step with %s', (size, expected) => {
    expect(mountWithKit(NovaCta, { props: { to: '/', size } }).classes()).toContain(expected)
  })

  it('lifts on hover only when asked', () => {
    expect(mountWithKit(NovaCta, { props: { to: '/', lift: true } }).classes()).toContain('motion-safe:hover:-translate-y-0.5')
    expect(mountWithKit(NovaCta, { props: { to: '/' } }).classes()).not.toContain('motion-safe:hover:-translate-y-0.5')
  })

  it('stretches with fullWidth', () => {
    expect(mountWithKit(NovaCta, { props: { to: '/', fullWidth: true } }).classes()).toContain('w-full')
  })
})
