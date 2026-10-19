/**
 * Tests for the create / edit flow: edit mode from the route, loading the entry into the form,
 * the dirty flag, saving through the store and the refusal sentence.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { defineComponent, h, reactive } from 'vue'
import { useStorefrontApi } from '@/composables/apis/useStorefrontApi'
import { ApiPath } from '@/lib/apiPaths'
import { useStorefrontCrud } from '@/composables/useStorefrontCrud'
import { useStorefrontForm } from '@/composables/useStorefrontForm'
import type { StorefrontForm } from '@/composables/useStorefrontForm'
import { json, problem } from '../helpers/fetch'
import { mountScreen } from '../helpers/mount'
import { requestsOf, stubRouteTable } from '../helpers/routes'

/** An entry as the tests use it. */
interface Widget {
  /** Entry ID. */
  id: number
  /** Display order. */
  sortOrder: number
  /** Name. */
  name: string
}

/** What the host exposes to the test. */
interface Host {
  /** The reactive form state. */
  form: { name: string }
  /** The flow under test. */
  flow: StorefrontForm
}

/** The host created by the last mount. */
let host: Host | undefined

/**
 * The host created by the last mount.
 *
 * @returns The host.
 * @throws When no host has been mounted.
 */
const current = (): Host => {
  if (!host) {
    throw new Error('No host mounted.')
  }
  return host
}

/** Component that runs the composable the way a form view does. */
const Harness = defineComponent({
  setup: () => {
    const store = useStorefrontCrud<Widget, { name: string }>(useStorefrontApi<Widget, { name: string }>(ApiPath.AdminFaqs))
    const form = reactive({ name: '' })
    const flow = useStorefrontForm<Widget, { name: string }>({
      store: {
        fetchOne: store.fetchOne,
        create: store.create,
        update: store.update,
        get actionError() {
          return store.actionError.value
        },
        get error() {
          return store.error.value
        },
        resetErrors: store.resetErrors,
      },
      form,
      listPath: '/list',
      populate: (item) => {
        form.name = item.name
      },
      build: () => ({ name: form.name }),
    })
    host = { form, flow }
    return () => h('div')
  },
})

afterEach(() => {
  vi.unstubAllGlobals()
  host = undefined
})

describe('useStorefrontForm', () => {
  it('create mode: saving posts the payload and returns to the list', async () => {
    const mock = stubRouteTable({ 'POST /admin/faqs': () => json(5), 'GET /admin/faqs': () => json([]) })
    const { router } = await mountScreen(Harness, {}, '/new', '/new')

    expect(current().flow.isEditMode.value).toBe(false)
    current().form.name = 'Made'
    await vi.waitFor(() => {
      expect(current().flow.isDirty.value).toBe(true)
    })
    await current().flow.save()

    expect(requestsOf(mock)).toContain('POST /admin/faqs')
    expect(router.currentRoute.value.path).toBe('/list')
    expect(current().flow.isDirty.value).toBe(false)
  })

  it('edit mode: loads the entry without counting that as an edit, and saves with PUT', async () => {
    const mock = stubRouteTable({
      'GET /admin/faqs': () => json([{ id: 3, sortOrder: 0, name: 'Stored' }]),
      'PUT /admin/faqs/3': () => new Response(null, { status: 204 }),
    })
    await mountScreen(Harness, {}, '/w/3', '/w/:id')

    expect(current().flow.isEditMode.value).toBe(true)
    expect(current().form.name).toBe('Stored')
    expect(current().flow.isDirty.value).toBe(false)

    await current().flow.save()
    expect(requestsOf(mock)).toContain('PUT /admin/faqs/3')
  })

  it('edit mode: an unknown id leaves the not-found sentence', async () => {
    stubRouteTable({ 'GET /admin/faqs': () => json([]) })
    await mountScreen(Harness, {}, '/w/9', '/w/:id')

    expect(current().flow.saveError.value).toBe('Entry #9 was not found.')
  })

  it('edit mode: a refused load shows the API sentence instead of not-found', async () => {
    stubRouteTable({ 'GET /admin/faqs': () => problem('Widgets are locked.', 423) })
    await mountScreen(Harness, {}, '/w/9', '/w/:id')

    expect(current().flow.saveError.value).toBe('Widgets are locked.')
  })

  it('a refused save keeps the API sentence and stays on the page', async () => {
    stubRouteTable({ 'POST /admin/faqs': () => problem('Name taken.', 409) })
    const { router } = await mountScreen(Harness, {}, '/new', '/new')

    await current().flow.save()

    expect(current().flow.saveError.value).toBe('Name taken.')
    expect(current().flow.saving.value).toBe(false)
    expect(router.currentRoute.value.path).toBe('/new')
  })

  it('cancel returns to the list', async () => {
    stubRouteTable({})
    const { router } = await mountScreen(Harness, {}, '/new', '/new')

    current().flow.cancel()

    await vi.waitFor(() => {
      expect(router.currentRoute.value.path).toBe('/list')
    })
  })
})
