<template>
  <UiStack gap="none" class="min-h-dvh bg-page" data-testid="default-layout">
    <!-- Demo-mode notice; renders nothing outside a demo -->
    <LayoutDemoBanner />

    <!-- Header supplied by the active template -->
    <component :is="header" />

    <!-- Main content -->
    <UiStack as="main" gap="none" class="flex-1">
      <slot />
    </UiStack>

    <!-- Footer supplied by the active template -->
    <component :is="footer" />

    <!-- Floating Action Button (Telegram / WhatsApp / Live Chat) -->
    <UiFloatingActions />

    <!-- Consent bar: public site only, never on the auth pages (they use no layout) -->
    <UiCookieBanner />

    <!-- Cart Drawer (Side panel) -->
    <LayoutCartDrawer />
  </UiStack>
</template>

<script setup lang="ts">
/**
 * Default layout with the active template's header and footer.
 * Applied to all pages unless a custom layout is specified.
 */
import { TemplateSlot } from '~/types/templateslot'

const { name, slot } = useTemplate()

/** The active template's header component. */
const header = slot(TemplateSlot.Header)

/** The active template's footer component. */
const footer = slot(TemplateSlot.Footer)

// Stamps the active template on <html> so global CSS can scope to it (nova's body
// tokens). Every template's body follows the colour-mode tokens; see
// assets/styles/global.css.
useHead({ htmlAttrs: { 'data-template': name } })

// The colour mode is not restored here any more: plugins/color-mode.ts renders the
// visitor's cookie (or the operator's default) into the <html> class on the server,
// so there is nothing left for the layout to do on mount.
</script>
