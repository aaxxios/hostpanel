/**
 * Mount tests for DomainRemindersTable: rows and the empty state.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import DomainRemindersTable from '@/components/clients/DomainRemindersTable.vue'
import type { DomainReminderItem } from '@/types/domain'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Builds a reminder fixture.
 *
 * @returns One reminder.
 */
const reminder = (): DomainReminderItem =>
  ({ id: 1, reminderType: 'Renewal 30 days', sentTo: 'a@example.com', sentAt: '2026-01-02T00:00:00Z' }) as DomainReminderItem

describe('DomainRemindersTable', () => {
  it('renders one row per reminder', () => {
    const wrapper = mount(DomainRemindersTable, {
      props: { reminders: [reminder()] },
      global: { plugins: [createTestI18n()] },
    })

    expect(wrapper.findAll('tbody tr')).toHaveLength(1)
    expect(wrapper.text()).toContain('Renewal 30 days')
    expect(wrapper.text()).toContain('a@example.com')
  })

  it('shows the empty message without reminders', () => {
    const wrapper = mount(DomainRemindersTable, {
      props: { reminders: [] },
      global: { plugins: [createTestI18n()] },
    })

    expect(wrapper.text()).toContain('No Records Found')
  })
})
