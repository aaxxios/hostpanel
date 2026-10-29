/**
 * The admin panel carries a copy of `brandPalette.ts` (`admin/src/lib/brandPalette.ts`) because it
 * cannot import across the repository; the client file is the single shared spec and this test is
 * what keeps the copy honest.
 *
 * @module tests/utils/brandPalette.shared.test
 */

import { readFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'
import { describe, expect, it } from 'vitest'

/**
 * Resolves a path relative to this test file.
 *
 * @param rel Relative path.
 * @returns The absolute file path.
 */
const here = (rel: string): string => fileURLToPath(new URL(rel, import.meta.url))

describe('admin copy of brandPalette.ts', () => {
  it('is identical to the client file below its header', () => {
    const client = readFileSync(here('../../utils/brandPalette.ts'), 'utf8')
    const admin = readFileSync(here('../../../admin/src/lib/brandPalette.ts'), 'utf8')
    // The admin copy starts with one doc comment explaining why it exists; everything after
    // that comment must match byte for byte.
    const body = admin.slice(admin.indexOf('*/') + 3)
    expect(body).toBe(client)
  })

  it('names the client file as its source in the header', () => {
    const admin = readFileSync(here('../../../admin/src/lib/brandPalette.ts'), 'utf8')
    expect(admin.slice(0, admin.indexOf('*/'))).toContain('client/utils/brandPalette.ts')
  })
})
