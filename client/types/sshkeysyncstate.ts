/** Last state the server confirmed for an SSH key. Wire values are the backend `SshKeySyncState` member names. */
export enum SshKeySyncState {
  /** Recorded, not yet confirmed. */
  Pending = 'Pending',
  /** Confirmed on the server. */
  Installed = 'Installed',
  /** The install was not confirmed; the key may or may not be on the server. */
  InstallFailed = 'InstallFailed',
  /** The revoke was not confirmed; the key may still log in. */
  RevokeFailed = 'RevokeFailed',
  /** Confirmed gone from the server. */
  Revoked = 'Revoked'
}
