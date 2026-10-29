/**
 * Tests for `server/utils/readBodyObject.ts`: a JSON object passes, anything else is a coded 400.
 *
 * @module tests/server/utils/readBodyObject.test
 */

import { describe, expect, it } from 'vitest'
import { makeEvent, refusal } from '../../helpers/bffHarness'
import { BAD_BODY, readBodyObject } from '~/server/utils/readBodyObject'

describe('readBodyObject', () => {
  it('returns a JSON object as sent', async () => {
    expect(await readBodyObject(makeEvent({ body: { email: 'a@b.test' } }))).toEqual({ email: 'a@b.test' })
  })

  it.each([
    ['no body', undefined],
    ['null', null],
    ['a string', 'text'],
    ['a number', 7],
    ['an array', [1, 2]],
  ])('refuses %s with a coded 400 and no sentence', async (_name, body) => {
    const error = await refusal(() => readBodyObject(makeEvent({ body })))

    expect(error).toMatchObject({ statusCode: 400, data: { code: BAD_BODY } })
    expect(error['statusMessage']).toBeUndefined()
  })
})
