/**
 * Tests for `pages/client/emails/[id].vue`: the message in a frame and the not-found state.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountPage, stubClientRuntime } from '~/tests/helpers/mountClient'
import EmailPage from '~/pages/client/emails/[id].vue'

/** The mocked client endpoints the store reaches. */
const clientApi = vi.hoisted(() => ({ fetchEmail: vi.fn() }))

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => clientApi }))

describe('email page', () => {
  beforeEach(() => {
    clientApi.fetchEmail.mockReset()
    stubClientRuntime({ params: { id: 'e1' } })
  })

  it('shows the subject and the body in a sandboxed frame', async () => {
    clientApi.fetchEmail.mockResolvedValue({ id: 1, to: 'a@b.test', subject: 'Welcome', sentAt: '2026-01-01T10:00:00Z', body: '<p>Hello</p>', success: true })

    const wrapper = await mountPage(EmailPage)

    expect(clientApi.fetchEmail).toHaveBeenCalledWith('e1')
    expect(wrapper.find('h1').text()).toBe('Welcome')
    expect(wrapper.find('iframe').attributes('srcdoc')).toBe('<p>Hello</p>')
    expect(wrapper.find('iframe').attributes('sandbox')).toBe('allow-same-origin')
  })

  it('links back to the email history', async () => {
    clientApi.fetchEmail.mockResolvedValue({ id: 1, to: 'a@b.test', subject: 'Welcome', sentAt: '2026-01-01T10:00:00Z', body: '', success: true })

    const wrapper = await mountPage(EmailPage)

    expect(wrapper.find('[data-testid="email-back"]').attributes('href')).toBe('/client/emails')
  })

  it('says the email was not found when the read fails', async () => {
    clientApi.fetchEmail.mockRejectedValue({ data: { detail: 'Not found.' } })

    const wrapper = await mountPage(EmailPage)

    expect(wrapper.find('[data-testid="ui-empty-state"]').text()).toContain('client.emails.notFound')
    expect(wrapper.find('iframe').exists()).toBe(false)
  })
})
