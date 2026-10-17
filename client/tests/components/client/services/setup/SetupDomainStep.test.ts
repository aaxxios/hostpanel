/**
 * Unit tests for SetupDomainStep: the editable and the locked domain, the error and the Continue button.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import SetupDomainStep from '~/components/client/services/setup/SetupDomainStep.vue'

/**
 * Mounts the step with defaults.
 *
 * @param props - Props to override.
 * @returns The wrapper.
 */
const mountStep = (props: Record<string, unknown> = {}) =>
  mountClient(SetupDomainStep, { props: { domain: 'example.com', domainError: '', hasPurchasedDomain: false, ...props } })

describe('SetupDomainStep', () => {
  it('lets the client edit a domain that was not bought with the plan', async () => {
    const wrapper = mountStep()

    await wrapper.find('[data-testid="setup-domain"]').setValue('other.org')

    expect(wrapper.emitted('update:domain')).toEqual([['other.org']])
    expect(wrapper.text()).toContain('client.setup.domainHintNew')
  })

  it('locks the domain bought with the plan', () => {
    const wrapper = mountStep({ hasPurchasedDomain: true })

    expect(wrapper.find('[data-testid="setup-domain"]').attributes('disabled')).toBeDefined()
    expect(wrapper.text()).toContain('client.setup.domainHintPurchased')
  })

  it('shows the error of an editable domain but not of a locked one', () => {
    const editable = mountStep({ domainError: 'Not a domain' })
    const locked = mountStep({ domainError: 'Not a domain', hasPurchasedDomain: true })

    expect(editable.text()).toContain('Not a domain')
    expect(locked.text()).not.toContain('Not a domain')
  })

  it('disables Continue without a domain or with an error', () => {
    expect(mountStep({ domain: '' }).find('[data-testid="setup-domain-next"]').attributes('disabled')).toBeDefined()
    expect(mountStep({ domainError: 'bad' }).find('[data-testid="setup-domain-next"]').attributes('disabled')).toBeDefined()
    expect(mountStep().find('[data-testid="setup-domain-next"]').attributes('disabled')).toBeUndefined()
  })

  it('asks for the next step', async () => {
    const wrapper = mountStep()

    await wrapper.find('[data-testid="setup-domain-next"]').trigger('click')

    expect(wrapper.emitted('next')).toHaveLength(1)
  })
})
