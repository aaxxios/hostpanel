/**
 * Unit tests for UiField: the label bound to the control id, error and hint lines, the list form
 * of an error and the scoped slot props.
 */
import { describe, expect, it } from 'vitest'
import { h } from 'vue'
import { mount } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiField from '~/components/ui/UiField.vue'

describe('UiField', () => {
  it('renders the label bound to the given control id', () => {
    const wrapper = mountWithKit(UiField, { props: { id: 'email', label: 'Email' } })

    expect(wrapper.find('label').attributes('for')).toBe('email')
    expect(wrapper.find('label').text()).toContain('Email')
  })

  it('adds the required asterisk', () => {
    const wrapper = mountWithKit(UiField, { props: { label: 'Email', required: true } })

    expect(wrapper.find('label span').text()).toBe('*')
  })

  it('renders the slot', () => {
    const wrapper = mountWithKit(UiField, { slots: { default: 'control' } })

    expect(wrapper.text()).toContain('control')
  })

  it('shows an error sentence as an alert', () => {
    const wrapper = mountWithKit(UiField, { props: { error: 'Email is taken' } })

    const message = wrapper.find('[data-testid="ui-field-error"]')
    expect(message.text()).toBe('Email is taken')
    expect(message.attributes('role')).toBe('alert')
  })

  it('shows the first sentence of an error list', () => {
    const wrapper = mountWithKit(UiField, { props: { error: ['First problem', 'Second problem'] } })

    expect(wrapper.find('[data-testid="ui-field-error"]').text()).toBe('First problem')
  })

  it('prefers the error over the hint', () => {
    const wrapper = mountWithKit(UiField, { props: { error: 'Bad', hint: 'Help' } })

    expect(wrapper.find('[data-testid="ui-field-hint"]').exists()).toBe(false)
    expect(wrapper.text()).not.toContain('Help')
  })

  it('shows the hint when there is no error', () => {
    const wrapper = mountWithKit(UiField, { props: { hint: 'Help' } })

    expect(wrapper.find('[data-testid="ui-field-hint"]').text()).toBe('Help')
  })

  it('treats an empty error list as no error', () => {
    const wrapper = mountWithKit(UiField, { props: { error: [], hint: 'Help' } })

    expect(wrapper.find('[data-testid="ui-field-error"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="ui-field-hint"]').exists()).toBe(true)
  })

  it('gives the slot the id, the invalid flag and the described-by id', () => {
    let received: Record<string, unknown> = {}
    mount(UiField, {
      props: { id: 'email', error: 'Bad' },
      slots: {
        default: (slotProps: Record<string, unknown>) => {
          received = slotProps
          return h('span')
        },
      },
    })

    expect(received).toEqual({ id: 'email', invalid: true, describedBy: 'email-message' })
  })
})
