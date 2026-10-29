/**
 * Shared runner of the report screen smoke tests: each case mounts one screen with its endpoints
 * answered at the network level and checks it shows its title and data, the refusal sentence
 * when the API refuses, and, for a screen that offers Retry, that a retry after a refusal shows the
 * data.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
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
  /** Answers by `"GET /reports/<name>"`; the client picker is always answered. */
  routes: RouteTable
  /** Texts the mounted screen must contain (its title, then something from its data). */
  expected: string[]
  /** True when a refused load offers Retry, which loads the report again. */
  retryable?: boolean
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

    it.each(cases.filter(reportCase => reportCase.retryable === true))(
      '$name loads the report again from Retry after a refusal',
      async (reportCase) => {
        /** Routes already refused once; each answers normally from its second call. */
        const refusedOnce = new Set<string>()
        const flaky = Object.fromEntries(Object.entries(reportCase.routes).map(([route, answer]) => [
          route,
          (body: unknown) => {
            if (refusedOnce.has(route)) {
              return answer(body)
            }
            refusedOnce.add(route)
            return problem('No access.', 403)
          },
        ]))
        stubRouteTable({ 'GET /reports/client-picker': () => json([]), ...flaky })
        const wrapper = await mountView(reportCase.view)

        await wrapper.get('[data-testid="report-error-retry"]').trigger('click')
        await flushPromises()

        expect(wrapper.find('[data-testid="report-error"]').exists()).toBe(false)
        for (const text of reportCase.expected) {
          expect(wrapper.text()).toContain(text)
        }
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
