/**
 * Tests for ReportTimestamp: the captured time in the report's own format.
 */
import { describe, expect, it } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import ReportTimestamp from '@/components/reports/ReportTimestamp.vue'
import { createTestI18n } from '../../helpers/i18n'

describe('ReportTimestamp', () => {
  it('prints "Report Generated on" with a DD/MM/YYYY HH:MM time', async () => {
    const wrapper = mount(ReportTimestamp, { global: { plugins: [createTestI18n()] } })
    await flushPromises()

    expect(wrapper.text()).toMatch(/^Report Generated on \d{2}\/\d{2}\/\d{4} \d{2}:\d{2}$/)
  })
})
