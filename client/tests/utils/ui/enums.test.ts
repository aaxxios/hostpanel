/**
 * Unit tests for `utils/ui/enums.ts`: the kit's closed value sets keep their wire values.
 *
 * @module tests/utils/ui/enums.test
 */

import { describe, expect, it } from 'vitest'
import * as kit from '~/utils/ui/enums'

describe('Ui enums', () => {
  it('keeps the three-step size scale', () => {
    expect(Object.values(kit.UiSize)).toEqual(['sm', 'md', 'lg'])
  })

  it('keeps the button variants a screen may write as a literal', () => {
    expect(Object.values(kit.UiButtonVariant)).toEqual(['primary', 'secondary', 'outline', 'ghost', 'subtle', 'danger', 'bare'])
  })

  it('keeps the heading levels numeric, one to six', () => {
    expect(kit.UiHeadingLevel.H1).toBe(1)
    expect(kit.UiHeadingLevel.H6).toBe(6)
  })

  it('keeps the rich-text node kinds', () => {
    expect(Object.values(kit.UiRichNodeType)).toEqual(['text', 'element'])
  })

  it('has no duplicated value inside any string enum', () => {
    for (const [name, members] of Object.entries(kit)) {
      const values = Object.values(members as Record<string, string | number>).filter(value => typeof value === 'string')
      expect(new Set(values).size, name).toBe(values.length)
    }
  })
})
