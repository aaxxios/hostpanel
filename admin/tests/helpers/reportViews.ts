/**
 * Shared runner of the report screen smoke tests: each case mounts one screen with its endpoints
 * answered at the network level and checks it shows its title and data, and the refusal sentence
 * when the API refuses.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import type { Component } from 'vue'
import { json, problem } from './fetch'
import { mountView } from './mount'
import { stubRouteTable } from './routes'
import type { RouteTable } from './routes'

/** One screen to smoke-test. */
export interface ReportCase {
  /** Screen name, for the test title. */
  name: string
  /** The screen. */
  view: Component
  /** Answers by `"GET /reports/..."`; the client picker is always answered. */
  routes: RouteTable
  /** Texts the mounted screen must contain (its title, then something from its data). */
  expected: string[]
}

/**
 * Registers the smoke tests of a list of screens.
 *
 * @param cases - The screens.
 */
export const runReportCases = (cases: ReportCase[]): void => {
  describe('report screens', () => {
    afterEach(() => {
      vi.unstubAllGlobals()
    })

    it.each(cases)('$name shows its title and its data', async (reportCase) => {
      stubRouteTable({ 'GET /reports/client-picker': () => json([{ id: 1, name: 'Acme' }]), ...reportCase.routes })
      const wrapper = await mountView(reportCase.view)

      for (const text of reportCase.expected) {
        expect(wrapper.text()).toContain(text)
      }
      expect(wrapper.find('[data-testid="report-error"]').exists()).toBe(false)
    })

    it.each(cases.filter(reportCase => Object.keys(reportCase.routes).length > 0))(
      '$name shows the sentence the API refused with',
      async (reportCase) => {
        const refused = Object.fromEntries(Object.keys(reportCase.routes).map(route => [route, () => problem('No access.', 403)]))
        stubRouteTable({ 'GET /reports/client-picker': () => json([]), ...refused })
        const wrapper = await mountView(reportCase.view)

        expect(wrapper.get('[data-testid="report-error"]').text()).toBe('No access.')
      },
    )
  })
}

/**
 * Registers the smoke tests of one screen.
 *
 * @param reportCase - The screen.
 */
export const runReportCase = (reportCase: ReportCase): void => {
  runReportCases([reportCase])
}
