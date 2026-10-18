/**
 * Unit tests for ClientAuthLogo: the home link around the logo.
 *
 * @module tests/components/client/AuthLogo.test
 */

import { describe, expect, it } from 'vitest'
import { mountWithLogo } from '~/tests/helpers/authScreens'
import AuthLogo from '~/components/client/AuthLogo.vue'

describe('ClientAuthLogo', () => {
  it('links the logo to the home page', () => {
    const wrapper = mountWithLogo(AuthLogo)

    const link = wrapper.find('[data-testid="auth-logo"]')
    expect(link.attributes('href')).toBe('/')
    expect(link.find('[data-testid="app-logo"]').exists()).toBe(true)
  })
})
