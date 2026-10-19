/**
 * Tests for the process-step structure: unique ids and a gap-free one-based order.
 *
 * @module tests/constants/processStep
 */
import { describe, expect, it } from 'vitest'
import { PROCESS_STEPS } from '~/constants/processStep'
import { ProcessStepId } from '~/types/processstepid'

describe('PROCESS_STEPS', () => {
  it('has one entry per step id', () => {
    const ids = PROCESS_STEPS.map((step) => step.id)
    expect(new Set(ids).size).toBe(ids.length)
    expect([...ids].sort()).toEqual(Object.values(ProcessStepId).sort())
  })

  it('is ordered 1..n without gaps', () => {
    expect(PROCESS_STEPS.map((step) => step.order)).toEqual(PROCESS_STEPS.map((_, index) => index + 1))
  })
})
