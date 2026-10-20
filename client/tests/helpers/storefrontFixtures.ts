/**
 * Neutral fixtures for the storefront content the API serves: testimonials, partners, projects
 * and FAQs. Names are placeholders ("Example Co", `example.test`), never a real company.
 *
 * @module tests/helpers/storefrontFixtures
 */

import { FaqCategory } from '~/types/faqcategory'
import type { FAQ } from '~/types/faq'
import type { Partner } from '~/types/partner'
import type { Project } from '~/types/project'
import type { Testimonial } from '~/types/testimonial'

/**
 * Builds a question.
 *
 * @param id The question id.
 * @param overrides Fields that differ from the defaults.
 * @returns The question.
 */
export const faqFixture = (id: number, overrides: Partial<FAQ> = {}): FAQ => ({
  id,
  category: FaqCategory.General,
  question: `Example question ${id}?`,
  answer: `Example answer ${id}.`,
  sortOrder: id,
  ...overrides,
})

/**
 * Builds a testimonial.
 *
 * @param id The testimonial id.
 * @param overrides Fields that differ from the defaults.
 * @returns The testimonial.
 */
export const testimonialFixture = (id: number, overrides: Partial<Testimonial> = {}): Testimonial => ({
  id,
  name: `Example Person ${id}`,
  company: 'Example Co',
  role: 'Founder',
  avatarUrl: null,
  quote: `Example quote ${id}.`,
  rating: 5,
  sortOrder: id,
  publishedAt: '2026-03-10T00:00:00+00:00',
  ...overrides,
})

/**
 * Builds a partner.
 *
 * @param id The partner id.
 * @param overrides Fields that differ from the defaults.
 * @returns The partner.
 */
export const partnerFixture = (id: number, overrides: Partial<Partner> = {}): Partner => ({
  id,
  name: `Example Co ${id}`,
  logoUrl: null,
  url: 'https://example.test',
  sortOrder: id,
  ...overrides,
})

/**
 * Builds a project.
 *
 * @param id The project id.
 * @param overrides Fields that differ from the defaults.
 * @returns The project.
 */
export const projectFixture = (id: number, overrides: Partial<Project> = {}): Project => ({
  id,
  slug: `example-project-${id}`,
  title: `Example project ${id}`,
  summary: null,
  description: null,
  imageUrls: [],
  url: 'https://example.test',
  sortOrder: id,
  ...overrides,
})
