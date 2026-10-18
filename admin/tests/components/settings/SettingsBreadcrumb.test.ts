/**
 * Tests for SettingsBreadcrumb: ancestors are links, the last step is the current page.
 */
import { describe, expect, it } from 'vitest'
import { mountScreen } from '../../helpers/mount'
import SettingsBreadcrumb from '@/components/settings/SettingsBreadcrumb.vue'

describe('SettingsBreadcrumb', () => {
  it('links every step but the last and marks the last as the current page', async () => {
    const { wrapper } = await mountScreen(SettingsBreadcrumb, {
      items: [
        { label: 'Settings', to: '/settings' },
        { label: 'Slides', to: '/settings/slides' },
        { label: 'Add slide' },
      ],
    })

    const links = wrapper.findAll('a')
    expect(links.map((link) => link.text())).toEqual(['Settings', 'Slides'])
    expect(links.map((link) => link.attributes('href'))).toEqual(['/settings', '/settings/slides'])
    expect(wrapper.get('[aria-current="page"]').text()).toBe('Add slide')
  })

  it('is a labelled navigation landmark', async () => {
    const { wrapper } = await mountScreen(SettingsBreadcrumb, { items: [{ label: 'Only' }] })

    expect(wrapper.get('nav').attributes('aria-label')).toBe('You are here:')
  })
})
