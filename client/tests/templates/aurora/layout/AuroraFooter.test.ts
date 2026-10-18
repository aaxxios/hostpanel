/**
 * Unit tests for AuroraFooter: the link columns, and the newsletter, contact and social entries
 * that exist only when the operator configured them.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraFooter from '~/templates/aurora/layout/AuroraFooter.vue'
import { stubAuroraGlobals } from '../auroraGlobals'

describe('AuroraFooter', () => {
  it('renders the three link columns with their destinations', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraFooter)

    expect(wrapper.findAll('h3').map((title) => title.text())).toEqual([
      'aurora.footer.company',
      'aurora.footer.support',
      'aurora.footer.legal',
    ])
    expect(wrapper.find('a[href="/terms"]').exists()).toBe(true)
    expect(wrapper.find('a[href="/domains/transfer"]').exists()).toBe(true)
  })

  it('shows no newsletter form, no social row and no contact line on a fresh install', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraFooter)

    expect(wrapper.find('[data-testid="newsletter-form"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid^="footer-social-"]').exists()).toBe(false)
    expect(wrapper.find('a[href^="mailto:"]').exists()).toBe(false)
  })

  it('posts the newsletter form natively to the configured endpoint', () => {
    stubAuroraGlobals({ settings: { 'portal.newsletter.action_url': 'https://mail.test/subscribe' } })
    const form = mountWithKit(AuroraFooter).find('[data-testid="newsletter-form"]')

    expect(form.attributes('action')).toBe('https://mail.test/subscribe')
    expect(form.attributes('method')).toBe('post')
    expect(form.attributes('target')).toBe('_blank')
    expect(form.find('input').attributes('name')).toBe('email')
    expect(form.find('input').attributes('placeholder')).toBe('aurora.footer.newsletterPlaceholder')
  })

  it('warns about an obviously wrong newsletter address only after the field was left', async () => {
    stubAuroraGlobals({ settings: { 'portal.newsletter.action_url': 'https://mail.test/subscribe' } })
    const wrapper = mountWithKit(AuroraFooter)
    const input = wrapper.find('[data-testid="newsletter-email"]')

    await input.setValue('not-an-address')
    expect(wrapper.find('[data-testid="newsletter-invalid"]').exists()).toBe(false)

    await input.trigger('blur')
    expect(wrapper.find('[data-testid="newsletter-invalid"]').exists()).toBe(true)
  })

  it('shows the contact entries and tax id the operator filled in', () => {
    stubAuroraGlobals({
      settings: {
        'portal.contact.email': 'hi@acme.test',
        'portal.contact.phone': '+374 (10) 123',
        'portal.legal.tax_id': 'TIN-1',
      },
    })
    const wrapper = mountWithKit(AuroraFooter)

    expect(wrapper.find('a[href="mailto:hi@acme.test"]').exists()).toBe(true)
    expect(wrapper.find('a[href="tel:+37410123"]').text()).toBe('+374 (10) 123')
    expect(wrapper.text()).toContain('TIN-1')
  })

  it('shows only the social profiles that are set, building Telegram from a handle', () => {
    stubAuroraGlobals({ settings: { 'portal.social.youtube': 'https://youtube.test/acme', 'portal.contact.telegram': 'acme' } })
    const wrapper = mountWithKit(AuroraFooter)

    expect(wrapper.find('[data-testid="footer-social-yt"]').attributes('href')).toBe('https://youtube.test/acme')
    expect(wrapper.find('[data-testid="footer-social-tg"]').attributes('href')).toBe('https://t.me/acme')
    expect(wrapper.find('[data-testid="footer-social-fb"]').exists()).toBe(false)
  })

  it('opens social profiles in a new tab without handing over the opener', () => {
    stubAuroraGlobals({ settings: { 'portal.social.youtube': 'https://youtube.test/acme' } })
    const link = mountWithKit(AuroraFooter).find('[data-testid="footer-social-yt"]')

    expect(link.attributes('target')).toBe('_blank')
    expect(link.attributes('rel')).toContain('noopener')
  })

  it('shows the operator site name beside the mark and in the copyright line', () => {
    stubAuroraGlobals({ siteName: 'Acme Host' })
    const text = mountWithKit(AuroraFooter).text()

    expect(text.match(/Acme Host/g)).toHaveLength(2)
    expect(text).not.toContain('Innovayse')
  })
})
