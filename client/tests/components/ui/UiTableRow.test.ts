/**
 * Unit tests for TableRow: the row with hover highlight and group marker.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import TableRow from '~/components/ui/UiTableRow.vue'

describe('TableRow', () => {
  it('highlights on hover by default', () => {
    const wrapper = mountWithKit(TableRow)

    expect(wrapper.element.tagName).toBe('TR')
    expect(wrapper.classes()).toContain('hover:bg-gray-50')
  })

  it('can opt out of the hover highlight', () => {
    const wrapper = mountWithKit(TableRow, { props: { hoverable: false } })

    expect(wrapper.classes()).not.toContain('hover:bg-gray-50')
  })

  it('marks itself as a group on request', () => {
    expect(mountWithKit(TableRow, { props: { group: true } }).classes()).toContain('group')
    expect(mountWithKit(TableRow).classes()).not.toContain('group')
  })
})
