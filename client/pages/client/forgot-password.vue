<template>
  <ClientForgotPasswordPanel />
</template>

<script setup lang="ts">
/**
 * Password reset request page at `/client/forgot-password`, the address the sign-in page links to.
 *
 * Local mode only: under SSO mode the page forwards to the SSO's reset page before rendering,
 * because the C# API refuses the request there. When no SSO address is configured there is
 * nowhere to forward to, so the local form renders instead of a link built from an empty value. The form itself is the shared
 * {@link ClientForgotPasswordPanel}.
 */

definePageMeta({ layout: false })

/** Runtime config; its public half carries the auth mode and the portal URLs. */
const config = useRuntimeConfig()
/** The SSO's public address; empty when none is configured. */
const ssoPublicUrl = (config.public.ssoPublicUrl as string | undefined) ?? ''
/** Whether the platform SSO, not a local form, is the identity provider. */
const { isSso } = useAuthMode()

// Under SSO mode the accounts -- and their passwords -- belong to innovayse-sso, and the C# API
// answers this page's request with 404 by design (`LocalAuthController.ForgotPasswordAsync`).
// Sent here anyway, a visitor got a form that could never succeed; they go to the SSO's own
// reset page instead, the same host `/client/login` hands them to for sign-in.
if (isSso.value && ssoPublicUrl) {
  await navigateTo(`${ssoPublicUrl}/forgot-password`, { external: true })
}
</script>
