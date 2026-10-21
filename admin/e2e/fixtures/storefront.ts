/**
 * Neutral fixtures for the storefront content flows (partners, projects, FAQ), as the admin API
 * returns them.
 */
import type { Faq } from '../../src/types/faq'
import type { Partner } from '../../src/types/partner'
import type { ServerLocation } from '../../src/types/serverlocation'
import type { Project } from '../../src/types/project'
import { FaqCategory } from '../../src/types/storefrontenums'

/** An active partner. */
export const acmePartner: Partner = {
  id: 51,
  translations: { en: { name: 'Acme Partner' } },
  logoUrl: '',
  url: 'https://partner.example.com',
  sortOrder: 0,
  isActive: true,
  createdAt: '2026-01-01T00:00:00Z',
}

/** An active server location with English text. */
export const frankfurtLocation: ServerLocation = {
  id: 81,
  countryCode: 'DE',
  sortOrder: 0,
  isActive: true,
  translations: { en: { name: 'Frankfurt', city: 'Frankfurt am Main', latencyNote: '20 ms to Europe' } },
}

/** An active project with English text. */
export const sampleProject: Project = {
  id: 61,
  slug: 'sample-site',
  imageUrls: [],
  url: 'https://project.example.com',
  sortOrder: 0,
  isActive: true,
  createdAt: '2026-01-01T00:00:00Z',
  translations: { en: { title: 'Sample Site', summary: 'A sample site', description: 'Longer text.' } },
}

/** An active FAQ entry with English text. */
export const sampleFaq: Faq = {
  id: 71,
  category: FaqCategory.General,
  sortOrder: 0,
  isActive: true,
  createdAt: '2026-01-01T00:00:00Z',
  translations: { en: { question: 'How does it work?', answer: 'Like this.' } },
}
