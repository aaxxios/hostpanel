/**
 * Per-item accent colour of the Ui* kit.
 *
 * A product, a slide or a plan carries its own colour as data (a hex from the catalogue), so no
 * Tailwind class can name it. `UiAccent` writes that colour once, as the `--accent` custom
 * property of a wrapper; every class here reads the property, so the colour-dependent look is a
 * literal Tailwind class (`text-[color:var(--accent)]`) and a screen never carries a hex or an
 * inline style of its own. A tint is made with `color-mix`, which keeps the opacity steps the old
 * `${color}15` hex suffixes had (15 = 8 %, 25 = 15 %, 40 = 25 %, 50 = 31 %).
 *
 * @module utils/ui/accent
 */

/** The custom property `UiAccent` writes and the classes below read. */
export const ACCENT_VARIABLE = '--accent'

/** Look of a UiAccentButton. */
export enum UiAccentAppearance {
  /** Filled with the accent. */
  Solid = 'solid',
  /** Tinted fill, accent text, thin accent border. */
  Soft = 'soft',
  /** Transparent, accent text, accent border. */
  Outline = 'outline',
}

/** Edge length of a UiAccentTile. */
export enum UiAccentTileSize {
  /** 1.5 rem, a heading marker. */
  Xs = 'xs',
  /** 3 rem, a list row. */
  Sm = 'sm',
  /** 3.5 rem, a card header. */
  Md = 'md',
  /** 4 rem, a detail header. */
  Lg = 'lg',
  /** 5 rem, a hero. */
  Xl = 'xl',
}

/** Whether a value is a colour a stylesheet may take from data: a `#rgb`, `#rrggbb` or `#rrggbbaa` hex. */
const ACCENT_COLOR = /^#(?:[0-9a-f]{3,4}|[0-9a-f]{6}|[0-9a-f]{8})$/i

/**
 * Checks a colour from the catalogue before it is written into a style.
 *
 * A colour is operator data; anything that is not a plain hex (a `url(...)`, a second declaration)
 * must never reach a stylesheet, so it is refused rather than escaped.
 *
 * @param value The colour string from the data.
 * @returns True when it is a hex colour.
 */
export const isAccentColor = (value: string | null | undefined): value is string =>
  typeof value === 'string' && ACCENT_COLOR.test(value)

/**
 * Classes that read the `--accent` property. Complete literals, so Tailwind emits them.
 */
export const ACCENT_CLASSES = {
  /** Accent as text or icon colour. */
  text: 'text-[color:var(--accent)]',
  /** Accent as a solid fill. */
  fill: 'bg-[color:var(--accent)]',
  /** A faint tint of the accent as a fill (about 8 %). */
  tint: 'bg-[color:color-mix(in_srgb,var(--accent)_8%,transparent)]',
  /** A soft tint of the accent as a fill (about 15 %). */
  tintStrong: 'bg-[color:color-mix(in_srgb,var(--accent)_15%,transparent)]',
  /** A soft accent border (about 25 %). */
  borderSoft: 'border-[color:color-mix(in_srgb,var(--accent)_25%,transparent)]',
  /** A medium accent border (about 40 %). */
  borderMedium: 'border-[color:color-mix(in_srgb,var(--accent)_40%,transparent)]',
  /** A full accent border. */
  border: 'border-[color:var(--accent)]',
  /** A soft glow around the element. */
  glow: 'shadow-[0_0_30px_color-mix(in_srgb,var(--accent)_20%,transparent)]',
} as const

/** Edge length and icon size per tile size. */
export const ACCENT_TILE_CLASSES: Record<UiAccentTileSize, { box: string; icon: string }> = {
  [UiAccentTileSize.Xs]: { box: 'w-6 h-6 rounded-lg border', icon: 'text-sm' },
  [UiAccentTileSize.Sm]: { box: 'w-12 h-12 rounded-xl border', icon: 'text-2xl' },
  [UiAccentTileSize.Md]: { box: 'w-14 h-14 rounded-xl border-2', icon: 'text-3xl' },
  [UiAccentTileSize.Lg]: { box: 'w-16 h-16 rounded-xl border-2', icon: 'text-4xl' },
  [UiAccentTileSize.Xl]: { box: 'w-20 h-20 rounded-2xl border-2', icon: 'text-5xl' },
}

/** Colour classes of a UiAccentButton per appearance. */
export const ACCENT_BUTTON_CLASSES: Record<UiAccentAppearance, string> = {
  [UiAccentAppearance.Solid]:
    'bg-[color:var(--accent)] text-white shadow-[0_8px_30px_color-mix(in_srgb,var(--accent)_25%,transparent)] hover:brightness-110',
  [UiAccentAppearance.Soft]:
    'border bg-[color:color-mix(in_srgb,var(--accent)_10%,transparent)] border-[color:color-mix(in_srgb,var(--accent)_25%,transparent)] text-[color:var(--accent)] hover:bg-[color:color-mix(in_srgb,var(--accent)_20%,transparent)]',
  [UiAccentAppearance.Outline]:
    'border-2 border-[color:color-mix(in_srgb,var(--accent)_40%,transparent)] text-[color:var(--accent)] hover:bg-white dark:hover:bg-white/5',
}
