/**
 * Unit tests for UiAccentButton: element choice, appearances, sizes, state and listeners.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiAccentButton from '~/components/ui/UiAccentButton.vue'
import { UiAccentAppearance } from '~/utils/ui/accent'
import { UiSize } from '~/utils/ui/enums'

describe('UiAccentButton', () => {
  it('renders a type=button button with its slot', () => {
    const wrapper = mountWithKit(UiAccentButton, { slots: { default: 'Go' } })

    expect(wrapper.element.tagName).toBe('BUTTON')
    expect(wrapper.attributes('type')).toBe('button')
    expect(wrapper.text()).toBe('Go')
  })

  it('renders a route link for `to` and an external anchor for `href`', () => {
    const route = mountWithKit(UiAccentButton, { props: { to: '/x' } })
    const anchor = mountWithKit(UiAccentButton, { props: { href: 'https://example.com' } })

    expect(route.element.tagName).toBe('A')
    expect(route.attributes('href')).toBe('/x')
    expect(anchor.attributes('target')).toBe('_blank')
    expect(anchor.attributes('rel')).toBe('noopener noreferrer')
  })

  it.each([
    [UiAccentAppearance.Solid, 'bg-[color:var(--accent)]'],
    [UiAccentAppearance.Soft, 'border'],
    [UiAccentAppearance.Outline, 'border-2'],
  ])('applies the %s appearance', (appearance, expected) => {
    expect(mountWithKit(UiAccentButton, { props: { appearance } }).classes()).toContain(expected)
  })

  it('applies size, pill and full width', () => {
    const wrapper = mountWithKit(UiAccentButton, { props: { size: UiSize.Lg, pill: true, fullWidth: true } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['px-6', 'rounded-full', 'w-full']))
  })

  it('is disabled when asked and passes clicks through', async () => {
    let clicks = 0
    const wrapper = mountWithKit(UiAccentButton, {
      attrs: {
        onClick: () => {
          clicks += 1
        },
      },
    })

    await wrapper.trigger('click')
    expect(clicks).toBe(1)
    expect(mountWithKit(UiAccentButton, { props: { disabled: true } }).attributes('disabled')).toBeDefined()
  })
})
