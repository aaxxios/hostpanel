/**
 * Unit tests for ClassicHeader: the classic template's header slot renders the shared site
 * header and nothing else.
 */
import { describe, expect, it } from 'vitest'
import { defineComponent, h } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ClassicHeader from '~/templates/classic/layout/ClassicHeader.vue'

/** Stand-in for the shared header so the test only proves the slot wires it in. */
const LayoutHeaderStub = defineComponent({
  name: 'LayoutHeader',
  setup: () => () => h('header', { 'data-testid': 'layout-header-stub' }),
})

describe('ClassicHeader', () => {
  it('renders the shared site header', () => {
    const wrapper = mountWithKit(ClassicHeader, { components: { LayoutHeader: LayoutHeaderStub } })

    expect(wrapper.find('[data-testid="layout-header-stub"]').exists()).toBe(true)
  })
})
