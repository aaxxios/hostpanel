/**
 * How the cover beside a home-page slide is drawn.
 *
 * @module types/slidecoverkind
 */

/** The source a slide's cover comes from. */
export enum SlideCoverKind {
  /** The slide's own uploaded picture. */
  Image = 'Image',
  /** The illustration generated for the slide's topic, painted as a mask in the slide colour. */
  Art = 'Art',
}
