/**
 * Unit tests for SetupCredentialsStep: the cleaned username, the errors and the two buttons.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import SetupCredentialsStep from '~/components/client/services/setup/SetupCredentialsStep.vue'

/**
 * Mounts the step with defaults.
 *
 * @param props - Props to override.
 * @returns The wrapper.
 */
const mountStep = (props: Record<string, unknown> = {}) =>
  mountClient(SetupCredentialsStep, { props: { username: 'web', password: 'longenough', usernameError: '', passwordError: '', canContinue: true, ...props } })

describe('SetupCredentialsStep', () => {
  it('cleans the username as it is typed', async () => {
    const wrapper = mountStep()

    await wrapper.find('[data-testid="setup-username"]').setValue('Web-Master 9!')

    expect(wrapper.emitted('update:username')).toEqual([['webmaster9']])
  })

  it('passes the password on as typed', async () => {
    const wrapper = mountStep()

    await wrapper.find('[data-testid="setup-password"]').setValue('Secret-pass')

    expect(wrapper.emitted('update:password')).toEqual([['Secret-pass']])
  })

  it('shows the errors it is given', () => {
    const wrapper = mountStep({ usernameError: 'Too short', passwordError: 'Too weak' })

    expect(wrapper.text()).toContain('Too short')
    expect(wrapper.text()).toContain('Too weak')
  })

  it('disables Continue until the values are acceptable', () => {
    expect(mountStep({ canContinue: false }).find('[data-testid="setup-credentials-next"]').attributes('disabled')).toBeDefined()
    expect(mountStep().find('[data-testid="setup-credentials-next"]').attributes('disabled')).toBeUndefined()
  })

  it('goes back and forward', async () => {
    const wrapper = mountStep()

    await wrapper.find('[data-testid="setup-credentials-back"]').trigger('click')
    await wrapper.find('[data-testid="setup-credentials-next"]').trigger('click')

    expect(wrapper.emitted('back')).toHaveLength(1)
    expect(wrapper.emitted('next')).toHaveLength(1)
  })
})
