/**
 * Pinia store for the storefront FAQ entries.
 *
 * Wraps the shared CRUD behaviour (`useStorefrontCrud`) around `useFaqsApi`. The only caller of
 * that API and of the image upload for this content type; a refusal is kept as the API's own
 * sentence, never thrown.
 */

import { defineStore } from 'pinia'
import { useFaqsApi } from '@/composables/apis/useFaqsApi'
import { useStorefrontCrud } from '@/composables/useStorefrontCrud'

/** Manages the storefront FAQ entries state and CRUD operations. */
export const useFaqsStore = defineStore('faqs', () => useStorefrontCrud(useFaqsApi()))
