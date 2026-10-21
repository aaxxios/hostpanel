/**
 * Endpoints under `/api/portal/public` for the storefront content the operator manages in the
 * admin: testimonials, partners, projects, FAQs and server locations.
 *
 * Every function returns a plain `Promise` through `apiFetch`, so `stores/storefront.ts` can call
 * them from a store action, including the action a server-rendered page wraps in
 * `useAsyncData`. The language travels as `X-Language`, stamped by `apiFetch`.
 *
 * @module composables/apis/useStorefrontApi
 */

import { apiFetch } from '~/composables/useApi'
import type { FAQ } from '~/types/faq'
import type { Partner } from '~/types/partner'
import type { Project } from '~/types/project'
import type { ServerLocation } from '~/types/serverlocation'
import type { Testimonial } from '~/types/testimonial'

/**
 * The storefront content surface, one function per endpoint.
 *
 * @returns The storefront endpoint functions.
 */
export const useStorefrontApi = () => {
  /**
   * Reads the active client testimonials.
   *
   * @returns The testimonials in display order.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchTestimonials = (): Promise<Testimonial[]> =>
    apiFetch<Testimonial[]>('/api/portal/public/testimonials')

  /**
   * Reads the active partners.
   *
   * @returns The partners in display order.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchPartners = (): Promise<Partner[]> =>
    apiFetch<Partner[]>('/api/portal/public/partners')

  /**
   * Reads the active server locations.
   *
   * @returns The locations in display order.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchLocations = (): Promise<ServerLocation[]> =>
    apiFetch<ServerLocation[]>('/api/portal/public/locations')

  /**
   * Reads the active portfolio projects.
   *
   * @returns The projects in display order.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchProjects = (): Promise<Project[]> =>
    apiFetch<Project[]>('/api/portal/public/projects')

  /**
   * Reads the active frequently asked questions.
   *
   * @returns The questions in display order.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchFaqs = (): Promise<FAQ[]> =>
    apiFetch<FAQ[]>('/api/portal/public/faqs')

  return {
    fetchTestimonials,
    fetchPartners,
    fetchLocations,
    fetchProjects,
    fetchFaqs
  }
}
