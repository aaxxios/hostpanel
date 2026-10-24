/**
 * Unit tests for LayoutHeaderDropdown: opening on click, the links in the panel, the current
 * state, and closing on Escape, on a chosen link and on an outside click.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import LayoutHeaderDropdown from '~/components/layout/HeaderDropdown.vue'
import type { HeaderNavItem } from '~/types/headernavitem'

/** A two-link group. */
const item: HeaderNavItem = {
  key: 'support',
  label: 'Support',
  children: [
    { key: 'help', label: 'Help center', icon: 'mdi:book-open-variant', desc: 'Guides', to: '/en/knowledgebase' },
    { key: 'faq', label: 'FAQ', icon: 'mdi:frequently-asked-questions', to: '/en/faq' },
  ],
}

/**
 * Mounts the dropdown in the document so outside clicks and focus work.
 *
 * @param path - The current page path.
 * @param active - Whether the group is the header's active entry.
 * @returns The wrapper.
 */
const mountDropdown = (path = '/en/', active = false) => mountWithKit(LayoutHeaderDropdown, { props: { item, path, active }, attachToBody: true })

describe('LayoutHeaderDropdown', () => {
  it('keeps the panel closed until the trigger is pressed', async () => {
    const wrapper = mountDropdown()
    const trigger = wrapper.find('[data-testid="header-nav-support-trigger"]')
    expect(wrapper.find('[data-testid="header-nav-support-panel"]').exists()).toBe(false)
    expect(trigger.attributes('aria-expanded')).toBe('false')

    await trigger.trigger('click')

    expect(wrapper.find('[data-testid="header-nav-support-panel"]').exists()).toBe(true)
    expect(trigger.attributes('aria-expanded')).toBe('true')
    expect(trigger.attributes('aria-haspopup')).toBe('menu')
  })

  it('lists every link with its description', async () => {
    const wrapper = mountDropdown()
    await wrapper.find('[data-testid="header-nav-support-trigger"]').trigger('click')

    const links = wrapper.findAll('[role="menuitem"]')
    expect(links.map((link) => link.attributes('href'))).toEqual(['/en/knowledgebase', '/en/faq'])
    expect(links[0]!.text()).toContain('Guides')
  })

  it('leads every row with its icon in a tinted tile, and fills the tile of the current row', async () => {
    const wrapper = mountDropdown('/en/faq')
    await wrapper.find('[data-testid="header-nav-support-trigger"]').trigger('click')

    const help = wrapper.find('[data-testid="header-nav-child-help-icon"]')
    const faq = wrapper.find('[data-testid="header-nav-child-faq-icon"]')
    expect(help.find('[data-icon="mdi:book-open-variant"]').exists()).toBe(true)
    expect(faq.find('[data-icon="mdi:frequently-asked-questions"]').exists()).toBe(true)
    expect(help.classes()).toContain('bg-classic-brand-soft')
    expect(faq.classes()).toContain('bg-classic-fill')
  })

  it('gives every row the same height and a hover and focus state, with or without a description', async () => {
    const wrapper = mountDropdown()
    await wrapper.find('[data-testid="header-nav-support-trigger"]').trigger('click')

    for (const row of wrapper.findAll('[role="menuitem"]')) {
      expect(row.classes()).toEqual(expect.arrayContaining(['group', 'min-h-[60px]', 'hover:bg-classic-surface-2', 'focus-visible:bg-classic-surface-2']))
    }
  })

  it('marks the current link and tints the trigger when it is the active entry', async () => {
    const wrapper = mountDropdown('/en/faq', true)
    await wrapper.find('[data-testid="header-nav-support-trigger"]').trigger('click')

    expect(wrapper.find('[data-testid="header-nav-child-faq"]').attributes('aria-current')).toBe('page')
    expect(wrapper.find('[data-testid="header-nav-child-help"]').attributes('aria-current')).toBeUndefined()
    expect(wrapper.find('[data-testid="header-nav-support-trigger"]').classes()).toEqual(expect.arrayContaining(['bg-classic-fill', 'text-classic-on-brand']))
  })

  it('closes on Escape and gives focus back to the trigger', async () => {
    const wrapper = mountDropdown()
    await wrapper.find('[data-testid="header-nav-support-trigger"]').trigger('click')

    await wrapper.find('[data-testid="header-nav-support-panel"]').trigger('keydown', { key: 'Escape' })

    expect(wrapper.find('[data-testid="header-nav-support-panel"]').exists()).toBe(false)
    expect(document.activeElement).toBe(wrapper.find('[data-testid="header-nav-support-trigger"]').element)
  })

  it('closes when a link is chosen', async () => {
    const wrapper = mountDropdown()
    await wrapper.find('[data-testid="header-nav-support-trigger"]').trigger('click')

    await wrapper.find('[data-testid="header-nav-child-faq"]').trigger('click')

    expect(wrapper.find('[data-testid="header-nav-support-panel"]').exists()).toBe(false)
  })

  it('closes on a click outside', async () => {
    const wrapper = mountDropdown()
    await wrapper.find('[data-testid="header-nav-support-trigger"]').trigger('click')

    document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }))
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="header-nav-support-panel"]').exists()).toBe(false)
  })

  it('leaves the trigger idle when the parent says another entry is active, even if a child matches', () => {
    const wrapper = mountDropdown('/en/faq', false)

    expect(wrapper.find('[data-testid="header-nav-support-trigger"]').classes()).not.toContain('bg-classic-fill')
  })
})
