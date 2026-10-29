/**
 * Client auth initialisation plugin: runs on CLIENT SIDE ONLY
 *
 * After Nuxt hydrates on the browser, if the `authed` cookie is present
 * (meaning the user logged in previously), we fetch their profile once and
 * keep it in the auth store.
 *
 * This ensures the client layout sidebar shows the user name/email immediately
 * on any client-side navigation without each page needing to fetch it separately.
 *
 * Note: `import.meta.client` guard makes this a no-op on the server,
 * preventing a redundant billing API call during SSR.
 *
 * @module plugins/client-auth
 */

/** Loads the signed-in user's profile once after hydration. */
export default defineNuxtPlugin(async () => {
  if (!import.meta.client) {
    return
  }

  const authStore = useAuthStore()

  // Only fetch if a session cookie exists
  if (authStore.isLoggedIn) {
    await authStore.fetchUser()
  }
})
