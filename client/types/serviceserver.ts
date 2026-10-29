/**
 * The server a hosting service runs on, as the backend's `ServiceServerDto` sends it.
 *
 * @module types/serviceserver
 */

/** The server a hosting service runs on. */
export interface ServiceServer {
  /** Display name of the server. */
  name: string
  /** Hostname of the server. */
  hostname: string
  /** IP address of the server; null when it has none. */
  ipAddress: string | null
}
