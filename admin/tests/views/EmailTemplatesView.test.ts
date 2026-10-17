/**
 * Mount tests of EmailTemplatesView (email templates): it renders what the API sent, shows the empty state where it
 * has one, and shows the API's sentence when the load is refused.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import EmailTemplatesView from '@/views/EmailTemplatesView.vue'
import { mountRoutedSettled } from '../helpers/mount'
import { json, problem } from '../helpers/fetch'
import { stubRoutes } from '../helpers/routes'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('EmailTemplatesView', () => {
  it('lists the templates', async () => {
    stubRoutes({ 'GET /api/admin/email-templates': () => json([{ id: 3, name: 'Welcome', subject: 'Hello', body: '' }]) })
    const wrapper = await mountRoutedSettled(EmailTemplatesView, '/settings/email-templates')

    expect(wrapper.get('[data-testid="email-templates-row-3"]').text()).toContain('Welcome')
    expect(wrapper.get('[data-testid="email-templates-row-3"]').text()).toContain('Hello')
  })

  it('says so when there are none', async () => {
    stubRoutes({ 'GET /api/admin/email-templates': () => json([]) })
    const wrapper = await mountRoutedSettled(EmailTemplatesView, '/settings/email-templates')

    expect(wrapper.get('[data-testid="email-templates-table"]').text()).toContain('No templates found')
  })

  it('shows the API sentence when the load is refused', async () => {
    stubRoutes({ 'GET /api/admin/email-templates': () => problem('Not allowed.', 403) })
    const wrapper = await mountRoutedSettled(EmailTemplatesView, '/settings/email-templates')

    expect(wrapper.get('[data-testid="email-templates-error"]').text()).toContain('Not allowed.')
  })
})
