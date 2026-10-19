/**
 * Addresses of a hosting service's control panel and webmail on its own server.
 *
 * @module utils/controlPanel
 */

import { CONTROL_PANEL_PORT, WEBMAIL_PORT } from '~/constants/controlPanel'
import { HTTPS_PREFIX } from '~/constants/urlScheme'

/**
 * Address of the control panel's plain sign-in page.
 *
 * @param host - The service's server host name.
 * @returns The secure address with the panel port.
 */
export const controlPanelUrl = (host: string): string => `${HTTPS_PREFIX}${host}:${CONTROL_PANEL_PORT}`

/**
 * Address of the service's webmail.
 *
 * @param host - The service's server host name.
 * @returns The secure address with the webmail port.
 */
export const webmailUrl = (host: string): string => `${HTTPS_PREFIX}${host}:${WEBMAIL_PORT}`

/**
 * Address of a service's website.
 *
 * @param domain - The service's domain name.
 * @returns The secure address of the site.
 */
export const websiteUrl = (domain: string): string => `${HTTPS_PREFIX}${domain}`
