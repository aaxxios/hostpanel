/**
 * Tests for ReportComingSoon: the placeholder of a report that is not built yet.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import ReportComingSoon from '@/components/reports/ReportComingSoon.vue'
import { createTestI18n } from '../../helpers/i18n'

describe('ReportComingSoon', () => {
  it('says the report is coming soon and under development', () => {
    const wrapper = mount(ReportComingSoon, { global: { plugins: [createTestI18n()] } })

    expect(wrapper.text()).toContain('Coming Soon')
    expect(wrapper.text()).toContain('This report is under development.')
    expect(wrapper.find('svg').exists()).toBe(true)
  })
})
