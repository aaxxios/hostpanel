/**
 * Unit tests for the chart theme: tokens are read from the root, with a fallback palette.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { paletteColor, readChartTheme } from '@/utils/ui/chart'

afterEach(() => {
  vi.restoreAllMocks()
})

describe('readChartTheme', () => {
  it('falls back to the built-in palette when the stylesheet declares nothing', () => {
    const theme = readChartTheme()

    expect(theme.palette).toHaveLength(6)
    expect(theme.grid).not.toBe('')
    expect(theme.tick).not.toBe('')
  })

  it('reads the chart variables from the root element', () => {
    const variables: Record<string, string> = { '--chart-grid': 'red', '--chart-1': 'blue' }
    vi.spyOn(globalThis, 'getComputedStyle').mockReturnValue({
      getPropertyValue: (name: string) => variables[name] ?? '',
    } as CSSStyleDeclaration)

    const theme = readChartTheme()

    expect(theme.grid).toBe('red')
    expect(theme.palette[0]).toBe('blue')
  })
})

describe('paletteColor', () => {
  it('cycles through the palette', () => {
    const theme = { grid: 'g', tick: 't', palette: ['a', 'b'] }

    expect(paletteColor(theme, 0)).toBe('a')
    expect(paletteColor(theme, 3)).toBe('b')
  })
})
