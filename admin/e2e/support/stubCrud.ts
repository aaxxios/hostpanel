/**
 * In-memory CRUD stub for the storefront content specs (testimonials, partners, projects, FAQ).
 *
 * All four endpoint sets answer the same way: the collection lists, a POST stores the body under
 * a fixed id, a PUT replaces an entry and a DELETE removes it. Writing that once keeps the specs
 * to what differs: the entity, its path and what the page shows.
 */
import type { Page } from '@playwright/test'
import type { ApiPath } from '../../src/lib/apiPaths'
import { HttpMethod } from '../../src/types/http'
import { stubApi } from './stubApi'
import type { RecordedCall, StubApi, StubRoute } from './stubApi'

/** Creation time the stub stamps on every entry it stores. */
export const CREATED_AT = '2026-01-01T00:00:00Z'

/** Prefix the same-origin proxy mounts the backend under, as the page sees it. */
const API_PREFIX = '/api'

/** What a stored entry has to carry for the stub to address and stamp it. */
export interface StubEntity {
  /** Entry ID. */
  id: number
  /** Creation time. */
  createdAt: string
}

/** Options of {@link stubCrud}. */
export interface StubCrudOptions<T extends StubEntity> {
  /** Route root of the entries, e.g. `ApiPath.AdminFaqs`. */
  root: ApiPath
  /** The list the "database" holds; mutated by the writes. */
  stored: T[]
  /** Id the stub gives the entry a POST creates. */
  newId: number
}

/**
 * Path of the collection endpoint as the page requests it.
 *
 * @param root - Route root of the entries.
 * @returns The path including the `/api` prefix.
 */
export const collectionPath = (root: ApiPath): string => `${API_PREFIX}${root}`

/**
 * Stubs the collection and member endpoints of one content type over an in-memory list, so a
 * write shows up in the next read.
 *
 * @param page - The page under test.
 * @param options - Route root, the stored list and the id a creation gets.
 * @returns The recorder handle.
 */
export const stubCrud = <T extends StubEntity>(page: Page, options: StubCrudOptions<T>): Promise<StubApi> => {
  const { root, stored, newId } = options
  const collection = collectionPath(root)
  const item = new RegExp(`^${collection}/\\d+$`)

  /**
   * Reads the entry id off a member path.
   *
   * @param call - The recorded member request.
   * @returns The id in the last path segment.
   */
  const idOf = (call: RecordedCall): number => Number(call.path.split('/').pop())

  const routes: StubRoute[] = [
    { method: HttpMethod.Get, path: collection, respond: () => ({ body: stored }) },
    {
      method: HttpMethod.Post,
      path: collection,
      respond: (call) => {
        stored.push({ ...(call.body as object), id: newId, createdAt: CREATED_AT } as T)
        return { body: newId }
      },
    },
    {
      method: HttpMethod.Put,
      path: item,
      respond: (call) => {
        const id = idOf(call)
        const index = stored.findIndex((entry) => entry.id === id)
        stored[index] = { ...(call.body as object), id, createdAt: CREATED_AT } as T
        return { status: 204 }
      },
    },
    {
      method: HttpMethod.Delete,
      path: item,
      respond: (call) => {
        const id = idOf(call)
        stored.splice(0, stored.length, ...stored.filter((entry) => entry.id !== id))
        return { status: 204 }
      },
    },
  ]

  return stubApi(page, { signedIn: true, routes })
}
