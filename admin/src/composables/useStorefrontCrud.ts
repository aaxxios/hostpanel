/**
 * State and actions shared by the storefront content stores (testimonials, partners, projects,
 * FAQ): the list, the load and refusal sentences, create / update / delete / reorder, and the
 * image upload. Each store fixes the endpoint set and the types; the behaviour lives once, here.
 */

import { ref } from 'vue'
import type { Ref } from 'vue'
import type { StorefrontApi } from '@/composables/apis/useStorefrontApi'
import { useUploadApi } from '@/composables/apis/useUploadApi'
import { runAction } from '@/lib/runAction'
import { ImageUploadScope } from '@/types/upload'
import type { StorefrontEntity, StorefrontOrderItem } from '@/types/storefront'

/** What {@link useStorefrontCrud} hands the store that wraps it. */
export interface StorefrontCrud<TItem, TPayload> {
  /** All entries from the admin API. */
  items: Ref<TItem[]>
  /** True while the list is loading. */
  loading: Ref<boolean>
  /** Sentence for the last failed load, null when none. */
  error: Ref<string | null>
  /** Sentence for the last failed create, update, delete or reorder, null when none. */
  actionError: Ref<string | null>
  /** Per-field messages of the last refused create or update, keyed by camelCase field name. */
  fieldErrors: Ref<Record<string, string>>
  /** Sentence for the last failed image upload, null when none. */
  uploadError: Ref<string | null>
  /** Clears every refusal sentence and field message, so a previous page's errors do not carry over. */
  resetErrors: () => void
  /** Loads every entry. */
  fetchAll: () => Promise<void>
  /** Finds one entry, loading the list first when it is empty. */
  fetchOne: (id: number) => Promise<TItem | undefined>
  /** Creates an entry; answers its id, or null when refused. */
  create: (payload: TPayload) => Promise<number | null>
  /** Updates an entry; answers false when refused. */
  update: (id: number, payload: TPayload) => Promise<boolean>
  /** Deletes an entry; answers false when refused. */
  remove: (id: number) => Promise<boolean>
  /** Saves the display order; answers false when refused. */
  updateOrder: (items: StorefrontOrderItem[]) => Promise<boolean>
  /** Uploads an image; answers its URL, or null when refused. */
  uploadImage: (file: File) => Promise<string | null>
}

/**
 * Builds the state and actions of one storefront content store.
 *
 * @param api - The content type's endpoint functions.
 * @returns The state refs and the actions; a refusal is kept as the API's own sentence, never thrown.
 */
export const useStorefrontCrud = <TItem extends StorefrontEntity, TPayload>(
  api: StorefrontApi<TItem, TPayload>,
): StorefrontCrud<TItem, TPayload> => {
  const { uploadImage: upload } = useUploadApi()

  /** All entries from the admin API. */
  const items = ref<TItem[]>([]) as Ref<TItem[]>

  /** True while the list is loading. */
  const loading = ref(false)

  /** Sentence for the last failed load, null when none. */
  const error = ref<string | null>(null)

  /** Sentence for the last failed create, update, delete or reorder, null when none. */
  const actionError = ref<string | null>(null)

  /** Per-field messages of the last refused create or update; empty otherwise. */
  const fieldErrors = ref<Record<string, string>>({})

  /** Sentence for the last failed image upload, null when none. */
  const uploadError = ref<string | null>(null)

  /** Clears every refusal sentence and field message. */
  const resetErrors = (): void => {
    error.value = null
    actionError.value = null
    fieldErrors.value = {}
    uploadError.value = null
  }

  /**
   * Loads every entry from the admin API.
   *
   * @returns Resolves when the request is over; a refusal is left in `error`.
   */
  const fetchAll = async (): Promise<void> => {
    loading.value = true
    const result = await runAction(error, api.list)
    loading.value = false
    if (result.ok) {
      items.value = result.value
    }
  }

  /**
   * Finds one entry; the cached list answers when it has been loaded.
   *
   * @param id - Entry ID.
   * @returns The entry, or undefined when there is none.
   */
  const fetchOne = async (id: number): Promise<TItem | undefined> => {
    if (items.value.length === 0) {
      await fetchAll()
    }
    return items.value.find((item) => item.id === id)
  }

  /**
   * Runs one mutating request. The list is reloaded either way: on success it shows the change, on a
   * refusal it restores the server's order, which a drag may already have rearranged on screen.
   *
   * @param task - The request to run.
   * @returns The request's value, or null when refused (see `actionError`).
   */
  const mutate = async <T>(task: () => Promise<T>): Promise<{ value: T } | null> => {
    const result = await runAction(actionError, task, fieldErrors)
    await fetchAll()
    return result.ok ? { value: result.value } : null
  }

  /**
   * Creates an entry and refreshes the list.
   *
   * @param payload - Entry data.
   * @returns The new entry's ID, or null when refused.
   */
  const create = async (payload: TPayload): Promise<number | null> => {
    const done = await mutate(() => api.create(payload))
    return done ? done.value : null
  }

  /**
   * Updates an entry and refreshes the list.
   *
   * @param id - Entry ID.
   * @param payload - Updated entry data.
   * @returns True when updated; false when refused.
   */
  const update = async (id: number, payload: TPayload): Promise<boolean> => {
    const done = await mutate(() => api.update(id, payload))
    return done !== null
  }

  /**
   * Deletes an entry and refreshes the list.
   *
   * @param id - Entry ID.
   * @returns True when deleted; false when refused.
   */
  const remove = async (id: number): Promise<boolean> => {
    const done = await mutate(() => api.remove(id))
    return done !== null
  }

  /**
   * Saves the display order of several entries and refreshes the list.
   *
   * @param order - Entry ID and new sort order pairs.
   * @returns True when saved; false when refused.
   */
  const updateOrder = async (order: StorefrontOrderItem[]): Promise<boolean> => {
    const done = await mutate(() => api.reorder(order))
    return done !== null
  }

  /**
   * Uploads an image and returns where it is served from.
   *
   * @param file - The image file.
   * @returns The stored image's URL, or null when refused (see `uploadError`).
   */
  const uploadImage = async (file: File): Promise<string | null> => {
    const result = await runAction(uploadError, async () => (await upload(ImageUploadScope.Storefront, file)).url)
    return result.ok ? result.value : null
  }

  return { items, loading, error, actionError, fieldErrors, uploadError, resetErrors, fetchAll, fetchOne, create, update, remove, updateOrder, uploadImage }
}
