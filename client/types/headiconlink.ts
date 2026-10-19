/**
 * One icon `<link>` the root component hands to `useHead`.
 *
 * `rel` is a plain string on purpose: unhead accepts any relation, and a single widened type
 * keeps the array homogeneous instead of a union of literal-`rel` objects it rejects.
 *
 * @module types/headiconlink
 */
export interface HeadIconLink {
  /** Value of the `rel` attribute (`icon`, `apple-touch-icon`, `mask-icon`). */
  rel: string
  /** Absolute-from-root URL of the icon file. */
  href: string
  /** Media type of the file, when known. */
  type?: string
  /** `WxH` for the `sizes` attribute, when the link takes one. */
  sizes?: string
  /** Tint of a `mask-icon` silhouette. */
  color?: string
}
