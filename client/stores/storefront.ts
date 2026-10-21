/**
 * Pinia store for the storefront content the operator manages in the admin: testimonials,
 * partners, projects, FAQs and server locations.
 *
 * A server-rendered section wraps a load action in `useAsyncData(key, () => storefront.loadX(),
 * { watch: [locale] })` and renders store state, which `@pinia/nuxt` carries from the server
 * render to the browser. An empty list is a valid state: the section that renders it hides
 * itself. The store owns the loading flag and the per-section failure sentences, so a section holds no
 * `catch`; transport belongs to {@link useStorefrontApi}.
 *
 * @module stores/storefront
 */

import { defineStore } from 'pinia'
import { useStorefrontApi } from '~/composables/apis/useStorefrontApi'
import { apiErrorMessage } from '~/lib/errorHandler'
import type { FAQ } from '~/types/faq'
import type { FaqCategory } from '~/types/faqcategory'
import type { Partner } from '~/types/partner'
import type { Project } from '~/types/project'
import type { ServerLocation } from '~/types/serverlocation'
import { StorefrontSection } from '~/types/storefrontsection'
import type { Testimonial } from '~/types/testimonial'

/**
 * Central store for the storefront content.
 *
 * @returns The storefront state and the actions that change it.
 */
export const useStorefrontStore = defineStore('storefront', () => {
  const api = useStorefrontApi()

  /** The active testimonials; empty until {@link loadTestimonials}. */
  const testimonials = ref<Testimonial[]>([])

  /** The active partners; empty until {@link loadPartners}. */
  const partners = ref<Partner[]>([])

  /** The active server locations; empty until {@link loadLocations}. */
  const locations = ref<ServerLocation[]>([])

  /** The active portfolio projects; empty until {@link loadProjects}. */
  const projects = ref<Project[]>([])

  /** The active questions; empty until {@link loadFaqs}. */
  const faqs = ref<FAQ[]>([])

  /** Number of storefront reads in flight. */
  const inFlight = ref(0)

  /** Whether any storefront read is in flight. */
  const loading = computed(() => inFlight.value > 0)

  /**
   * The sentence for each section's last failed read; empty when that section has none to show.
   * Kept per section so a concurrent successful read of another section cannot clear it.
   */
  const errors = ref<Record<StorefrontSection, string>>({
    [StorefrontSection.Testimonials]: '',
    [StorefrontSection.Partners]: '',
    [StorefrontSection.Projects]: '',
    [StorefrontSection.Faqs]: '',
    [StorefrontSection.Locations]: ''
  })

  /**
   * Runs one storefront read with the loading flag and the failure sentence handled once.
   *
   * @param section - The section the read belongs to; its sentence is the one set or cleared.
   * @param read - The read to run.
   * @param fallback - What to return when the read fails.
   * @returns The read's result, or the fallback after a failure ({@link errors} then holds the
   * sentence for that section).
   */
  const guarded = async <T>(section: StorefrontSection, read: () => Promise<T>, fallback: T): Promise<T> => {
    inFlight.value += 1
    errors.value[section] = ''
    try {
      return await read()
    } catch (e: unknown) {
      errors.value[section] = apiErrorMessage(e)
      return fallback
    } finally {
      inFlight.value -= 1
    }
  }

  /**
   * Reads the testimonials in the active language.
   *
   * @returns The testimonials; empty after a failure.
   */
  const loadTestimonials = async (): Promise<Testimonial[]> => {
    testimonials.value = await guarded(StorefrontSection.Testimonials, () => api.fetchTestimonials(), [] as Testimonial[])
    return testimonials.value
  }

  /**
   * Reads the partners in the active language.
   *
   * @returns The partners; empty after a failure.
   */
  const loadPartners = async (): Promise<Partner[]> => {
    partners.value = await guarded(StorefrontSection.Partners, () => api.fetchPartners(), [] as Partner[])
    return partners.value
  }

  /**
   * Reads the server locations in the active language.
   *
   * @returns The locations; empty after a failure.
   */
  const loadLocations = async (): Promise<ServerLocation[]> => {
    locations.value = await guarded(StorefrontSection.Locations, () => api.fetchLocations(), [] as ServerLocation[])
    return locations.value
  }

  /**
   * Reads the portfolio projects in the active language.
   *
   * @returns The projects; empty after a failure.
   */
  const loadProjects = async (): Promise<Project[]> => {
    projects.value = await guarded(StorefrontSection.Projects, () => api.fetchProjects(), [] as Project[])
    return projects.value
  }

  /**
   * Reads the questions in the active language.
   *
   * @returns The questions; empty after a failure.
   */
  const loadFaqs = async (): Promise<FAQ[]> => {
    faqs.value = await guarded(StorefrontSection.Faqs, () => api.fetchFaqs(), [] as FAQ[])
    return faqs.value
  }

  /**
   * Picks the questions of one topic among those already read.
   *
   * @param category - The topic to keep.
   * @returns The questions of that topic, in display order.
   */
  const faqsIn = (category: FaqCategory): FAQ[] => faqs.value.filter(faq => faq.category === category)

  return {
    testimonials,
    partners,
    locations,
    projects,
    faqs,
    loading,
    errors,
    loadTestimonials,
    loadPartners,
    loadLocations,
    loadProjects,
    loadFaqs,
    faqsIn
  }
})
