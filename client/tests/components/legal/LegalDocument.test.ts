/**
 * Unit tests for LegalDocument: the hero, the numbered or icon-led sections with every content
 * kind they can hold, and the optional closing contact card.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import LegalDocument from '~/components/legal/LegalDocument.vue'
import type { LegalSection } from '~/types/legalsection'

/** A document with one plain section and one section that uses every content kind. */
const SECTIONS: LegalSection[] = [
  { id: 'intro', title: 'Introduction', paragraphs: ['First paragraph', 'Second paragraph'] },
  {
    id: 'full',
    title: 'Everything',
    icon: 'mdi:database',
    paragraphs: ['Body'],
    items: ['One', 'Two'],
    cards: [{ title: 'Card title', description: 'Card text' }],
    note: 'Remember this',
    link: { label: 'a@b.test', href: 'mailto:a@b.test' },
  },
]

describe('LegalDocument', () => {
  beforeEach(() => {
    vi.stubGlobal('useLocalePath', () => (path: string) => `/en${path}`)
  })

  it('scopes a ruled, square-marker look to the classic template', () => {
    const wrapper = mountWithKit(LegalDocument, {
      props: { title: 'Terms', sections: SECTIONS, contactTitle: 'Questions?' },
    })

    expect(wrapper.find('[data-testid="legal-section-intro"]').classes()).toContain('[.tpl-classic_&]:border-classic-border')
    expect(wrapper.find('[data-testid="legal-section-marker"]').classes()).toContain('[.tpl-classic_&]:bg-classic-brand-soft')
    expect(wrapper.find('[data-testid="legal-contact"]').classes()).toContain('[.tpl-classic_&]:bg-classic-surface-2')
  })

  it('shows the title as the h1 and the last-updated line', () => {
    const wrapper = mountWithKit(LegalDocument, {
      props: { title: 'Terms', meta: 'Last Updated: 2026-03-12', sections: SECTIONS },
    })

    expect(wrapper.find('h1').text()).toBe('Terms')
    expect(wrapper.find('[data-testid="legal-last-updated"]').text()).toBe('Last Updated: 2026-03-12')
  })

  it('omits the last-updated line when none is given', () => {
    const wrapper = mountWithKit(LegalDocument, { props: { title: 'Terms', sections: SECTIONS } })

    expect(wrapper.find('[data-testid="legal-last-updated"]').exists()).toBe(false)
  })

  it('renders one block per section, in order, with its paragraphs', () => {
    const wrapper = mountWithKit(LegalDocument, { props: { title: 'Terms', sections: SECTIONS } })

    const intro = wrapper.find('[data-testid="legal-section-intro"]')
    expect(intro.find('h2').text()).toBe('Introduction')
    expect(intro.text()).toContain('First paragraph')
    expect(intro.text()).toContain('Second paragraph')
    expect(wrapper.find('[data-testid="legal-section-full"]').exists()).toBe(true)
  })

  it('numbers a section without an icon and shows the icon of one that has it', () => {
    const wrapper = mountWithKit(LegalDocument, { props: { title: 'Terms', sections: SECTIONS } })

    const markers = wrapper.findAll('[data-testid="legal-section-marker"]')
    expect(markers[0]?.text()).toBe('1')
    expect(markers[1]?.find('[data-icon="mdi:database"]').exists()).toBe(true)
  })

  it('renders entries, cards, the note and the link of a section', () => {
    const wrapper = mountWithKit(LegalDocument, { props: { title: 'Terms', sections: SECTIONS } })

    const full = wrapper.find('[data-testid="legal-section-full"]')
    expect(full.find('[data-testid="legal-section-items"]').findAll('li')).toHaveLength(2)
    expect(full.find('[data-testid="legal-section-card"]').text()).toContain('Card title')
    expect(full.find('[data-testid="legal-section-note"]').text()).toContain('Remember this')
    expect(full.find('[data-testid="legal-section-link"]').attributes('href')).toBe('mailto:a@b.test')
  })

  it('draws a checklist without bullets', () => {
    const wrapper = mountWithKit(LegalDocument, {
      props: { title: 'Cookies', sections: [{ id: 'uses', title: 'Uses', items: ['A'], checklist: true }] },
    })

    expect(wrapper.find('[data-testid="legal-section-items"]').classes()).not.toContain('list-disc')
  })

  it('draws a bulleted list when the section is not a checklist', () => {
    const wrapper = mountWithKit(LegalDocument, {
      props: { title: 'Privacy', sections: [{ id: 'data', title: 'Data', items: ['A'] }] },
    })

    expect(wrapper.find('[data-testid="legal-section-items"]').classes()).toContain('list-disc')
  })

  it('has no contact card unless one is worded', () => {
    const wrapper = mountWithKit(LegalDocument, { props: { title: 'Cookies', sections: SECTIONS } })

    expect(wrapper.find('[data-testid="legal-contact"]').exists()).toBe(false)
  })

  it('links the contact card to the localised contact page', () => {
    const wrapper = mountWithKit(LegalDocument, {
      props: {
        title: 'Terms',
        sections: SECTIONS,
        contactTitle: 'Questions?',
        contactDescription: 'Write to us',
        contactLink: 'Contact us',
      },
    })

    const card = wrapper.find('[data-testid="legal-contact"]')
    expect(card.text()).toContain('Questions?')
    expect(card.text()).toContain('Write to us')
    expect(wrapper.find('[data-testid="legal-contact-link"]').attributes('href')).toBe('/en/contact')
  })
})
