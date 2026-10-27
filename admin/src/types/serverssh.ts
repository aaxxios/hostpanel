/**
 * SSH key management settings of one provisioning server, as `api/admin/servers/{id}/ssh-settings`
 * answers and accepts them. The agent's private key is write-only: it is sent, never read back.
 */

/** A server's SSH key management settings. */
export interface ServerSshSettingsDto {
  /** The server. */
  serverId: number
  /** Whether the server's control panel module supports SSH key management. */
  isSupported: boolean
  /** Whether customers on this server may manage keys. */
  isEnabled: boolean
  /** Hostname customers connect to. */
  sshHostname: string
  /** Port customers connect to. */
  sshPort: number
  /** Host HostPanel reaches the key agent on. */
  agentHost: string
  /** SSH port of the key agent. */
  agentPort: number
  /** Management user on the server. */
  agentUsername: string
  /** Whether a private key is stored. */
  hasAgentPrivateKey: boolean
  /** Pinned host key fingerprint, `SHA256:…`. */
  agentHostKeyFingerprint: string
  /** Connect timeout, seconds. */
  connectTimeoutSeconds: number
  /** Operation timeout, seconds. */
  operationTimeoutSeconds: number
  /** Outcome of the last agent health check, null when none ran. */
  isHealthy: boolean | null
  /** When the last health check ran. */
  lastHealthCheckAt: string | null
}

/** The body of a settings save. */
export interface ServerSshSettingsPayload {
  /** Whether customers may manage keys. */
  isEnabled: boolean
  /** Hostname customers connect to. */
  sshHostname: string
  /** Port customers connect to. */
  sshPort: number
  /** Agent host. */
  agentHost: string
  /** Agent SSH port. */
  agentPort: number
  /** Management user. */
  agentUsername: string
  /** New private key, or null to keep the stored one. */
  agentPrivateKey: string | null
  /** Pinned host key fingerprint. */
  agentHostKeyFingerprint: string
  /** Connect timeout, seconds. */
  connectTimeoutSeconds: number
  /** Operation timeout, seconds. */
  operationTimeoutSeconds: number
}

/** Outcome of an agent health check. */
export interface SshAgentTestResultDto {
  /** Whether the agent answered. */
  isHealthy: boolean
  /** The agent's version when it answered. */
  agentVersion: string | null
  /** The provider's reason code when it did not. */
  error: string | null
}
