/**
 * Unit tests for ServiceConfigOptions: one line per chosen option.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import ServiceConfigOptions from '~/components/client/services/ServiceConfigOptions.vue'

describe('ServiceConfigOptions', () => {
  it('lists every option with its value', () => {
    const options = [{ id: 1, option: 'Location', value: 'Frankfurt' }, { id: 2, option: 'Backups', value: 'Daily' }]
    const wrapper = mountClient(ServiceConfigOptions, { props: { options } })

    expect(wrapper.findAll('[data-testid="ui-key-value"]')).toHaveLength(2)
    expect(wrapper.text()).toContain('Location')
    expect(wrapper.text()).toContain('Frankfurt')
    expect(wrapper.text()).toContain('Daily')
  })
})
