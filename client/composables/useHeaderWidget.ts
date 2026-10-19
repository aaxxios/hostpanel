/**
 * Tells a screen whether the platform shell's header widget will fill its mount points.
 *
 * @module composables/useHeaderWidget
 */

import { isHeaderWidgetEnabled } from '~/utils/headerWidget'

/**
 * Reads the deployment's widget availability from the runtime configuration.
 *
 * A header uses it to choose between the widget's mount point and hostpanel's own account menu.
 *
 * @returns `enabled`: true when the widget script is loaded for this deployment.
 */
export const useHeaderWidget = () => {
  const config = useRuntimeConfig()

  /** True when the widget script is loaded, false on a standalone or demo deployment. */
  const enabled = computed<boolean>(() =>
    isHeaderWidgetEnabled(config.public.authMode as string | undefined, config.public.mainUrl as string | undefined))

  return { enabled }
}
