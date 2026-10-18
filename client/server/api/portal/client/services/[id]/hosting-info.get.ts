/**
 * GET /api/portal/client/services/:id/hosting-info
 * Returns real-time nameserver and SSL certificate info for the service's domain.
 * Does DNS NS lookup + TLS handshake to get live data.
 *
 * The domain is read from the caller's own service list. `MyServicesController` declares no
 * bare `{id}` GET -- this route called `/me/services/{id}` and 404'd, taking the nameserver and
 * SSL panels on the service page with it. `services/[id]/index.get.ts` already reads the list
 * and picks the row out of it for the same reason; the list is client-scoped by the credential,
 * so an id belonging to somebody else simply is not in it.
 *
 * @module server/api/portal/client/services/[id]/hosting-info.get
 */

import dns from 'node:dns/promises'
import tls from 'node:tls'
import type { HostingInfo } from '~/types/hostinginfo'

/** The certificate summary the service page renders; null when no certificate could be read. */
type SslInfo = HostingInfo['ssl']

/** The members of a service row this route reads. */
interface ServiceRow {
  /** Service primary key. */
  id: number
  /** The service's domain, when it has one. */
  domain?: string
}

/**
 * Resolves nameservers for a given domain.
 *
 * @param domain - Domain name to look up
 * @returns Sorted list of nameserver hostnames, empty on failure
 */
const resolveNameservers = async (domain: string): Promise<string[]> => {
  try {
    const ns = await dns.resolveNs(domain)
    return ns.sort()
  } catch {
    return []
  }
}

/**
 * Formats a Date as DD/MM/YYYY string.
 *
 * @param d - Date to format
 * @returns Formatted date string
 */
const formatDate = (d: Date): string => {
  const day = String(d.getDate()).padStart(2, '0')
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const year = d.getFullYear()
  return `${day}/${month}/${year}`
}

/**
 * Checks TLS certificate validity for a domain on port 443.
 *
 * @param domain - Domain name to connect to
 * @returns Certificate info object, or null on failure
 */
const checkSsl = (domain: string): Promise<SslInfo> => {
  return new Promise((resolve) => {
    const socket = tls.connect(
      { host: domain, port: 443, servername: domain, timeout: 5000 },
      () => {
        const cert = socket.getPeerCertificate()
        socket.destroy()

        if (!cert || !cert.valid_from) {
          resolve(null)
          return
        }

        const validFrom = new Date(cert.valid_from)
        const validTo = new Date(cert.valid_to)
        const now = new Date()
        const valid = now >= validFrom && now <= validTo && !socket.authorizationError

        // A certificate subject field is multi-valued: Node types `CN` and `O` as
        // `string | string[]`, and a real issuer with two organisation entries arrives as an
        // array. Rendered straight into the page it would have read `[object Array]`; the
        // first entry is the one a person means by "who issued this".
        const issuerField = cert.issuer?.CN || cert.issuer?.O || ''

        resolve({
          valid,
          issuer: Array.isArray(issuerField) ? issuerField[0] ?? '' : issuerField,
          startDate: formatDate(validFrom),
          expiryDate: formatDate(validTo),
        })
      },
    )

    socket.on('error', () => {
      socket.destroy()
      resolve(null)
    })

    socket.setTimeout(5000, () => {
      socket.destroy()
      resolve(null)
    })
  })
}

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const services = await internalApiCall<ServiceRow[]>(event, '/me/services')
  const service = services.find(s => String(s.id) === id)

  if (!service) {
    // The backend has no single-service read, so this refusal is the BFF's own: a code, no sentence.
    throw createError({ statusCode: 404, data: { code: 'SERVICE_NOT_FOUND' } })
  }

  const domain = service.domain
  if (!domain) {
    return { nameservers: [], ssl: null }
  }

  // Run DNS and SSL checks in parallel
  const [nameservers, ssl] = await Promise.all([
    resolveNameservers(domain),
    checkSsl(domain),
  ])

  return { nameservers, ssl }
})
