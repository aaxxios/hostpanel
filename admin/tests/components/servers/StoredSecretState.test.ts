/**
 * Unit tests for StoredSecretState: the stored line with Clear, the pending-clear line with Cancel,
 * nothing at all when no value is stored, and the disabled state while a save runs.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import StoredSecretState from '@/components/servers/StoredSecretState.vue'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Mounts the stored-state line of the password field.
 *
 * @param overrides - Props to change.
 * @returns The wrapper.
 */
const mountState = (overrides: Record<string, unknown> = {}) =>
  mount(StoredSecretState, {
    props: { hasValue: true, cleared: false, testid: 'server-password', ...overrides },
    global: { plugins: [createTestI18n()] },
  })

describe('StoredSecretState', () => {
  it('says a value is stored and asks the parent to clear it', async () => {
    const wrapper = mountState()

    expect(wrapper.get('[data-testid="server-password-stored"]').text()).toBe('••••••, set')
    await wrapper.get('[data-testid="server-password-clear"]').trigger('click')

    expect(wrapper.emitted('clear')).toHaveLength(1)
    expect(wrapper.find('[data-testid="server-password-pending-clear"]').exists()).toBe(false)
  })

  it('says the value is cleared on save and lets the operator take it back', async () => {
    const wrapper = mountState({ cleared: true })

    expect(wrapper.get('[data-testid="server-password-pending-clear"]').text()).toBe('Cleared when you save')
    expect(wrapper.find('[data-testid="server-password-clear"]').exists()).toBe(false)
    await wrapper.get('[data-testid="server-password-cancel-clear"]').trigger('click')

    expect(wrapper.emitted('cancel')).toHaveLength(1)
  })

  it('renders nothing when no value is stored', () => {
    const wrapper = mountState({ hasValue: false })

    expect(wrapper.find('[data-testid="server-password-stored"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="server-password-pending-clear"]').exists()).toBe(false)
  })

  it('blocks both actions while disabled', () => {
    expect(mountState({ disabled: true }).get('[data-testid="server-password-clear"]').attributes('disabled')).toBeDefined()
    expect(mountState({ cleared: true, disabled: true }).get('[data-testid="server-password-cancel-clear"]').attributes('disabled')).toBeDefined()
  })
})
