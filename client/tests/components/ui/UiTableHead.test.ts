/**
 * Unit tests for TableHead: the thead band of a table.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import TableHead from '~/components/ui/UiTableHead.vue'

describe('TableHead', () => {
  it('is a thead with a ruled, tinted band', () => {
    const wrapper = mountWithKit(TableHead, { slots: { default: '<tr></tr>' } })

    expect(wrapper.element.tagName).toBe('THEAD')
    expect(wrapper.classes()).toContain('border-b')
  })
})
