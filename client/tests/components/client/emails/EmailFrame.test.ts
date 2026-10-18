/**
 * Unit tests for EmailFrame: the sandboxed srcdoc frame, the title and the height custom property.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import EmailFrame from '~/components/client/emails/EmailFrame.vue'

describe('EmailFrame', () => {
  it('puts the markup in the srcdoc of a sandboxed frame', () => {
    const wrapper = mountClient(EmailFrame, { props: { html: '<p>Hi</p><script>alert(1)</script>', title: 'Message content' } })
    const frame = wrapper.find('iframe')

    expect(frame.attributes('srcdoc')).toBe('<p>Hi</p><script>alert(1)</script>')
    expect(frame.attributes('sandbox')).toBe('allow-same-origin')
    expect(frame.attributes('sandbox')).not.toContain('allow-scripts')
  })

  it('names the frame for assistive technology', () => {
    const wrapper = mountClient(EmailFrame, { props: { html: '', title: 'Message content' } })

    expect(wrapper.find('iframe').attributes('title')).toBe('Message content')
  })

  it('grows to the loaded content through a custom property', async () => {
    const wrapper = mountClient(EmailFrame, { props: { html: '<p>Hi</p>', title: 't' }, attachToBody: true })
    const frame = wrapper.find('iframe').element as HTMLIFrameElement
    Object.defineProperty(frame, 'contentDocument', { value: { body: { scrollHeight: 400 } }, configurable: true })

    await wrapper.find('iframe').trigger('load')

    expect(frame.style.getPropertyValue('--frame-height')).toBe('432px')
    wrapper.unmount()
  })
})
