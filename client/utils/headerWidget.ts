/**
 * Decides whether the platform shell's header widget (app launcher and account chip) is served
 * to this deployment.
 *
 * One answer for the plugin that loads the script and for the screens that either mount the
 * widget or draw their own account menu, so the two can never disagree: a header that waits for
 * a widget that is never loaded stays empty.
 *
 * @module utils/headerWidget
 */

import { AuthMode } from '~/types/authmode'

/**
 * Whether the widget script is loaded for the given deployment.
 *
 * Only an `sso` deployment with a real portal address has one. Under `local` auth there is no
 * SSO for the widget's account switcher to call, and the developer-machine default host
 * (`*.local`) exists only on a developer's laptop.
 *
 * @param authMode - The configured authentication mode.
 * @param mainUrl - The configured main portal address; may be empty.
 * @returns True when the widget script is requested and its mount points will be filled.
 */
export const isHeaderWidgetEnabled = (authMode: string | undefined, mainUrl: string | undefined): boolean => {
  if (authMode !== AuthMode.Sso) {
    return false
  }
  if (!mainUrl || /\.local(:\d+)?\/?$/.test(mainUrl)) {
    return false
  }
  return true
}
