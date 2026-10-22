/**
 * Unit tests for TrialDetails: the feature lines, the three perks and the closing trial button.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import TrialDetails from '~/components/catalog/TrialDetails.vue'
import { TrialPerk } from '~/types/trialperk'

/**
 * Mounts the details.
 *
 * @param features The feature lines.
 * @returns The wrapper.
 */
const mountDetails = (features: string[] = ['Boards', 'Reports']) =>
  mountWithKit(TrialDetails, { props: { name: 'Gamma Tool', color: '#10b981', trialUrl: '/products/gamma-tool', features } })

describe('TrialDetails', () => {
  it('lists the product features', () => {
    const wrapper = mountDetails()

    expect(wrapper.find('[data-testid="trial-features"]').text()).toContain('Boards')
    expect(wrapper.find('[data-testid="trial-features"]').text()).toContain('Reports')
  })

  it('hides the feature section when the product has none', () => {
    expect(mountDetails([]).find('[data-testid="trial-features"]').exists()).toBe(false)
  })

  it('shows one card per perk', () => {
    const wrapper = mountDetails()

    for (const perk of Object.values(TrialPerk)) {
      const card = wrapper.find(`[data-testid="trial-perk-${perk}"]`)
      expect(card.text()).toContain(`trial.perks.${perk}.title`)
      expect(card.text()).toContain(`trial.perks.${perk}.desc`)
    }
  })

  it('wears the classic look under the classic template, and the brand accent without a colour', () => {
    const wrapper = mountDetails()
    const classes = wrapper.find('[data-testid="trial-perks"]').classes().join(' ')

    expect(classes).toContain('[.tpl-classic_&]:bg-classic-page')
    expect(classes).not.toContain('--brand-primary-500')
  })

  it('offers the trial again at the bottom', () => {
    const cta = mountDetails().find('[data-testid="trial-cta-bottom"]')

    expect(cta.attributes('href')).toBe('/products/gamma-tool')
    expect(cta.attributes('target')).toBe('_blank')
  })
})
