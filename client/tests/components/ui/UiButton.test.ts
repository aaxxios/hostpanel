/**
 * Unit tests for UiButton: element choice, variants and sizes, disabled and loading state, and
 * that listeners reach the root element.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiButton from '~/components/ui/UiButton.vue'
import { UiButtonVariant, UiSize } from '~/utils/ui/enums'

describe('UiButton', () => {
  it('renders a type=button button with its slot', () => {
    const wrapper = mountWithKit(UiButton, { slots: { default: 'Save' } })

    expect(wrapper.element.tagName).toBe('BUTTON')
    expect(wrapper.attributes('type')).toBe('button')
    expect(wrapper.text()).toBe('Save')
  })

  it.each([
    [UiButtonVariant.Primary, 'bg-primary-700'],
    [UiButtonVariant.Danger, 'bg-red-600'],
    [UiButtonVariant.Outline, 'border-primary-600'],
    [UiButtonVariant.Ghost, 'hover:bg-gray-100'],
    [UiButtonVariant.Subtle, 'bg-primary-500/10'],
  ])('applies the %s variant classes', (variant, expected) => {
    const wrapper = mountWithKit(UiButton, { props: { variant } })

    expect(wrapper.classes()).toContain(expected)
  })

  it('applies the size classes', () => {
    const wrapper = mountWithKit(UiButton, { props: { size: UiSize.Lg } })

    expect(wrapper.classes()).toContain('px-6')
  })

  it('stretches with fullWidth', () => {
    const wrapper = mountWithKit(UiButton, { props: { fullWidth: true } })

    expect(wrapper.classes()).toContain('w-full')
  })

  it('is disabled when disabled', () => {
    const wrapper = mountWithKit(UiButton, { props: { disabled: true } })

    expect(wrapper.attributes('disabled')).toBeDefined()
  })

  it('is disabled and shows a spinner while loading', () => {
    const wrapper = mountWithKit(UiButton, { props: { loading: true }, slots: { default: 'Save' } })

    expect(wrapper.attributes('disabled')).toBeDefined()
    expect(wrapper.find('svg.animate-spin').exists()).toBe(true)
  })

  it('renders an anchor with target and rel for href', () => {
    const wrapper = mountWithKit(UiButton, { props: { href: 'https://example.com', target: '_blank', rel: 'noopener' } })

    expect(wrapper.element.tagName).toBe('A')
    expect(wrapper.attributes('href')).toBe('https://example.com')
    expect(wrapper.attributes('target')).toBe('_blank')
    expect(wrapper.attributes('rel')).toBe('noopener')
  })

  it('renders a link to the route for to', () => {
    const wrapper = mountWithKit(UiButton, { props: { to: '/pricing' } })

    expect(wrapper.element.tagName).toBe('A')
    expect(wrapper.attributes('href')).toBe('/pricing')
  })

  it('passes click listeners and data-testid to the root', async () => {
    const onClick = vi.fn()
    const wrapper = mountWithKit(UiButton, { attrs: { onClick, 'data-testid': 'save-button' } })

    await wrapper.trigger('click')

    expect(onClick).toHaveBeenCalledTimes(1)
    expect(wrapper.attributes('data-testid')).toBe('save-button')
  })

  it('is structure only when bare: no palette and no padding step', () => {
    const wrapper = mountWithKit(UiButton, { props: { variant: UiButtonVariant.Bare } })

    expect(wrapper.classes()).toContain('inline-flex')
    expect(wrapper.classes()).not.toContain('px-4')
    expect(wrapper.classes().some(name => name.startsWith('bg-'))).toBe(false)
  })

  it('does not animate the outline, so a mouse press never flashes one', () => {
    const classes = mountWithKit(UiButton, { slots: { default: 'Go' } }).find('button').classes()

    expect(classes).toContain('transition')
    expect(classes).not.toContain('transition-all')
  })

  it('offsets the focus ring from the surface in both themes', () => {
    const classes = mountWithKit(UiButton, { slots: { default: 'Go' } }).find('button').classes()

    expect(classes).toContain('ring-offset-white')
    expect(classes).toContain('dark:ring-offset-ui-surface')
  })

  it('lightens the outline variant on dark surfaces', () => {
    const classes = mountWithKit(UiButton, { props: { variant: UiButtonVariant.Outline } }).classes()

    expect(classes).toContain('dark:text-primary-400')
    expect(classes).toContain('dark:border-primary-400')
  })

  it('carries the template hook classes classic.css restyles', () => {
    const wrapper = mountWithKit(UiButton, { props: { variant: UiButtonVariant.Primary } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['ui-btn', 'ui-btn-primary']))
  })
})
