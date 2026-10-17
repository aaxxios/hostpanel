/**
 * Tests for runAction: success value, failure sentence from the API, the shared no-answer
 * sentence, error reset and the optional field-error capture.
 */

import { describe, expect, it } from 'vitest'
import { ref } from 'vue'
import { runAction } from '@/lib/runAction'
import { ApiError } from '@/composables/useApi'

describe('runAction', () => {
  it('returns the value and leaves error null', async () => {
    const error = ref<string | null>('old')
    const result = await runAction(error, async () => 7)

    expect(result).toEqual({ ok: true, value: 7 })
    expect(error.value).toBeNull()
  })

  it('stores the API detail sentence on failure', async () => {
    const error = ref<string | null>(null)
    const result = await runAction(error, async () => {
      throw new ApiError(400, { type: 'X', detail: 'Server says no.' })
    })

    expect(result).toEqual({ ok: false })
    expect(error.value).toBe('Server says no.')
  })

  it('stores the shared no-answer sentence when nothing answered', async () => {
    const error = ref<string | null>(null)
    await runAction(error, async () => {
      throw new Error('network')
    })

    expect(error.value).toBe('Could not reach the server. Check your connection and try again.')
  })

  it('captures the per-field messages and clears them on the next success', async () => {
    const error = ref<string | null>(null)
    const fields = ref<Record<string, string>>({})

    await runAction(
      error,
      async () => {
        throw new ApiError(400, { type: 'VALIDATION_ERROR', detail: 'Invalid.', errors: { email: ['Required.'] } })
      },
      fields,
    )
    expect(fields.value).toEqual({ email: 'Required.' })

    await runAction(error, async () => 1, fields)
    expect(fields.value).toEqual({})
  })
})
