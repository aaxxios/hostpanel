<template>
  <component :is="checkout" />
</template>

<script setup lang="ts">
/**
 * Checkout page.
 *
 * Thin by the same rule as the other public routes: the page owns the route,
 * the template owns the markup. Unlike the others there is no SEO block:
 * `routeRules` marks `/checkout/**` `ssr: false` with an
 * `X-Robots-Tag: noindex, nofollow` header, so there is nothing to describe to
 * a crawler and nothing a meta tag could add.
 *
 * Both templates resolve to the same component: `aurora` has no checkout of its
 * own, and this page carries the ordering flow.
 */
import { TemplateSlot } from '~/types/templateslot'

const { slot } = useTemplate()

/** The active template's checkout component. */
const checkout = slot(TemplateSlot.Checkout)
</script>
