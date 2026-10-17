<script setup lang="ts">
/**
 * Admin sidebar with navigation links.
 *
 * Supports expandable submenus for items with children.
 * Responsive behaviour:
 * - lg+: full sidebar (220px), always visible
 * - Emits `navigate` on link click so the mobile drawer can close.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useBrandingStore } from '@/stores/branding'
import UiButton from '@/components/ui/UiButton.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiIconButton from '@/components/ui/UiIconButton.vue'
import UiImage from '@/components/ui/UiImage.vue'
import UiLink from '@/components/ui/UiLink.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'

/** A sub-menu child item. */
interface NavChild {
  /** Route path. */
  to: string
  /** i18n key for the display label. */
  labelKey: string
}

/** A top-level navigation item, optionally with children. */
interface NavItem {
  /** Route path (used when no children). */
  to: string
  /** i18n key for the display label. */
  labelKey: string
  /** SVG path data for the icon. */
  icon: string
  /** Optional sub-menu items. */
  children?: NavChild[]
}

/** Emitted when a nav link is clicked (used to close mobile drawer). */
const emit = defineEmits<{
  /** Emitted after any navigation link is clicked. */
  navigate: []
}>()

/** Translation function. */
const { t } = useI18n()

/** Auth store: the sign-out action. */
const authStore = useAuthStore()

/** Router, for the redirect after sign-out. */
const router = useRouter()

/** Current route, to mark the active item. */
const route = useRoute()

/**
 * The operator's logo for this dark sidebar, falling back to the built-in mark.
 *
 * The panel used to hardcode /logo.svg, so a white-labelled deployment showed the
 * shipped mark through its whole back office while the storefront showed the
 * operator's. It then read `portal.logo` alone — the *light-background* variant —
 * and drew a logo made for white paper onto this dark panel. The store now picks the
 * dark-background variant first, the way the storefront does in its dark theme.
 */
const branding = useBrandingStore()

/**
 * Source of the logo image. The built-in fallback is the dark-background mark: `/logo.svg`
 * draws "host" in near-black for white paper, which vanished into this dark panel.
 */
const logoSrc = computed<string>(() => branding.logoOnDarkUrl || '/logo-dark.svg')

/** Set of expanded parent item routes (keyed by `to`, stable across locales). */
const expanded = ref<Set<string>>(new Set())

/** Chevron drawn after the label of an item that has a sub-menu. */
const CHEVRON_MARKUP = '<polyline points="9 18 15 12 9 6"/>'

/** Sign-out glyph. */
const SIGN_OUT_MARKUP = '<path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4M16 17l5-5-5-5M21 12H9"/>'

