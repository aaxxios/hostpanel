import type { AppRuntimeConfig } from '@/types/runtimeConfig'

/**
 * Normalises a raw base path: a leading slash, no trailing slash, `/` for empty input.
 *
 * @param raw - The configured value, possibly missing or malformed.
 * @returns The canonical base path (`/` or `/segment[/segment]`).
 */
export const normalizeBasePath = (raw: string | undefined): string => {
  const trimmed = (raw ?? '').trim().replace(/\/+$/, '')
  if (trimmed === '') {
    return '/'
  }
  return trimmed.startsWith('/') ? trimmed : `/${trimmed}`
}

/**
 * Reads the runtime configuration injected by `config.js`.
 *
 * @returns The configuration with every field normalised; the root base when `config.js` is absent.
 */
export const getRuntimeConfig = (): AppRuntimeConfig => ({
  basePath: normalizeBasePath(window.__APP_CONFIG__?.basePath),
})

/**
 * Builds the URL of a file in `public/` under the deployment's base path.
 *
 * @param file - Path inside `public/`, without a leading slash.
 * @returns The root-relative URL including the base path.
 */
export const publicAssetUrl = (file: string): string =>
  `${getRuntimeConfig().basePath.replace(/\/+$/, '')}/${file}`
