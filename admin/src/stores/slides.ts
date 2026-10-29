/**
 * Pinia store for managing homepage slides.
 *
 * Provides CRUD operations and sort order management for the admin slides settings page. The
 * only caller of `useSlidesApi` and the slide upload; a refusal is kept as the API's own
 * sentence, never thrown.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useSlidesApi } from '@/composables/apis/useSlidesApi'
import { useUploadApi } from '@/composables/apis/useUploadApi'
import { runAction } from '@/lib/runAction'
import { ImageUploadScope } from '@/types/upload'
import type { CreateSlidePayload, Slide, SlideOrderItem, UpdateSlidePayload } from '@/types/slide'

/** Manages homepage slides state and CRUD operations. */
export const useSlidesStore = defineStore('slides', () => {
  const api = useSlidesApi()
  const { uploadImage: upload } = useUploadApi()

  /** All slides from the admin API. */
  const slides = ref<Slide[]>([])

  /** True while the list is loading. */
  const loading = ref(false)

  /** Sentence for the last failed load, null when none. */
  const error = ref<string | null>(null)

  /** Sentence for the last failed create, update, delete or reorder, null when none. */
  const actionError = ref<string | null>(null)

  /** Per-field messages of the last refused create or update, keyed by camelCase field name; empty otherwise. */
  const fieldErrors = ref<Record<string, string>>({})

  /** Sentence for the last failed image upload, null when none. */
  const uploadError = ref<string | null>(null)

  /**
   * Fetches all slides from the admin API.
   *
   * @returns Resolves when the request is over; a refusal is left in `error`.
   */
  const fetchSlides = async (): Promise<void> => {
    loading.value = true
    const result = await runAction(error, api.listSlides)
    loading.value = false
    if (result.ok) {
      slides.value = result.value
    }
  }

  /**
   * Uploads a slide image and returns where it is served from.
   *
   * @param file - The image file.
   * @returns The stored image's URL, or null when refused (see `uploadError`).
   */
  const uploadImage = async (file: File): Promise<string | null> => {
    const result = await runAction(uploadError, async () => (await upload(ImageUploadScope.Storefront, file)).url)
    return result.ok ? result.value : null
  }

  /**
   * Runs one mutating request and refreshes the list when it succeeds.
   *
   * @param task - The request to run.
   * @returns The request's value, or null when refused (see `actionError`).
   */
  const mutate = async <T>(task: () => Promise<T>): Promise<{ value: T } | null> => {
    const result = await runAction(actionError, task, fieldErrors)
    if (!result.ok) {
      return null
    }
    await fetchSlides()
    return { value: result.value }
  }

  /**
   * Creates a new slide and refreshes the list.
   *
   * @param payload - Slide data with translations.
   * @returns The new slide's ID, or null when refused (see `actionError`).
   */
  const createSlide = async (payload: CreateSlidePayload): Promise<number | null> => {
    const done = await mutate(() => api.createSlide(payload))
    return done ? done.value : null
  }

  /**
   * Updates an existing slide and refreshes the list.
   *
   * @param id - Slide ID.
   * @param payload - Updated slide data with translations.
   * @returns True when updated; false when refused (see `actionError`).
   */
  const updateSlide = async (id: number, payload: UpdateSlidePayload): Promise<boolean> => {
    const done = await mutate(() => api.updateSlide(id, payload))
    return done !== null
  }

  /**
   * Deletes a slide and refreshes the list.
   *
   * @param id - Slide ID to delete.
   * @returns True when deleted; false when refused (see `actionError`).
   */
  const deleteSlide = async (id: number): Promise<boolean> => {
    const done = await mutate(() => api.deleteSlide(id))
    return done !== null
  }

  /**
   * Updates sort order for multiple slides.
   *
   * @param items - Array of id + sortOrder pairs.
   * @returns True when saved; false when refused (see `actionError`).
   */
  const updateOrder = async (items: SlideOrderItem[]): Promise<boolean> => {
    const done = await mutate(() => api.updateSlideOrder(items))
    return done !== null
  }

  /**
   * Fetches a single slide by ID.
   *
   * Returns from the local cache if available, otherwise fetches all slides first.
   *
   * @param id - Slide ID to look up.
   * @returns The matching slide, or undefined if not found.
   */
  const fetchSlide = async (id: number): Promise<Slide | undefined> => {
    if (slides.value.length === 0) {
      await fetchSlides()
    }
    return slides.value.find(s => s.id === id)
  }

  return { slides, loading, error, actionError, fieldErrors, uploadError, fetchSlides, fetchSlide, createSlide, updateSlide, deleteSlide, updateOrder, uploadImage }
})
