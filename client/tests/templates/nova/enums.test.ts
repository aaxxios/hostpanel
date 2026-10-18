/**
 * Unit tests for nova's closed value sets: the members keep the literal strings templates write.
 *
 * @module tests/templates/nova/enums.test
 */
import { describe, expect, it } from 'vitest'
import { NovaCtaSize, NovaCtaTone } from '~/templates/nova/enums'

describe('NovaCtaTone', () => {
  it('keeps the three tones as the strings templates write', () => {
    expect(Object.values(NovaCtaTone)).toEqual(['accent', 'outline', 'quiet'])
  })

  it('exposes each tone under its member name', () => {
    expect(NovaCtaTone.Accent).toBe('accent')
    expect(NovaCtaTone.Outline).toBe('outline')
    expect(NovaCtaTone.Quiet).toBe('quiet')
  })
})

describe('NovaCtaSize', () => {
  it('keeps the four sizes as the strings templates write', () => {
    expect(Object.values(NovaCtaSize)).toEqual(['lg', 'md', 'card', 'bar'])
  })

  it('exposes each size under its member name', () => {
    expect(NovaCtaSize.Lg).toBe('lg')
    expect(NovaCtaSize.Md).toBe('md')
    expect(NovaCtaSize.Card).toBe('card')
    expect(NovaCtaSize.Bar).toBe('bar')
  })
})
