/**
 * Unit tests for the knowledge-base ArticleRow: it links to the article and shows the excerpt and
 * the view count only when the article has them.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ArticleRow from '~/components/kb/ArticleRow.vue'

describe('ArticleRow', () => {
  beforeEach(() => {
    vi.stubGlobal('useLocalePath', () => (path: string) => `/en${path}`)
  })

  it('links to the localised article page', () => {
    const wrapper = mountWithKit(ArticleRow, { props: { article: { id: 7, title: 'Reset a password' } } })

    expect(wrapper.attributes('href')).toBe('/en/knowledgebase/7')
    expect(wrapper.attributes('data-testid')).toBe('kb-article-7')
  })

  it('shows the title', () => {
    const wrapper = mountWithKit(ArticleRow, { props: { article: { id: 7, title: 'Reset a password' } } })

    expect(wrapper.find('[data-testid="kb-article-title"]').text()).toBe('Reset a password')
  })

  it('shows the excerpt and the view count when present', () => {
    const wrapper = mountWithKit(ArticleRow, {
      props: { article: { id: 7, title: 'T', excerpt: 'A short lead', views: 42 } },
    })

    expect(wrapper.find('[data-testid="kb-article-excerpt"]').text()).toBe('A short lead…')
    expect(wrapper.find('[data-testid="kb-article-views"]').text()).toBe('42')
  })

  it('leaves out the excerpt and the view count when absent', () => {
    const wrapper = mountWithKit(ArticleRow, { props: { article: { id: 7, title: 'T' } } })

    expect(wrapper.find('[data-testid="kb-article-excerpt"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="kb-article-views"]').exists()).toBe(false)
  })
})
