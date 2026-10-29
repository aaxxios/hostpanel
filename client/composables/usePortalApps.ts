/**
 * The app-launcher entries the header offers.
 *
 * The launcher points at other web applications (task tracker, ERP, webmail and so
 * on) that only exist in a deployment running them. Every URL in `runtimeConfig` has
 * a `*.local` development default, so "has a value" cannot decide whether an app
 * is really there; the launcher is therefore off unless an operator turns it on
 * with `portal.apps.enabled`, and each entry still has to resolve to a URL of
 * its own before it renders. A stock install shows no launcher rather than eight
 * links into nothing.
 *
 * @module composables/usePortalApps
 */

import { ACCOUNT_APP_PATH, CLIENT_AREA_PATH } from '~/constants/clientRoutes'
import { PortalSettingKey } from '~/constants/portalSettingKey'
import { PortalAppId } from '~/types/portalappid'
import type { PortalApp } from '~/types/portalapp'

/** The static part of one launcher entry. */
interface AppDefinition {
  /** Stable id of the entry. */
  id: PortalAppId
  /** Operator setting holding this app's URL override. */
  settingKey: PortalSettingKey
  /** Two-letter tile label. */
  tag: string
  /** CSS gradient of the tile. */
  tint: string
  /** Key in `runtimeConfig.public` holding the app's host, or empty for a settings-only app. */
  envKey: string
  /** Path the app lives under on a shared host. */
  path?: string
}

/**
 * Static part of each entry: identity, tile styling and where its URL comes
 * from. `envKey` names a `runtimeConfig.public` key; entries with none are
 * reachable only by the matching `portal.apps.*` setting in the admin panel.
 */
const APP_DEFS: ReadonlyArray<AppDefinition> = [
  { id: PortalAppId.Account, settingKey: PortalSettingKey.AppsAccount, tag: 'Ac', tint: 'linear-gradient(135deg, #7DE3FF, #5D3FFF)', envKey: 'baseUrl', path: ACCOUNT_APP_PATH },
  { id: PortalAppId.Tasks, settingKey: PortalSettingKey.AppsTasks, tag: 'Tk', tint: 'linear-gradient(135deg, #38E8A0, #00D1FF)', envKey: 'tasksUrl' },
  { id: PortalAppId.Erp, settingKey: PortalSettingKey.AppsErp, tag: 'Er', tint: 'linear-gradient(135deg, #B9A6FF, #5D3FFF)', envKey: 'erpUrl' },
  { id: PortalAppId.Hostpanel, settingKey: PortalSettingKey.AppsHostpanel, tag: 'Hp', tint: 'linear-gradient(135deg, #FF9CE0, #B9A6FF)', envKey: 'baseUrl', path: CLIENT_AREA_PATH },
  { id: PortalAppId.Sheets, settingKey: PortalSettingKey.AppsSheets, tag: 'Sh', tint: 'linear-gradient(135deg, #00D1FF, #38E8A0)', envKey: 'sheetsUrl' },
  { id: PortalAppId.Mail, settingKey: PortalSettingKey.AppsMail, tag: 'Ml', tint: 'linear-gradient(135deg, #5D3FFF, #00D1FF)', envKey: 'emailUrl' },
  { id: PortalAppId.Docs, settingKey: PortalSettingKey.AppsDocs, tag: 'Dc', tint: 'linear-gradient(135deg, #7DE3FF, #B9A6FF)', envKey: '' },
  { id: PortalAppId.Calendar, settingKey: PortalSettingKey.AppsCalendar, tag: 'Cl', tint: 'linear-gradient(135deg, #FF9CE0, #5D3FFF)', envKey: '' },
]

/**
 * Whether a deployment has opted into the launcher.
 *
 * The admin setting arrives as text, but Nuxt coerces `NUXT_PUBLIC_*` values, so
 * the environment side is a real boolean by the time it is read. Both have to be
 * accepted, or switching the launcher on from the environment silently does
 * nothing.
 *
 * @param setting Value of `portal.apps.enabled`, empty when unset.
 * @param fromConfig Value of `runtimeConfig.public.portalAppsEnabled`.
 * @returns Whether the launcher is shown.
 */
