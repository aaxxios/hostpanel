/**
 * Unit tests for ThemeToggle: hidden when the operator disallows switching, otherwise a button
 * that toggles the mode and names the mode it would switch to once mounted.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { nextTick, ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ThemeToggle from '~/components/ui/UiThemeToggle.vue'

/** The color-mode state the stubbed composable exposes. */
const state = { isDark: ref(false), applied: ref<string | null>('light'), userToggleEnabled: ref(true), toggle: vi.fn() }

describe('ThemeToggle', () => {
  beforeEach(() => {
    state.isDark.value = false
    state.applied.value = 'light'
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

  it('names the action from the first render when the server already knew the mode', () => {
    state.isDark.value = true
    state.applied.value = 'dark'

    const wrapper = mountWithKit(ThemeToggle)

    expect(wrapper.find('[data-testid="theme-toggle"]').attributes('aria-label')).toBe('ui.theme.switchLight')
  })

  it('draws both icons and lets the dark variant pick one', () => {
    const wrapper = mountWithKit(ThemeToggle)

    expect(wrapper.findAll('svg')).toHaveLength(2)
  })

  it('shows the moon in dark mode and the sun in light mode, by class so SSR and hydration agree', () => {
    const wrapper = mountWithKit(ThemeToggle)

    expect(wrapper.find('[data-testid="theme-toggle-moon"]').classes()).toEqual(expect.arrayContaining(['hidden', 'dark:block']))
    expect(wrapper.find('[data-testid="theme-toggle-sun"]').classes()).toEqual(expect.arrayContaining(['block', 'dark:hidden']))
  })
})
