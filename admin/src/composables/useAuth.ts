/** Thin composable over the auth store for components that only need the sign-in state. */
import { useAuthStore } from '@/stores/auth'

/**
 * Composable for authentication actions.
 *
 * Delegates to the Pinia auth store.
 *
 * @returns login, logout actions and reactive auth state.
 */
export const useAuth = () => {
  const store = useAuthStore()

  return {
    /** Currently authenticated user, null if not logged in. */
    user: store.user,
    /** True when a user is authenticated. */
    isAuthenticated: store.isAuthenticated,
    /** Logs in with email and password. */
    login: store.login,
    /** Logs out the current user. */
    logout: store.logout,
  }
}
