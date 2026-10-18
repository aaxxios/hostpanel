/**
 * Unit tests for NovaPerformance: a list of performance cards, with storage, cache and CDN claims
 * hidden until the operator has filled their setting in.
 *
 * @module tests/templates/nova/sections/NovaPerformance.test
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaPerformance from '~/templates/nova/sections/NovaPerformance.vue'
import { PERFORMANCE_FEATURES } from '~/templates/nova/content'
import type { Feature } from '~/templates/nova/types/feature'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'

/**
 * Counts the features that do not wait on an operator setting.
 *
 * @param features A content list.
 * @returns How many are always shown.
 */
const ungated = (features: readonly Feature[]): number => features.filter(feature => !feature.requiresSetting).length

describe('NovaPerformance', () => {
  it('hides storage, cache and CDN claims until their settings exist', () => {
    stubTemplateGlobals()

    expect(mountWithKit(NovaPerformance).findAll('ul > li')).toHaveLength(ungated(PERFORMANCE_FEATURES))
  })
})
