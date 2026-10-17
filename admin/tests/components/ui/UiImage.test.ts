/**
 * Unit tests for UiImage: native img, decorative by default, fit and radius.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiImage from '@/components/ui/UiImage.vue'

describe('UiImage', () => {
  it('renders a native img carrying the given attributes', () => {
    const wrapper = mount(UiImage, { props: { src: '/a.png', alt: 'logo' }, attrs: { class: 'w-4' } })

    expect(wrapper.element.tagName).toBe('IMG')
    expect(wrapper.attributes('src')).toBe('/a.png')
    expect(wrapper.attributes('alt')).toBe('logo')
    expect(wrapper.classes()).toContain('w-4')
  })

  it('is decorative unless it has alternative text', () => {
    expect(mount(UiImage, { props: { src: '/a.png' } }).attributes('alt')).toBe('')
  })

  it('applies fit and radius', () => {
    const wrapper = mount(UiImage, { props: { src: '/a.png', fit: 'cover', rounded: 'full' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['object-cover', 'rounded-full']))
  })
})
