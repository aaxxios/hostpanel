/**
 * Unit tests for UiList: bulleted, numbered, plain and divided lists.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiList from '~/components/ui/UiList.vue'
import { UiListStyle } from '~/utils/ui/enums'

describe('UiList', () => {
  it('is a bulleted ul by default and renders its items', () => {
    const wrapper = mountWithKit(UiList, { slots: { default: '<li>one</li>' } })

    expect(wrapper.element.tagName).toBe('UL')
    expect(wrapper.classes()).toEqual(expect.arrayContaining(['list-disc', 'pl-5']))
    expect(wrapper.find('li').text()).toBe('one')
  })

  it('is a numbered ol when ordered', () => {
    const wrapper = mountWithKit(UiList, { props: { ordered: true } })

    expect(wrapper.element.tagName).toBe('OL')
    expect(wrapper.classes()).toContain('list-decimal')
  })

  it('has no markers when plain', () => {
    const wrapper = mountWithKit(UiList, { props: { listStyle: UiListStyle.Plain } })

    expect(wrapper.classes()).toContain('space-y-2')
    expect(wrapper.classes()).not.toContain('list-disc')
  })

  it('separates items with hairlines when divided', () => {
    const wrapper = mountWithKit(UiList, { props: { listStyle: UiListStyle.Divided } })

    expect(wrapper.classes()).toContain('divide-y')
    expect(wrapper.classes()).not.toContain('list-disc')
  })

  it('carries no markers and no spacing when bare', () => {
    const wrapper = mountWithKit(UiList, { props: { listStyle: UiListStyle.Bare } })

    expect(wrapper.classes()).toEqual([])
  })
})
