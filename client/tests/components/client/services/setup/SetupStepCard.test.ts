/**
 * Unit tests for SetupStepCard: the title block and the slot.
 */
import { describe, expect, it } from 'vitest'
import { Globe } from 'lucide-vue-next'
import { mountClient } from '~/tests/helpers/mountClient'
import SetupStepCard from '~/components/client/services/setup/SetupStepCard.vue'

describe('SetupStepCard', () => {
  it('shows the title, the subtitle and the icon tile', () => {
    const wrapper = mountClient(SetupStepCard, { props: { icon: Globe, title: 'Your domain', subtitle: 'Pick one' } })

    expect(wrapper.find('h2').text()).toBe('Your domain')
    expect(wrapper.text()).toContain('Pick one')
    expect(wrapper.find('[data-testid="ui-tile"]').exists()).toBe(true)
  })

  it('renders the fields it is given', () => {
    const wrapper = mountClient(SetupStepCard, { props: { icon: Globe, title: 't', subtitle: 's' }, slots: { default: '<span data-testid="field">x</span>' } })

    expect(wrapper.find('[data-testid="field"]').exists()).toBe(true)
  })
})