export const isLauncherEnabled = (setting: string, fromConfig: unknown): boolean => {
  /**
   * Whether a text value reads as switched on.
   *
   * @param value Setting or config text.
   * @returns True for `true`, `1`, `yes` or `on`, in any case.
   */
  const truthy = (value: string): boolean => /^(true|1|yes|on)$/i.test(value.trim())

  if (setting) {
    return truthy(setting)
  }
  if (typeof fromConfig === 'boolean') {
    return fromConfig
  }
  return typeof fromConfig === 'string' && truthy(fromConfig)
}

/**
 * Final URL for one app, or empty when the deployment has none for it.
 *
 * A per-app setting holds a complete URL and wins outright; the environment
 * fallback names only a host the deployment already knows, so the path the app
 * lives under is appended to that rather than to the operator's own value.
 *
 * @param override Operator-set `portal.apps.<id>` value.
 * @param base URL from `runtimeConfig.public`.
 * @param path Path the app lives under, for hosts shared with other apps.
 * @returns The URL, or empty when neither value is set.
 */
export const resolveAppUrl = (override: string, base: string, path = ''): string => {
  if (override) {
    return override.trim()
  }
  if (!base) {
    return ''
  }
  return `${base.trim().replace(/\/+$/, '')}${path}`
}

/**
 * Tells a linked application which account to open as.
 *
 * The linked applications accept `?authuser=<email>` and open the visit as that
 * account (or ask which account to use when they do not hold it). Without it a click
 * from a browser signed in to two accounts lands on the first one.
 *
 * @param url Resolved app URL.
 * @param email The signed-in account, or nothing when nobody is.
 * @returns The URL with the hand-off, or unchanged when there is nobody to hand off or
 *   the value is not an absolute URL (a relative path is this origin, which already knows).
 */
export const withAccountHandoff = (url: string, email: string | undefined): string => {
  if (!email || !/^https?:\/\//i.test(url)) {
    return url
  }
  try {
    const target = new URL(url)
    // Set rather than appended: a second authuser would be ambiguous, and the last
    // writer should win.
    target.searchParams.set('authuser', email)
    return target.toString()
  } catch {
    return url
  }
}

/**
 * Resolves the launcher entries for the current deployment.
 *
 * @returns The entries to show: empty when the launcher is switched off.
 */
export const usePortalApps = () => {
  const { get } = usePortalSettings()
  const config = useRuntimeConfig()
  const { t } = useI18n()
  const { user } = storeToRefs(useAuthStore())

  /**
   * The deployment's URL for an app, before any per-app setting overrides it.
   *
   * @param envKey Key in `runtimeConfig.public`, or empty for a settings-only app.
   * @returns The trimmed URL, or empty when the key is empty or holds no string.
   */
  const fromEnv = (envKey: string): string => {
    if (!envKey) {
      return ''
    }
    const value = config.public[envKey as keyof typeof config.public]
    return typeof value === 'string' ? value.trim() : ''
  }

  /** Whether the deployment has switched the launcher on. */
  const enabled = computed(() =>
    isLauncherEnabled(get(PortalSettingKey.AppsEnabled, ''), config.public.portalAppsEnabled))

  /** The entries that resolved to a URL. */
  const apps = computed<PortalApp[]>(() => {
    if (!enabled.value) {
      return []
    }

    return APP_DEFS.flatMap(def => {
      const url = resolveAppUrl(get(def.settingKey, ''), fromEnv(def.envKey), def.path)
      if (!url) {
        return []
      }

      return [{
        id: def.id,
        tag: def.tag,
        tint: def.tint,
        label: t(`aurora.apps.items.${def.id}.label`),
        desc: t(`aurora.apps.items.${def.id}.desc`),
        url: withAccountHandoff(url, user.value?.email),
      }]
    })
  })

  return { apps }
}
