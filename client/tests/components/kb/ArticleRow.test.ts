/**
 * Unit tests for the knowledge-base ArticleRow: it links to the article and shows the excerpt
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
    const wrapper = mountWithKit(ArticleRow, { props: { article: { id: 7, title: 'Reset a password', excerpt: '', category: 'Account' } } })

    expect(wrapper.attributes('href')).toBe('/en/knowledgebase/7')
    expect(wrapper.attributes('data-testid')).toBe('kb-article-7')
  })

  it('shows the title', () => {
    const wrapper = mountWithKit(ArticleRow, { props: { article: { id: 7, title: 'Reset a password', excerpt: '', category: 'Account' } } })

    expect(wrapper.find('[data-testid="kb-article-title"]').text()).toBe('Reset a password')
  })

  it('shows the excerpt when the article has one', () => {
    const wrapper = mountWithKit(ArticleRow, {
      props: { article: { id: 7, title: 'T', excerpt: 'A short lead', category: 'Account' } },
    })

    expect(wrapper.find('[data-testid="kb-article-excerpt"]').text()).toBe('A short lead…')
  })

  it('leaves out an empty excerpt', () => {
    const wrapper = mountWithKit(ArticleRow, { props: { article: { id: 7, title: 'T', excerpt: '', category: 'Account' } } })

    expect(wrapper.find('[data-testid="kb-article-excerpt"]').exists()).toBe(false)
  })

  it('carries the classic look through template-scoped classes and keeps the kit look otherwise', () => {
    const wrapper = mountWithKit(ArticleRow, { props: { article: { id: 7, title: 'T', excerpt: 'E', category: 'Account' } } })

    expect(wrapper.classes()).toContain('[.tpl-classic_&]:hover:bg-classic-surface-2')
    expect(wrapper.classes()).toContain('hover:bg-gray-50')
    expect(wrapper.find('[data-testid="kb-article-title"]').classes()).toContain('[.tpl-classic_&]:text-classic-ink')
    expect(wrapper.find('[data-testid="kb-article-excerpt"]').classes()).toContain('[.tpl-classic_&]:text-classic-muted')
  })
})
