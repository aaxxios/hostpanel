/**
 * Per-item accent colour of the Ui* kit.
 *
 * A product, a slide or a plan carries its own colour as data (a hex from the catalogue), so no
 * Tailwind class can name it. `UiAccent` writes that colour once, as the `--accent` custom
 * property of a wrapper; every class here reads the property, so the colour-dependent look is a
 * literal Tailwind class (`text-[color:var(--accent)]`) and a screen never carries a hex or an
 * inline style of its own. A tint is made with `color-mix` at fixed opacity steps (8 %, 15 %,
 * 25 % and 31 %).
 *
 * @module utils/ui/accent
 */

/** The custom property `UiAccent` writes and the classes below read. */
export const ACCENT_VARIABLE = '--accent'

/** The custom property holding the text colour that reads on a solid accent fill. */
export const ACCENT_INK_VARIABLE = '--accent-ink'

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
 * A colour is operator data; anything that is not a plain hex (a `url()`, a second declaration)
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
  /** Accent as text or icon colour: a deeper mix on a light page so it holds AA, a lightened mix on a dark one. */
  text: 'text-[color:color-mix(in_srgb,var(--accent)_65%,black)] dark:text-[color:color-mix(in_srgb,var(--accent)_75%,white)]',
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
    'bg-[color:var(--accent)] text-[color:var(--accent-ink,white)] shadow-[0_8px_30px_color-mix(in_srgb,var(--accent)_25%,transparent)] hover:brightness-110',
  [UiAccentAppearance.Soft]:
    'border bg-[color:color-mix(in_srgb,var(--accent)_10%,transparent)] border-[color:color-mix(in_srgb,var(--accent)_25%,transparent)] text-[color:color-mix(in_srgb,var(--accent)_65%,black)] dark:text-[color:color-mix(in_srgb,var(--accent)_75%,white)] hover:bg-[color:color-mix(in_srgb,var(--accent)_20%,transparent)]',
  [UiAccentAppearance.Outline]:
    'border-2 border-[color:color-mix(in_srgb,var(--accent)_40%,transparent)] text-[color:color-mix(in_srgb,var(--accent)_65%,black)] dark:text-[color:color-mix(in_srgb,var(--accent)_75%,white)] hover:bg-white dark:hover:bg-white/5',
}

/**
 * Picks the text colour that reads on a solid fill of the given hex colour.
 *
 * Uses the WCAG relative luminance: a light accent gets near-black text, a dark one white, so a
 * solid accent button never carries white on a pale colour. CSS cannot choose between two colours
 * by luminance, which is why the choice is made here and written next to `--accent`.
 *
 * @param hex A hex colour accepted by `isAccentColor`.
 * @returns `white` or a near-black hex.
 */
export const accentInk = (hex: string): string => {
  const raw = hex.slice(1)
  const full = raw.length <= 4 ? raw.split('').map((c) => c + c).join('') : raw
  const channel = (i: number): number => {
    const v = parseInt(full.slice(i, i + 2), 16) / 255
    return v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4
  }
  const luminance = 0.2126 * channel(0) + 0.7152 * channel(2) + 0.0722 * channel(4)
  // White wins above a 4.5:1 ratio against the fill, i.e. luminance below about 0.18.
  return luminance < 0.18 ? 'white' : '#08090f'
}

/**
 * Class that points `--accent` at the portal's primary brand colour, for a tile or panel that sits
 * outside any `UiAccent` (a section card that has no catalogue colour of its own). Without it the
 * accent classes have no colour to mix and the tile turns white, which vanishes on a light page.
 * A complete literal, so Tailwind emits it; the fallback matches the Tailwind primary-500 default.
 */
export const ACCENT_BRAND_CLASS = '[--accent:rgb(var(--brand-primary-500,2_119_182))]'
