/**
 * Tests for the client-side CSV download of the reports.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { buildCsv, downloadCsv } from '@/lib/reportsCsv'

describe('buildCsv', () => {
  it('quotes every field and joins lines with newlines', () => {
    expect(buildCsv(['Name', 'Amount'], [['Acme', 5], ['Beta', 6]])).toBe('"Name","Amount"\n"Acme","5"\n"Beta","6"')
  })

  it('doubles a quote inside a field', () => {
    expect(buildCsv(['Note'], [['say "hi"']])).toBe('"Note"\n"say ""hi"""')
  })
})

describe('downloadCsv', () => {
  afterEach(() => {
    vi.restoreAllMocks()
    vi.unstubAllGlobals()
  })

  it('offers the rows as a named file and frees the object address', () => {
    const create = vi.fn(() => 'blob:report')
    const revoke = vi.fn()
    vi.stubGlobal('URL', { createObjectURL: create, revokeObjectURL: revoke })
    const click = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => undefined)

    downloadCsv('top.csv', ['A'], [[1]])

    expect(create).toHaveBeenCalledTimes(1)
    expect(click).toHaveBeenCalledTimes(1)
    expect(revoke).toHaveBeenCalledWith('blob:report')
  })
})
