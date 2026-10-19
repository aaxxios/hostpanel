/**
 * Unit tests for PluginCard: name, version and status, and the remove event.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import PluginCard from '@/components/plugins/PluginCard.vue'
import { createTestI18n } from '../../helpers/i18n'

/** An installed plugin that is already running. */
const plugin = {
  id: 'ineco',
  name: 'Ineco Pay',
  version: '1.2.0',
  author: 'Hostpanel',
  description: 'Card payments',
  type: 'payment',
  category: 'Payments',
  color: '#123456',
  isLoaded: true,
}

/**
 * Mounts the card.
 *
 * @param overrides - Props to change.
 * @returns The wrapper.
 */
const mountCard = (overrides: Record<string, unknown> = {}) =>
  mount(PluginCard, { props: { plugin, removing: false, ...overrides }, global: { plugins: [createTestI18n()] } })

describe('PluginCard', () => {
  it('shows the plugin and its active status', () => {
    const wrapper = mountCard()

    expect(wrapper.text()).toContain('Ineco Pay')
    expect(wrapper.text()).toContain('v1.2.0')
    expect(wrapper.text()).toContain('Active')
  })

  it('marks a plugin that is not loaded yet as pending restart', () => {
    const wrapper = mountCard({ plugin: { ...plugin, isLoaded: false } })

    expect(wrapper.text()).toContain('Pending restart')
  })

  it('emits remove with the plugin id', async () => {
    const wrapper = mountCard()

    await wrapper.get('[data-testid="plugin-remove-ineco"]').trigger('click')

    expect(wrapper.emitted('remove')).toEqual([['ineco']])
  })

  it('disables the remove button while removing', () => {
    const wrapper = mountCard({ removing: true })

    expect(wrapper.get('[data-testid="plugin-remove-ineco"]').attributes('disabled')).toBeDefined()
    expect(wrapper.text()).toContain('Removing')
  })
})
