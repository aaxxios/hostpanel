/**
 * Unit tests for ClassicIconTile: a flat brand-tinted square carrying one icon.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ClassicIconTile from '~/components/classic/ClassicIconTile.vue'

describe('ClassicIconTile', () => {
  it('draws the named icon', () => {
    const wrapper = mountWithKit(ClassicIconTile, { props: { icon: 'mdi:lock-check' } })

    expect(wrapper.find('[data-icon="mdi:lock-check"]').exists()).toBe(true)
  })

  it('uses the soft brand tint and the solid brand colour, with no accent variable or glow', () => {
    const wrapper = mountWithKit(ClassicIconTile, { props: { icon: 'mdi:web' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['bg-classic-brand-soft', 'text-classic-brand']))
    expect(wrapper.html()).not.toMatch(/--accent|shadow-\[/)
  })
})
