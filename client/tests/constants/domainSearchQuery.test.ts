/**
 * Unit test for the shared domain-search query parameter: the name is the one the domains page
 * has always read, so existing links keep working.
 */
import { describe, expect, it } from 'vitest'
import { DOMAIN_SEARCH_QUERY_PARAM } from '~/constants/domainSearchQuery'

describe('DOMAIN_SEARCH_QUERY_PARAM', () => {
  it('is the `domain` parameter', () => {
    expect(DOMAIN_SEARCH_QUERY_PARAM).toBe('domain')
  })
})
