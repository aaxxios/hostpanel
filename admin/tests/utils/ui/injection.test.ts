/**
 * Unit tests for the provide/inject contracts: a part reads what its table provides.
 */
import { mount } from '@vue/test-utils'
import { defineComponent, h, inject } from 'vue'
import { describe, expect, it } from 'vitest'
import { UI_TABLE_CONTEXT, UiTableDensity } from '@/utils/ui/injection'

/** A table part that prints the density it injected, or `none` outside a table. */
const Part = defineComponent({
  setup: () => {
    const context = inject(UI_TABLE_CONTEXT, null)
    return () => h('span', context?.density ?? 'none')
  },
})

describe('table context', () => {
  it('keeps the density strings', () => {
    expect(Object.values(UiTableDensity)).toEqual(['plain', 'compact', 'comfortable'])
  })

  it('hands the provided density to a descendant', () => {
    const wrapper = mount(Part, {
      global: { provide: { [UI_TABLE_CONTEXT as symbol]: { density: UiTableDensity.Compact } } },
    })
    expect(wrapper.text()).toBe('compact')
  })

  it('leaves a part outside a table without a context', () => {
    expect(mount(Part).text()).toBe('none')
  })
})
