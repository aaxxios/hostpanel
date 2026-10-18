/**
 * Unit tests for ClientOutcomePanel: icon square, heading, message and action slot.
 *
 * @module tests/components/client/OutcomePanel.test
 */

import { describe, expect, it } from 'vitest'
import { Check } from 'lucide-vue-next'
import { mountClient } from '~/tests/helpers/mountClient'
import OutcomePanel from '~/components/client/OutcomePanel.vue'
import { UiHeadingLevel } from '~/utils/ui/enums'

describe('ClientOutcomePanel', () => {
  it('renders the title as a level-2 heading by default', () => {
    const wrapper = mountClient(OutcomePanel, { props: { icon: Check, title: 'Done' } })

    expect(wrapper.find('h2').text()).toBe('Done')
  })

  it('renders the title as the requested heading level', () => {
    const wrapper = mountClient(OutcomePanel, { props: { icon: Check, title: 'Done', headingLevel: UiHeadingLevel.H1 } })

    expect(wrapper.find('h1').text()).toBe('Done')
    expect(wrapper.find('h2').exists()).toBe(false)
  })

  it('renders the message only when given', () => {
    const without = mountClient(OutcomePanel, { props: { icon: Check, title: 'Done' } })
    const withMessage = mountClient(OutcomePanel, { props: { icon: Check, title: 'Done', message: 'All good' } })

    expect(without.text()).not.toContain('All good')
    expect(withMessage.text()).toContain('All good')
  })

  it('tints the icon square by tone', () => {
    const wrapper = mountClient(OutcomePanel, { props: { icon: Check, title: 'Done', tone: 'success' } })

    expect(wrapper.find('svg').element.parentElement?.className).toContain('bg-green-500/10')
  })

  it('renders the action slot below the message', () => {
    const wrapper = mountClient(OutcomePanel, { props: { icon: Check, title: 'Done' }, slots: { default: 'Go on' } })

    expect(wrapper.text()).toContain('Go on')
  })
})
