/**
 * Unit tests for UiSkeleton: shapes, line count and the shorter last line.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'

describe('UiSkeleton', () => {
  it('is hidden from assistive technology and pulses', () => {
    const wrapper = mount(UiSkeleton)

    expect(wrapper.attributes('aria-hidden')).toBe('true')
    expect(wrapper.find('.animate-pulse').exists()).toBe(true)
  })

  it('draws one line by default and the requested number of lines', () => {
    expect(mount(UiSkeleton).findAll('.animate-pulse')).toHaveLength(1)
    expect(mount(UiSkeleton, { props: { lines: 3 } }).findAll('.animate-pulse')).toHaveLength(3)
  })

  it('shortens only the last of several lines', () => {
    const rows = mount(UiSkeleton, { props: { lines: 3 } }).findAll('.animate-pulse')

    expect(rows[0]?.classes()).toContain('w-full')
    expect(rows[2]?.classes()).toContain('w-2/3')
  })

  it('draws a single shape for block and circle whatever the line count', () => {
    expect(mount(UiSkeleton, { props: { shape: 'block', lines: 4 } }).findAll('.animate-pulse')).toHaveLength(1)
    expect(mount(UiSkeleton, { props: { shape: 'circle' } }).get('.animate-pulse').classes()).toContain('rounded-full')
  })
})
