/**
 * Whether keys can be managed on a service now and, if not, why. Wire values are the backend
 * `SshAccessState` member names.
 */
export enum SshAccessState {
  /** Keys can be listed, added and revoked. */
  Available = 'Available',
  /** The hosting account has no login shell. */
  ShellDisabled = 'ShellDisabled',
  /** The server's control panel has no SSH key support. */
  Unsupported = 'Unsupported',
  /** SSH key management is not enabled on the service's server. */
  NotConfigured = 'NotConfigured',
  /** The service is not active. */
  ServiceInactive = 'ServiceInactive',
  /** The server did not answer. */
  Unreachable = 'Unreachable'
}
