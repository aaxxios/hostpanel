/**
 * Unit tests for the service value sets: the reasons the admin records with a suspension or a
 * termination.
 */
import { describe, expect, it } from 'vitest'
import { ServiceChangeReason } from '@/types/service'

describe('ServiceChangeReason', () => {
  it('keeps the sentences recorded with the command', () => {
    expect(ServiceChangeReason.Suspended).toBe('Admin suspended')
    expect(ServiceChangeReason.Terminated).toBe('Admin terminated')
  })
})
