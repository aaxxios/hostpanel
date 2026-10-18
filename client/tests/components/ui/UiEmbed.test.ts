/**
 * Unit tests for UiEmbed: the frame is titled, lazy, and sized by its height step.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiEmbed from '~/components/ui/UiEmbed.vue'
import { UiEmbedHeight } from '~/utils/ui/embed'

describe('UiEmbed', () => {
  it('renders an iframe with the given address and accessible name', () => {
    const wrapper = mountWithKit(UiEmbed, { props: { src: 'https://maps.test/x', title: 'Office map' } })

    const frame = wrapper.find('iframe')
    expect(frame.attributes('src')).toBe('https://maps.test/x')
    expect(frame.attributes('title')).toBe('Office map')
  })

  it('loads the frame lazily', () => {
    const wrapper = mountWithKit(UiEmbed, { props: { src: 'https://maps.test/x', title: 'Office map' } })

    expect(wrapper.find('iframe').attributes('loading')).toBe('lazy')
  })

  it('uses the default height step', () => {
    const wrapper = mountWithKit(UiEmbed, { props: { src: 'https://maps.test/x', title: 'Office map' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['h-48', 'sm:h-56', 'md:h-64']))
  })

  it('uses the tall height step', () => {
    const wrapper = mountWithKit(UiEmbed, {
      props: { src: 'https://maps.test/x', title: 'Office map', height: UiEmbedHeight.Lg },
    })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['h-64', 'md:h-96']))
  })
})
