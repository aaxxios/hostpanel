/**
 * Unit tests for NovaFooter: link columns from content, and contact and social entries that
 * exist only when the operator filled them in.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaFooter from '~/templates/nova/layout/NovaFooter.vue'
import { FOOTER_COLUMNS } from '~/templates/nova/content'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'

describe('NovaFooter', () => {
  it('renders every column with all of its links', () => {
    stubTemplateGlobals()
    const wrapper = mountWithKit(NovaFooter)

    for (const column of FOOTER_COLUMNS) {
      for (const link of column.links) {
        expect(wrapper.find(`[data-testid="nova-footer-${column.key}-${link.key}"]`).attributes('href')).toBe(link.to)
      }
    }
  })

  it('publishes no contact entry and no social row on a fresh install', () => {
    stubTemplateGlobals()
    const wrapper = mountWithKit(NovaFooter)

    expect(wrapper.find('[data-testid="nova-footer-email"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="nova-footer-phone"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid^="nova-footer-social-"]').exists()).toBe(false)
    expect(wrapper.text()).not.toContain('nova.footer.social')
  })

  it('shows the contact entries the operator configured', () => {
    stubTemplateGlobals({ settings: { 'portal.contact.email': 'hi@acme.test', 'portal.contact.phone': '+374 (10) 123' } })
    const wrapper = mountWithKit(NovaFooter)

    expect(wrapper.find('[data-testid="nova-footer-email"]').attributes('href')).toBe('mailto:hi@acme.test')
    expect(wrapper.find('[data-testid="nova-footer-phone"]').attributes('href')).toBe('tel:+37410123')
  })

  it('shows only the social profiles that are set, building the Telegram URL from a handle', () => {
    stubTemplateGlobals({ settings: { 'portal.social.youtube': 'https://youtube.test/acme', 'portal.contact.telegram': 'acme' } })
    const wrapper = mountWithKit(NovaFooter)

    expect(wrapper.find('[data-testid="nova-footer-social-youtube"]').attributes('href')).toBe('https://youtube.test/acme')
    expect(wrapper.find('[data-testid="nova-footer-social-telegram"]').attributes('href')).toBe('https://t.me/acme')
    expect(wrapper.find('[data-testid="nova-footer-social-facebook"]').exists()).toBe(false)
  })

  it('opens social profiles in a new tab without handing over the opener', () => {
    stubTemplateGlobals({ settings: { 'portal.social.youtube': 'https://youtube.test/acme' } })
    const link = mountWithKit(NovaFooter).find('[data-testid="nova-footer-social-youtube"]')

    expect(link.attributes('target')).toBe('_blank')
    expect(link.attributes('rel')).toContain('noopener')
  })

  it('shows the tax id only when set', () => {
    stubTemplateGlobals({ settings: { 'portal.legal.tax_id': 'TIN-1' } })

    expect(mountWithKit(NovaFooter).text()).toContain('TIN-1')
  })
})
