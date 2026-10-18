/**
 * Unit tests for UiWidgetFrame: it exposes its DOM node for a widget to mount into and reflects
 * the widget's state in its frame colour.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiWidgetFrame from '~/components/ui/UiWidgetFrame.vue'

describe('UiWidgetFrame', () => {
  it('exposes the box element a widget mounts into', () => {
    const wrapper = mountWithKit(UiWidgetFrame, { attachToBody: true })

    const exposed = wrapper.vm as unknown as { el: HTMLDivElement | null }
    expect(exposed.el).toBe(wrapper.element)
    wrapper.unmount()
  })

  it('is neutral by default', () => {
    const wrapper = mountWithKit(UiWidgetFrame)

    expect(wrapper.classes()).toContain('border-gray-300')
    expect(wrapper.classes()).not.toContain('border-cyan-500/30')
    expect(wrapper.classes()).not.toContain('border-red-500/50')
  })

  it('shows the ready colour once the widget has drawn', () => {
    const wrapper = mountWithKit(UiWidgetFrame, { props: { ready: true } })

    expect(wrapper.classes()).toContain('border-cyan-500/30')
  })

  it('shows the error colour, which outranks ready', () => {
    const wrapper = mountWithKit(UiWidgetFrame, { props: { ready: true, invalid: true } })

    expect(wrapper.classes()).toContain('border-red-500/50')
    expect(wrapper.classes()).not.toContain('border-cyan-500/30')
  })
})
