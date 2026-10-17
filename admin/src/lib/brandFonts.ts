/**
 * The typefaces an operator can pick, with the Tailwind utility that previews each option.
 *
 * Mirrors `client/utils/brandFonts.ts` and `PortalSettingKeys.FontNames` on the backend —
 * the values are what the API accepts for `portal.brand.font_heading` / `font_body`. The
 * faces are the same `@fontsource` packages the storefront ships, imported in `main.css` so
 * an option previews in its own face without a request to a font CDN. The stacks themselves
 * live in `main.css` as `--font-brand-*` tokens, so no screen writes a `font-family` inline.
 */

/** The typefaces the API accepts; the values are the setting values. */
export enum BrandFont {
  /** Inter with an Armenian fallback. */
  Inter = 'inter',
  /** Noto Sans Armenian. */
  NotoSansArmenian = 'noto-sans-armenian',
  /** Noto Serif Armenian. */
  NotoSerifArmenian = 'noto-serif-armenian',
  /** The operating system's own face. */
  System = 'system',
}

/** One option in the typography card. */
export interface BrandFontOption {
  /** The setting value. */
  value: BrandFont
  /** The i18n key of the option's label. */
  labelKey: string
  /** The Tailwind utility that renders text in this face (a literal, so Tailwind emits it). */
  fontClass: string
}

/** The options, in the order the card lists them. */
export const BRAND_FONT_OPTIONS: BrandFontOption[] = [
  { value: BrandFont.Inter, labelKey: 'settings.typography.fonts.inter', fontClass: 'font-brand-inter' },
  { value: BrandFont.NotoSansArmenian, labelKey: 'settings.typography.fonts.notoSansArmenian', fontClass: 'font-brand-noto-sans-armenian' },
  { value: BrandFont.NotoSerifArmenian, labelKey: 'settings.typography.fonts.notoSerifArmenian', fontClass: 'font-brand-noto-serif-armenian' },
  { value: BrandFont.System, labelKey: 'settings.typography.fonts.system', fontClass: 'font-brand-system' },
]

/**
 * The preview utility class for a setting value.
 *
 * @param value - Raw setting value; empty or unknown means "the template's own".
 * @returns The Tailwind class, or `''` to inherit.
 */
export const previewClass = (value: string): string =>
  BRAND_FONT_OPTIONS.find((option) => option.value === value)?.fontClass ?? ''
