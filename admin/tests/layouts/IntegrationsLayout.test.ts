/**
 * Mount tests for IntegrationsLayout: the category sidebar sits beside the routed page.
 */
import { describe, expect, it } from 'vitest'
import IntegrationsLayout from '@/layouts/IntegrationsLayout.vue'
import { mountView } from '../helpers/mount'

describe('IntegrationsLayout', () => {
  it('renders the category sidebar inside the layout', async () => {
    const wrapper = await mountView(IntegrationsLayout, { path: '/integrations/email' })

    expect(wrapper.find('[data-testid="integrations-layout"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="integrations-nav-email"]').exists()).toBe(true)
  })
})
