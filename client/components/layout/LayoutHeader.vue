<template>
  <header
    :class="headerClasses"
    class="fixed top-0 left-0 right-0 z-40 transition-all duration-300"
    data-testid="layout-header"
  >
    <UiContainer>
      <UiStack direction="row" align="center" justify="between" gap="none" class="h-16 lg:h-20">
        <!-- Logo -->
        <UiLink :to="localePath('/')" tone="inherit" :underline="false" class="flex items-center" data-testid="layout-logo">
          <!-- Follows the colour mode: dark wordmark on the dark bar, light wordmark on the light one. -->
          <AppLogo
            :surface="LogoSurface.Auto"
            img-class="h-10 sm:h-12 lg:h-14 xl:h-16 w-auto max-w-[200px] object-contain transition-transform hover:scale-105"
          />
        </UiLink>

        <!-- Desktop navigation (1280px+) -->
        <nav class="hidden xl:flex items-center space-x-6 lg:space-x-8" data-testid="layout-desktop-nav">
          <template v-for="link in navLinks" :key="link.href ?? link.label">
            <!-- Regular link -->
            <UiLink
              v-if="!link.children"
              :to="link.href"
              tone="muted"
              :underline="false"
              class="relative text-sm lg:text-base font-medium rounded-md px-1 py-1 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500/60 focus-visible:ring-offset-2 focus-visible:ring-offset-white dark:focus-visible:ring-offset-gray-950 after:absolute after:inset-x-1 after:-bottom-0.5 after:h-0.5 after:rounded-full after:bg-primary-500 after:transition-transform after:origin-left hover:after:scale-x-100"
              :class="isActive(link.href) ? 'text-primary-600 dark:text-primary-400 after:scale-x-100' : 'text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 after:scale-x-0'"
              :aria-current="isActive(link.href) ? 'page' : undefined"
              data-testid="layout-nav-link"
            >
              {{ link.label }}
            </UiLink>

            <!-- Dropdown group -->
            <UiStack v-else gap="none" class="relative group" data-testid="layout-nav-group">
              <UiButton
