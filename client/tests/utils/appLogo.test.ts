import { describe, expect, it } from 'vitest'
import { LogoSurface, STATIC_LOGO_DARK, STATIC_LOGO_LIGHT, resolveAppLogo } from '~/utils/appLogo'

/** The pair with nothing uploaded. */
const empty = { light: '', dark: '', hasDarkVariant: false }

describe('resolveAppLogo', () => {
  it('uses the bundled pair, swapped by mode, when nothing is uploaded', () => {
    expect(resolveAppLogo(empty, LogoSurface.Auto)).toEqual({
      light: STATIC_LOGO_LIGHT,
      dark: STATIC_LOGO_DARK,
      uploaded: false,
      swap: true,
    })
  })

  it('uses only the dark bundled wordmark on an always-dark surface', () => {
    const plan = resolveAppLogo(empty, LogoSurface.Dark)
    expect(plan.dark).toBe(STATIC_LOGO_DARK)
    expect(plan.swap).toBe(false)
  })

  it('prefers the operator upload and swaps only when the two differ', () => {
    const same = { light: '/u/a.png', dark: '/u/a.png', hasDarkVariant: false }
    const split = { light: '/u/a.png', dark: '/u/b.png', hasDarkVariant: true }
    expect(resolveAppLogo(same, LogoSurface.Auto)).toMatchObject({ uploaded: true, swap: false })
    expect(resolveAppLogo(split, LogoSurface.Auto)).toMatchObject({ dark: '/u/b.png', swap: true })
    expect(resolveAppLogo(split, LogoSurface.Dark).dark).toBe('/u/b.png')
  })
})
