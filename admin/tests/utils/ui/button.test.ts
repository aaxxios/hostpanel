/**
 * Unit tests for the button vocabulary: every look has classes, and `plain` adds none.
 */
import { describe, expect, it } from 'vitest'
import { buttonBaseClasses, buttonVariantClasses, UiButtonVariant } from '@/utils/ui/button'

describe('button variants', () => {
  it('keeps the named looks', () => {
    expect(Object.values(UiButtonVariant)).toEqual(['plain', 'primary', 'secondary', 'ghost', 'danger', 'link'])
  })

  it('maps every look, with plain empty and the rest non-empty', () => {
    for (const variant of Object.values(UiButtonVariant)) {
      if (variant === UiButtonVariant.Plain) {
        expect(buttonVariantClasses[variant]).toBe('')
      } else {
        expect(buttonVariantClasses[variant].length).toBeGreaterThan(0)
      }
    }
  })

  it('shares a base set with a focus ring and a disabled state', () => {
    expect(buttonBaseClasses).toContain('focus-visible:ring-2')
    expect(buttonBaseClasses).toContain('disabled:opacity-50')
  })
})
