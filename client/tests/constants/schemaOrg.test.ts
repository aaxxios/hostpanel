/**
 * Unit tests for `constants/schemaOrg.ts`.
 *
 * @module tests/constants/schemaOrg.test
 */

import { describe, expect, it } from 'vitest'
import { SCHEMA_ORG_CONTEXT } from '~/constants/schemaOrg'

describe('schema.org constants', () => {
  it('names the schema.org vocabulary as the JSON-LD context', () => {
    expect(SCHEMA_ORG_CONTEXT).toBe('https://schema.org')
  })
})
