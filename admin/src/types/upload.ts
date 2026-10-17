/**
 * Shapes of the image upload endpoints of the admin API.
 */

/** Branding slots the admin API accepts an image for. */
export const BrandingImageKind = {
  /** The main logo, for light backgrounds. */
  Logo: 'logo',
  /** The logo variant for dark backgrounds. */
  LogoDark: 'logodark',
  /** The square logo mark. */
  LogoMark: 'logomark',
  /** The browser tab icon. */
  Favicon: 'favicon',
} as const

/** One of the {@link BrandingImageKind} values. */
export type BrandingImageKind = (typeof BrandingImageKind)[keyof typeof BrandingImageKind]

/** The answer of an upload endpoint. */
export interface UploadedImage {
  /** Public URL the stored image is served from. */
  url: string
}
