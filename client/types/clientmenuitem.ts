/**
 * One quick link of the single sign-on account widget menu.
 *
 * @module types/clientmenuitem
 */
import type { ClientMenuIcon } from './clientmenuicon'

/** One quick link of the widget menu. */
export interface ClientMenuItem {
  /** The sentence in the reader's language; the field the widget renders. */
  label: string
  /** The `client.nav.*` locale key of the sentence; the reader's side resolves it in its language. */
  labelKey: string
  /** Site-relative address the link opens. */
  url: string
  /** Icon the widget draws. */
  icon: ClientMenuIcon
}
