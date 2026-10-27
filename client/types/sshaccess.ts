/** SSH access summary of one hosting service, as `GET api/me/services/{id}/ssh` answers it. */
import type { SshAccessState } from '~/types/sshaccessstate'

/** SSH access summary of one service. */
export interface SshAccess {
  /** Access state. */
  state: SshAccessState
  /** Hostname to connect to, when the server is configured. */
  hostname: string | null
  /** Port to connect to, when the server is configured. */
  port: number | null
  /** The customer's own hosting account name, when configured. */
  username: string | null
  /** Ready-to-copy `ssh` command, only when keys can be managed. */
  connectCommand: string | null
  /** How many keys the service may hold. */
  maxKeys: number
}
