/**
 * Unit tests for UiTooltip: opening and closing from the trigger, the content slot and closing on
 * an outside click.
 */
import { describe, expect, it } from 'vitest'
import { nextTick } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiTooltip from '~/components/ui/UiTooltip.vue'

describe('UiTooltip', () => {
  it('renders the trigger and hides the content at first', () => {
    const wrapper = mountWithKit(UiTooltip, { slots: { default: 'Info', content: 'Tip text' } })

    expect(wrapper.find('[data-testid="ui-tooltip-trigger"]').text()).toBe('Info')
    expect(wrapper.find('[data-testid="ui-tooltip-content"]').exists()).toBe(false)
  })

  it('shows the content as a tooltip when the trigger is clicked', async () => {
    const wrapper = mountWithKit(UiTooltip, { slots: { default: 'Info', content: 'Tip text' } })

    await wrapper.find('[data-testid="ui-tooltip-trigger"]').trigger('click')

    const content = wrapper.find('[data-testid="ui-tooltip-content"]')
    expect(content.text()).toBe('Tip text')
    expect(content.attributes('role')).toBe('tooltip')
  })

  it('hides the content when the trigger is clicked again', async () => {
    const wrapper = mountWithKit(UiTooltip, { slots: { default: 'Info', content: 'Tip text' } })
    await wrapper.find('[data-testid="ui-tooltip-trigger"]').trigger('click')

    await wrapper.find('[data-testid="ui-tooltip-trigger"]').trigger('click')

    expect(wrapper.find('[data-testid="ui-tooltip-content"]').exists()).toBe(false)
  })

  it('closes when the page is clicked elsewhere', async () => {
    const wrapper = mountWithKit(UiTooltip, { slots: { default: 'Info', content: 'Tip text' }, attachToBody: true })
    await wrapper.find('[data-testid="ui-tooltip-trigger"]').trigger('click')
    expect(wrapper.find('[data-testid="ui-tooltip-content"]').exists()).toBe(true)

    // The outside-click listener ignores clicks in the same task as the one that opened the tooltip,
    // so let that task finish; then a real click is a press followed by a release.
    await new Promise((resolve) => setTimeout(resolve, 0))
    document.body.dispatchEvent(new PointerEvent('pointerdown', { bubbles: true }))
    document.body.click()
    await nextTick()

    expect(wrapper.find('[data-testid="ui-tooltip-content"]').exists()).toBe(false)
    wrapper.unmount()
  })
})
