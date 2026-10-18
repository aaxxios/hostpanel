/**
 * Unit tests for ClientLayoutTopBar: welcome text, the widget mount points and the mobile menu
 * button.
 *
 * @module tests/components/client/layout/TopBar.test
 */

import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import TopBar from '~/components/client/layout/TopBar.vue'

/** The theme and language controls need runtime services this test does not provide. */
const STUBS = { UiThemeToggle: true, UiLanguageSwitcher: true }

describe('ClientLayoutTopBar', () => {
  it('greets the signed-in person by name', () => {
    const wrapper = mountClient(TopBar, { props: { fullName: 'Ada Lovelace', sidebarOpen: false }, stubs: STUBS })

    expect(wrapper.find('[data-testid="client-topbar-name"]').text()).toBe('Ada Lovelace')
  })

  it('hands the person to both widget mount points', () => {
    const user = JSON.stringify({ email: 'ada@example.com' })
    const wrapper = mountClient(TopBar, { props: { fullName: 'Ada', widgetUser: user, sidebarOpen: false }, stubs: STUBS })

    expect(wrapper.find('#inno-launcher-mount').attributes('data-user')).toBe(user)
    expect(wrapper.find('#inno-account-mount').attributes('data-user')).toBe(user)
  })

  it('leaves data-user off while the profile is loading', () => {
    const wrapper = mountClient(TopBar, { props: { fullName: '', sidebarOpen: false }, stubs: STUBS })

    expect(wrapper.find('#inno-account-mount').attributes('data-user')).toBeUndefined()
  })

  it('emits toggleSidebar from the menu button', async () => {
    const wrapper = mountClient(TopBar, { props: { fullName: 'Ada', sidebarOpen: false }, stubs: STUBS })

    await wrapper.find('[data-testid="client-menu-toggle"]').trigger('click')

    expect(wrapper.emitted('toggleSidebar')).toHaveLength(1)
  })

  it('links the mobile brand to the home page', () => {
    const wrapper = mountClient(TopBar, { props: { fullName: 'Ada', sidebarOpen: false }, stubs: STUBS })

    expect(wrapper.find('[data-testid="client-topbar-brand"]').attributes('href')).toBe('/')
  })
})
