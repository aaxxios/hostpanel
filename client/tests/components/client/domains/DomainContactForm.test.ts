/**
 * Unit tests for DomainContactForm: the eleven fields, their values and the edits it reports.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import DomainContactForm from '~/components/client/domains/DomainContactForm.vue'
import { emptyContact } from '~/utils/domainContacts'

describe('DomainContactForm', () => {
  it('renders one input per contact field', () => {
    const wrapper = mountClient(DomainContactForm, { props: { values: emptyContact(), disabled: false } })

    expect(wrapper.findAll('input')).toHaveLength(11)
  })

  it('shows the values it is given and a placeholder for each field', () => {
    const wrapper = mountClient(DomainContactForm, { props: { values: { ...emptyContact(), firstName: 'Ada' }, disabled: false } })

    expect((wrapper.find('[data-testid="domain-contact-firstName"]').element as HTMLInputElement).value).toBe('Ada')
    expect(wrapper.findAll('input').every((input) => Boolean(input.attributes('placeholder')))).toBe(true)
  })

  it('reports an edit with the field it belongs to', async () => {
    const wrapper = mountClient(DomainContactForm, { props: { values: emptyContact(), disabled: false } })

    await wrapper.find('[data-testid="domain-contact-city"]').setValue('London')

    expect(wrapper.emitted('change')).toEqual([['city', 'London']])
  })

  it('disables every field when told to', () => {
    const wrapper = mountClient(DomainContactForm, { props: { values: emptyContact(), disabled: true } })

    expect(wrapper.findAll('input').every((input) => input.attributes('disabled') !== undefined)).toBe(true)
  })

  it('uses e-mail and telephone inputs for those fields', () => {
    const wrapper = mountClient(DomainContactForm, { props: { values: emptyContact(), disabled: false } })

    expect(wrapper.find('[data-testid="domain-contact-email"]').attributes('type')).toBe('email')
    expect(wrapper.find('[data-testid="domain-contact-phone"]').attributes('type')).toBe('tel')
  })
})
