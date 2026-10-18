/**
 * Unit tests for CardHeader: the heading, the optional icon and the trailing slot.
 */
import { describe, expect, it } from 'vitest'
import { Settings } from 'lucide-vue-next'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import CardHeader from '~/components/ui/UiCardHeader.vue'

describe('CardHeader', () => {
  it('renders the title as an h2', () => {
    const wrapper = mountWithKit(CardHeader, { props: { title: 'Profile' } })

    expect(wrapper.find('h2').text()).toBe('Profile')
  })

  it('renders no icon without one', () => {
    const wrapper = mountWithKit(CardHeader, { props: { title: 'Profile' } })

    expect(wrapper.find('svg').exists()).toBe(false)
  })

  it('renders the icon component before the title', () => {
    const wrapper = mountWithKit(CardHeader, { props: { title: 'Profile', icon: Settings } })

    expect(wrapper.find('h2 svg').exists()).toBe(true)
  })

  it('renders the actions slot', () => {
    const wrapper = mountWithKit(CardHeader, { props: { title: 'Profile' }, slots: { default: 'Edit' } })

    expect(wrapper.text()).toContain('Edit')
  })
})
