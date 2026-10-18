/**
 * Mount tests for DomainContactModal: placeholders, trimmed payload and close.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import DomainContactModal from '@/components/clients/DomainContactModal.vue'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Mounts the modal with the dialog teleport inlined.
 *
 * @returns The wrapper.
 */
const mountModal = () =>
  mount(DomainContactModal, {
    props: { saving: false },
    global: { plugins: [createTestI18n()], stubs: { teleport: true } },
  })

describe('DomainContactModal', () => {
  it('renders every contact field with a placeholder', () => {
    const inputs = mountModal().findAll('input')

    expect(inputs).toHaveLength(11)
    expect(inputs.every(i => (i.attributes('placeholder') ?? '') !== '')).toBe(true)
  })

  it('emits the trimmed payload on submit', async () => {
    const wrapper = mountModal()

    await wrapper.get('[data-testid="clients-domain-contact-firstName"]').setValue('  Ada ')
    await wrapper.get('[data-testid="clients-domain-contact-country"]').setValue('AM')
    await wrapper.get('form').trigger('submit')

    const payload = wrapper.emitted('save')?.[0]?.[0] as Record<string, string>
    expect(payload.firstName).toBe('Ada')
    expect(payload.country).toBe('AM')
  })

  it('emits close from cancel', async () => {
    const wrapper = mountModal()

    await wrapper.get('[data-testid="clients-domain-contact-cancel"]').trigger('click')

    expect(wrapper.emitted('close')).toHaveLength(1)
  })
})
