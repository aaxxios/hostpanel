/**
 * Heights of an embedded frame.
 *
 * @module utils/ui/embed
 */

/** Height step of a `UiEmbed` frame. */
export enum UiEmbedHeight {
  /** Compact, 12rem growing to 16rem. */
  Sm = 'sm',
  /** Default, 12rem growing to 14rem and 16rem. */
  Md = 'md',
  /** Tall, 16rem growing to 24rem. */
  Lg = 'lg',
}

/** Height utilities per step. */
export const EMBED_HEIGHT_CLASSES: Record<UiEmbedHeight, string> = {
  [UiEmbedHeight.Sm]: 'h-48 md:h-64',
  [UiEmbedHeight.Md]: 'h-48 sm:h-56 md:h-64',
  [UiEmbedHeight.Lg]: 'h-64 md:h-96',
}
