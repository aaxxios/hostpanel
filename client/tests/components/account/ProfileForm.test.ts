/**
 * Unit tests for ProfileForm: the seeded draft, the save call, outcomes and the language field.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import ProfileForm from '~/components/account/ProfileForm.vue'
import { useAccountStore } from '~/stores/account'
import { ActionOutcome } from '~/types/actionoutcome'
import type { ClientUser } from '~/types/clientuser'

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useAuthApi', () => ({ useAuthApi: () => ({}) }))
vi.mock('qrcode', () => ({ default: { toDataURL: async (): Promise<string> => '' } }))
vi.mock('~/stores/catalog', async () => {
  const { ref } = await import('vue')
  return { useCatalogStore: () => ({ countries: ref([{ value: 'AM', label: 'Armenia' }]) }) }
})

/** The profile being edited. */
const user: ClientUser = { id: 1, firstname: 'Jane', lastname: 'Doe', email: 'jane@example.com', permissions: 0, country: 'AM' }

/**
 * Mounts the form.
 *
 * @param ownsLanguage - Whether the language select is offered.
 * @returns The wrapper.
 */
const mountForm = (ownsLanguage = true) =>
  mountAccount(ProfileForm, {
    props: {
      user,
      ownsLanguage,
      paymentOptions: [{ value: '', label: 'Default' }],
      languageOptions: [{ value: '', label: 'Default' }, { value: 'en', label: 'English' }]
    }
  })

describe('ProfileForm', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  it('seeds the inputs from the profile', () => {
    const wrapper = mountForm()

    expect((wrapper.find('[data-testid="account-firstname"]').element as HTMLInputElement).value).toBe('Jane')
    expect((wrapper.find('[data-testid="account-email"]').element as HTMLInputElement).value).toBe('jane@example.com')
  })

  it('gives every text field a placeholder', () => {
    const wrapper = mountForm()

    for (const input of wrapper.findAll('input[type="text"], input[type="email"], input[type="tel"]')) {
      expect(input.attributes('placeholder')).toBeTruthy()
    }
  })

  it('saves the draft and announces success', async () => {
    const wrapper = mountForm()
    const save = vi.spyOn(useAccountStore(), 'saveProfile').mockResolvedValue(ActionOutcome.Succeeded)

    await wrapper.find('[data-testid="account-firstname"]').setValue('Janet')
    await wrapper.find('form').trigger('submit')
    await Promise.resolve()

    expect(save).toHaveBeenCalledWith(expect.objectContaining({ firstname: 'Janet', emailprefs: expect.any(Object) }))
    expect(wrapper.emitted('saved')).toHaveLength(1)
  })

  it('stays open when the save is refused', async () => {
    const wrapper = mountForm()
    vi.spyOn(useAccountStore(), 'saveProfile').mockResolvedValue(ActionOutcome.Failed)

    await wrapper.find('form').trigger('submit')
    await Promise.resolve()

    expect(wrapper.emitted('saved')).toBeUndefined()
  })

  it('emits cancel from the cancel button', async () => {
    const wrapper = mountForm()

    await wrapper.find('[data-testid="account-profile-cancel"]').trigger('click')

    expect(wrapper.emitted('cancel')).toHaveLength(1)
  })

  it('shows the API sentence of a refused save', async () => {
    const wrapper = mountForm()
    useAccountStore().saveError = 'Email taken'
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="ui-form-error"]').text()).toBe('Email taken')
  })
})
