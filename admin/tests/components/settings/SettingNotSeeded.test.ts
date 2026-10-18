/**
 * Tests for SettingNotSeeded: the warning shown instead of a control whose settings row does not
 * exist. It is plain text now — the message used to be rendered as HTML.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import SettingNotSeeded from '@/components/settings/SettingNotSeeded.vue'
import { createTestI18n } from '../../helpers/i18n'

describe('SettingNotSeeded', () => {
  it('names the missing key and the environment variable the portal falls back to', () => {
    const wrapper = mount(SettingNotSeeded, {
      props: { settingKey: 'portal.site.name', envName: 'NUXT_PUBLIC_PORTAL_SITE_NAME' },
      global: { plugins: [createTestI18n()] },
    })

    expect(wrapper.text()).toContain('portal.site.name')
    expect(wrapper.text()).toContain('NUXT_PUBLIC_PORTAL_SITE_NAME')
    expect(wrapper.text()).toContain('has not been seeded yet')
  })

  it('is announced as a warning and carries no markup of its own from the message', () => {
    const wrapper = mount(SettingNotSeeded, {
      props: { settingKey: 'a<b>c', envName: 'ENV' },
      global: { plugins: [createTestI18n()] },
    })

    expect(wrapper.attributes('role')).toBe('alert')
    expect(wrapper.find('b').exists()).toBe(false)
    expect(wrapper.text()).toContain('a<b>c')
  })
})
