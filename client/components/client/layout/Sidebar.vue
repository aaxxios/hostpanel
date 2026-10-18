<template>
  <!--
    Mobile: fixed overlay that slides in and out.
    Desktop: a normal flex child that fills the height and scrolls internally.
  -->
  <aside
    class="fixed inset-y-0 left-0 z-20 flex w-64 flex-col border-r border-gray-200 bg-ui-surface transition-transform duration-300 dark:border-white/10 lg:relative lg:inset-y-auto lg:z-auto lg:flex-shrink-0 lg:translate-x-0"
    :class="open ? 'translate-x-0' : '-translate-x-full'"
    data-testid="client-sidebar"
  >
    <!-- Logo -->
    <div class="flex flex-shrink-0 items-center justify-between border-b border-gray-200 px-6 py-5 dark:border-white/10">
      <UiLink to="/" tone="inherit" :underline="false" testid="client-sidebar-brand" class="text-lg font-bold">
        {{ $t('client.nav.brand') }}
      </UiLink>
      <UiIconButton
        :icon="X"
        :label="$t('ui.close')"
        variant="ghost"
        size="sm"
        class="lg:hidden"
        testid="client-sidebar-close"
        @click="$emit('close')"
      />
    </div>

    <!-- Navigation: scrolls when the items overflow -->
    <nav class="flex-1 space-y-1 overflow-y-auto px-3 py-4">
      <ClientLayoutNavItem
        v-for="item in items"
        :key="item.to"
        :item="item"
        :active="isActive(item.to)"
        @navigate="$emit('close')"
      />
    </nav>

    <!-- Sign out: always at the bottom -->
    <div class="flex-shrink-0 border-t border-gray-100 px-3 py-3 dark:border-white/5">
      <UiButton variant="ghost" full-width class="!justify-start gap-3 hover:!text-red-500" data-testid="client-sign-out" @click="$emit('signOut')">
        <LogOut :size="18" :stroke-width="2" />
        {{ $t('client.nav.signOut') }}
      </UiButton>
    </div>
  </aside>
</template>

<script setup lang="ts">
/**
 * The client area's left navigation: brand, the permitted links and the sign-out button.
 */
import { LogOut, X } from 'lucide-vue-next'
import type { ClientNavItem } from '~/types/clientnavitem'

/** Props of ClientLayoutSidebar. */
interface Props {
  /** The links to show, already filtered by the visitor's permissions. */
  items: ClientNavItem[]
  /** Whether the mobile drawer is slid in; always visible on desktop. */
  open: boolean
}

defineProps<Props>()

/** Events of ClientLayoutSidebar. */
defineEmits<{
  /** The drawer should close (a link was followed or the close button pressed). */
  close: []
  /** The visitor asked to sign out. */
  signOut: []
}>()

/** The current route, read for its query and path. */
const route = useRoute()

/**
 * Whether a link's section contains the current route.
 *
 * @param path - The link target.
 * @returns True for the page itself and everything below it.
 */
const isActive = (path: string): boolean => route.path === path || route.path.startsWith(`${path}/`)
</script>
