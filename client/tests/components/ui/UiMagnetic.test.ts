/**
 * Unit tests for UiMagnetic: the slot, the pull toward the cursor inside the range, no pull
 * outside it, and the reset when the cursor leaves. The offset is read from the custom
 * properties the component writes.
 */
import { describe, expect, it } from 'vitest'
import { nextTick } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiMagnetic from '~/components/ui/UiMagnetic.vue'

/**
 * Reads a custom property from the moving element.
 *
 * @param wrapper The mounted component.
 * @param name The custom property, e.g. --mag-x.
 * @returns The property value.
 */
const read = (wrapper: ReturnType<typeof mountWithKit>, name: string): string =>
  (wrapper.find('[data-testid="ui-magnetic-target"]').element as HTMLElement).style.getPropertyValue(name)

describe('UiMagnetic', () => {
  it('renders its slot', () => {
    const wrapper = mountWithKit(UiMagnetic, { slots: { default: 'Hover me' } })

    expect(wrapper.text()).toBe('Hover me')
  })

  it('starts at rest with the settle duration set', () => {
    const wrapper = mountWithKit(UiMagnetic, { props: { duration: 300 } })

    expect(read(wrapper, '--mag-x')).toBe('0px')
    expect(read(wrapper, '--mag-duration')).toBe('300ms')
  })

  it('pulls toward the cursor when it is within range', async () => {
    const wrapper = mountWithKit(UiMagnetic, { props: { strength: 0.5, range: 100 } })

    // The test DOM has no layout, so the element centre is (0, 0) and the cursor offset is the pull base.
    await wrapper.trigger('mousemove', { clientX: 20, clientY: 40 })
    await nextTick()

    expect(read(wrapper, '--mag-x')).toBe('10px')
    expect(read(wrapper, '--mag-y')).toBe('20px')
  })

  it('does not pull when the cursor is outside the range', async () => {
    const wrapper = mountWithKit(UiMagnetic, { props: { range: 10 } })

    await wrapper.trigger('mousemove', { clientX: 200, clientY: 200 })
    await nextTick()

    expect(read(wrapper, '--mag-x')).toBe('0px')
  })

  it('returns to rest when the cursor leaves', async () => {
    const wrapper = mountWithKit(UiMagnetic, { props: { range: 100 } })
    await wrapper.trigger('mousemove', { clientX: 20, clientY: 20 })

    await wrapper.trigger('mouseleave')
    await nextTick()

    expect(read(wrapper, '--mag-x')).toBe('0px')
  })
})
