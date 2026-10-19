/**
 * Pinia store for the storefront testimonials.
 *
 * Wraps the shared CRUD behaviour (`useStorefrontCrud`) around `useTestimonialsApi`. The only caller of
 * that API and of the image upload for this content type; a refusal is kept as the API's own
 * sentence, never thrown.
 */

import { defineStore } from 'pinia'
import { useTestimonialsApi } from '@/composables/apis/useTestimonialsApi'
import { useStorefrontCrud } from '@/composables/useStorefrontCrud'

/** Manages the storefront testimonials state and CRUD operations. */
export const useTestimonialsStore = defineStore('testimonials', () => useStorefrontCrud(useTestimonialsApi()))
