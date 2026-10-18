/**
 * Tests for `lib/data.ts`: the marketing seed collections keep the invariants the sections that
 * render them rely on -- unique ids, the keys their locale text is looked up by, and sane numbers.
 *
 * @module tests/lib/data.test
 */

import { describe, expect, it } from 'vitest'
import { FaqCategory } from '~/types/faqcategory'
import { clients, faqs, processSteps, products, projects, teamStats, testimonials, timeline } from '~/lib/data'

/**
 * Collects the ids of a collection.
 *
 * @param items Entries with an `id`.
 * @returns The ids in order.
 */
const idsOf = (items: ReadonlyArray<{ id: string }>): string[] => items.map((item) => item.id)

describe('lib/data', () => {
  it.each([
    ['products', products],
    ['projects', projects],
    ['testimonials', testimonials],
    ['faqs', faqs],
    ['clients', clients],
    ['processSteps', processSteps],
  ])('%s is not empty and has unique ids', (_name, items) => {
    const ids = idsOf(items)

    expect(ids.length).toBeGreaterThan(0)
    expect(new Set(ids).size).toBe(ids.length)
  })

  it('reports positive team statistics', () => {
    expect(Object.values(teamStats).every((value) => value > 0)).toBe(true)
  })

  it('gives every product an image, a colour and a learn-more link', () => {
    for (const product of products) {
      expect(product.image).not.toBe('')
      expect(product.color).toMatch(/^#[0-9a-f]{6}$/i)
      expect(product.learnMoreUrl).toMatch(/^\//)
    }
  })

  it('gives every project at least one category, one image and a plausible year', () => {
    for (const project of projects) {
      expect(project.category.length).toBeGreaterThan(0)
      expect(project.images.length).toBeGreaterThan(0)
      expect(project.year).toBeGreaterThan(2000)
    }
  })

  it('names the author of every project that claims a testimonial', () => {
    for (const project of projects.filter((entry) => entry.hasTestimonial)) {
      expect(project.testimonialAuthor).toBeTruthy()
    }
  })

  it('rates every testimonial between one and five', () => {
    for (const testimonial of testimonials) {
      expect(testimonial.rating).toBeGreaterThanOrEqual(1)
      expect(testimonial.rating).toBeLessThanOrEqual(5)
    }
  })

  it('files every FAQ under a known category', () => {
    const known = Object.values(FaqCategory)

    expect(faqs.every((faq) => known.includes(faq.category))).toBe(true)
  })

  it('numbers the process steps consecutively from one', () => {
    expect(processSteps.map((step) => step.order)).toEqual(processSteps.map((_step, index) => index + 1))
  })

  it('lists the timeline in ascending years', () => {
    const years = timeline.map((event) => event.year)

    expect(years).toEqual([...years].sort((a, b) => a - b))
  })
})
