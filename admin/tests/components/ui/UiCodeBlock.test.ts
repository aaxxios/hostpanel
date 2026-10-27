/**
 * Unit tests for UiCodeBlock: the text is shown verbatim, copied exactly, and the button tells the
 * reader it worked; a refused clipboard leaves the text in place.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import UiCodeBlock from '@/components/ui/UiCodeBlock.vue'

/** Mounts a block with a recognisable command. */
const mountBlock = () =>
  mount(UiCodeBlock, {
    props: { code: 'sudo ./install.sh --provider cwp "ssh-ed25519 AAAA x"', copyLabel: 'Copy', copiedLabel: 'Copied' },
    attrs: { 'data-testid': 'cmd' },
  })

describe('UiCodeBlock', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the text verbatim with the test id on the box', () => {
    const wrapper = mountBlock()

    expect(wrapper.get('[data-testid="cmd"]').text()).toBe('sudo ./install.sh --provider cwp "ssh-ed25519 AAAA x"')
    expect(wrapper.find('[data-testid="cmd-copy"]').exists()).toBe(true)
  })

  it('copies exactly the text and confirms', async () => {
    const writeText = vi.fn().mockResolvedValue(undefined)
    vi.stubGlobal('navigator', { clipboard: { writeText } })
    const wrapper = mountBlock()

    await wrapper.get('[data-testid="cmd-copy"]').trigger('click')
    await flushPromises()

    expect(writeText).toHaveBeenCalledWith('sudo ./install.sh --provider cwp "ssh-ed25519 AAAA x"')
    expect(wrapper.get('[data-testid="cmd-copy"]').text()).toBe('Copied')
  })

  it('stays on the copy label when the clipboard is refused', async () => {
    vi.stubGlobal('navigator', { clipboard: { writeText: vi.fn().mockRejectedValue(new Error('denied')) } })
    const wrapper = mountBlock()

    await wrapper.get('[data-testid="cmd-copy"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="cmd-copy"]').text()).toBe('Copy')
  })
})
