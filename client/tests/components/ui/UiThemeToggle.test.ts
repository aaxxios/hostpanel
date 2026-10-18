/**
 * Unit tests for ThemeToggle: hidden when the operator disallows switching, otherwise a button
 * that toggles the mode and names the mode it would switch to once mounted.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { nextTick, ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ThemeToggle from '~/components/ui/UiThemeToggle.vue'

/** The color-mode state the stubbed composable exposes. */
const state = { isDark: ref(false), userToggleEnabled: ref(true), toggle: vi.fn() }

describe('ThemeToggle', () => {
  beforeEach(() => {
    state.isDark.value = false
    state.userToggleEnabled.value = true
    state.toggle.mockClear()
    vi.stubGlobal('useAppColorMode', () => state)
  })

  it('renders nothing when the operator disallows switching', () => {
    state.userToggleEnabled.value = false

    const wrapper = mountWithKit(ThemeToggle)

    expect(wrapper.find('[data-testid="theme-toggle"]').exists()).toBe(false)
  })

  it('renders a button that toggles the mode', async () => {
    const wrapper = mountWithKit(ThemeToggle)

    await wrapper.find('[data-testid="theme-toggle"]').trigger('click')

    expect(state.toggle).toHaveBeenCalledTimes(1)
  })

  it('offers dark mode while light, and light mode while dark, once mounted', async () => {
    const wrapper = mountWithKit(ThemeToggle)
    await nextTick()
    expect(wrapper.find('[data-testid="theme-toggle"]').attributes('aria-label')).toBe('ui.theme.switchDark')

    state.isDark.value = true
    await nextTick()

    expect(wrapper.find('[data-testid="theme-toggle"]').attributes('aria-label')).toBe('ui.theme.switchLight')
  })

  it('draws both icons and lets the dark variant pick one', () => {
    const wrapper = mountWithKit(ThemeToggle)

    expect(wrapper.findAll('svg')).toHaveLength(2)
  })
})
