/**
 * Tests for StorefrontImageField: the preview and remove button, typing an address, and picking a
 * file that is uploaded through the supplied function.
 */
import { describe, expect, it, vi } from 'vitest'
import StorefrontImageField from '@/components/storefront/StorefrontImageField.vue'
import { mountScreen } from '../../helpers/mount'

/**
 * Mounts the field.
 *
 * @param modelValue - The current address.
 * @param upload - The upload function.
 * @param uploadError - The store's upload sentence.
 * @returns The wrapper.
 */
const mountField = async (modelValue: string, upload: (file: File) => Promise<string | null> = () => Promise.resolve(null), uploadError?: string) =>
  (await mountScreen(StorefrontImageField, { modelValue, label: 'Logo', testid: 'logo', upload, uploadError })).wrapper

describe('StorefrontImageField', () => {
  it('shows no preview while the address is empty', async () => {
    const wrapper = await mountField('')

    expect(wrapper.find('[data-testid="logo-preview"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('Logo')
  })

  it('previews the address and removes it with the button', async () => {
    const wrapper = await mountField('/img/a.png')
    expect(wrapper.get('[data-testid="logo-preview"]').attributes('src')).toBe('/img/a.png')

    await wrapper.get('[data-testid="logo-remove"]').trigger('click')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual([''])
  })

  it('emits the typed address', async () => {
    const wrapper = await mountField('')

    await wrapper.get('[data-testid="logo-url"]').setValue('https://cdn.example.com/x.png')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['https://cdn.example.com/x.png'])
  })

  it('uploads a picked file and emits the stored address', async () => {
    const upload = vi.fn(() => Promise.resolve('/img/up.png'))
    const wrapper = await mountField('', upload)
    const input = wrapper.get('[data-testid="logo-file"]')
    const file = new File(['x'], 'a.png')
    Object.defineProperty(input.element, 'files', { value: [file], configurable: true })

    await input.trigger('change')

    await vi.waitFor(() => {
      expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['/img/up.png'])
    })
    expect(upload).toHaveBeenCalledWith(file)
  })

  it("does not show the store's upload sentence before this field has failed", async () => {
    const wrapper = await mountField('', () => Promise.resolve(null), 'The file is too large.')

    expect(wrapper.text()).not.toContain('The file is too large.')
  })

  it('keeps the address when the upload is refused and shows the sentence', async () => {
    const wrapper = await mountField('/img/old.png', () => Promise.resolve(null), 'The file is too large.')
    const input = wrapper.get('[data-testid="logo-file"]')
    Object.defineProperty(input.element, 'files', { value: [new File(['x'], 'a.png')], configurable: true })

    await input.trigger('change')

    await vi.waitFor(() => {
      expect(wrapper.text()).toContain('The file is too large.')
    })
    expect(wrapper.emitted('update:modelValue')).toBeUndefined()
  })
})
