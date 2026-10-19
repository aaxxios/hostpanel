/**
 * Values of the `portal.chat.provider` setting the storefront recognises.
 *
 * @module types/chatprovider
 */

/** Live-chat providers the operator can select. */
export enum ChatProvider {
  /** The value shipped by the first release; the wire value is kept because existing installs still store it. */
  LegacyChat = 'chatwoot',
  /** The current provider. */
  Innochat = 'innochat'
}
