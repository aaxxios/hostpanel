/**
 * Closed value sets of the nova template's own components.
 *
 * Declared as enums so a section never writes a bare tone or size; a component declares its prop
 * as the template-literal form (`${NovaCtaTone}`) so a template may still write `tone="outline"`
 * while a script uses `NovaCtaTone.Outline`.
 *
 * @module templates/nova/enums
 */

/** Look of a nova call-to-action link. */
export enum NovaCtaTone {
  /** Filled accent: the one thing a section asks for. */
  Accent = 'accent',
  /** Framed, transparent: the secondary action. */
  Outline = 'outline',
  /** No frame, muted text: a plain navigation link. */
  Quiet = 'quiet',
}

/** Size step of a nova call-to-action link. */
export enum NovaCtaSize {
  /** Hero and closing-section buttons. */
  Lg = 'lg',
  /** In-section buttons. */
  Md = 'md',
  /** Buttons inside a pricing card. */
  Card = 'card',
  /** Header bar and menu panel buttons; the caller sets the horizontal padding. */
  Bar = 'bar',
}
