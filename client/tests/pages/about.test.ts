/**
 * Tests for the About page: the operator's story as one section, and a redirect to contact when no
 * story is set.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountPage, registerPageComponents, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import LegalDocument from '~/components/legal/LegalDocument.vue'
import Page from '~/pages/about.vue'

/**
 * Stands in for the story reader.
 *
 * @param paragraphs The paragraphs the operator set.
 */
const stubStory = (paragraphs: string[]): void => {
  vi.stubGlobal('useAboutText', () => ({ paragraphs: ref(paragraphs) }))
}

/** `navigateTo` spy, renewed before each test. */
let navigateTo: ReturnType<typeof vi.fn>

describe('pages/about', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
    registerPageComponents({ LegalDocument })
    navigateTo = vi.fn()
    vi.stubGlobal('navigateTo', navigateTo)
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the title and every paragraph of the story', async () => {
    stubStory(['We host sites.', 'We register domains.'])

    const wrapper = await mountPage(Page)

    expect(wrapper.find('h1').text()).toBe('about.title')
    const story = wrapper.find('[data-testid="legal-section-story"]')
    expect(story.text()).toContain('We host sites.')
    expect(story.text()).toContain('We register domains.')
    expect(navigateTo).not.toHaveBeenCalled()
  })

  it('sends the visitor to contact when no story is set', async () => {
    stubStory([])

    await mountPage(Page)

    expect(navigateTo).toHaveBeenCalledWith('/en/contact', { redirectCode: 302 })
  })
})
