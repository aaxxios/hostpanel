/**
 * The `accept` lists of the upload controls, built from the {@link MimeType} members.
 */

import { MimeType } from '@/types/http'

/**
 * Builds the value of an `accept` attribute.
 *
 * @param types - The media types the control allows.
 * @returns The types joined by commas.
 */
const acceptOf = (...types: MimeType[]): string => types.join(',')

/** Formats a logo upload takes. */
export const LOGO_ACCEPT = acceptOf(MimeType.Jpeg, MimeType.Png, MimeType.Webp, MimeType.Gif, MimeType.Bmp)

/** Formats the favicon upload takes. */
export const FAVICON_ACCEPT = acceptOf(MimeType.Png, MimeType.Jpeg, MimeType.Webp)

/** Formats a storefront illustration upload takes. */
export const ILLUSTRATION_ACCEPT = acceptOf(MimeType.Jpeg, MimeType.Png, MimeType.Webp, MimeType.Gif, MimeType.Svg)
