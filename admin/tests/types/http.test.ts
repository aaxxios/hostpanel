/**
 * Unit tests for the HTTP enums: the verbs `fetch` sends and the media types the admin names.
 */
import { describe, expect, it } from 'vitest'
import { HttpMethod, MimeType } from '@/types/http'

describe('http enums', () => {
  it('holds the upper-case verbs', () => {
    expect(Object.values(HttpMethod)).toEqual(['GET', 'POST', 'PUT', 'PATCH', 'DELETE'])
  })

  it('names the JSON media type', () => {
    expect(MimeType.Json).toBe('application/json')
  })
})
