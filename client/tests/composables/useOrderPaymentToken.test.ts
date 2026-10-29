/**
 * Tests for `useOrderPaymentToken`: the per-order token survives a trip to the bank in
 * `sessionStorage`, expires, and is dropped on request.
 *
 * @module tests/composables/useOrderPaymentToken.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { PAYMENT_TOKEN_TTL_MS, useOrderPaymentToken } from '~/composables/useOrderPaymentToken'

describe('useOrderPaymentToken', () => {
  beforeEach(() => {
    sessionStorage.clear()
    localStorage.clear()
  })

  afterEach(() => {
    sessionStorage.clear()
    localStorage.clear()
    vi.useRealTimers()
    vi.unstubAllGlobals()
  })

  it('recalls nothing for an order that stored no token', () => {
    expect(useOrderPaymentToken().recall(7)).toBeUndefined()
  })

  it('stores a token under a per-order key in session storage only and recalls it', () => {
    const { remember, recall } = useOrderPaymentToken()

    remember(7, 'tok-7', 70)

    expect(sessionStorage.getItem('order-payment-token:7')).toContain('tok-7')
    expect(localStorage.getItem('order-payment-token:7')).toBeNull()
    expect(recall(7)).toBe('tok-7')
  })

  it('recalls the invoice id stored beside the token', () => {
    const { remember, recallInvoiceId } = useOrderPaymentToken()

    remember(7, 'tok-7', 70)

    expect(recallInvoiceId(7)).toBe(70)
  })

  it('treats a numeric and a string order id as the same order', () => {
    const { remember, recall } = useOrderPaymentToken()

    remember(12, 'tok', 1)

    expect(recall('12')).toBe('tok')
  })

  it('keeps tokens of different orders apart', () => {
    const { remember, recall } = useOrderPaymentToken()

    remember(1, 'a', 10)
    remember(2, 'b', 20)

    expect(recall(1)).toBe('a')
    expect(recall(2)).toBe('b')
  })

  it('forgets only the named order', () => {
    const { remember, recall, forget } = useOrderPaymentToken()
    remember(1, 'a', 10)
    remember(2, 'b', 20)

    forget(1)

    expect(recall(1)).toBeUndefined()
    expect(recall(2)).toBe('b')
  })

  it('stops recalling a token once its time to live has passed and drops the entry', () => {
    vi.useFakeTimers()
    const { remember, recall } = useOrderPaymentToken()
    remember(3, 'tok', 30)

    vi.advanceTimersByTime(PAYMENT_TOKEN_TTL_MS + 1)

    expect(recall(3)).toBeUndefined()
    expect(sessionStorage.getItem('order-payment-token:3')).toBeNull()
  })

  it('still recalls a token inside its time to live', () => {
    vi.useFakeTimers()
    const { remember, recall } = useOrderPaymentToken()
    remember(3, 'tok', 30)

    vi.advanceTimersByTime(PAYMENT_TOKEN_TTL_MS - 1)

    expect(recall(3)).toBe('tok')
  })

  it('drops a malformed entry instead of recalling it', () => {
    sessionStorage.setItem('order-payment-token:4', 'not json')

    expect(useOrderPaymentToken().recall(4)).toBeUndefined()
    expect(sessionStorage.getItem('order-payment-token:4')).toBeNull()
  })
})
