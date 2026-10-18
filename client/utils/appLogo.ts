/**
 * Which logo `AppLogo` shows, kept free of Nuxt so the rule can be unit-tested.
 *
 * @module utils/appLogo
 */

import type { BrandLogoPair } from '~/utils/brandLogo'

/** The kind of background a logo sits on. */
export enum LogoSurface {
  /** Follows the page colour mode: both wordmarks are rendered and swapped by the `dark:` variant. */
  Auto = 'auto',
  /** Always dark whatever the page mode (sign-in pages, the footer): the dark-background wordmark only. */
  Dark = 'dark',
}

/** The bundled wordmark for light backgrounds (dark text). */
export const STATIC_LOGO_LIGHT = '/logo.svg'

/** The bundled wordmark for dark backgrounds (light text). */
export const STATIC_LOGO_DARK = '/logo-dark.svg'

/** What `AppLogo` renders. */
export interface AppLogoPlan {
  /** Wordmark URL for light backgrounds; `''` when the surface is always dark. */
  light: string
  /** Wordmark URL for dark backgrounds. */
  dark: string
  /** Whether the operator uploaded a logo, so a plain img is needed instead of the image optimiser. */
  uploaded: boolean
  /** Whether both images are rendered and swapped by the `dark:` variant. */
  swap: boolean
}

/**
 * Resolves the logo URLs for a surface.
 *
 * The operator's upload wins; with none, the bundled pair is used so the wordmark text
 * stays readable on both backgrounds instead of one fixed-colour file.
 *
 * @param pair The operator's wordmarks with their fallbacks (`''` when nothing is uploaded).
 * @param surface The background the logo sits on.
 * @returns The plan to render.
 */
export const resolveAppLogo = (pair: BrandLogoPair, surface: LogoSurface): AppLogoPlan => {
  const uploaded = Boolean(pair.light || pair.dark)
  const light = uploaded ? pair.light : STATIC_LOGO_LIGHT
  const dark = uploaded ? pair.dark : STATIC_LOGO_DARK
  if (surface === LogoSurface.Dark) {
    return { light: '', dark, uploaded, swap: false }
  }
  return { light, dark, uploaded, swap: dark !== light }
}
