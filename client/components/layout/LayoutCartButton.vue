<template>
  <UiIconButton
    :icon="ShoppingCart"
    :label="$t('cart.title')"
    class="relative"
    testid="layout-cart-button"
    @click="cart.open()"
  >
    <ClientOnly>
      <UiStack
        v-if="cart.count > 0"
        as="span"
        direction="row"
        align="center"
        justify="center"
        gap="none"
        class="absolute -top-1.5 -right-1.5 min-w-[18px] h-[18px] px-1 rounded-full bg-primary-700 text-on-primary text-[10px] font-bold"
        data-testid="layout-cart-count"
      >
        {{ cart.count }}
      </UiStack>
    </ClientOnly>
  </UiIconButton>
</template>

<script setup lang="ts">
/**
 * LayoutCartButton - the header's cart icon with the item-count badge; a click opens the cart
 * drawer. The badge is client-only because the cart lives in localStorage and the server render
 * cannot know it.
 */
import { ShoppingCart } from 'lucide-vue-next'
import { useCartStore } from '~/stores/cart'

/** The cart store: the count shown in the badge and the action that opens the drawer. */
const cart = useCartStore()
</script>