data-testid="layout-header-trigger"
                variant="ghost"
                size="sm"
                class="gap-1 text-sm lg:text-base font-medium hover:text-primary-600 dark:hover:text-primary-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500/60 focus-visible:ring-offset-2 focus-visible:ring-offset-white dark:focus-visible:ring-offset-gray-950"
                :class="isGroupActive(link) ? 'text-primary-600 dark:text-primary-400' : 'text-gray-700 dark:text-gray-300'"
              >
                {{ link.label }}
                <ChevronDown :size="14" class="transition-transform duration-200 group-hover:rotate-180" />
              </UiButton>

              <!-- Dropdown panel -->
              <UiStack
                gap="none"
                class="absolute left-1/2 -translate-x-1/2 top-full pt-2 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200"
              >
                <UiCard padding="none" class="w-52 rounded-xl bg-white/95 dark:bg-gray-900/95 shadow-2xl backdrop-blur-lg overflow-hidden">
                  <UiLink
                    v-for="child in link.children"
                    :key="child.href"
                    :to="child.href"
                    tone="muted"
                    :underline="false"
                    class="flex items-center gap-3 px-4 py-3 text-sm rounded-none hover:bg-gray-100 dark:hover:bg-white/10 focus-visible:outline-none focus-visible:bg-gray-100 dark:focus-visible:bg-white/10"
                    :class="isActive(child.href) ? 'bg-primary-50 text-primary-700 dark:bg-primary-500/10 dark:text-primary-300' : 'text-gray-700 dark:text-gray-300'"
                    :aria-current="isActive(child.href) ? 'page' : undefined"
                    data-testid="layout-nav-child"
                  >
                    <component :is="navIcon(child.icon)" v-if="child.icon" :size="16" class="text-primary-600 dark:text-primary-400 flex-shrink-0" />
                    <UiStack gap="none" as="span">
                      <UiText as="span" size="sm" weight="medium">{{ child.label }}</UiText>
                      <UiText v-if="child.desc" as="span" size="xs" tone="subtle" class="mt-0.5">{{ child.desc }}</UiText>
                    </UiStack>
                  </UiLink>
                </UiCard>
              </UiStack>
            </UiStack>
          </template>
        </nav>

        <!-- Language switcher, cart and account widgets (desktop 1280px+) -->
        <UiStack direction="row" align="center" gap="sm" class="hidden xl:flex">
          <!-- Global App Launcher widget mount point -->
          <UiStack id="inno-launcher-mount" gap="none" />

          <UiLanguageSwitcher layout="responsive" />

          <UiCurrencySwitcher />

          <!-- Light/dark switch: hidden by the component when the operator turns the toggle off -->
          <UiThemeToggle />

          <LayoutCartButton />

          <!-- Global Account widget mount point (App Launcher widget injects here) -->
          <UiStack id="inno-account-mount" gap="none" />
        </UiStack>

        <!-- Mobile: cart and burger grouped on the right -->
        <UiStack direction="row" align="center" gap="xs" class="xl:hidden">
          <UiThemeToggle />

          <LayoutCartButton />

          <UiIconButton
            :icon="mobileMenuOpen ? X : Menu"
            :label="mobileMenuOpen ? $t('nav.closeMenu') : $t('nav.openMenu')"
            variant="ghost"
            testid="layout-mobile-toggle"
            @click="toggleMobileMenu"
          />
        </UiStack>
      </UiStack>
    </UiContainer>

    <!-- Mobile navigation (below 1280px) -->
    <Transition
      enter-active-class="transition-all duration-300 ease-in-out overflow-hidden"
      leave-active-class="transition-all duration-300 ease-in-out overflow-hidden"
      enter-from-class="max-h-0 opacity-0"
      enter-to-class="max-h-[600px] opacity-100"
      leave-from-class="max-h-[600px] opacity-100"
      leave-to-class="max-h-0 opacity-0"
    >
      <UiStack
        v-if="mobileMenuOpen"
        gap="none"
        class="xl:hidden border-t border-gray-200 dark:border-gray-800 bg-white/95 dark:bg-gray-900/95 backdrop-blur-lg"
        data-testid="layout-mobile-menu"
      >
        <UiContainer as="nav">
          <UiStack gap="xs" class="py-3 sm:py-4">
            <template v-for="link in navLinks" :key="link.href ?? link.label">
              <!-- Regular mobile link -->
              <UiLink
                v-if="!link.children"
                :to="link.href"
                tone="muted"
                :underline="false"
                class="block py-2.5 sm:py-3 px-3 sm:px-4 rounded-lg text-sm sm:text-base hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-primary-600 dark:hover:text-primary-400 font-medium focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500/60 focus-visible:ring-offset-2 focus-visible:ring-offset-white dark:focus-visible:ring-offset-gray-950"
                :class="isActive(link.href) ? 'bg-primary-50 text-primary-700 dark:bg-primary-500/10 dark:text-primary-300' : 'text-gray-700 dark:text-gray-300'"
                :aria-current="isActive(link.href) ? 'page' : undefined"
                data-testid="layout-mobile-link"
                @click="closeMobileMenu"
              >
                {{ link.label }}
              </UiLink>

              <!-- Mobile expandable group -->
              <UiStack v-else gap="none">
                <UiButton
                  variant="ghost"
                  full-width
                  class="justify-between text-sm sm:text-base font-medium"
                  data-testid="layout-mobile-group"
                  @click="toggleMobileGroup(link.label)"
                >
                  {{ link.label }}
                  <ChevronDown
                    :size="16"
                    class="transition-transform duration-200"
                    :class="expandedMobileGroups.includes(link.label) ? 'rotate-180' : ''"
                  />
                </UiButton>
                <UiStack v-if="expandedMobileGroups.includes(link.label)" gap="none" class="pl-4 mt-1 space-y-1">
                  <UiLink
                    v-for="child in link.children"
                    :key="child.href"
                    :to="child.href"
                    tone="muted"
                    :underline="false"
                    class="flex items-center gap-2 py-2 px-3 rounded-lg text-sm hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-primary-600 dark:hover:text-primary-400"
                    data-testid="layout-mobile-child"
                    @click="closeMobileMenu"
                  >
                    <component :is="navIcon(child.icon)" v-if="child.icon" :size="14" class="text-primary-600 dark:text-primary-400 flex-shrink-0" />
                    {{ child.label }}
                  </UiLink>
                </UiStack>
              </UiStack>
            </template>

            <!-- Client area, signed in: expandable group -->
            <UiStack v-if="isLoggedIn" gap="none">
              <UiButton
                variant="ghost"
                full-width
                class="justify-between text-sm sm:text-base font-medium"
                data-testid="layout-mobile-client-group"
                @click="toggleMobileGroup(CLIENT_AREA_GROUP)"
              >
                <UiStack direction="row" align="center" gap="xs" as="span">
                  <UiAvatar :first-name="user?.firstname ?? ''" :last-name="user?.lastname ?? ''" :email="user?.email ?? ''" size="sm" />
                  {{ user?.firstname ?? $t('nav.clientArea') }}
                </UiStack>
                <ChevronDown
                  :size="16"
                  class="transition-transform duration-200"
                  :class="expandedMobileGroups.includes(CLIENT_AREA_GROUP) ? 'rotate-180' : ''"
                />
              </UiButton>
              <UiStack v-if="expandedMobileGroups.includes(CLIENT_AREA_GROUP)" gap="none" class="pl-4 mt-1 space-y-1">
                <UiLink
                  v-for="clientLink in clientLinks"
                  :key="clientLink.to"
                  :to="clientLink.to"
                  tone="muted"
                  :underline="false"
                  class="flex items-center gap-2 py-2 px-3 rounded-lg text-sm hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-primary-600 dark:hover:text-primary-400"
                  data-testid="layout-client-link"
                  @click="closeMobileMenu"
                >
                  <component :is="clientLink.icon" :size="14" class="text-primary-600 dark:text-primary-400 flex-shrink-0" />
                  {{ clientLink.label }}
                </UiLink>
                <UiButton
                  variant="ghost"
                  size="sm"
                  full-width
                  class="justify-start gap-2 px-3 text-red-600 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300"
                  data-testid="layout-logout"
                  @click="handleLogout"
                >
                  <LogOut :size="14" class="flex-shrink-0" />
                  {{ $t('client.nav.signOut') }}
                </UiButton>
              </UiStack>
            </UiStack>

            <!-- Client area, signed out: simple link (SSO mode goes directly to SSO) -->
            <UiLink
              v-else
              :href="clientAreaHref"
              tone="muted"
              :underline="false"
              class="flex items-center gap-2 py-2.5 sm:py-3 px-3 sm:px-4 rounded-lg text-sm sm:text-base text-gray-700 hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-primary-600 dark:hover:text-primary-400 font-medium"
              data-testid="layout-client-area-link"
              @click="closeMobileMenu"
            >
              <User :size="16" />
              {{ $t('nav.clientArea') }}
            </UiLink>

            <!-- Language switcher in mobile -->
            <UiStack gap="none" class="pt-2 pb-2 px-3 sm:px-4">
              <UiLanguageSwitcher layout="responsive" />
              <UiCurrencySwitcher class="mt-2" />
            </UiStack>
          </UiStack>
        </UiContainer>
      </UiStack>
    </Transition>
  </header>

  <!-- Spacer to prevent content from going under the fixed header -->
  <UiStack gap="none" class="h-16 lg:h-20" />
