/**
 * Unit tests for TableFoot: the tfoot ruled from the body.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import TableFoot from '~/components/ui/UiTableFoot.vue'

describe('TableFoot', () => {
  it('is a tfoot ruled from the body', () => {
    const wrapper = mountWithKit(TableFoot)

    expect(wrapper.element.tagName).toBe('TFOOT')
    expect(wrapper.classes()).toContain('border-t')
  })
})