/** Top-level navigation items with SVG path icons. */
const navItems: NavItem[] = [
  {
    to: '/dashboard', labelKey: 'nav.dashboard',
    icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6',
  },
  {
    to: '/clients', labelKey: 'nav.clients',
    icon: 'M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2M9 11a4 4 0 100-8 4 4 0 000 8zm12 2v2m0 0v2m0-2h2m-2 0h-2',
    children: [
      { to: '/clients', labelKey: 'nav.clientsChildren.view' },
      { to: '/clients/users', labelKey: 'nav.clientsChildren.users' },
      { to: '/clients/shared-hostings', labelKey: 'nav.clientsChildren.sharedHostings' },
      { to: '/clients/addons', labelKey: 'nav.clientsChildren.addons' },
      { to: '/clients/domain-registrations', labelKey: 'nav.clientsChildren.domainRegistrations' },
      { to: '/clients/cancellations', labelKey: 'nav.clientsChildren.cancellations' },
      { to: '/clients/affiliates', labelKey: 'nav.clientsChildren.affiliates' },
    ],
  },
  {
    to: '/billing', labelKey: 'nav.billing',
    icon: 'M9 14l6-6m-5.5.5h.01m4.99 5h.01M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16l3.5-2 3.5 2 3.5-2 3.5 2z',
    children: [
      { to: '/billing/transactions', labelKey: 'nav.billingChildren.transactions' },
      { to: '/billing/invoices', labelKey: 'nav.billingChildren.invoices' },
      { to: '/billing/billable-items', labelKey: 'nav.billingChildren.billableItems' },
      { to: '/billing/quotes', labelKey: 'nav.billingChildren.quotes' },
      { to: '/billing/offline-cc', labelKey: 'nav.billingChildren.offlineCc' },
      { to: '/billing/disputes', labelKey: 'nav.billingChildren.disputes' },
      { to: '/billing/gateway-log', labelKey: 'nav.billingChildren.gatewayLog' },
    ],
  },
  {
    to: '/reports', labelKey: 'nav.reports',
    icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z',
  },
  {
    to: '/orders', labelKey: 'nav.orders',
    icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 7h6m-6 4h4',
  },
  {
    to: '/domains', labelKey: 'nav.domains',
    icon: 'M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9',
  },
  {
    to: '/support', labelKey: 'nav.support',
    icon: 'M18.364 5.636l-3.536 3.536m0 5.656l3.536 3.536M9.172 9.172L5.636 5.636m3.536 9.192l-3.536 3.536M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-5 0a4 4 0 11-8 0 4 4 0 018 0z',
    children: [
      { to: '/support/overview', labelKey: 'nav.supportChildren.overview' },
      { to: '/support/tickets', labelKey: 'nav.supportChildren.tickets' },
      { to: '/support/tickets/new', labelKey: 'nav.supportChildren.newTicket' },
      { to: '/support/predefined-replies', labelKey: 'nav.supportChildren.predefinedReplies' },
      { to: '/support/announcements', labelKey: 'nav.supportChildren.announcements' },
      { to: '/support/downloads', labelKey: 'nav.supportChildren.downloads' },
      { to: '/support/knowledgebase', labelKey: 'nav.supportChildren.knowledgebase' },
      { to: '/support/network-issues', labelKey: 'nav.supportChildren.networkIssues' },
    ],
  },
  {
    to: '/plugins', labelKey: 'nav.plugins',
    icon: 'M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4',
  },
  {
    to: '/servers', labelKey: 'nav.servers',
    icon: 'M5 12H3a2 2 0 00-2 2v4a2 2 0 002 2h18a2 2 0 002-2v-4a2 2 0 00-2-2h-2M5 12V8a2 2 0 012-2h10a2 2 0 012 2v4M5 12h14M8 20h.01M12 20h.01M16 20h.01',
  },
  {
    to: '/integrations', labelKey: 'nav.integrations',
    icon: 'M13 10V3L4 14h7v7l9-11h-7z',
  },
  {
    to: '/migration', labelKey: 'nav.migration',
    icon: 'M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4M17 8l-5-5-5 5M12 3v12',
  },
  {
    to: '/settings', labelKey: 'nav.settings',
    icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z',
    children: [
      { to: '/settings', labelKey: 'nav.settingsChildren.general' },
      { to: '/settings/products', labelKey: 'nav.settingsChildren.products' },
      { to: '/settings/tld-pricing', labelKey: 'nav.settingsChildren.tldPricing' },
      { to: '/settings/slides', labelKey: 'nav.settingsChildren.slides' },
      { to: '/settings/email-templates', labelKey: 'nav.settingsChildren.emailTemplates' },
      { to: '/integrations/payments', labelKey: 'nav.settingsChildren.gateways' },
      { to: '/settings/currencies', labelKey: 'nav.settingsChildren.currencies' },
    ],
  },
]

/** First letter of the account label, for the footer tile. */
const adminInitial = computed<string>(() => t('common.admin').charAt(0).toUpperCase())

/**
 * Turns SVG path data into the shape markup `UiIcon` draws.
 *
 * @param path - The `d` attribute of a path.
 * @returns Inner SVG markup holding that single path.
 */
const iconMarkup = (path: string): string => `<path d="${path}"/>`

/**
 * Builds the `data-testid` of a nav entry from its route.
 *
 * @param to - The route path.
 * @returns A stable id such as `nav-clients-users`.
 */
const testidOf = (to: string): string => `nav-${to.replace(/^\//, '').replaceAll('/', '-')}`

/**
 * Logs out the current user and redirects to login.
 *
 * @returns Promise that resolves after logout.
 */
const handleLogout = async (): Promise<void> => {
  await authStore.logout()
  await router.push('/login')
}

/**
 * Checks if a nav item or any of its children is currently active.
 *
 * @param item - The navigation item to check.
 * @returns True if the current route matches.
 */
const isActive = (item: NavItem): boolean => {
  if (item.to !== '/dashboard' && route.path.startsWith(item.to)) {
    return true
  }
  return route.path === item.to
}

/**
 * Checks if a child route is the currently active route.
 *
 * @param to - The child route path.
 * @returns True if the current route matches exactly.
 */
const isChildActive = (to: string): boolean => {
  if (to === '/clients' || to === '/settings') {
    return route.path === to
  }
  return route.path.startsWith(to)
}

/**
 * Toggles the expanded state of a parent nav item.
 *
 * @param to - The route path of the item to toggle.
 */
