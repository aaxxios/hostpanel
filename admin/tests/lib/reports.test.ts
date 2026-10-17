/**
 * Tests for the report registry the sidebar and router share.
 */
import { describe, expect, it } from 'vitest'
import { REPORTS } from '@/lib/reports'
import { reportIcon } from '@/lib/icons'

describe('REPORTS', () => {
  it('has unique slugs and a translated label for every report', () => {
    const slugs = REPORTS.map(report => report.slug)

    expect(new Set(slugs).size).toBe(slugs.length)
    expect(REPORTS).toHaveLength(34)
    for (const report of REPORTS) {
      expect(report.label).not.toBe('')
      expect(reportIcon(report.icon)).not.toBe('')
    }
  })

  it('loads each view lazily', async () => {
    const first = REPORTS[0]

    expect(first?.slug).toBe('aging-invoices')
    expect(await first?.component()).toBeTruthy()
  })
})
