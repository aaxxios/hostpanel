/**
 * The typefaces an operator can pick, and the CSS stack each one resolves to.
 *
 * Every face here is an `@fontsource` package the storefront already ships and imports
 * in `assets/css/main.css`, so choosing one never causes a request to a font CDN. The
 * names mirror `PortalSettingKeys.FontNames` on the backend, which is what the API
 * accepts for `portal.brand.font_heading` / `font_body`.
 *
 * Every stack ends in Noto Sans Armenian: Inter has no Armenian glyphs, and an
 * Armenian-locale page set to Inter would otherwise fall through to whatever the device
 * has: usually a serif that matches nothing else on the page.
 *
 * @module utils/brandFonts
 */

/** A typeface an operator can pick; the values are the names the backend accepts. */
export enum BrandFont {
  /** Inter, the storefront's default face. */
  Inter = 'inter',
  /** Noto Sans Armenian. */
  NotoSansArmenian = 'noto-sans-armenian',
  /** Noto Serif Armenian. */
  NotoSerifArmenian = 'noto-serif-armenian',
  /** The device's own UI face. */
  System = 'system',
}

/** The Armenian fallback every stack ends in. */
const ARMENIAN_FALLBACK = "'Noto Sans Armenian', system-ui, sans-serif"

/** The CSS `font-family` stack per face. */
export const FONT_STACKS: Record<BrandFont, string> = {
  [BrandFont.Inter]: `'Inter Variable', Inter, ${ARMENIAN_FALLBACK}`,
  [BrandFont.NotoSansArmenian]: `'Noto Sans Armenian', Inter, system-ui, sans-serif`,
  [BrandFont.NotoSerifArmenian]: `'Noto Serif Armenian', Georgia, 'Times New Roman', ${ARMENIAN_FALLBACK}`,
  [BrandFont.System]: `system-ui, -apple-system, 'Segoe UI', Roboto, ${ARMENIAN_FALLBACK}`,
}

/** The faces, in the order the admin panel lists them. */
export const BRAND_FONTS = Object.values(BrandFont)

/**
 * Resolves a setting value to a stack.
 *
 * @param value Raw `portal.brand.font_*` value.
 * @returns The stack, or `''` when the value is empty or unknown: the template then keeps its own.
 */
export const fontStack = (value: string | null | undefined): string => {
  return value && value in FONT_STACKS ? FONT_STACKS[value as BrandFont] : ''
}
