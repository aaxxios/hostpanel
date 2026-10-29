/**
 * Unit tests for the field vocabulary: the shared class sets.
 */
import { describe, expect, it } from 'vitest'
import {
  fieldBaseClasses,
  fieldInvalidClasses,
  fieldWidthClasses,
} from '@/utils/ui/input'

describe('field vocabulary', () => {
  it('applies width through :where() so a screen class always wins', () => {
    expect(fieldWidthClasses).toContain(':where(&)')
  })

  it('reads the theme tokens, and the invalid set swaps the border to red', () => {
    expect(fieldBaseClasses).toContain('bg-surface-field')
    expect(fieldBaseClasses).toContain('border-border')
    expect(fieldInvalidClasses).toContain('border-status-red')
  })
})

describe('field focus look', () => {
  it('draws a keyboard-only ring at 30% instead of a 10% wash', () => {
    expect(fieldBaseClasses).toContain('focus-visible:ring-primary-500/30')
    expect(fieldInvalidClasses).toContain('focus-visible:ring-status-red/30')
  })
})
