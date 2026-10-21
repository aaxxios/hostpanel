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

/** Scheme prefix of an e-mail link; the address is appended. */
export const MAILTO_PREFIX = 'mailto:'

/** Scheme prefix of a telephone link; the dialable number is appended. */
export const TEL_PREFIX = 'tel:'
