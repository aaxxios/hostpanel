/**
 * Unit tests for the media types: the JSON type and the accept lists of the upload controls.
 */
import { describe, expect, it } from 'vitest'
import { FAVICON_ACCEPT, ILLUSTRATION_ACCEPT, LOGO_ACCEPT } from '@/lib/mimeTypes'
import { MimeType } from '@/types/http'

describe('mimeTypes', () => {
  it('names the JSON content type', () => {
    expect(MimeType.Json).toBe('application/json')
  })

  it('lists the logo formats in one comma-separated value', () => {
    expect(LOGO_ACCEPT).toBe('image/jpeg,image/png,image/webp,image/gif,image/bmp')
  })

  it('lists the favicon formats, bitmaps and vectors left out', () => {
    expect(FAVICON_ACCEPT).toBe('image/png,image/jpeg,image/webp')
  })

  it('lets an illustration be a vector but not a bitmap', () => {
    expect(ILLUSTRATION_ACCEPT).toContain(MimeType.Svg)
    expect(ILLUSTRATION_ACCEPT).not.toContain(MimeType.Bmp)
  })
})
