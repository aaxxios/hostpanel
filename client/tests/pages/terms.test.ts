/**
 * Tests for the Terms of Service page: it hands its copy to LegalDocument section by section and
 * keeps itself out of search results.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountPage, registerPageComponents, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import type { PublicPageSpies } from '~/tests/helpers/publicPage'
import LegalDocument from '~/components/legal/LegalDocument.vue'
import Page from '~/pages/terms.vue'

/** Spies of the stood-in runtime, renewed before each test. */
let spies: PublicPageSpies

describe('pages/terms', () => {
  beforeEach(() => {
    spies = stubPublicPageRuntime()
    registerPageComponents({ LegalDocument })
  })

  it('shows the title, the badge and the dated last-updated line', async () => {
    const wrapper = await mountPage(Page)

    expect(wrapper.find('h1').text()).toBe('terms.title')
    expect(wrapper.find('[data-testid="ui-page-hero-badge"]').text()).toBe('terms.badge')
    expect(wrapper.find('[data-testid="legal-last-updated"]').text()).toBe('terms.lastUpdated: 2026-03-12')
  })

  it('renders the eight sections with their paragraphs', async () => {
    const wrapper = await mountPage(Page)

    for (const id of ['acceptance', 'services', 'accounts', 'intellectual', 'payment', 'termination', 'liability', 'changes']) {
      expect(wrapper.find(`[data-testid="legal-section-${id}"]`).exists()).toBe(true)
    }
    expect(wrapper.find('[data-testid="legal-section-services"]').text()).toContain('terms.sections.services.content.2')
    expect(wrapper.find('[data-testid="legal-section-acceptance"]').text()).not.toContain('terms.sections.acceptance.content.2')
  })

  it('closes with a contact card linking to the localised contact page', async () => {
    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="legal-contact"]').text()).toContain('terms.contact.title')
    expect(wrapper.find('[data-testid="legal-contact-link"]').attributes('href')).toBe('/en/contact')
  })

  it('asks search engines not to index it', async () => {
    await mountPage(Page)

    expect(spies.useSeoMeta).toHaveBeenCalledWith(expect.objectContaining({
      title: 'seo.legal.terms.title',
      robots: 'noindex, follow',
    }))
  })
})
