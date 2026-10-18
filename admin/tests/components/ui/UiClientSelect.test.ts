/**
 * Unit tests for UiClientSelect: shows the chosen client, filters, picks and clears.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiClientSelect from '@/components/ui/UiClientSelect.vue'
import { createTestI18n } from '../../helpers/i18n'

/** Clients used by the tests. */
const clients = [
  { id: 1, name: 'Acme Ltd', email: 'hello@acme.test' },
  { id: 2, name: 'Beta Corp', status: 'Active' },
]

/**
 * Mounts the picker with the English catalogue.
 *
 * @param modelValue - Id of the chosen client.
 * @returns The wrapper.
 */
const mountSelect = (modelValue: string | number | null) =>
  mount(UiClientSelect, { props: { modelValue, clients }, global: { plugins: [createTestI18n()] } })

describe('UiClientSelect', () => {
  it('shows the name of the chosen client', () => {
    expect((mountSelect(1).get('input').element as HTMLInputElement).value).toBe('Acme Ltd')
  })

  it('shows the placeholder for no choice', () => {
    expect(mountSelect(null).get('input').attributes('placeholder')).toBe('Select a client...')
  })

  it('opens the list on click and lists every client with name and id', async () => {
    const wrapper = mountSelect(null)

    await wrapper.get('input').trigger('click')

    expect(wrapper.find('[data-testid="client-select-option-1"]').text()).toContain('Acme Ltd - #1')
    expect(wrapper.find('[data-testid="client-select-option-2"]').text()).toContain('Active')
  })

  it('filters by name, email or id', async () => {
    const wrapper = mountSelect(null)
    await wrapper.get('input').trigger('click')

    await wrapper.get('input').setValue('acme')
    expect(wrapper.findAll('[data-testid^="client-select-option-"]')).toHaveLength(1)

    await wrapper.get('input').setValue('2')
    expect(wrapper.find('[data-testid="client-select-option-2"]').exists()).toBe(true)
  })

  it('says when nothing matches', async () => {
    const wrapper = mountSelect(null)
    await wrapper.get('input').trigger('click')

    await wrapper.get('input').setValue('zzz')

    expect(wrapper.text()).toContain('No clients found')
  })

  it('emits the id of the picked client as a string and closes', async () => {
    const wrapper = mountSelect(null)
    await wrapper.get('input').trigger('click')

    await wrapper.get('[data-testid="client-select-option-2"]').trigger('click')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['2'])
    expect(wrapper.find('[data-testid="client-select-option-2"]').exists()).toBe(false)
  })

  it('clears the choice when the field is emptied', async () => {
    const wrapper = mountSelect(1)

    await wrapper.get('input').setValue('')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual([''])
  })

  it('closes on a click outside', async () => {
    const wrapper = mountSelect(null)
    await wrapper.get('input').trigger('click')

    document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }))
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="client-select-option-1"]').exists()).toBe(false)
  })
})
