/**
 * Unit tests for SubmitHint: the muted line under a busy submit button, whose live region stays
 * in the page so a change of its text is announced.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SubmitHint from '~/components/checkout/SubmitHint.vue'

describe('SubmitHint', () => {
  it('shows the message in a small muted polite live region', () => {
    const wrapper = mountWithKit(SubmitHint, { props: { message: 'Taking you to the bank' } })

    const hint = wrapper.find('[data-testid="checkout-submit-hint"]')
    expect(hint.text()).toBe('Taking you to the bank')
    expect(hint.attributes('aria-live')).toBe('polite')
    expect(hint.classes()).toEqual(expect.arrayContaining(['text-xs', 'mt-3']))
  })

  it('keeps the live region without spacing when there is nothing to say', () => {
    const wrapper = mountWithKit(SubmitHint)

    const hint = wrapper.find('[data-testid="checkout-submit-hint"]')
    expect(hint.exists()).toBe(true)
    expect(hint.text()).toBe('')
    expect(hint.classes()).not.toContain('mt-3')
  })
})
