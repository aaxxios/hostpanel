/**
 * Unit tests for UiPagination: window of pages, navigation and edge states.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiPagination from '@/components/ui/UiPagination.vue'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Mounts the pagination with the English catalogue.
 *
 * @param props - Props for the component.
 * @returns The wrapper.
 */
const mountPagination = (props: { page: number; total: number; pageSize: number; testid?: string }) =>
  mount(UiPagination, { props, global: { plugins: [createTestI18n()] } })

describe('UiPagination', () => {
  it('renders nothing for a single page', () => {
    expect(mountPagination({ page: 1, total: 10, pageSize: 10 }).find('nav').exists()).toBe(false)
  })

  it('shows every page of a short list and marks the current one', () => {
    const wrapper = mountPagination({ page: 2, total: 30, pageSize: 10 })

    expect(wrapper.find('[data-testid="pagination-page-1"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="pagination-page-3"]').exists()).toBe(true)
    expect(wrapper.get('[data-testid="pagination-page-2"]').attributes('aria-current')).toBe('page')
  })

  it('collapses far pages into a gap around the current page', () => {
    const wrapper = mountPagination({ page: 5, total: 100, pageSize: 10 })
    const numbers = wrapper.findAll('[data-testid^="pagination-page-"]').map(b => b.text())

    expect(numbers).toEqual(['1', '4', '5', '6', '10'])
    expect(wrapper.text()).toContain('…')
  })

  it('goes to the next and previous page', async () => {
    const wrapper = mountPagination({ page: 2, total: 50, pageSize: 10 })

    await wrapper.get('[data-testid="pagination-next"]').trigger('click')
    await wrapper.get('[data-testid="pagination-prev"]').trigger('click')

    expect(wrapper.emitted('update:page')).toEqual([[3], [1]])
  })

  it('goes to a clicked page but not to the current one', async () => {
    const wrapper = mountPagination({ page: 2, total: 50, pageSize: 10 })

    await wrapper.get('[data-testid="pagination-page-3"]').trigger('click')
    await wrapper.get('[data-testid="pagination-page-2"]').trigger('click')

    expect(wrapper.emitted('update:page')).toEqual([[3]])
  })

  it('disables previous on the first page and next on the last', () => {
    const first = mountPagination({ page: 1, total: 30, pageSize: 10 })
    const last = mountPagination({ page: 3, total: 30, pageSize: 10 })

    expect(first.get('[data-testid="pagination-prev"]').attributes('disabled')).toBeDefined()
    expect(last.get('[data-testid="pagination-next"]').attributes('disabled')).toBeDefined()
  })

  it('prefixes its test ids', () => {
    const wrapper = mountPagination({ page: 1, total: 30, pageSize: 10, testid: 'orders' })

    expect(wrapper.find('[data-testid="orders-next"]').exists()).toBe(true)
  })
})
