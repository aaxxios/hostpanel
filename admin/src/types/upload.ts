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

/** Content areas that own an image upload endpoint under the scope name. */
export const ImageUploadScope = {
  /** Homepage slides. */
  Slides: 'slides',
  /** Storefront content: testimonial avatars, partner logos, project pictures. */
  Storefront: 'storefront',
} as const

/** One of the {@link ImageUploadScope} values. */
export type ImageUploadScope = (typeof ImageUploadScope)[keyof typeof ImageUploadScope]

/** One stored rendition of an uploaded image. */
export interface UploadedAsset {
  /** Public URL the rendition is served from. */
  url: string
  /** MIME type of the rendition. */
  contentType: string
  /** Edge length in pixels of a square rendition; null when the format has no fixed size. */
  pixelSize: number | null
  /** What the rendition is used for (the backend role name, e.g. Primary or Icon). */
  role: string
}

/** The answer of an upload endpoint (200). */
export interface UploadedImage {
  /** Public URL the stored image is served from. */
  url: string
  /** Every rendition stored for the image (the original and any generated sizes). */
  assets: UploadedAsset[]
}
