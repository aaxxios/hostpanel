/**
 * Chart vocabulary and theme reading for `UiChart`.
 *
 * Report screens hand `UiChart` labels and series; this module turns the active theme tokens
 * (CSS variables on `<html>`) into the concrete colours chart.js needs, so a screen never
 * configures the chart library and a theme switch recolours every chart.
 */

/** Closed set of chart kinds `UiChart` draws. */
export const UiChartType = {
  Bar: 'bar',
  Line: 'line',
  Pie: 'pie',
  Doughnut: 'doughnut',
} as const

/** One of the chart kinds. */
export type UiChartType = (typeof UiChartType)[keyof typeof UiChartType]

/** Closed set of chart heights. */
export const UiChartHeight = {
  /** The `sm` option. */
  Sm: 'sm',
  /** The `md` option. */
  Md: 'md',
  /** The `lg` option. */
  Lg: 'lg',
} as const

/** One of the UiChartHeight values. */
export type UiChartHeight = (typeof UiChartHeight)[keyof typeof UiChartHeight]

/** One data series of a chart. */
export interface UiChartSeries {
  /** Legend label. */
  label: string
  /** Plotted values, one per label. */
  data: number[]
  /** Explicit colour; omitted, the series takes the next colour of the theme palette. */
  color?: string
}

/** Concrete colours resolved from the theme tokens. */
export interface UiChartTheme {
  /** Grid line colour. */
  grid: string
  /** Axis tick and legend text colour. */
  tick: string
  /** Series palette, cycled by series (or by slice for pie and doughnut). */
  palette: string[]
}

/** Palette used when the stylesheet is not loaded (unit tests, first paint). */
const FALLBACK_THEME: UiChartTheme = {
  grid: 'rgba(255,255,255,0.05)',
  tick: '#64748b',
  palette: ['#0ea5e9', '#a855f7', '#34d399', '#fbbf24', '#f87171', '#38bdf8'],
}

/** Number of palette colours the stylesheet declares (`--chart-1` .. `--chart-6`). */
const PALETTE_SIZE = 6

/**
 * Reads one CSS variable from the root element.
 *
 * @param styles - Computed style of the root element.
 * @param name - Variable name including the leading dashes.
 * @param fallback - Value used when the variable is empty.
 * @returns The trimmed value, or the fallback.
 */
const readVar = (styles: CSSStyleDeclaration, name: string, fallback: string): string => {
  const value = styles.getPropertyValue(name).trim()
  return value === '' ? fallback : value
}

/**
 * Resolves the chart colours from the active theme.
 *
 * @returns Grid, tick and palette colours for the current light/dark mode.
 */
export const readChartTheme = (): UiChartTheme => {
  if (typeof document === 'undefined') {
    return FALLBACK_THEME
  }
  const styles = getComputedStyle(document.documentElement)
  const palette: string[] = []
  for (let index = 1; index <= PALETTE_SIZE; index += 1) {
    palette.push(readVar(styles, `--chart-${index}`, FALLBACK_THEME.palette[index - 1] ?? '#0ea5e9'))
  }
  return {
    grid: readVar(styles, '--chart-grid', FALLBACK_THEME.grid),
    tick: readVar(styles, '--chart-tick', FALLBACK_THEME.tick),
    palette,
  }
}

/**
 * Picks the colour for the series (or slice) at an index, cycling the palette.
 *
 * @param theme - Resolved theme.
 * @param index - Zero-based series or slice index.
 * @returns A CSS colour string.
 */
export const paletteColor = (theme: UiChartTheme, index: number): string => {
  return theme.palette[index % theme.palette.length] ?? '#0ea5e9'
}
