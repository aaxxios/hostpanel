/**
 * Unit tests for ClientNoProfileNotice: the explanation shown for an account that is not a
 * customer, read from the client store.
 *
 * @module tests/components/client/NoProfileNotice.test
 */

import { describe, expect, it } from 'vitest'
import { nextTick } from 'vue'
import { mountClient } from '~/tests/helpers/mountClient'
import NoProfileNotice from '~/components/client/NoProfileNotice.vue'
import { useClientStore } from '~/stores/client'

describe('ClientNoProfileNotice', () => {
  it('shows the API sentence the store kept', async () => {
    const wrapper = mountClient(NoProfileNotice)
    useClientStore().clientProfileMessage = 'No customer record for this account.'
    await nextTick()

    expect(wrapper.find('[data-testid="no-profile-notice"]').text()).toContain('No customer record for this account.')
  })

  it('falls back to the local sentence when the API sent none', () => {
    const wrapper = mountClient(NoProfileNotice)

    expect(wrapper.text()).toContain('client.noProfile.body')
  })

  it('offers the admin panel and the hosting catalogue as ways out', () => {
    const wrapper = mountClient(NoProfileNotice)

    expect(wrapper.find('[data-testid="no-profile-admin"]').attributes('href')).toBe('/admin')
    expect(wrapper.find('[data-testid="no-profile-browse"]').attributes('href')).toBe('/hosting')
  })

  it('is an info notice, not an error', () => {
    const wrapper = mountClient(NoProfileNotice)

    expect(wrapper.find('[data-testid="no-profile-notice"]').classes().join(' ')).toContain('blue')
  })
})
