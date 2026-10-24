/**
 * Which logo `Logo` shows, kept free of Nuxt so the rule can be unit-tested.
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

/** The bundled square mark (the cube), drawn beside the site name when no logo is uploaded. */
export const STATIC_LOGO_MARK = '/favicon.svg'

/**
 * Classes that lay a light plate behind a logo drawn for light backgrounds, so it stays readable
 * on a dark surface. Used when the operator uploaded one logo only: it is the light-background
 * logo, and with no dark variant to swap to it would otherwise sit on black.
 */
export const LOGO_PLATE_CLASSES = 'rounded-md bg-white/90 px-2 py-1'

/** {@link LOGO_PLATE_CLASSES} applied only while the page is in dark mode (the `dark:` variant). */
export const LOGO_PLATE_CLASSES_DARK_MODE = 'dark:rounded-md dark:bg-white/90 dark:px-2 dark:py-1'

/** What `Logo` renders. */
export interface AppLogoPlan {
  /** Wordmark URL for light backgrounds; `''` when the surface is always dark. */
  light: string
  /** Wordmark URL for dark backgrounds. */
  dark: string
  /** Whether the operator uploaded a logo, so a plain img is needed instead of the image optimiser. */
  uploaded: boolean
  /** Whether both images are rendered and swapped by the `dark:` variant. */
  swap: boolean
  /**
   * Whether the upload is the only logo (no dark variant), so it needs a light plate behind it
   * wherever it can land on a dark background.
   */
  plate: boolean
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
    return { light: '', dark, uploaded, swap: false, plate: uploaded && !pair.hasDarkVariant }
  }
  const swap = dark !== light
  return { light, dark, uploaded, swap, plate: uploaded && !swap }
}
