/**
 * Unit tests for NovaTestimonials: nothing without real quotes, one card per quote with them.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaTestimonials from '~/templates/nova/sections/NovaTestimonials.vue'

describe('NovaTestimonials', () => {
  it('renders no section while there is no source of quotes', () => {
    expect(mountWithKit(NovaTestimonials).find('h2').exists()).toBe(false)
  })

  it('renders each supplied quote with its attribution', () => {
    const wrapper = mountWithKit(NovaTestimonials, {
      props: {
        items: [
          { key: 'a', name: 'Ada', role: 'CTO', quote: 'Fast.' },
          { key: 'b', name: 'Bo', quote: 'Solid.' },
        ],
      },
    })
    const cards = wrapper.findAll('li')

    expect(cards).toHaveLength(2)
    expect(cards[0]!.find('blockquote').text()).toBe('Fast.')
    expect(cards[0]!.text()).toContain('CTO')
    expect(cards[1]!.text()).not.toContain('CTO')
  })
})
