<template>
  <section class="relative py-14 md:py-20 bg-ui-page overflow-hidden [.tpl-classic_&]:border-b [.tpl-classic_&]:border-classic-border [.tpl-classic_&]:bg-classic-surface-2 [.tpl-classic_&]:py-10 md:[.tpl-classic_&]:py-14" data-testid="cart-hero">
    <UiDecor class="inset-0 [.tpl-classic_&]:hidden">
      <UiDecor class="top-0 left-1/4 w-[500px] h-[500px] bg-primary-500/20 rounded-full blur-[140px] animate-blob" />
      <UiDecor class="bottom-0 right-1/3 w-[400px] h-[400px] bg-cyan-500/15 rounded-full blur-[130px] animate-blob animation-delay-2000" />
    </UiDecor>

    <UiContainer class="relative z-10 text-center [.tpl-classic_&]:text-left">
      <div>
        <span class="inline-flex items-center gap-2 px-4 py-2 mb-6 rounded-full glass border border-primary-500/20 backdrop-blur-sm [.tpl-classic_&]:rounded-md [.tpl-classic_&]:border-0 [.tpl-classic_&]:bg-classic-brand-soft [.tpl-classic_&]:backdrop-blur-none">
          <ShoppingCart :size="14" :stroke-width="2" class="text-primary-600 dark:text-primary-400 [.tpl-classic_&]:text-classic-brand" />
          <UiText as="span" size="sm" weight="medium" tone="muted">{{ $t('cart.title') }}</UiText>
        </span>
        <UiHeading :level="1" class="text-4xl md:text-5xl mb-4 leading-tight [.tpl-classic_&]:text-3xl [.tpl-classic_&]:font-semibold md:[.tpl-classic_&]:text-4xl">{{ $t('cart.title') }}</UiHeading>
        <UiText tone="muted" size="lg">{{ $t('cart.subtitle') }}</UiText>
      </div>
    </UiContainer>
  </section>
</template>

<script setup lang="ts">
/**
 * Hero of the cart page: a badge, the page title and its lead line, over soft animated glows.
 * Under the classic template (the `tpl-classic` root class) it is a flat, left-aligned band with a
 * hairline and no glows.
 */
import { ShoppingCart } from 'lucide-vue-next'
</script>
