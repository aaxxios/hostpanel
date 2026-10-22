/**
 * Layouts and the item type of `UiLogoWall`.
 *
 * @module utils/ui/logoWall
 */

/** How a `UiLogoWall` lays its logos out. */
export enum UiLogoWallLayout {
  /** A centred, balanced grid of logo cells; for a short list of partners. */
  Grid = 'grid',
  /** One compact centred row that wraps on a narrow screen; for a "trusted by" strip. */
  Row = 'row',
  /** A scrolling strip that loops without a gap; for a long list of partners. */
  Marquee = 'marquee',
}

/** One logo of a `UiLogoWall`: a partner or a customer, already localised. */
export interface LogoWallItem {
  /** Stable identifier, used for the key and the test ids. */
  id: number
  /** Name shown as the alt text, and as the wordmark when there is no logo. */
  name: string
  /** URL of the logo image, or null for a typographic wordmark. */
  logoUrl: string | null
  /** The partner's website, or null for a plain (unlinked) logo. */
  url: string | null
}
