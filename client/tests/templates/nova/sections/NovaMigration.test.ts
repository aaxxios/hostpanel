/**
 * Unit tests for NovaMigration: the ordered steps and the operator-gated "free" badge.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaMigration from '~/templates/nova/sections/NovaMigration.vue'
import { MIGRATION_FREE_SETTING, MIGRATION_STEPS } from '~/templates/nova/content'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'

describe('NovaMigration', () => {
  it('lists the steps in an ordered list, numbered from one', () => {
    stubTemplateGlobals()
    const wrapper = mountWithKit(NovaMigration)
    const steps = wrapper.find('ol').findAll('li')

    expect(steps).toHaveLength(MIGRATION_STEPS.length)
    expect(steps[0]!.text()).toContain('1')
    expect(steps[0]!.text()).toContain(MIGRATION_STEPS[0]!.titleKey)
  })

  it('shows no free badge until the operator switches it on', () => {
    stubTemplateGlobals()

    expect(mountWithKit(NovaMigration).text()).not.toContain('nova.migration.free')
  })

  it('shows the free badge when the setting is true', () => {
    stubTemplateGlobals({ settings: { [MIGRATION_FREE_SETTING]: 'true' } })

    expect(mountWithKit(NovaMigration).text()).toContain('nova.migration.free')
  })

  it('links the contact action', () => {
    stubTemplateGlobals()

    expect(mountWithKit(NovaMigration).find('[data-testid="nova-migration-cta"]').attributes('href')).toBe('/contact')
  })
})
