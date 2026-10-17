/**
 * Unit tests for the image vocabulary.
 */
import { describe, expect, it } from 'vitest'
import { UiImageFit, UiImageRounded } from '@/utils/ui/image'

describe('UiImageFit', () => {
  it('lists unique, non-empty values', () => {
    const values = Object.values(UiImageFit)

    expect(values.length).toBeGreaterThan(0)
    expect(new Set(values).size).toBe(values.length)
    expect(values.every((value) => value.length > 0)).toBe(true)
  })
})

describe('UiImageRounded', () => {
  it('lists unique, non-empty values', () => {
    const values = Object.values(UiImageRounded)

    expect(values.length).toBeGreaterThan(0)
    expect(new Set(values).size).toBe(values.length)
    expect(values.every((value) => value.length > 0)).toBe(true)
  })
})
