/**
 * Unit tests for UiDrawer: shown only when open, closing paths, edge and footer.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiDrawer from '@/components/ui/UiDrawer.vue'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Mounts the drawer with the teleport inlined.
 *
 * @param props - Props for the drawer.
 * @param slots - Slots for the drawer.
 * @returns The wrapper.
 */
const mountDrawer = (props: InstanceType<typeof UiDrawer>['$props'], slots: Record<string, string> = {}) =>
  mount(UiDrawer, { props, slots, global: { plugins: [createTestI18n()], stubs: { teleport: true } } })

describe('UiDrawer', () => {
  it('renders a labelled modal panel when open', () => {
    const wrapper = mountDrawer({ open: true, title: 'Filters' }, { default: '<p data-testid="body" />' })

    expect(wrapper.get('[role="dialog"]').attributes('aria-modal')).toBe('true')
    expect(wrapper.get('h2').text()).toBe('Filters')
    expect(wrapper.find('[data-testid="body"]').exists()).toBe(true)
  })

  it('renders nothing when closed', () => {
    expect(mountDrawer({ open: false, title: 'Filters' }).find('[role="dialog"]').exists()).toBe(false)
  })

  it('closes from the button, Escape and the backdrop', async () => {
    const wrapper = mountDrawer({ open: true, title: 'Filters' })

    await wrapper.get('[data-testid="drawer-close"]').trigger('click')
    await wrapper.get('[role="dialog"]').trigger('keydown', { key: 'Escape' })
    await wrapper.get('.fixed').trigger('click')

    expect(wrapper.emitted('close')).toHaveLength(3)
  })

  it('attaches to the chosen edge', () => {
    expect(mountDrawer({ open: true, title: 'x' }).get('aside').classes()).toContain('right-0')
    expect(mountDrawer({ open: true, title: 'x', side: 'left' }).get('aside').classes()).toContain('left-0')
  })

  it('renders the footer slot only when given', () => {
    expect(mountDrawer({ open: true, title: 'x' }).find('.border-t').exists()).toBe(false)
    expect(mountDrawer({ open: true, title: 'x' }, { footer: '<b data-testid="f" />' }).find('[data-testid="f"]').exists()).toBe(true)
  })
})
