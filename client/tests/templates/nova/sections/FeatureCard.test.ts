/**
 * Unit tests for FeatureCard: a list item with an icon, a title and an optional body.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import FeatureCard from '~/templates/nova/sections/FeatureCard.vue'

describe('FeatureCard', () => {
  it('is a list item showing the icon, title key and body key', () => {
    const wrapper = mountWithKit(FeatureCard, {
      props: { feature: { key: 'ssl', icon: 'lucide:lock', titleKey: 'nova.t', bodyKey: 'nova.b' } },
    })

    expect(wrapper.element.tagName).toBe('LI')
    expect(wrapper.find('[data-icon="lucide:lock"]').exists()).toBe(true)
    expect(wrapper.find('h3').text()).toBe('nova.t')
    expect(wrapper.text()).toContain('nova.b')
  })

  it('renders no body paragraph when the feature has none', () => {
    const wrapper = mountWithKit(FeatureCard, { props: { feature: { key: 'ssl', icon: 'lucide:lock', titleKey: 'nova.t' } } })

    expect(wrapper.findAll('p')).toHaveLength(0)
  })
})
