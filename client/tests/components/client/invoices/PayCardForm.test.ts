/**
 * Unit tests for PayCardForm: the four fields and the formatting of what is typed.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import PayCardForm from '~/components/client/invoices/PayCardForm.vue'

describe('PayCardForm', () => {
  it('has the four fields, each with a placeholder', () => {
    const wrapper = mountClient(PayCardForm)

    expect(wrapper.findAll('input')).toHaveLength(4)
    expect(wrapper.findAll('input').every((input) => Boolean(input.attributes('placeholder')))).toBe(true)
  })

  it('groups the card number in fours and drops letters', async () => {
    const wrapper = mountClient(PayCardForm)

    await wrapper.find('[data-testid="pay-card-number"]').setValue('4242a4242424242424242')

    expect(wrapper.emitted('update:cardNumber')).toEqual([['4242 4242 4242 4242']])
  })

  it('formats the expiry as month and year', async () => {
    const wrapper = mountClient(PayCardForm)

    await wrapper.find('[data-testid="pay-card-expiry"]').setValue('1230')

    expect(wrapper.emitted('update:expiryDate')).toEqual([['12 / 30']])
  })

  it('keeps at most four digits of the security code', async () => {
    const wrapper = mountClient(PayCardForm)

    await wrapper.find('[data-testid="pay-card-cvv"]').setValue('12345')

    expect(wrapper.emitted('update:cvv')).toEqual([['1234']])
  })

  it('passes the card name on as typed', async () => {
    const wrapper = mountClient(PayCardForm)

    await wrapper.find('[data-testid="pay-card-description"]').setValue('Work card')

    expect(wrapper.emitted('update:cardDescription')).toEqual([['Work card']])
  })
})
