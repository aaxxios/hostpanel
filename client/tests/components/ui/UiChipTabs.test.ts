/**
 * Unit tests for UiChipTabs: tab-list semantics, v-model, keyboard navigation, counts and the
 * readable brand look of the chosen chip.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiChipTabs from '~/components/ui/UiChipTabs.vue'
import type { ChipTab } from '~/types/chiptab'

/** Three chips, one with a count, to exercise wrapping and Home / End. */
const TABS: ChipTab[] = [
  { value: 'all', label: 'All', count: 12, testid: 'chip-all' },
  { value: 'tech', label: 'Tech', testid: 'chip-tech' },
  { value: 'local', label: 'Local', testid: 'chip-local' },
]

/**
 * Mounts the tabs attached to the document so focus can be asserted.
 *
 * @param modelValue - The chosen value.
 * @returns The wrapper.
 */
const mountTabs = (modelValue: string) =>
  mountWithKit(UiChipTabs, {
    props: { tabs: TABS, modelValue, label: 'Category', testid: 'chips' },
    attachToBody: true,
  })

describe('UiChipTabs', () => {
  it('renders a labelled tab list with one tab per chip and aria-selected on the chosen one', () => {
    const wrapper = mountTabs('tech')

    expect(wrapper.find('[role="tablist"]').attributes('aria-label')).toBe('Category')
    expect(wrapper.findAll('[role="tab"]')).toHaveLength(3)
    expect(wrapper.find('[data-testid="chip-tech"]').attributes('aria-selected')).toBe('true')
    expect(wrapper.find('[data-testid="chip-all"]').attributes('aria-selected')).toBe('false')
    wrapper.unmount()
  })

  it('shows the count after the label', () => {
    const wrapper = mountTabs('all')

    expect(wrapper.find('[data-testid="chip-all"]').text()).toBe('All 12')
    expect(wrapper.find('[data-testid="chip-tech"]').text()).toBe('Tech')
    wrapper.unmount()
  })

  it('makes only the chosen chip a tab stop', () => {
    const wrapper = mountTabs('local')

    expect(wrapper.find('[data-testid="chip-local"]').attributes('tabindex')).toBe('0')
    expect(wrapper.find('[data-testid="chip-all"]').attributes('tabindex')).toBe('-1')
    wrapper.unmount()
  })

  it('emits the clicked value and stays quiet for the current one', async () => {
    const wrapper = mountTabs('all')

    await wrapper.find('[data-testid="chip-local"]').trigger('click')
    await wrapper.find('[data-testid="chip-all"]').trigger('click')

    expect(wrapper.emitted('update:modelValue')).toEqual([['local']])
    wrapper.unmount()
  })

  it('moves the choice and focus with the arrow keys, wrapping at both ends', async () => {
    const wrapper = mountTabs('local')

    await wrapper.find('[data-testid="chip-local"]').trigger('keydown', { key: 'ArrowRight' })
    expect(document.activeElement).toBe(wrapper.find('[data-testid="chip-all"]').element)
    await wrapper.find('[data-testid="chip-local"]').trigger('keydown', { key: 'ArrowLeft' })

    expect(wrapper.emitted('update:modelValue')).toEqual([['all'], ['tech']])
    wrapper.unmount()
  })

  it('jumps with Home and End and ignores other keys', async () => {
    const wrapper = mountTabs('tech')
    const current = wrapper.find('[data-testid="chip-tech"]')

    await current.trigger('keydown', { key: 'End' })
    await current.trigger('keydown', { key: 'Home' })
    await current.trigger('keydown', { key: 'Enter' })

    expect(wrapper.emitted('update:modelValue')).toEqual([['local'], ['all']])
    wrapper.unmount()
  })

  it('paints the chosen chip with the brand gradient and the contrast text token, no raw dark text', () => {
    const wrapper = mountTabs('tech')
    const active = wrapper.find('[data-testid="chip-tech"]').classes()

    expect(active).toEqual(expect.arrayContaining(['bg-brand', 'text-on-brand', 'rounded-full']))
    expect(active.some(c => /^(dark:)?text-(gray|black|white)/.test(c))).toBe(false)
    wrapper.unmount()
  })

  it('keeps idle chips readable in both themes with hover states and a focus ring', () => {
    const wrapper = mountTabs('tech')
    const idle = wrapper.find('[data-testid="chip-all"]').classes()

    expect(idle).toEqual(expect.arrayContaining(['text-gray-600', 'dark:text-gray-300', 'hover:text-gray-900', 'dark:hover:text-white', 'focus-visible:ring-2']))
    expect(idle).not.toContain('bg-brand')
    wrapper.unmount()
  })

  it('carries the template hooks: ui-chip on every chip, ui-chip-active on the chosen one, ui-chip-count on the count', () => {
    const wrapper = mountTabs('all')

    expect(wrapper.find('[data-testid="chip-all"]').classes()).toEqual(expect.arrayContaining(['ui-chip', 'ui-chip-active']))
    expect(wrapper.find('[data-testid="chip-tech"]').classes()).toContain('ui-chip')
    expect(wrapper.find('[data-testid="chip-tech"]').classes()).not.toContain('ui-chip-active')
    expect(wrapper.find('[data-testid="chip-all"] .ui-chip-count').exists()).toBe(true)
    wrapper.unmount()
  })

  it('scrolls inside its own box instead of widening the page', () => {
    const wrapper = mountTabs('all')
    const box = wrapper.find('[data-testid="chips"]').classes()

    expect(box).toEqual(expect.arrayContaining(['overflow-x-auto', 'max-w-full', 'w-full']))
    wrapper.unmount()
  })
})
