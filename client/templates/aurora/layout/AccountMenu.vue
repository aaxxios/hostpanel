<template>
  <UiPopover v-model="open" panel-class="fixed right-[clamp(16px,4vw,48px)] top-[74px] w-[min(280px,calc(100vw-32px))]">
    <template #trigger="{ toggle }">
      <UiButton
        variant="bare"
        :aria-label="menuLabel"
        :aria-expanded="open"
        aria-haspopup="menu"
        data-testid="account-menu-trigger"
        class="h-10 w-10 rounded-xl border border-line2"
        :class="open ? 'bg-acbg' : ''"
        @click="toggle"
      >
        <UiAvatar
          :first-name="firstName"
          :last-name="lastName"
          :email="email"
          size="sm"
        />
      </UiButton>
    </template>

    <template #default="{ close }">
      <UiStack direction="row" align="center" gap="sm" class="min-w-0 px-3 py-2.5">
        <UiAvatar :first-name="firstName" :last-name="lastName" :email="email" />
        <UiStack gap="none" class="min-w-0">
          <UiText
            as="span"
            size="sm"
            weight="semibold"
            tone="inherit"
            truncate
            class="text-tx"
            data-testid="account-menu-name"
          >
            {{ displayName }}
          </UiText>
          <UiText as="span" size="xs" tone="inherit" truncate class="text-mut2">{{ email }}</UiText>
        </UiStack>
      </UiStack>

      <div class="my-1 h-px bg-line2" />

      <UiButton
        v-for="item in menuItems"
        :key="item.key"
        variant="bare"
        :to="item.to"
        role="menuitem"
        :data-testid="`account-menu-${item.key}`"
        class="w-full justify-start rounded-xl px-3 py-2.5 text-sm font-normal text-tx hover:bg-acbg"
        @click="close"
      >
        {{ item.label }}
      </UiButton>

      <div class="my-1 h-px bg-line2" />

      <UiButton
        variant="bare"
        role="menuitem"
        data-testid="account-menu-sign-out"
        class="w-full justify-start rounded-xl px-3 py-2.5 text-sm font-normal text-danger hover:bg-acbg"
        @click="onSignOut"
      >
        {{ t('client.nav.signOut') }}
      </UiButton>
    </template>
  </UiPopover>
</template>

<script setup lang="ts">
/**
 * aurora template account menu — the signed-in half of the site header.
 *
 * The public header used to render "Sign in" / "Get started" to everyone,
 * because it never consulted the session at all: the auth store was wired into
 * the client-area shell and into the legacy `components/layout/LayoutHeader.vue`, but
 * the active template's header read neither. This is the treatment that belongs
 * beside the apps launcher — avatar, account destinations, sign out.
 *
 * The avatar comes from `UiAvatar` and the floating panel from `UiPopover`, so nothing is
 * drawn by hand twice and the outside-click / Escape handling lives in one place.
 */
import {
  CLIENT_ACCOUNT_PATH, CLIENT_DASHBOARD_PATH, CLIENT_DOMAINS_PATH, CLIENT_INVOICES_PATH, CLIENT_SERVICES_PATH
} from '~/constants/clientRoutes'
const { t } = useI18n()
/** Prefixes a path with the active locale. */
const localePath = useLocalePath()
/** The session store. */
const authStore = useAuthStore()
/** Session state of the customer. */
const { user } = storeToRefs(authStore)
/** Session actions. */
const { fetchUser, signOut } = authStore

/** Whether the panel is open (v-model of the popover). */
const open = ref(false)

/** Given name, empty until the profile request resolves. */
const firstName = computed(() => user.value?.firstname ?? '')

/** Family name, empty until the profile request resolves. */
const lastName = computed(() => user.value?.lastname ?? '')

/** E-mail address, empty until the profile request resolves. */
const email = computed(() => user.value?.email ?? '')

/**
 * Name shown in the panel header.
 *
 * Falls back to the generic client-area label rather than an empty line: the
 * menu is rendered from the `authed` cookie, which is known during server
 * rendering, whereas the profile only arrives after mount.
 */
const displayName = computed(() =>
  [firstName.value, lastName.value].filter(Boolean).join(' ') || t('nav.clientArea'))

/** Accessible name for the trigger, which is otherwise only an avatar. */
const menuLabel = computed(() => displayName.value)

/** Destinations offered in the panel, in the order the client-area nav uses. */
const menuItems = computed(() => [
  { key: 'dashboard', label: t('client.nav.dashboard'), to: localePath(CLIENT_DASHBOARD_PATH) },
  { key: 'services', label: t('client.nav.services'), to: localePath(CLIENT_SERVICES_PATH) },
  { key: 'domains', label: t('client.nav.domains'), to: localePath(CLIENT_DOMAINS_PATH) },
  { key: 'invoices', label: t('client.nav.invoices'), to: localePath(CLIENT_INVOICES_PATH) },
  { key: 'account', label: t('client.nav.account'), to: localePath(CLIENT_ACCOUNT_PATH) },
])

/** Ends the session; in SSO mode this navigates the browser away. */
const onSignOut = async (): Promise<void> => {
  open.value = false
  await signOut()
}

onMounted(() => {
  // The cookie says there is a session; the name and e-mail behind it still have
  // to be fetched, and only on the client — the panel renders without them.
  fetchUser()
})
</script>
