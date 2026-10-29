/**
 * Tests for the knowledgebase mapping in `server/utils/kb.ts`: the article shape the pages read.
 *
 * @module tests/server/utils/kb.test
 */

import { describe, expect, it } from 'vitest'
import { toKbArticle } from '~/server/utils/kb'
import type { BackendKbArticle } from '~/types/backendkbarticle'

/**
 * Builds a backend article, overriding only what a case cares about.
 *
 * @param overrides Fields to replace.
 * @returns A published article.
 */
const article = (overrides: Partial<BackendKbArticle> = {}): BackendKbArticle => ({
  id: 1,
  title: 'Reset a password',
  content: '<p>Open <strong>settings</strong>.</p>',
  category: 'Account',
  ...overrides,
})

describe('toKbArticle', () => {
  it('keeps the body as sent and derives a tag-free excerpt', () => {
    const mapped = toKbArticle(article())

    expect(mapped.body).toBe('<p>Open <strong>settings</strong>.</p>')
    expect(mapped.excerpt).toBe('Open settings .')
    expect(mapped).toMatchObject({ id: 1, title: 'Reset a password' })
  })

  it('cuts the excerpt at 140 characters', () => {
    expect(toKbArticle(article({ content: 'x'.repeat(500) })).excerpt).toHaveLength(140)
  })

  it('invents no view count or update date', () => {
    expect(toKbArticle(article())).not.toHaveProperty('views')
    expect(toKbArticle(article())).not.toHaveProperty('dateupdated')
  })
})
