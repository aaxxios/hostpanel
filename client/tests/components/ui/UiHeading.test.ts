/**
 * Unit tests for UiHeading: the semantic level picks the element, the visual size is separate and
 * defaults from the level.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiHeading from '~/components/ui/UiHeading.vue'
import { UiHeadingLevel, UiHeadingSize } from '~/utils/ui/enums'

describe('UiHeading', () => {
  it('renders an h2 by default with its slot', () => {
    const wrapper = mountWithKit(UiHeading, { slots: { default: 'Title' } })

    expect(wrapper.element.tagName).toBe('H2')
    expect(wrapper.text()).toBe('Title')
  })

  it.each([
    [UiHeadingLevel.H1, 'H1'],
    [UiHeadingLevel.H3, 'H3'],
    [UiHeadingLevel.H6, 'H6'],
  ])('renders level %s as %s', (level, tagName) => {
    expect(mountWithKit(UiHeading, { props: { level } }).element.tagName).toBe(tagName)
  })

  it('accepts a plain number for the level', () => {
    expect(mountWithKit(UiHeading, { props: { level: 4 } }).element.tagName).toBe('H4')
  })

  it.each([
    [UiHeadingLevel.H1, 'text-3xl'],
    [UiHeadingLevel.H2, 'text-2xl'],
    [UiHeadingLevel.H3, 'text-lg'],
    [UiHeadingLevel.H5, 'uppercase'],
  ])('sizes level %s by default with %s', (level, expected) => {
    expect(mountWithKit(UiHeading, { props: { level } }).classes()).toContain(expected)
  })

  it('lets the visual size differ from the level', () => {
    const wrapper = mountWithKit(UiHeading, { props: { level: UiHeadingLevel.H2, size: UiHeadingSize.Display } })

    expect(wrapper.element.tagName).toBe('H2')
    expect(wrapper.classes()).toContain('text-4xl')
  })

  it('passes extra classes and the testid through', () => {
    const wrapper = mountWithKit(UiHeading, { attrs: { class: 'mb-4', 'data-testid': 'page-title' } })

    expect(wrapper.classes()).toContain('mb-4')
    expect(wrapper.attributes('data-testid')).toBe('page-title')
  })

  it('paints nothing for the bare size, so the screen supplies size and colour', () => {
    const wrapper = mountWithKit(UiHeading, { props: { level: UiHeadingLevel.H2, size: UiHeadingSize.Bare } })

    expect(wrapper.element.tagName).toBe('H2')
    expect(wrapper.classes()).toEqual([])
  })
})
