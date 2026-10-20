/**
 * Fixtures for the testimonials flow: one testimonial as `GET /api/admin/testimonials` returns it.
 */
import type { Testimonial } from '../../src/types/testimonial'

/** An active testimonial with an English and a Russian quote. */
export const annTestimonial: Testimonial = {
  id: 31,
  name: 'Ann Lee',
  company: 'Acme',
  avatarUrl: '',
  rating: 5,
  sortOrder: 0,
  isActive: true,
  publishedAt: null,
  createdAt: '2026-01-01T00:00:00Z',
  translations: {
    en: { role: 'CTO', quote: 'Fast and calm hosting.' },
    ru: { role: 'Технический директор', quote: 'Быстрый и спокойный хостинг.' },
  },
}
