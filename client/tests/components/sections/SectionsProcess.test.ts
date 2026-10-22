/**
 * Unit tests for SectionsProcess: one card per step, numbered in order, with the translated copy.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SectionsProcess from '~/components/sections/SectionsProcess.vue'
import { CLASSIC_COMPONENTS } from '~/tests/helpers/classicKit'
import { PROCESS_STEPS as processSteps } from '~/constants/processStep'
import { HOME_SECTION_SCROLL_MARGIN_CLASS } from '~/constants/homeSections'
import { HomeSectionId } from '~/types/homesectionid'

/** Mounts the section. */
const mountProcess = () => mountWithKit(SectionsProcess, { components: CLASSIC_COMPONENTS })

describe('SectionsProcess', () => {
  it('carries the process anchor the header links to, with room above it', () => {
    const section = mountProcess().find('[data-testid="sections-process"]')

    expect(section.attributes('id')).toBe(HomeSectionId.Process)
    expect(section.classes()).toContain(HOME_SECTION_SCROLL_MARGIN_CLASS)
  })

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
    expect(wrapper.text()).toContain('process.title')
    expect(wrapper.text()).toContain('process.description')
  })

  it('shows each step icon on a flat brand tile', () => {
    const first = processSteps[0]

    const tile = mountProcess().find(`[data-testid="process-step-${first?.id}"] [data-testid="classic-icon-tile"]`)
    expect(tile.exists()).toBe(true)
    expect(tile.find(`[data-icon="${first?.icon}"]`).exists()).toBe(true)
  })
})
