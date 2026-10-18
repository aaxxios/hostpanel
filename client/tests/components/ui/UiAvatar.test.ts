/**
 * Unit tests for UiAvatar: initials from the name, the e-mail and question-mark fallbacks, the
 * hover title, the sizes and that the circle is hidden from assistive technology.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiAvatar from '~/components/ui/UiAvatar.vue'
import { UiAvatarSize } from '~/utils/ui/enums'

describe('UiAvatar', () => {
  it('shows both initials when the full name is known', () => {
    const wrapper = mountWithKit(UiAvatar, { props: { firstName: 'Jane', lastName: 'Doe' } })

    expect(wrapper.text()).toBe('JD')
    expect(wrapper.attributes('title')).toBe('Jane Doe')
  })

  it('shows one initial when only a first name is known', () => {
    const wrapper = mountWithKit(UiAvatar, { props: { firstName: 'Jane' } })

    expect(wrapper.text()).toBe('J')
  })

  it('falls back to the e-mail before the name has loaded', () => {
    const wrapper = mountWithKit(UiAvatar, { props: { email: 'jane@example.com' } })

    expect(wrapper.text()).toBe('j')
    expect(wrapper.attributes('title')).toBe('jane@example.com')
  })

  it('falls back to a question mark when nothing is known', () => {
    const wrapper = mountWithKit(UiAvatar)

    expect(wrapper.text()).toBe('?')
    expect(wrapper.attributes('title')).toBeUndefined()
  })

  it('is hidden from assistive technology', () => {
    const wrapper = mountWithKit(UiAvatar)

    expect(wrapper.attributes('aria-hidden')).toBe('true')
  })

  it('maps the sizes to diameters', () => {
    expect(mountWithKit(UiAvatar).classes()).toContain('h-9')
    expect(mountWithKit(UiAvatar, { props: { size: UiAvatarSize.Sm } }).classes()).toContain('h-6')
  })
})
