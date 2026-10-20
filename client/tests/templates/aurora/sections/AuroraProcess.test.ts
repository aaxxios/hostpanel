/**
 * Unit tests for AuroraProcess: the four numbered steps beside the section headline.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraProcess from '~/templates/aurora/sections/AuroraProcess.vue'
import { stubAuroraGlobals } from '../auroraGlobals'

describe('AuroraProcess', () => {
  it('renders the four steps in order with their copy', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraProcess)

    for (const n of ['01', '02', '03', '04']) {
      expect(wrapper.find(`[data-testid="process-step-${n}"]`).exists()).toBe(true)
    }
    expect(wrapper.find('[data-testid="process-step-02"]').text()).toContain('aurora.process.step2Title')
    expect(wrapper.find('[data-testid="process-step-02"]').text()).toContain('aurora.process.step2Body')
  })

  it('renders the headline and the anchor id the header links to', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraProcess)

    expect(wrapper.find('h2').text()).toBe('aurora.process.title')
    expect(wrapper.attributes('id')).toBe('process')
  })

  it('keeps the intro column sticky from lg up and shows both calls to action', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraProcess)

    expect(wrapper.find('[data-testid="process-aside"]').classes()).toEqual(expect.arrayContaining(['lg:sticky', 'lg:self-start']))
    expect(wrapper.find('[data-testid="process-start"]').text()).toBe('aurora.process.start')
    expect(wrapper.find('[data-testid="process-start"]').attributes('href')).toBe('/hosting')
    expect(wrapper.find('[data-testid="process-talk"]').attributes('href')).toBe('/contact')
    expect(wrapper.find('[data-testid="process-note"]').text()).toBe('aurora.process.note')
  })
})
