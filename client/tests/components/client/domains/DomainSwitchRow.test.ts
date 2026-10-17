/**
 * Unit tests for DomainSwitchRow: the state-dependent name, the description and the asked value.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import DomainSwitchRow from '~/components/client/domains/DomainSwitchRow.vue'

/** Props every mount needs. */
const base = { onLabel: 'Lock enabled', offLabel: 'Lock disabled', description: 'Prevents transfers', toggleLabel: 'Registrar lock', testid: 'row' }

describe('DomainSwitchRow', () => {
  it('names the setting by its state', () => {
    const on = mountClient(DomainSwitchRow, { props: { ...base, enabled: true } })
    const off = mountClient(DomainSwitchRow, { props: { ...base, enabled: false } })

    expect(on.text()).toContain('Lock enabled')
    expect(off.text()).toContain('Lock disabled')
    expect(on.text()).toContain('Prevents transfers')
  })

  it('asks for the opposite value when the switch is clicked', async () => {
    const wrapper = mountClient(DomainSwitchRow, { props: { ...base, enabled: false } })

    await wrapper.find('[data-testid="row-toggle"]').trigger('click')

    expect(wrapper.emitted('toggle')).toEqual([[true]])
  })

  it('does not ask while the change is in flight', async () => {
    const wrapper = mountClient(DomainSwitchRow, { props: { ...base, enabled: false, saving: true } })

    await wrapper.find('[data-testid="row-toggle"]').trigger('click')

    expect(wrapper.emitted('toggle')).toBeUndefined()
  })
})
