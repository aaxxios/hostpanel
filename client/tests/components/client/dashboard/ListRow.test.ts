/**
 * Unit tests for ClientDashboardListRow: a clickable dashboard list row.
 *
 * @module tests/components/client/dashboard/ListRow.test
 */

import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import ListRow from '~/components/client/dashboard/ListRow.vue'

describe('ClientDashboardListRow', () => {
  it('links to the given route and renders its content', () => {
    const wrapper = mountClient(ListRow, { props: { to: '/client/invoices/7', testid: 'row-7' }, slots: { default: 'Invoice 7' } })

    const row = wrapper.find('[data-testid="row-7"]')
    expect(row.attributes('href')).toBe('/client/invoices/7')
    expect(row.text()).toContain('Invoice 7')
  })
})
