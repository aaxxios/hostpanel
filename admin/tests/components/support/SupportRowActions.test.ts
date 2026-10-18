/**
 * Unit tests for SupportRowActions: edit is a link or a button depending on `editTo`, delete only
 * asks, and both controls carry the testids built from the prefix.
 */
import { describe, expect, it } from 'vitest'
import { mount, RouterLinkStub } from '@vue/test-utils'
import SupportRowActions from '@/components/support/SupportRowActions.vue'
import { createTestI18n } from '../../helpers/i18n'

describe('SupportRowActions', () => {
  it('renders edit as a route link when editTo is given', () => {
    const wrapper = mount(SupportRowActions, {
      props: { testidPrefix: 'row-1', editTo: '/things/1/edit' },
      global: { plugins: [createTestI18n()], stubs: { RouterLink: RouterLinkStub } },
    })

    const edit = wrapper.getComponent(RouterLinkStub)
    expect(edit.props('to')).toBe('/things/1/edit')
    expect(wrapper.find('[data-testid="row-1-edit"]').exists()).toBe(true)
    expect(wrapper.find('button[data-testid="row-1-edit"]').exists()).toBe(false)
  })

  it('renders edit as a button that emits edit when there is no route', async () => {
    const wrapper = mount(SupportRowActions, {
      props: { testidPrefix: 'row-2' },
      global: { plugins: [createTestI18n()] },
    })

    await wrapper.get('button[data-testid="row-2-edit"]').trigger('click')

    expect(wrapper.emitted('edit')).toHaveLength(1)
  })

  it('emits delete when the bin is clicked and names the control for assistive tech', async () => {
    const wrapper = mount(SupportRowActions, {
      props: { testidPrefix: 'row-3' },
      global: { plugins: [createTestI18n()] },
    })
    const bin = wrapper.get('[data-testid="row-3-delete"]')

    await bin.trigger('click')

    expect(wrapper.emitted('delete')).toHaveLength(1)
    expect(bin.attributes('aria-label')).toBe('Delete')
  })
})
