<template>
  <!-- One header for mobile and desktop, so the widget mount ids exist once -->
  <header
    class="z-30 flex flex-shrink-0 items-center justify-between border-b border-gray-200 bg-ui-surface px-4 py-3 dark:border-white/10 lg:px-8 lg:py-4"
    data-testid="client-topbar"
  >
    <!-- Left: logo on mobile, welcome text on desktop -->
    <div class="mr-3 min-w-0">
      <UiLink to="/" tone="inherit" :underline="false" testid="client-topbar-brand" class="text-lg font-bold lg:hidden">
        {{ $t('client.nav.brand') }}
      </UiLink>
      <UiText as="span" size="sm" tone="muted" truncate class="hidden lg:block">
        {{ $t('client.nav.welcomeBack') }}
        <UiText as="span" size="sm" weight="medium" data-testid="client-topbar-name">{{ fullName }}</UiText>
      </UiText>
    </div>

    <!-- Right: controls -->
    <div class="flex flex-shrink-0 items-center gap-2 lg:gap-3">
      <!-- Desktop only: theme toggle, language switcher, app launcher -->
      <div class="hidden items-center gap-3 lg:flex">
        <UiThemeToggle />
        <UiLanguageSwitcher />
        <!-- data-user hands the signed-in person to the widget so its launcher appends
             ?authuser= to every tile from the first paint, instead of only after its own
             profile round-trip. The widget reads it from either mount; this is the one
             present at every width. -->
        <div id="inno-launcher-mount" :data-user="widgetUser" />
      </div>
      <!-- Account popup: always visible, mobile needs account switching too -->
      <div id="inno-account-mount" :data-user="widgetUser" />
      <!-- Hamburger: mobile only -->
      <UiIconButton
        :icon="sidebarOpen ? X : Menu"
        :label="$t('client.nav.toggleMenu')"
        variant="ghost"
        class="lg:hidden"
        testid="client-menu-toggle"
        @click="$emit('toggleSidebar')"
      />
    </div>
  </header>
</template>

<script setup lang="ts">
/**
 * The client area's top bar: welcome text, theme and language controls, the platform widget
 * mounts and the mobile menu button.
 */
import { Menu, X } from 'lucide-vue-next'

/** Props of ClientLayoutTopBar. */
interface Props {
  /** Full name of the signed-in person. */
  fullName: string
  /** The person in the widget's own JSON shape, or undefined while the profile loads. */
  widgetUser?: string
  /** Whether the mobile sidebar is open, so the button shows a cross. */
  sidebarOpen: boolean
}

withDefaults(defineProps<Props>(), {
  widgetUser: undefined
})

/** Events of ClientLayoutTopBar. */
defineEmits<{
  /** The mobile menu button was pressed. */
  toggleSidebar: []
}>()
</script>
