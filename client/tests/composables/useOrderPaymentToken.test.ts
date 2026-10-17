/**
 * Tests for `useOrderPaymentToken`: the per-order token survives a trip to the bank in
 * `localStorage`.
 *
 * @module tests/composables/useOrderPaymentToken.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useOrderPaymentToken } from '~/composables/useOrderPaymentToken'

describe('useOrderPaymentToken', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  afterEach(() => {
    localStorage.clear()
    vi.unstubAllGlobals()
  })

  it('recalls nothing for an order that stored no token', () => {
    expect(useOrderPaymentToken().recall(7)).toBeUndefined()
  })

  it('stores a token under a per-order key and recalls it', () => {
    const { remember, recall } = useOrderPaymentToken()

    remember(7, 'tok-7')

    expect(localStorage.getItem('order-payment-token:7')).toBe('tok-7')
    expect(recall(7)).toBe('tok-7')
  })

  it('treats a numeric and a string order id as the same order', () => {
    const { remember, recall } = useOrderPaymentToken()

    remember(12, 'tok')

    expect(recall('12')).toBe('tok')
  })

  it('keeps tokens of different orders apart', () => {
    const { remember, recall } = useOrderPaymentToken()

    remember(1, 'a')
    remember(2, 'b')

    expect(recall(1)).toBe('a')
    expect(recall(2)).toBe('b')
  })

  it('forgets only the named order', () => {
    const { remember, recall, forget } = useOrderPaymentToken()
    remember(1, 'a')
    remember(2, 'b')

    forget(1)

    expect(recall(1)).toBeUndefined()
    expect(recall(2)).toBe('b')
  })
})
