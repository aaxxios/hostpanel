/**
 * Unit tests for ProfileView: read-only details, dashes for empty values, the language row.
 */
import { describe, expect, it } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import ProfileView from '~/components/account/ProfileView.vue'
import type { ClientUser } from '~/types/clientuser'
import { decodeEmailPrefs } from '~/utils/emailPreferences'

/** A profile with a company and no address. */
const user: ClientUser = {
  id: 1, firstname: 'Jane', lastname: 'Doe', companyname: 'Acme', email: 'jane@example.com', permissions: 0, countryname: 'Armenia'
}

/**
 * Mounts the view with the given overrides.
 *
 * @param overrides - Props that replace the defaults.
 * @returns The wrapper.
 */
const mountView = (overrides: Record<string, unknown> = {}) =>
  mountAccount(ProfileView, {
    props: {
      user,
      paymentMethodLabel: 'Card',
      languageLabel: 'English',
      ownsLanguage: true,
      emailPrefs: decodeEmailPrefs(undefined),
      ...overrides
    }
  })

describe('ProfileView', () => {
  it('shows the identity values', () => {
    const text = mountView().text()

    expect(text).toContain('Jane')
    expect(text).toContain('Acme')
    expect(text).toContain('Card')
    expect(text).toContain('Armenia')
  })

  it('shows a dash for an empty address field', () => {
    expect(mountView().text()).toContain('—')
  })

  it('shows the language row when the deployment owns the language', () => {
    expect(mountView().text()).toContain('English')
  })

  it('hides the language row under SSO', () => {
    expect(mountView({ ownsLanguage: false }).text()).not.toContain('English')
  })

  it('renders the email preferences read-only', () => {
    const wrapper = mountView()

    expect(wrapper.find('[data-testid="account-email-prefs"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="account-pref-general"]').attributes('disabled')).toBeDefined()
  })
})
