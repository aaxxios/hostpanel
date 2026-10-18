/**
 * Unit tests for UiSplit: ratio templates that stack on narrow screens.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiSplit from '@/components/ui/UiSplit.vue'

describe('UiSplit', () => {
  it('is two thirds / one third by default', () => {
    expect(mount(UiSplit).classes()).toEqual(expect.arrayContaining(['grid', 'grid-cols-1', 'lg:grid-cols-[2fr_1fr]', 'gap-6']))
  })

  it.each([
    ['half', 'lg:grid-cols-2'],
    ['three-quarters', 'lg:grid-cols-[3fr_1fr]'],
  ] as const)('maps the %s ratio to %s', (ratio, expected) => {
    expect(mount(UiSplit, { props: { ratio } }).classes()).toContain(expected)
  })
})
