/**
 * Unit tests for LanguageSwitcher: the current language on the trigger, the menu, switching, the
 * placement and the responsive card layout.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import LanguageSwitcher from '~/components/ui/UiLanguageSwitcher.vue'
import { UiLanguageLayout, UiPlacement } from '~/utils/ui/enums'

/** The location the browser path assigns to; a hard reload cannot run in a unit test. */
const location = { href: '' }

describe('LanguageSwitcher', () => {
  beforeEach(() => {
    location.href = ''
    vi.stubGlobal('location', location)
    vi.stubGlobal('useI18n', () => ({
      t: (key: string) => key,
      locale: ref('ru'),
      locales: ref([
        { code: 'en', name: 'English' },
        { code: 'ru', name: 'Russian' },
        { code: 'hy', name: 'Armenian' },
      ]),
    }))
    vi.stubGlobal('useSwitchLocalePath', () => (code: string) => '/' + code)
  })

  it('shows the active language on the trigger', () => {
    const wrapper = mountWithKit(LanguageSwitcher)

    const trigger = wrapper.find('[data-testid="language-switcher-trigger"]')
    expect(trigger.text()).toBe('RU')
    expect(trigger.find('img').attributes('src')).toBe('/flags/ru.svg')
  })

  it('keeps the menu closed until the trigger is clicked', async () => {
    const wrapper = mountWithKit(LanguageSwitcher)
    expect(wrapper.find('[role="menu"]').exists()).toBe(false)

    await wrapper.find('[data-testid="language-switcher-trigger"]').trigger('click')

    expect(wrapper.find('[role="menu"]').exists()).toBe(true)
    expect(wrapper.findAll('[role="menuitem"]')).toHaveLength(3)
    expect(wrapper.find('[data-testid="language-switcher-trigger"]').attributes('aria-expanded')).toBe('true')
  })

  it('switches to the chosen language and closes the menu', async () => {
    const wrapper = mountWithKit(LanguageSwitcher)
    await wrapper.find('[data-testid="language-switcher-trigger"]').trigger('click')

    await wrapper.find('[data-testid="language-option-hy"]').trigger('click')
    await flushPromises()

    expect(location.href).toBe('/hy')
    expect(wrapper.find('[role="menu"]').exists()).toBe(false)
  })

  it('opens upward when placed on top', async () => {
    const wrapper = mountWithKit(LanguageSwitcher, { props: { placement: UiPlacement.Top } })

    await wrapper.find('[data-testid="language-switcher-trigger"]').trigger('click')

    expect(wrapper.find('[role="menu"]').classes()).toContain('bottom-full')
  })

  it('has no card row in the dropdown layout', () => {
    const wrapper = mountWithKit(LanguageSwitcher)

    expect(wrapper.find('[data-testid="language-card-en"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="language-switcher-trigger"]').classes()).not.toContain('hidden')
  })

  it('adds the card row and hides the trigger below xl in the responsive layout', () => {
    const wrapper = mountWithKit(LanguageSwitcher, { props: { layout: UiLanguageLayout.Responsive } })

    expect(wrapper.findAll('[data-testid^="language-card-"]')).toHaveLength(3)
    expect(wrapper.find('[data-testid="language-switcher-trigger"]').classes()).toEqual(expect.arrayContaining(['hidden', 'xl:flex']))
  })

  it('switches from a card', async () => {
    const wrapper = mountWithKit(LanguageSwitcher, { props: { layout: UiLanguageLayout.Responsive } })

    await wrapper.find('[data-testid="language-card-en"]').trigger('click')

    expect(location.href).toBe('/en')
  })
})
