/**
 * Unit tests for UiImage: the source and alt text, loading, fit and rounding, and the size attributes.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiImage from '~/components/ui/UiImage.vue'
import { UiImageFit, UiImageLoading, UiImageRounding } from '~/utils/ui/enums'

describe('UiImage', () => {
  it('renders the source with its alternative text', () => {
    const wrapper = mountWithKit(UiImage, { props: { src: '/logo.png', alt: 'Company logo' } })

    expect(wrapper.element.tagName).toBe('IMG')
    expect(wrapper.attributes('src')).toBe('/logo.png')
    expect(wrapper.attributes('alt')).toBe('Company logo')
  })

  it('accepts an empty alt for a decorative image', () => {
    const wrapper = mountWithKit(UiImage, { props: { src: '/dots.svg', alt: '' } })

    expect(wrapper.attributes('alt')).toBe('')
  })

  it('covers its box by default and can contain instead', () => {
    expect(mountWithKit(UiImage, { props: { src: '/a.png', alt: 'a' } }).classes()).toContain('object-cover')
    expect(mountWithKit(UiImage, { props: { src: '/a.png', alt: 'a', fit: UiImageFit.Contain } }).classes()).toContain('object-contain')
  })

  it.each([
    [UiImageRounding.Md, 'rounded-xl'],
    [UiImageRounding.Full, 'rounded-full'],
  ])('rounds %s with %s', (rounded, expected) => {
    const wrapper = mountWithKit(UiImage, { props: { src: '/a.png', alt: 'a', rounded } })

    expect(wrapper.classes()).toContain(expected)
  })

  it('has square corners by default', () => {
    const wrapper = mountWithKit(UiImage, { props: { src: '/a.png', alt: 'a' } })

    expect(wrapper.classes().some((name) => name.startsWith('rounded'))).toBe(false)
  })

  it('puts the testid on the image', () => {
    const wrapper = mountWithKit(UiImage, { props: { src: '/a.png', alt: 'a', testid: 'hero-image', loading: UiImageLoading.Eager } })

    expect(wrapper.attributes('data-testid')).toBe('hero-image')
  })

  it('renders a plain img for a URL the optimiser cannot fetch', () => {
    const wrapper = mountWithKit(UiImage, { props: { src: '/api/uploads/logo.png', alt: 'Logo', unoptimised: true, testid: 'logo' } })

    expect(wrapper.element.tagName).toBe('IMG')
    expect(wrapper.attributes('src')).toBe('/api/uploads/logo.png')
    expect(wrapper.attributes('data-testid')).toBe('logo')
  })
})
