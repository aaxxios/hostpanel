/**
 * The share card a page gets when it has no image of its own, and the Open Graph and
 * Twitter fields that describe it.
 *
 * One place for the file name and its size, because `useSeo` and the product page both
 * emit it and a crawler that is told the wrong dimensions crops the card.
 *
 * @module utils/shareImage
 */

/** Root-relative path of the bundled share card in `public/`. */
export const DEFAULT_SHARE_IMAGE_PATH = '/og-image.jpg'

/** Width of the bundled share card, in pixels. */
export const DEFAULT_SHARE_IMAGE_WIDTH = 1200

/** Height of the bundled share card, in pixels. */
export const DEFAULT_SHARE_IMAGE_HEIGHT = 630

/** Media type of the bundled share card. */
export const DEFAULT_SHARE_IMAGE_TYPE = 'image/jpeg'

/** The `useSeoMeta` fields for one share image. */
export interface ShareImageMeta {
  /** Absolute URL of the image, as Open Graph requires. */
  ogImage: string
  /** Absolute URL of the image for the Twitter card. */
  twitterImage: string
  /** Description of the image for screen readers and crawlers. */
  ogImageAlt: string
  /** Description of the image on the Twitter card. */
  twitterImageAlt: string
  /** Image width in pixels; only known for the bundled card. */
  ogImageWidth?: number
  /** Image height in pixels; only known for the bundled card. */
  ogImageHeight?: number
  /** Image media type; only known for the bundled card. */
  ogImageType?: 'image/jpeg'
}

/**
 * Builds the share-image fields for a page.
 *
 * A page's own image gets only its URL and alt text, because its size and type are not
 * known here; the bundled card also gets its width, height and type.
 *
 * @param baseUrl The configured site URL, without a trailing slash.
 * @param alt Text describing the image.
 * @param image The page's own image path, relative to the site root; omitted for the bundled card.
 * @returns The fields to spread into `useSeoMeta`.
 */
export const shareImageMeta = (baseUrl: string, alt: string, image?: string): ShareImageMeta => {
  if (image) {
    const url = `${baseUrl}${image}`
    return { ogImage: url, twitterImage: url, ogImageAlt: alt, twitterImageAlt: alt }
  }

  const url = `${baseUrl}${DEFAULT_SHARE_IMAGE_PATH}`
  return {
    ogImage: url,
    twitterImage: url,
    ogImageAlt: alt,
    twitterImageAlt: alt,
    ogImageWidth: DEFAULT_SHARE_IMAGE_WIDTH,
    ogImageHeight: DEFAULT_SHARE_IMAGE_HEIGHT,
    ogImageType: DEFAULT_SHARE_IMAGE_TYPE,
  }
}
