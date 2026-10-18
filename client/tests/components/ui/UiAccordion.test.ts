/**
 * Unit tests for UiAccordion: single and multiple open modes, the default open items and the
 * scoped content slot.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiAccordion from '~/components/ui/UiAccordion.vue'

/** Sections used by every test. */
const ITEMS = [
  { title: 'First', content: 'First body' },
  { title: 'Second', content: 'Second body' },
]

describe('UiAccordion', () => {
  it('renders every title and no body while closed', () => {
    const wrapper = mountWithKit(UiAccordion, { props: { items: ITEMS } })

    expect(wrapper.text()).toContain('First')
    expect(wrapper.text()).toContain('Second')
    expect(wrapper.text()).not.toContain('First body')
  })

  it('opens a section and reflects it in aria-expanded', async () => {
    const wrapper = mountWithKit(UiAccordion, { props: { items: ITEMS } })

    await wrapper.find('[data-testid="ui-accordion-toggle-0"]').trigger('click')

    expect(wrapper.text()).toContain('First body')
    expect(wrapper.find('[data-testid="ui-accordion-toggle-0"]').attributes('aria-expanded')).toBe('true')
  })

  it('closes the other section in single mode', async () => {
    const wrapper = mountWithKit(UiAccordion, { props: { items: ITEMS, defaultOpen: [0] } })

    await wrapper.find('[data-testid="ui-accordion-toggle-1"]').trigger('click')

    expect(wrapper.text()).toContain('Second body')
    expect(wrapper.text()).not.toContain('First body')
  })

  it('keeps both open in multiple mode', async () => {
    const wrapper = mountWithKit(UiAccordion, { props: { items: ITEMS, multiple: true, defaultOpen: [0] } })

    await wrapper.find('[data-testid="ui-accordion-toggle-1"]').trigger('click')

    expect(wrapper.text()).toContain('First body')
    expect(wrapper.text()).toContain('Second body')
  })

  it('closes an open section when clicked again', async () => {
    const wrapper = mountWithKit(UiAccordion, { props: { items: ITEMS, defaultOpen: [0] } })

    await wrapper.find('[data-testid="ui-accordion-toggle-0"]').trigger('click')

    expect(wrapper.text()).not.toContain('First body')
  })

  it('lets a content slot replace the body', () => {
    const wrapper = mountWithKit(UiAccordion, {
      props: { items: ITEMS, defaultOpen: [0] },
      slots: { 'content-0': 'Custom body' },
    })

    expect(wrapper.text()).toContain('Custom body')
    expect(wrapper.text()).not.toContain('First body')
  })

  it('keeps closed bodies in the page, hidden, when keepContent is set', async () => {
    // Attached, because visibility is computed from styles of an element that is in the document.
    const wrapper = mountWithKit(UiAccordion, { props: { items: ITEMS, keepContent: true }, attachToBody: true })

    expect(wrapper.html()).toContain('First body')
    const body = wrapper.findAll('div').find((node) => node.text() === 'First body')
    expect(body?.isVisible()).toBe(false)

    await wrapper.find('[data-testid="ui-accordion-toggle-0"]').trigger('click')

    expect(wrapper.findAll('div').find((node) => node.text() === 'First body')?.isVisible()).toBe(true)
    wrapper.unmount()
  })
})
