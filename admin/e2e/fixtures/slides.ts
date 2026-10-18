/**
 * Fixtures for the slides list: one slide with an English translation as `GET /api/admin/slides`
 * returns it.
 */
import type { Slide } from '../../src/types/slide'

/** An active slide for every audience. */
export const welcomeSlide: Slide = {
  id: 21,
  iconName: 'rocket',
  brandColor: '#6366f1',
  imageUrl: '',
  demoUrl: null,
  learnMoreUrl: null,
  productId: null,
  productName: null,
  sortOrder: 1,
  isActive: true,
  targetAudience: 'All',
  visibleFrom: null,
  visibleUntil: null,
  createdAt: '2026-10-01T09:00:00Z',
  translations: [
    {
      locale: 'en',
      title: 'Welcome',
      tagline: 'Start here',
      description: null,
      features: null,
      ctaText: null,
      ctaUrl: null,
    },
  ],
}
