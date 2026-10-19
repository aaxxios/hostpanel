/**
 * One icon `<link>` the root component hands to `useHead`.
 *
 * A union with one member per `rel`, on purpose: unhead types `link` as a discriminated union
 * keyed by `rel` (a favicon, an Apple touch icon and a Safari mask icon each have their own
 * shape, and a mask icon requires `color`). An object whose `rel` is a plain string, or a
 * union of several `rel` values, matches none of those members and is rejected, so each
 * member here carries exactly one `rel`.
 *
 * Object type aliases rather than interfaces: unhead's link types carry a `data-${string}`
 * index signature, and TypeScript gives an object type alias an implicit index signature but
 * never an interface.
 *
 * @module types/headiconlink
 */
import type { BrandingIconRel } from '~/types/brandingiconrel'

/** Any icon link the root component emits: a favicon, an iOS touch icon or a Safari mask icon. */
export type HeadIconLink =
  | {
    /** Always `icon`. */
    rel: BrandingIconRel.Icon
    /** Absolute-from-root URL of the icon file. */
    href: string
    /** Media type of the file, when known. */
    type?: string
    /** `WxH` for the `sizes` attribute, when the link takes one. */
    sizes?: string
  }
  | {
    /** Always `apple-touch-icon`. */
    rel: BrandingIconRel.AppleTouchIcon
    /** Absolute-from-root URL of the icon file. */
    href: string
    /** Media type of the file, when known. */
    type?: string
    /** `WxH` for the `sizes` attribute, when the link takes one. */
    sizes?: string
  }
  | {
    /** Always `mask-icon`. */
    rel: BrandingIconRel.MaskIcon
    /** Absolute-from-root URL of the SVG silhouette. */
    href: string
    /** Tint of the silhouette; required by the spec and by unhead. */
    color: string
  }
