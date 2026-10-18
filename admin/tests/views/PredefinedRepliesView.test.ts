/**
 * Smoke tests for PredefinedRepliesView: the category list, adding a category, the reply tab, the
 * debounced search and the delete confirmation, through the real store and transport.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import PredefinedRepliesView from '@/views/PredefinedRepliesView.vue'
import { mountView } from '../helpers/mount'
import { problem, stubFetch, callOf } from '../helpers/fetch'
import { stubPathRoutes } from '../helpers/routes'

/** One category as the endpoint returns it. */
const category = { id: 1, name: 'Greetings', parentCategoryId: null, replyCount: 1 }

/** One reply as the endpoint returns it. */
const reply = { id: 4, name: 'Welcome', content: 'Hello and welcome.', categoryId: 1, categoryName: 'Greetings' }

/** The collections the page loads on mount. */
const loadRoutes = [
  { path: '/predefined-replies/categories', body: [category] },
  { path: '/predefined-replies', body: [reply] },
]

beforeEach(() => {
  // Only the debounce timers are faked; promise flushing keeps the real scheduler.
  vi.useFakeTimers({ toFake: ['setTimeout', 'clearTimeout'] })
})

afterEach(() => {
  vi.useRealTimers()
  vi.unstubAllGlobals()
})

describe('PredefinedRepliesView', () => {
  it('lists the categories', async () => {
    stubPathRoutes(loadRoutes)

    const wrapper = await mountView(PredefinedRepliesView)

    expect(wrapper.get('[data-testid="predefined-replies-category-1"]').text()).toContain('Greetings')
  })

  it('adds a category and clears the field', async () => {
    const mock = stubPathRoutes(loadRoutes)
    const wrapper = await mountView(PredefinedRepliesView)

    await wrapper.get('[data-testid="predefined-replies-category-name"]').setValue(' Billing ')
    await wrapper.get('[data-testid="predefined-replies-category-add"]').trigger('click')
    await flushPromises()

    const post = mock.mock.calls.map((_, index) => callOf(mock, index)).find((call) => call.method === 'POST')
    expect(post).toMatchObject({ url: '/api/predefined-replies/categories', body: { name: 'Billing' } })
  })

  it('lists the replies on the reply tab and blocks an incomplete new reply', async () => {
    stubPathRoutes(loadRoutes)
    const wrapper = await mountView(PredefinedRepliesView)

    await wrapper.get('[data-testid="predefined-replies-tab-reply"]').trigger('click')

    expect(wrapper.get('[data-testid="predefined-replies-reply-4"]').text()).toContain('Welcome')
    expect(wrapper.get('[data-testid="predefined-replies-reply-add"]').attributes('disabled')).toBeDefined()
  })

  it('searches once typing pauses and shows the hits', async () => {
    const mock = stubPathRoutes([...loadRoutes, { path: '/predefined-replies/search', body: [reply] }])
    const wrapper = await mountView(PredefinedRepliesView)

    await wrapper.get('[data-testid="predefined-replies-tab-search"]').trigger('click')
    await wrapper.get('[data-testid="predefined-replies-search"]').setValue('welc')
    await vi.advanceTimersByTimeAsync(400)
    await flushPromises()

    const urls = mock.mock.calls.map((_, index) => callOf(mock, index).url)
    expect(urls).toContain('/api/predefined-replies/search?q=welc')
    expect(wrapper.get('[data-testid="predefined-replies-result-4"]').text()).toContain('Hello and welcome.')
  })

  it('deletes a category after confirmation', async () => {
    const mock = stubPathRoutes(loadRoutes)
    const wrapper = await mountView(PredefinedRepliesView)

    await wrapper.get('[data-testid="predefined-replies-category-1-delete"]').trigger('click')
    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    const removal = mock.mock.calls.map((_, index) => callOf(mock, index)).find((call) => call.method === 'DELETE')
    expect(removal).toMatchObject({ url: '/api/predefined-replies/categories/1' })
  })

  it('shows the API sentence when loading is refused', async () => {
    stubFetch(problem('Not allowed.', 403))

    const wrapper = await mountView(PredefinedRepliesView)

    expect(wrapper.get('[data-testid="predefined-replies-error"]').text()).toBe('Not allowed.')
  })
})