</template>

<script setup lang="ts">
/**
 * LayoutHeader - the site header: logo, navigation, language switcher, cart and the client-area
 * entry.
 *
 * Features:
 * - Blurred bar once the page has scrolled.
 * - Dropdown groups for grouped navigation (hosting, domains) on desktop.
 * - Burger menu with expandable groups below 1280px.
 * - Client-area link that is state-aware: sign-in when signed out, a group of account links
 *   (with sign-out) when signed in.
 */
import { storeToRefs } from 'pinia'
import type { Component } from 'vue'
import {
  CLIENT_ACCOUNT_PATH,
  CLIENT_DASHBOARD_PATH,
  CLIENT_DOMAINS_PATH,
  CLIENT_INVOICES_PATH,
  CLIENT_LOGIN_PATH,
  CLIENT_SERVICES_PATH,
  CLIENT_TICKETS_PATH,
  SSO_AUTHORIZE_PATH
} from '~/constants/clientRoutes'
import {
  ChevronDown, FileText, Globe, LayoutDashboard, LogOut, Menu, MessageSquare, Server, Settings, User, X
} from 'lucide-vue-next'
import { useAuthStore } from '~/stores/auth'
import { useCartStore } from '~/stores/cart'
import { LogoSurface } from '~/utils/appLogo'
import { HomeSectionId } from '~/types/homesectionid'
import { homeSectionPath } from '~/constants/homeSections'
import { NavIcon } from '~/types/navicon'
import type { NavLink } from '~/types/navlink'
import { AuthMode } from '~/types/authmode'

