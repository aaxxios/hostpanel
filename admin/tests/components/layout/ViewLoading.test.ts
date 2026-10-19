/**
 * Mount tests for ViewLoading: a busy status region with skeletons and the loading word.
 */
import { describe, expect, it } from 'vitest'
import ViewLoading from '@/components/layout/ViewLoading.vue'
import { mountWithApp } from '../../helpers/mount'

describe('ViewLoading', () => {
  it('announces a busy status region with skeletons', () => {
    const wrapper = mountWithApp(ViewLoading)
    const region = wrapper.get('[data-testid="view-loading"]')

    expect(region.attributes('role')).toBe('status')
    expect(region.attributes('aria-busy')).toBe('true')
    expect(region.findAll('.animate-pulse').length).toBeGreaterThan(0)
  })

  it('says it is loading', () => {
    const wrapper = mountWithApp(ViewLoading)

    expect(wrapper.text()).toContain('Loading')
  })
})
