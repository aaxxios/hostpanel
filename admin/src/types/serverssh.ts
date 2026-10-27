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
  /** The reason code when it is not healthy (`binding_mismatch`, `unreachable`, …). */
  error: string | null
  /** The provider profile the agent runs, when it answered. */
  provider?: string | null
}

/** What the admin panel needs to build the one-command installer for a server. */
export interface ServerSshSetupDto {
  /** The server. */
  serverId: number
  /** Agent profile for the server's module (`cwp`), null when the module is unsupported. */
  profile: string | null
  /** The management public key to install, null until an identity is generated. */
  publicKey: string | null
  /** The id the installer binds the agent to. */
  binding: string | null
  /** The management user the installer creates. */
  agentUsername: string
  /** Agent version of the bundle this HostPanel serves. */
  bundleVersion: string
  /** SHA256 the installer verifies before unpacking. */
  bundleSha256: string
  /** Path of the bundle on this HostPanel. */
  bundlePath: string
}

/** The host key a server's agent endpoint presented to a scan. */
export interface SshHostKeyScanDto {
  /** Host key algorithm. */
  algorithm: string
  /** SHA256 fingerprint. */
  fingerprint: string
  /** Whether it equals the fingerprint already pinned. */
  matchesPinned: boolean
}