/** Translate function. */
const { t } = useI18n()

/** Prefixes a path with the active locale. */
const localePath = useLocalePath()

/** Key of the signed-in account group in {@link expandedMobileGroups}; no nav label can equal it. */
const CLIENT_AREA_GROUP = 'clientArea'

/** Scroll distance (px) after which the header turns translucent. */
const SCROLL_BLUR_THRESHOLD = 50

/** Glyph of each navigation icon key. */
const NAV_ICONS: Record<NavIcon, Component> = {
  [NavIcon.Server]: Server,
  [NavIcon.Globe]: Globe
}

/**
 * Resolves a nav entry's icon key to its glyph component.
 *
 * A key outside the map would otherwise render nothing; the fallback keeps an entry given a new
 * key showing a generic glyph rather than a hole.
 *
 * @param key - The `icon` field of a nav entry.
 * @returns The component to render.
 */
const navIcon = (key: NavIcon | undefined): Component => (key ? NAV_ICONS[key] : undefined) ?? Server

/**
 * Navigation links.
 * Entries with `children` render as a dropdown (desktop) / expandable group (mobile).
 */
const navLinks = computed<NavLink[]>(() => [
  {
    label: t('nav.hosting'),
    children: [
      {
        href: localePath('/hosting'),
        label: t('nav.hosting'),
        desc: t('nav.hostingDesc'),
        icon: NavIcon.Server
      },
      {
        href: localePath('/domains'),
        label: t('nav.domains'),
        desc: t('nav.domainsDesc'),
        icon: NavIcon.Globe
      }
    ]
  },
  { label: t('nav.services'), href: homeSectionPath(localePath, HomeSectionId.Services) },
  { label: t('nav.process'), href: homeSectionPath(localePath, HomeSectionId.Process) },
  { label: t('nav.products'), href: localePath('/products') }
])

/** Links of the signed-in account group. */
const clientLinks = computed(() => [
  { to: localePath(CLIENT_DASHBOARD_PATH), icon: LayoutDashboard, label: t('client.nav.dashboard') },
  { to: localePath(CLIENT_SERVICES_PATH), icon: Server, label: t('client.nav.services') },
  { to: localePath(CLIENT_DOMAINS_PATH), icon: Globe, label: t('client.nav.domains') },
  { to: localePath(CLIENT_INVOICES_PATH), icon: FileText, label: t('client.nav.invoices') },
  { to: localePath(CLIENT_TICKETS_PATH), icon: MessageSquare, label: t('client.nav.support') },
  { to: localePath(CLIENT_ACCOUNT_PATH), icon: Settings, label: t('client.nav.account') }
])

/** Authentication state for the client-area entry. */
const authStore = useAuthStore()

// storeToRefs, not a plain destructure: state and getters lose their reactivity when pulled
// off a store by hand, and the template above binds them.
/** Whether a client is signed in, and who. */
const { isLoggedIn, user } = storeToRefs(authStore)