const toggleExpand = (to: string): void => {
  const next = new Set(expanded.value)
  if (next.has(to)) {
    next.delete(to)
  } else {
    next.add(to)
  }
  expanded.value = next
}

/**
 * Returns whether a parent item is currently expanded.
 * Auto-expands if the current route matches any child.
 *
 * @param item - The navigation item.
 * @returns True if expanded.
 */
const isExpanded = (item: NavItem): boolean => {
  if (expanded.value.has(item.to)) {
    return true
  }
  return item.children !== undefined && isActive(item)
}
</script>

<template>
  <aside class="flex h-full min-h-0 w-[220px] shrink-0 flex-col border-r border-border bg-surface-panel" data-testid="app-sidebar">
    <!-- Logo -->
    <UiStack direction="row" align="center" gap="none" class="border-b border-border px-5 py-[18px]">
      <UiImage :src="logoSrc" alt="" class="h-7 w-auto" />
    </UiStack>

    <!-- Nav links -->
    <nav class="flex-1 overflow-y-auto px-2.5 pb-2 pt-3" :aria-label="t('layout.navigation')">
      <UiStack gap="xs">
        <template v-for="item in navItems" :key="item.to">
          <!-- Item WITH children: toggle button, then the sub-menu -->
          <template v-if="item.children">
            <UiButton
              type="button"
              variant="plain"
              class="flex w-full items-center gap-2.5 rounded-[9px] px-3 py-2 text-left text-[0.84rem] font-medium transition-all duration-150"
              :class="isActive(item)
                ? 'bg-primary-500/10 text-text-primary'
                : 'text-text-secondary hover:bg-surface-hover hover:text-text-primary'"
              :aria-expanded="isExpanded(item)"
              :data-testid="testidOf(item.to)"
              @click="toggleExpand(item.to)"
            >
              <UiIcon :markup="iconMarkup(item.icon)" size="sm" :class="isActive(item) ? 'text-primary-400' : 'opacity-60'" />
              <span class="flex-1">{{ t(item.labelKey) }}</span>
              <UiIcon :markup="CHEVRON_MARKUP" size="sm" class="text-text-muted transition-transform duration-200" :class="isExpanded(item) ? 'rotate-90' : ''" />
            </UiButton>

            <UiStack v-show="isExpanded(item)" gap="xs" class="mb-0.5 mt-0.5 pl-[1.65rem]">
              <UiLink
                v-for="child in item.children"
                :key="child.to"
                :to="child.to"
                variant="plain"
                class="flex items-center gap-2 rounded-[7px] px-3 py-[5px] text-[0.78rem] font-medium no-underline transition-all duration-150"
                :class="isChildActive(child.to)
                  ? 'bg-primary-500/10 text-primary-400'
                  : 'text-text-muted hover:bg-surface-hover hover:text-text-secondary'"
                :data-testid="testidOf(child.to)"
                @click="emit('navigate')"
              >
                {{ t(child.labelKey) }}
              </UiLink>
            </UiStack>
          </template>

          <!-- Item WITHOUT children: direct link -->
          <UiLink
            v-else
            :to="item.to"
            variant="plain"
            class="flex items-center gap-2.5 rounded-[9px] px-3 py-2 text-[0.84rem] font-medium no-underline transition-all duration-150"
            :class="isActive(item)
              ? 'bg-primary-500/10 text-text-primary'
              : 'text-text-secondary hover:bg-surface-hover hover:text-text-primary'"
            :data-testid="testidOf(item.to)"
            @click="emit('navigate')"
          >
            <UiIcon :markup="iconMarkup(item.icon)" size="sm" :class="isActive(item) ? 'text-primary-400' : 'opacity-60'" />
            <span>{{ t(item.labelKey) }}</span>
          </UiLink>
        </template>
      </UiStack>
    </nav>

    <!-- Footer -->
    <UiStack direction="row" align="center" justify="between" class="border-t border-border px-3 py-3">
      <UiStack direction="row" align="center" gap="sm">
        <UiText tone="plain" class="flex h-7 w-7 shrink-0 items-center justify-center rounded-[7px] gradient-brand font-display text-[0.7rem] font-bold text-white">
          {{ adminInitial }}
        </UiText>
        <UiText tone="secondary" size="md" class="font-medium">{{ t('common.admin') }}</UiText>
      </UiStack>

      <UiIconButton :label="t('common.signOut')" size="sm" data-testid="sidebar-sign-out" @click="handleLogout">
        <UiIcon :markup="SIGN_OUT_MARKUP" size="sm" />
      </UiIconButton>
    </UiStack>
  </aside>
</template>
