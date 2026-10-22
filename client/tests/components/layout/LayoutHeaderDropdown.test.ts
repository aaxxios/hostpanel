/**
 * Unit tests for LayoutHeaderDropdown: opening on click, the links in the panel, the current
 * state, and closing on Escape, on a chosen link and on an outside click.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import LayoutHeaderDropdown from '~/components/layout/LayoutHeaderDropdown.vue'
import type { HeaderNavItem } from '~/types/headernavitem'

/** A two-link group. */
const item: HeaderNavItem = {
  key: 'support',
  label: 'Support',
  children: [
    { key: 'help', label: 'Help center', desc: 'Guides', to: '/en/knowledgebase' },
    { key: 'faq', label: 'FAQ', to: '/en/faq' },
  ],
}

/**
 * Mounts the dropdown in the document so outside clicks and focus work.
 *
 * @param path - The current page path.
 * @returns The wrapper.
 */
const mountDropdown = (path = '/en/') => mountWithKit(LayoutHeaderDropdown, { props: { item, path }, attachToBody: true })

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

  it('marks the current link and tints the trigger when a child is the page', async () => {
    const wrapper = mountDropdown('/en/faq')
    await wrapper.find('[data-testid="header-nav-support-trigger"]').trigger('click')

    expect(wrapper.find('[data-testid="header-nav-child-faq"]').attributes('aria-current')).toBe('page')
    expect(wrapper.find('[data-testid="header-nav-child-help"]').attributes('aria-current')).toBeUndefined()
    expect(wrapper.find('[data-testid="header-nav-support-trigger"]').classes()).toContain('text-classic-brand')
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
})
