/**
 * Unit tests for UiAlert: the meaning variants, the title, the body and the action slot.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiAlert from '~/components/ui/UiAlert.vue'
import { UiAlertVariant } from '~/utils/ui/enums'

describe('UiAlert', () => {
  it('announces itself as an alert and renders the body', () => {
    const wrapper = mountWithKit(UiAlert, { slots: { default: 'Something happened' } })

    expect(wrapper.attributes('role')).toBe('alert')
    expect(wrapper.text()).toContain('Something happened')
  })

  it('defaults to the warning look', () => {
    const wrapper = mountWithKit(UiAlert)

    expect(wrapper.classes()).toContain('bg-yellow-50')
  })

  it.each([
    [UiAlertVariant.Error, 'bg-red-50'],
    [UiAlertVariant.Info, 'bg-blue-50'],
    [UiAlertVariant.Success, 'bg-green-50'],
  ])('paints the %s variant with %s', (variant, expected) => {
    const wrapper = mountWithKit(UiAlert, { props: { variant } })

    expect(wrapper.classes()).toContain(expected)
  })

  it('renders the title above the body', () => {
    const wrapper = mountWithKit(UiAlert, { props: { title: 'Heads up' }, slots: { default: 'Body' } })

    expect(wrapper.find('p').text()).toBe('Heads up')
  })

  it('renders the action slot', () => {
    const wrapper = mountWithKit(UiAlert, { slots: { action: 'Retry' } })

    expect(wrapper.text()).toContain('Retry')
  })

  it('draws the leading icon at the requested size', () => {
    const wrapper = mountWithKit(UiAlert, { props: { iconSize: 24 } })

    expect(wrapper.find('svg').attributes('width')).toBe('24')
  })
})
