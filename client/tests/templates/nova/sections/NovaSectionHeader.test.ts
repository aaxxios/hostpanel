/**
 * Unit tests for NovaSectionHeader: eyebrow, h2, optional lead and the badge slot.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaSectionHeader from '~/templates/nova/sections/NovaSectionHeader.vue'

describe('NovaSectionHeader', () => {
  it('renders the eyebrow, an h2 and the lead', () => {
    const wrapper = mountWithKit(NovaSectionHeader, { props: { eyebrow: 'Plans', title: 'Pick one', lead: 'Any time.' } })

    expect(wrapper.find('h2').text()).toBe('Pick one')
    expect(wrapper.text()).toContain('Plans')
    expect(wrapper.text()).toContain('Any time.')
  })

  it('renders no lead paragraph when none is given', () => {
    const wrapper = mountWithKit(NovaSectionHeader, { props: { eyebrow: 'Plans', title: 'Pick one' } })

    expect(wrapper.findAll('p')).toHaveLength(0)
  })

  it('renders the badge slot beside the eyebrow', () => {
    const wrapper = mountWithKit(NovaSectionHeader, {
      props: { eyebrow: 'Migration', title: 'Move' },
      slots: { badge: '<span data-testid="badge">Free</span>' },
    })

    expect(wrapper.find('[data-testid="badge"]').text()).toBe('Free')
  })
})
