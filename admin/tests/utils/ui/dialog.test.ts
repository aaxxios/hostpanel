/**
 * Unit tests for the dialog vocabulary.
 */
import { describe, expect, it } from 'vitest'
import { UiDialogVariant, UiDialogSize } from '@/utils/ui/dialog'

describe('UiDialogVariant', () => {
  it('lists unique, non-empty values', () => {
    const values = Object.values(UiDialogVariant)

    expect(values.length).toBeGreaterThan(0)
    expect(new Set(values).size).toBe(values.length)
    expect(values.every((value) => value.length > 0)).toBe(true)
  })
})

describe('UiDialogSize', () => {
  it('lists unique, non-empty values', () => {
    const values = Object.values(UiDialogSize)

    expect(values.length).toBeGreaterThan(0)
    expect(new Set(values).size).toBe(values.length)
    expect(values.every((value) => value.length > 0)).toBe(true)
  })
})
