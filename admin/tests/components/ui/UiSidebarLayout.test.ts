/**
 * Tests for UiSidebarLayout: the sidebar slot beside the scrolling content slot.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiSidebarLayout from '@/components/ui/UiSidebarLayout.vue'

describe('UiSidebarLayout', () => {
  it('renders the sidebar and the content, content scrolling on its own', () => {
    const wrapper = mount(UiSidebarLayout, {
      slots: {
        sidebar: '<nav data-testid="side">side</nav>',
        default: '<p data-testid="content">content</p>',
      },
    })

    expect(wrapper.find('[data-testid="side"]').exists()).toBe(true)
    expect(wrapper.get('[data-testid="content"]').element.parentElement?.classList.contains('overflow-auto')).toBe(true)
  })

  it('puts the sidebar before the content', () => {
    const wrapper = mount(UiSidebarLayout, {
      slots: { sidebar: '<nav data-testid="side" />', default: '<p data-testid="content" />' },
    })

    expect(wrapper.element.firstElementChild?.getAttribute('data-testid')).toBe('side')
  })
})
