/**
 * The live-chat providers the storefront's loader understands. Mirrors `plugins/live-chat.ts`; the
 * members are the strings the API stores for `portal.chat.provider`.
 */

/** A live-chat provider. */
export enum ChatProvider {
  /** No chat widget. */
  Off = '',
  /** Chatwoot. */
  Chatwoot = 'chatwoot',
  /** The in-house chat. */
  Innochat = 'innochat',
}
