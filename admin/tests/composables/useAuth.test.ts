/**
 * Tests for `useAuth`: it exposes the auth store's session state and its two actions.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useAuth } from '@/composables/useAuth'
import { useAuthStore } from '@/stores/auth'

beforeEach(() => {
  setActivePinia(createPinia())
})

describe('useAuth', () => {
  it('reports no user while signed out', () => {
    const auth = useAuth()

    expect(auth.user).toBeNull()
    expect(auth.isAuthenticated).toBe(false)
  })

  it('reads the signed-in user from the store', () => {
    useAuthStore().user = { email: 'a@b.c', roles: ['Admin'] }

    const auth = useAuth()

    expect(auth.user).toEqual({ email: 'a@b.c', roles: ['Admin'] })
    expect(auth.isAuthenticated).toBe(true)
  })

  it('hands out the store actions', () => {
    const auth = useAuth()
    const store = useAuthStore()

    expect(auth.login).toBe(store.login)
    expect(auth.logout).toBe(store.logout)
  })
})
