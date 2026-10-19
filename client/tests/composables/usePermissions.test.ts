/**
 * Tests for `usePermissions`: bit-flag checks against the signed-in user's permission integer.
 *
 * @module tests/composables/usePermissions.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { Permission, usePermissions } from '~/composables/usePermissions'

/**
 * Stubs the auth and client stores: the client store holds the user, the auth store holds
 * `authPermissions` (nobody by default).
 *
 * @param permissions The client store user's permission integer; `null` stands for nobody loaded.
 * @param authPermissions The auth store user's permission integer; `null` stands for nobody.
 */
const stubUser = (permissions: number | null, authPermissions: number | null = null): void => {
  /** The user the client store exposes. */
  const user = ref(permissions === null ? null : { permissions })
  /** The user the auth store exposes. */
  const authUser = ref(authPermissions === null ? null : { permissions: authPermissions })
  vi.stubGlobal('useAuthStore', () => ({ user: authUser }))
  vi.stubGlobal('useClientStore', () => ({ user }))
  vi.stubGlobal('storeToRefs', (store: unknown) => store)
}

describe('usePermissions', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('exposes the permission constants', () => {
    stubUser(null)

    expect(usePermissions().Permission).toBe(Permission)
  })

  it('grants a flag the user holds and refuses one they do not', () => {
    stubUser(Permission.ViewDomains | Permission.ViewPayInvoices)
    const { hasPermission } = usePermissions()

    expect(hasPermission(Permission.ViewDomains)).toBe(true)
    expect(hasPermission(Permission.PlaceNewOrders)).toBe(false)
  })

  it('treats the None flag as always granted', () => {
    stubUser(null)
    const { hasPermission, hasAllPermissions } = usePermissions()

    expect(hasPermission(Permission.None)).toBe(true)
    expect(hasAllPermissions(Permission.None)).toBe(true)
  })

  it('grants nothing to a visitor who is not signed in', () => {
    stubUser(null)
    const { hasPermission, hasAllPermissions } = usePermissions()

    expect(hasPermission(Permission.ViewDomains)).toBe(false)
    expect(hasAllPermissions(Permission.ViewDomains)).toBe(false)
  })

  it('grants every flag to the owner default', () => {
    stubUser(Permission.All)
    const { hasPermission } = usePermissions()

    expect(hasPermission(Permission.ModifyMasterProfile)).toBe(true)
    expect(hasPermission(Permission.PlaceNewOrders)).toBe(true)
  })

  it('hasPermission accepts any one bit of a combined mask', () => {
    stubUser(Permission.ViewDomains)

    expect(usePermissions().hasPermission(Permission.ViewDomains | Permission.ViewEmails)).toBe(true)
  })

  it('hasAllPermissions needs every bit of a combined mask', () => {
    stubUser(Permission.ViewDomains)
    const { hasAllPermissions } = usePermissions()

    expect(hasAllPermissions(Permission.ViewDomains)).toBe(true)
    expect(hasAllPermissions(Permission.ViewDomains | Permission.ViewEmails)).toBe(false)
  })

  it('reads the client store even when the auth store never loaded its user', () => {
    stubUser(Permission.ViewDomains, null)

    expect(usePermissions().hasPermission(Permission.ViewDomains)).toBe(true)
  })

  it('falls back to the auth store user while the client store has none', () => {
    stubUser(null, Permission.ViewPayInvoices)

    expect(usePermissions().hasPermission(Permission.ViewPayInvoices)).toBe(true)
  })
})
