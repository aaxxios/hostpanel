/**
 * Unit tests for UiCard: themed surface, optional padding, attribute pass-through.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiCard from '@/components/ui/UiCard.vue'

describe('UiCard', () => {
  it('renders the slot inside the surface classes', () => {
    const wrapper = mount(UiCard, { slots: { default: 'Body text' } })

    expect(wrapper.text()).toBe('Body text')
    expect(wrapper.classes()).toEqual(expect.arrayContaining(['bg-surface-card', 'border', 'border-border', 'rounded-2xl']))
  })

  it('keeps the classes and attributes the caller passes', () => {
    const wrapper = mount(UiCard, { attrs: { class: 'p-5 mb-5', 'data-testid': 'card' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['p-5', 'mb-5']))
    expect(wrapper.attributes('data-testid')).toBe('card')
  })

  it('adds no padding by default and pads on request', () => {
    expect(mount(UiCard).classes()).not.toContain('p-5')
    expect(mount(UiCard, { props: { padding: 'md' } }).classes()).toContain('p-5')
  })

  it('moves to the elevated surface', () => {
    const wrapper = mount(UiCard, { props: { elevated: true } })

    expect(wrapper.classes()).toContain('bg-surface-elevated')
    expect(wrapper.classes()).not.toContain('bg-surface-card')
  })
})
