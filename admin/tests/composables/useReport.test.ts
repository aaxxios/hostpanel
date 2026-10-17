/**
 * Tests for useReport: loads on mount through the reports store, keeps the data, waits when told.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { defineComponent, h } from 'vue'
import { useReport } from '@/composables/useReport'
import { reportPath } from '@/lib/reportsQuery'
import { json, problem } from '../helpers/fetch'
import { requestsOf, stubRouteTable } from '../helpers/routes'

/**
 * Mounts a host that loads `/reports/x?year=2024` and prints the value it got.
 *
 * @param immediate - Whether the host loads on mount.
 * @returns The wrapper; its `vm` exposes `load`.
 */
const mountHost = (immediate: boolean) =>
  mount(
    defineComponent({
      setup(_, { expose }) {
        const { data, load } = useReport<{ a: number }>(() => reportPath('/reports/x', { year: 2024 }), immediate)
        expose({ load })
        return () => h('p', String(data.value?.a ?? 'none'))
      },
    }),
    { global: { plugins: [createPinia()] } },
  )

describe('useReport', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('loads when mounted and keeps what the store returned', async () => {
    const mock = stubRouteTable({ 'GET /reports/x': () => json({ a: 5 }) })
    const wrapper = mountHost(true)
    await flushPromises()

    expect(wrapper.text()).toBe('5')
    expect(requestsOf(mock)).toEqual(['GET /reports/x?year=2024'])
  })

  it('keeps the previous data when a load is refused', async () => {
    let answer: () => Response = () => json({ a: 7 })
    stubRouteTable({ 'GET /reports/x': () => answer() })
    const wrapper = mountHost(true)
    await flushPromises()

    answer = () => problem('No access.', 403)
    await (wrapper.vm as unknown as { load: () => Promise<void> }).load()

    expect(wrapper.text()).toBe('7')
  })

  it('does not load on mount when told to wait', async () => {
    const mock = stubRouteTable({ 'GET /reports/x': () => json({ a: 1 }) })
    const wrapper = mountHost(false)
    await flushPromises()

    expect(requestsOf(mock)).toEqual([])
    expect(wrapper.text()).toBe('none')
  })
})