/** Auth actions: reload the signed-in client, sign out. */
const { fetchUser, logout } = authStore

/** Runtime config: tells whether sign-in goes through the single-sign-on server. */
const runtimeConfig = useRuntimeConfig()

/** In SSO mode, link directly to the SSO authorize endpoint (skips the intermediate page). */
const clientAreaHref = computed(() =>
  runtimeConfig.public.authMode === AuthMode.Sso
    ? SSO_AUTHORIZE_PATH
    : localePath(CLIENT_LOGIN_PATH)
)

/** Mobile menu open/close state. */
const mobileMenuOpen = ref(false)

/** Which mobile nav groups are currently expanded. */
const expandedMobileGroups = ref<string[]>([])

/** Scroll Y position for the header blur effect. */
const scrollY = ref(0)

/** Cart store: its items are loaded from storage when the header mounts. */
const cartStore = useCartStore()

/**
 * Toggles a mobile nav group open or closed.
 *
 * @param label - Key of the group (its label, or {@link CLIENT_AREA_GROUP}).
 */
const toggleMobileGroup = (label: string): void => {
  const idx = expandedMobileGroups.value.indexOf(label)
  if (idx === -1) {
    expandedMobileGroups.value.push(label)
  } else {
    expandedMobileGroups.value.splice(idx, 1)
  }
}

/** Toggles the mobile menu, locking body scroll while it is open. */
const toggleMobileMenu = (): void => {
  mobileMenuOpen.value = !mobileMenuOpen.value
  if (typeof document !== 'undefined') {
    document.body.style.overflow = mobileMenuOpen.value ? 'hidden' : ''
  }
}

/** Closes the mobile menu and re-enables body scroll. */
const closeMobileMenu = (): void => {
  mobileMenuOpen.value = false
  expandedMobileGroups.value = []
  if (typeof document !== 'undefined') {
    document.body.style.overflow = ''
  }
}

/** Signs the client out, closes the menu and goes to the sign-in page. */
const handleLogout = async (): Promise<void> => {
  await logout()
  closeMobileMenu()
  await navigateTo(localePath(CLIENT_LOGIN_PATH))
}

/** Records the scroll position for the blur effect. */
const handleScroll = (): void => {
  scrollY.value = window.scrollY
}

/** Header background classes - blurred when scrolled down. */
const headerClasses = computed(() => {
  if (scrollY.value > SCROLL_BLUR_THRESHOLD) {
    return 'bg-white/80 dark:bg-gray-900/80 backdrop-blur-lg shadow-md border-b border-gray-200 dark:border-gray-800'
  }
  return 'bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800'
})

onMounted(async () => {
  window.addEventListener('scroll', handleScroll)
  cartStore.init()
  if (isLoggedIn.value) {
    await fetchUser()
  }

  // Reinit the widget so it picks up any accounts saved by the callback flow
  const widgetHost = window as unknown as { InnoWidget?: { reinit: () => void } }
  widgetHost.InnoWidget?.reinit?.()
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
  if (typeof document !== 'undefined') {
    document.body.style.overflow = ''
  }
})

/** Current route; a change closes the mobile menu. */
const route = useRoute()

/**
 * Whether a navigation target is the page being viewed. A section link matches the home path
 * with its hash; any other link matches its own path or a page below it.
 *
 * @param href - The link target, possibly undefined for a group.
 * @returns True for the current target.
 */
const isActive = (href: string | undefined): boolean => {
  if (!href) {
    return false
  }
  if (href.includes('#')) {
    return `${route.path}${route.hash ?? ''}` === href
  }
  return route.path === href || route.path.startsWith(`${href}/`)
}

/**
 * Whether one of a dropdown group's children is the page being viewed.
 *
 * @param link - The group entry.
 * @returns True when a child is active.
 */
const isGroupActive = (link: NavLink): boolean => (link.children ?? []).some(child => isActive(child.href))
watch(() => route.path, () => {
  closeMobileMenu()
})
</script>
