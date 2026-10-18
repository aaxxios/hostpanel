/**
 * Tests for reading a failed portal call: the code a page branches on, the field messages a form
 * renders, and the one sentence a page shows.
 *
 * A refusal reaches the browser in one of two shapes, and both are covered: the BFF envelope
 * (`statusMessage` carries the backend's `detail`, `data` carries code / title / field errors)
 * and a problem+json body that was not re-wrapped. The offline case is the only place the client
 * speaks, so it is asserted to be the translated `errors.noAnswer` key and never `err.message`
 * (which would embed the upstream address).
 *
 * @module tests/lib/errorHandler.test
 */

import { describe, expect, it } from 'vitest'
import { PortalErrorCode, apiErrorCode, apiErrorMessage, apiFieldErrors } from '~/lib/errorHandler'

/** The BFF envelope `internalApiCall` + `server/error.ts` produce, as `$fetch` exposes it in `err.data`. */
const bff = (statusMessage: string | undefined, data: Record<string, unknown>) => ({
  data: { statusCode: 400, statusMessage, message: statusMessage ?? '', data },
})

describe('apiErrorCode', () => {
  it('reads the code out of a rejection that came through a Nuxt server route', () => {
    expect(apiErrorCode(bff(undefined, { code: 'INVITE_SIGN_IN_REQUIRED' }))).toBe(PortalErrorCode.InviteSignInRequired)
  })

  it('reads `type` out of a problem+json body that was not re-wrapped', () => {
    expect(apiErrorCode({ data: { type: 'CLIENT_PROFILE_NOT_FOUND' } })).toBe(PortalErrorCode.ClientProfileNotFound)
  })

  it('answers null when the response carried no code, or no response arrived', () => {
    expect(apiErrorCode({ data: { statusCode: 400, message: 'This invitation has expired.' } })).toBeNull()
    expect(apiErrorCode(bff('Sentence', { code: null }))).toBeNull()
    expect(apiErrorCode(new Error('offline'))).toBeNull()
    expect(apiErrorCode(null)).toBeNull()
  })
})

describe('apiFieldErrors', () => {
  it('keeps the first message of every field of a BFF-forwarded VALIDATION_ERROR', () => {
    const err = bff('One or more validation errors occurred.', {
      code: 'VALIDATION_ERROR',
      errors: { email: ['Email is invalid.', 'Second'], password: ['Too short.'] },
    })

    expect(apiFieldErrors(err)).toEqual({ email: 'Email is invalid.', password: 'Too short.' })
  })

  it('reads the `errors` of a problem+json body that was not re-wrapped', () => {
    expect(apiFieldErrors({ data: { type: 'VALIDATION_ERROR', errors: { name: ['Required.'] } } })).toEqual({ name: 'Required.' })
  })

  it('skips fields with no usable message and answers an empty object when there are none', () => {
    expect(apiFieldErrors(bff('x', { errors: { a: [], b: ['  '], c: ['Ok'] } }))).toEqual({ c: 'Ok' })
    expect(apiFieldErrors(bff('x', { code: 'NOT_FOUND', errors: null }))).toEqual({})
    expect(apiFieldErrors(undefined)).toEqual({})
  })
})

describe('apiErrorMessage', () => {
  it('prefers a field message over the generic detail', () => {
    const err = bff('One or more validation errors occurred.', { code: 'VALIDATION_ERROR', errors: { email: ['Email is invalid.'] } })

    expect(apiErrorMessage(err)).toBe('Email is invalid.')
  })

  it('reads `detail` from a problem+json body that was not re-wrapped', () => {
    const err = { data: { type: 'NOT_FOUND', title: 'Not Found', detail: 'The invoice was not found.' } }

    expect(apiErrorMessage(err)).toBe('The invoice was not found.')
  })

  it('quotes the backend sentence the BFF carried in `statusMessage`, in whatever language it arrived', () => {
    expect(apiErrorMessage(bff('Срок действия этого приглашения истёк.', { code: 'INVITATION_EXPIRED' }))).toBe('Срок действия этого приглашения истёк.')
  })

  it('falls back to the title when the backend sent no detail', () => {
    expect(apiErrorMessage({ data: { type: 'X', title: 'Only a title' } })).toBe('Only a title')
    expect(apiErrorMessage(bff(undefined, { code: 'X', title: 'Forwarded title' }))).toBe('Forwarded title')
  })

  it('speaks only when the server said nothing, and never quotes err.message', () => {
    // No Nuxt instance in a unit test, so the translator hands back the message key.
    expect(apiErrorMessage({})).toBe('errors.noAnswer')
    expect(apiErrorMessage(new Error('[GET] "http://hostpanel-api:5148/api/me": 502'))).toBe('errors.noAnswer')
    expect(apiErrorMessage(bff(undefined, { code: null }))).toBe('errors.noAnswer')
  })
})

describe('apiErrorMessage for a refused demo action', () => {
  it('shows the localized sentence the backend sends, with no client-authored wording', () => {
    expect(apiErrorMessage({ data: { type: 'DEMO_ACTION_DISABLED', detail: 'Not available in the demo.' } })).toBe('Not available in the demo.')
    expect(apiErrorMessage(bff('Not available in the demo.', { code: 'DEMO_ACTION_DISABLED' }))).toBe('Not available in the demo.')
  })
})
