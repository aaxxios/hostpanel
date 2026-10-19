/**
 * Unit tests for SectionsProcess: one card per step, numbered in order, with the translated copy.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SectionsProcess from '~/components/sections/SectionsProcess.vue'
import MarketingSectionHeader from '~/components/marketing/MarketingSectionHeader.vue'
import { PROCESS_STEPS as processSteps } from '~/constants/processStep'

/** Mounts the section. */
const mountProcess = () => mountWithKit(SectionsProcess, { components: { MarketingSectionHeader } })

describe('SectionsProcess', () => {
  it('renders a card per step', () => {
    expect(mountProcess().findAll('[data-testid^="process-step-"]')).toHaveLength(processSteps.length)
  })

  it('numbers each step with two digits and shows its translated title and description', () => {
    const wrapper = mountProcess()
    const first = processSteps[0]

    const card = wrapper.find(`[data-testid="process-step-${first?.id}"]`)
    expect(card.text()).toContain(String(first?.order).padStart(2, '0'))
    expect(card.text()).toContain(`process.steps.${first?.id}.title`)
    expect(card.text()).toContain(`process.steps.${first?.id}.description`)
  })

  it('shows the section header', () => {
    const wrapper = mountProcess()

    expect(wrapper.text()).toContain('process.badge')
    expect(wrapper.text()).toContain('process.headingHighlight')
    expect(wrapper.text()).toContain('process.description')
  })
})
