/**
 * Unit tests for TableBody: the tbody with hairlines between rows.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import TableBody from '~/components/ui/UiTableBody.vue'

describe('TableBody', () => {
  it('is a tbody with hairlines between rows', () => {
    const wrapper = mountWithKit(TableBody)

    expect(wrapper.element.tagName).toBe('TBODY')
    expect(wrapper.classes()).toContain('divide-y')
  })
})
