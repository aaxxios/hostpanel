/**
 * Address pieces the storefront builds chat links from.
 *
 * @module constants/contactChannel
 */

import { HTTPS_PREFIX } from '~/constants/urlScheme'

/** Base of a Telegram chat link; the operator's handle is appended. */
export const TELEGRAM_LINK_BASE = `${HTTPS_PREFIX}t.me/`

/** Base of a WhatsApp chat link; the digits-only number is appended. */
export const WHATSAPP_LINK_BASE = `${HTTPS_PREFIX}wa.me/`
