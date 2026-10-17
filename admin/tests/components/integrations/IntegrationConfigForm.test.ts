/**
 * Mount tests for IntegrationConfigForm: one control per described field, edits go into the save
 * payload (toggles as "true"/"false"), and the test button asks the parent to test.
 */
import { describe, expect, it } from 'vitest'
import IntegrationConfigForm from '@/components/integrations/IntegrationConfigForm.vue'
import { mountWithApp } from '../../helpers/mount'
import { integrationDetail } from '../../helpers/integrationFixtures'

/**
 * Mounts the form for the default detail.
 *
 * @param props - Props to change.
 * @returns The wrapper.
 */
const mountForm = (props: Record<string, unknown> = {}) =>
  mountWithApp(IntegrationConfigForm, { props: { integration: integrationDetail(), loading: false, testing: false, ...props } })

describe('IntegrationConfigForm', () => {
  it('renders one control per field definition', () => {
    const wrapper = mountForm()

    expect(wrapper.find('[data-testid="integration-field-secretKey"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="integration-field-mode"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="integration-field-note"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="integration-field-live"]').exists()).toBe(true)
  })

  it('emits the edited values and the toggled switch on save', async () => {
    const wrapper = mountForm()

    await wrapper.get('[data-testid="integration-field-secretKey"]').setValue('sk_live_new')
    await wrapper.get('[data-testid="integration-field-live"]').trigger('click')
    await wrapper.get('[data-testid="integration-save"]').trigger('click')

    const payload = wrapper.emitted('save')?.[0]?.[0] as { isEnabled: boolean; config: Record<string, string> }
    expect(payload.isEnabled).toBe(true)
    expect(payload.config.secretKey).toBe('sk_live_new')
    expect(payload.config.live).toBe('true')
    expect(payload.config.note).toBe('hello')
  })

  it('emits the enabled switch state', async () => {
    const wrapper = mountForm()

    await wrapper.get('[data-testid="integration-enabled"]').trigger('click')
    await wrapper.get('[data-testid="integration-save"]').trigger('click')

    const payload = wrapper.emitted('save')?.[0]?.[0] as { isEnabled: boolean }
    expect(payload.isEnabled).toBe(false)
  })

  it('emits test from the test button', async () => {
    const wrapper = mountForm()

    await wrapper.get('[data-testid="integration-test"]').trigger('click')

    expect(wrapper.emitted('test')).toHaveLength(1)
  })

  it('blocks both actions while the other one runs', () => {
    const saving = mountForm({ loading: true })
    expect(saving.get('[data-testid="integration-test"]').attributes('disabled')).toBeDefined()

    const testing = mountForm({ testing: true })
    expect(testing.get('[data-testid="integration-save"]').attributes('disabled')).toBeDefined()
  })
})
