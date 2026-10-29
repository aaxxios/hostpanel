/**
 * Tests for the fallback labels: a present value passes through, a missing one becomes "Unknown"
 * or "N/A", and the registration period is pluralised, with "N/A" for 0 or less.
 */
import { describe, expect, it } from 'vitest'
import { defineComponent } from 'vue'
import { mount } from '@vue/test-utils'
import { useFallbackLabels } from '@/composables/useFallbackLabels'
import type { FallbackLabels } from '@/composables/useFallbackLabels'
import { createTestI18n } from '../helpers/i18n'

/**
 * Mounts a throwaway component so the composable runs inside a real i18n context.
 *
 * @returns The labels under test.
 */
const labels = (): FallbackLabels => {
  let result: FallbackLabels | undefined
  mount(
    defineComponent({
      setup: () => {
        result = useFallbackLabels()
        return () => null
      },
    }),
    { global: { plugins: [createTestI18n()] } },
  )
  if (!result) {
    throw new Error('composable did not run')
  }
  return result
}

describe('useFallbackLabels', () => {
  it('passes a present value through and shows Unknown for a missing one', () => {
    const { orUnknown } = labels()
    expect(orUnknown('Ada')).toBe('Ada')
    expect(orUnknown('  ')).toBe('Unknown')
    expect(orUnknown('')).toBe('Unknown')
    expect(orUnknown(null)).toBe('Unknown')
  })

  it('shows N/A for an empty value', () => {
    const { orNotAvailable } = labels()
    expect(orNotAvailable(null)).toBe('N/A')
    expect(orNotAvailable('x')).toBe('x')
  })

  it('pluralises years and treats 0 or less as unknown', () => {
    const { yearsLabel } = labels()
    expect(yearsLabel(1)).toBe('1 year')
    expect(yearsLabel(3)).toBe('3 years')
    expect(yearsLabel(0)).toBe('N/A')
    expect(yearsLabel(-1)).toBe('N/A')
  })
})
