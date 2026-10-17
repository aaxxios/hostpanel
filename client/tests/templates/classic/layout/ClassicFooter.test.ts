/**
 * Unit tests for ClassicFooter: the classic template's footer slot renders the shared site
 * footer and nothing else.
 */
import { describe, expect, it } from 'vitest'
import { defineComponent, h } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ClassicFooter from '~/templates/classic/layout/ClassicFooter.vue'

/** Stand-in for the shared footer so the test only proves the slot wires it in. */
const LayoutFooterStub = defineComponent({
  name: 'LayoutFooter',
  setup: () => () => h('footer', { 'data-testid': 'layout-footer-stub' }),
})

describe('ClassicFooter', () => {
  it('renders the shared site footer', () => {
    const wrapper = mountWithKit(ClassicFooter, { components: { LayoutFooter: LayoutFooterStub } })

    expect(wrapper.find('[data-testid="layout-footer-stub"]').exists()).toBe(true)
  })
})
