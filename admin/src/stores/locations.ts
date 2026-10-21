/**
 * Pinia store for the storefront server locations.
 *
 * Wraps the shared CRUD behaviour (`useStorefrontCrud`) around `useLocationsApi`. The only caller
 * of that API for this content type; a refusal is kept as the API's own sentence, never thrown.
 */

import { defineStore } from 'pinia'
import { useLocationsApi } from '@/composables/apis/useLocationsApi'
import { useStorefrontCrud } from '@/composables/useStorefrontCrud'

/** Manages the storefront server locations state and CRUD operations. */
export const useLocationsStore = defineStore('locations', () => useStorefrontCrud(useLocationsApi()))
