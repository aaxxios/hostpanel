/**
 * Tests for `usePermissions`: bit-flag checks against the signed-in user's permission integer.
 *
 * @module tests/composables/usePermissions.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { Permission, usePermissions } from '~/composables/usePermissions'

/**
 * Stubs the auth store with a user holding the given permissions.
 *
 * @param permissions The user's permission integer; `null` stands for nobody signed in.
 */
const stubUser = (permissions: number | null): void => {
  /** The signed-in user the store exposes. */
  const user = ref(permissions === null ? null : { permissions })
  vi.stubGlobal('useAuthStore', () => ({ user }))
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
})
