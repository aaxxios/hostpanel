/**
 * Unit tests for ClientSummaryRow: a label/value line with a replaceable value.
 *
 * @module tests/components/client/SummaryRow.test
 */

import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import SummaryRow from '~/components/client/SummaryRow.vue'

describe('ClientSummaryRow', () => {
  it('shows the label and the value', () => {
    const wrapper = mountClient(SummaryRow, { props: { label: 'Order', value: '1234' }, attrs: { 'data-testid': 'row' } })

    expect(wrapper.text()).toContain('Order')
    expect(wrapper.text()).toContain('1234')
  })

  it('replaces the value with the slot', () => {
    const wrapper = mountClient(SummaryRow, { props: { label: 'Payment', value: 'ignored' }, slots: { default: 'Paid badge' } })

    expect(wrapper.text()).toContain('Paid badge')
    expect(wrapper.text()).not.toContain('ignored')
  })

  it('carries the test id on the row', () => {
    const wrapper = mountClient(SummaryRow, { props: { label: 'Order', value: '1', testid: 'order-row' } })

    expect(wrapper.find('[data-testid="order-row"]').exists()).toBe(true)
  })
})
