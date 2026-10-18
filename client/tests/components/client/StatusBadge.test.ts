/**
 * Unit tests for ClientStatusBadge: the localised status word in the status table's tone.
 *
 * @module tests/components/client/StatusBadge.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountClient } from '~/tests/helpers/mountClient'
import StatusBadge from '~/components/client/StatusBadge.vue'

/**
 * Makes the translator know (or not know) the status catalogue.
 *
 * @param known - Whether `te` finds the `client.status.*` keys.
 */
const stubTranslator = (known: boolean): void => {
  vi.stubGlobal('useI18n', () => ({
    t: (key: string) => `translated:${key}`,
    te: () => known,
    locale: ref('en')
  }))
}

afterEach(() => {
  stubTranslator(false)
})

describe('ClientStatusBadge', () => {
  it('shows the translated word when the catalogue names the status', () => {
    stubTranslator(true)

    const wrapper = mountClient(StatusBadge, { props: { status: 'Active' } })

    expect(wrapper.find('[data-testid="status-badge"]').text()).toBe('translated:client.status.Active')
  })

  it('shows the status as the API sent it when the catalogue does not name it', () => {
    stubTranslator(false)

    const wrapper = mountClient(StatusBadge, { props: { status: 'Quarantined' } })

    expect(wrapper.find('[data-testid="status-badge"]').text()).toBe('Quarantined')
  })

  it.each([
    ['Active', 'bg-green-100'],
    ['Unpaid', 'bg-yellow-100'],
    ['Overdue', 'bg-red-100'],
    ['Open', 'bg-blue-100'],
    ['Cancelled', 'bg-gray-100']
  ])('paints %s with %s', (status, toneClass) => {
    const wrapper = mountClient(StatusBadge, { props: { status } })

    expect(wrapper.find('[data-testid="status-badge"]').classes()).toContain(toneClass)
  })

  it('paints an unknown status in the neutral tone', () => {
    const wrapper = mountClient(StatusBadge, { props: { status: 'Quarantined' } })

    expect(wrapper.find('[data-testid="status-badge"]').classes()).toContain('bg-gray-100')
  })
})
