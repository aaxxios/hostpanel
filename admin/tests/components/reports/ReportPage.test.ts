/**
 * Tests for ReportPage: title and description, the filters slot, the loading placeholder, the
 * refusal sentence from the store with its Retry button, and the "generated on" line after the
 * content.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import ReportPage from '@/components/reports/ReportPage.vue'
import { useReportsStore } from '@/stores/reports'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Mounts the page with a filter and a body.
 *
 * @param retry - The screen's reload, when it offers one.
 * @returns The wrapper.
 */
const mountPage = (retry?: () => unknown) =>
  mount(ReportPage, {
    props: { title: 'Aging', description: 'Overdue invoices', retry },
    slots: { filters: '<p data-testid="filter-slot">filters</p>', default: '<p data-testid="body-slot">body</p>' },
    global: { plugins: [createTestI18n(), createPinia()] },
  })

describe('ReportPage', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the title, description, filters, body and the generated-on line', () => {
    const wrapper = mountPage()

    expect(wrapper.text()).toContain('Aging')
    expect(wrapper.text()).toContain('Overdue invoices')
    expect(wrapper.find('[data-testid="filter-slot"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="body-slot"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="report-timestamp"]').exists()).toBe(true)
  })

  it('replaces the body with a placeholder while the store loads, keeping the filters', async () => {
    const wrapper = mountPage()
    useReportsStore().loading = true
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="report-loading"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="body-slot"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="filter-slot"]').exists()).toBe(true)
  })

  it('shows the sentence the store holds when a load was refused', async () => {
    const wrapper = mountPage()
    useReportsStore().error = 'No access.'
    await wrapper.vm.$nextTick()

    expect(wrapper.get('[data-testid="report-error"]').text()).toBe('No access.')
    expect(wrapper.find('[data-testid="body-slot"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="report-error-retry"]').exists()).toBe(false)
  })

  it('offers Retry on a refused load when the screen can reload', async () => {
    const retry = vi.fn()
    const wrapper = mountPage(retry)
    useReportsStore().error = 'No access.'
    await wrapper.vm.$nextTick()

    expect(wrapper.get('[data-testid="report-error"]').text()).toBe('No access.')
    await wrapper.get('[data-testid="report-error-retry"]').trigger('click')

    expect(retry).toHaveBeenCalledTimes(1)
  })
})
