/**
 * GET /api/portal/demo
 *
 * Proxies the anonymous `GET /api/public/demo` so the browser and the server render learn whether
 * this deployment is a demo, when its data resets next and which accounts are published.
 *
 * Never fails the request: a backend that is down, or an older one without the endpoint, answers
 * as "not a demo", so the portal renders normally either way.
 */
import type { DemoStatus } from '~/types/demostatus'

export default defineEventHandler(async (event): Promise<DemoStatus> => {
  try {
    const status = await internalApiCall<DemoStatus>(event, '/public/demo', { anonymous: true })
    return {
      enabled: Boolean(status?.enabled),
      nextResetAt: status?.nextResetAt ?? null,
      accounts: status?.accounts ?? []
    }
  } catch {
    // Unreachable or too old to know the endpoint: not a demo, and not an error here.
    return { enabled: false, nextResetAt: null, accounts: [] }
  }
})
