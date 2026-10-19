/**
 * Pinia store for the storefront partners.
 *
 * Wraps the shared CRUD behaviour (`useStorefrontCrud`) around `usePartnersApi`. The only caller of
 * that API and of the image upload for this content type; a refusal is kept as the API's own
 * sentence, never thrown.
 */

import { defineStore } from 'pinia'
import { usePartnersApi } from '@/composables/apis/usePartnersApi'
import { useStorefrontCrud } from '@/composables/useStorefrontCrud'

/** Manages the storefront partners state and CRUD operations. */
export const usePartnersStore = defineStore('partners', () => useStorefrontCrud(usePartnersApi()))
