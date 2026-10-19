/**
 * Pinia store for the storefront projects.
 *
 * Wraps the shared CRUD behaviour (`useStorefrontCrud`) around `useProjectsApi`. The only caller of
 * that API and of the image upload for this content type; a refusal is kept as the API's own
 * sentence, never thrown.
 */

import { defineStore } from 'pinia'
import { useProjectsApi } from '@/composables/apis/useProjectsApi'
import { useStorefrontCrud } from '@/composables/useStorefrontCrud'

/** Manages the storefront projects state and CRUD operations. */
export const useProjectsStore = defineStore('projects', () => useStorefrontCrud(useProjectsApi()))
