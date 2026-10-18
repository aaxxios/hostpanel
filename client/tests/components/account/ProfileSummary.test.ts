/**
 * Unit tests for ProfileSummary: identity line and the two-factor badge.
 */
import { describe, expect, it } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import ProfileSummary from '~/components/account/ProfileSummary.vue'
import type { ClientUser } from '~/types/clientuser'

/** A minimal profile. */
const user: ClientUser = { id: 1, firstname: 'Jane', lastname: 'Doe', email: 'jane@example.com', permissions: 0 }

describe('ProfileSummary', () => {
  it('shows the full name and email', () => {
    const wrapper = mountAccount(ProfileSummary, { props: { user, twoFactorEnabled: false } })

    expect(wrapper.find('[data-testid="account-full-name"]').text()).toBe('Jane Doe')
    expect(wrapper.text()).toContain('jane@example.com')
  })

  it('says two-factor is enabled', () => {
    const wrapper = mountAccount(ProfileSummary, { props: { user, twoFactorEnabled: true } })

    expect(wrapper.find('[data-testid="account-2fa-badge"]').text()).toBe('client.twoFactor.enabled')
  })

  it('says two-factor is disabled', () => {
    const wrapper = mountAccount(ProfileSummary, { props: { user, twoFactorEnabled: false } })

    expect(wrapper.find('[data-testid="account-2fa-badge"]').text()).toBe('client.twoFactor.disabled')
  })
})
