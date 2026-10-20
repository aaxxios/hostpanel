<template>
  <section class="py-8 md:py-20 bg-ui-surface relative overflow-hidden" data-testid="products-comparison">
    <UiDecor class="inset-0">
      <UiDecor class="inset-0 bg-gradient-to-br from-primary-500/5 via-transparent to-primary-500/5 dark:to-secondary-500/5" />
      <UiDecor class="top-1/2 right-1/4 w-96 h-96 bg-primary-500/10 rounded-full blur-[150px]" />
    </UiDecor>

    <UiContainer class="relative z-10">
      <MarketingSectionHeader class="mb-12" :badge="$t('products.comparison.title')" :subtitle="$t('products.comparison.subtitle')">
        <template #title>
          <i18n-t keypath="products.comparison.headline" tag="span">
            <template #perfectMatch>
              <span class="text-primary-600 dark:text-primary-400">{{ $t('products.comparison.perfectMatchHighlight') }}</span>
            </template>
          </i18n-t>
        </template>
      </MarketingSectionHeader>

      <div
        class="max-w-6xl mx-auto animate-reveal-up motion-reduce:animate-none"
        :style="revealStyle(200)"
      >
        <UiTable scrollable :label="$t('products.comparison.title')">
          <UiTableHead>
            <tr>
              <th class="text-left p-6 text-gray-900 dark:text-white font-bold text-lg w-1/4">{{ $t('products.comparison.headers.product') }}</th>
              <th class="p-6 text-center">
                <div class="text-gray-600 dark:text-gray-400 font-semibold text-sm mb-1">{{ $t('products.comparison.headers.starter') }}</div>
                <div class="text-xs text-gray-600 dark:text-gray-400">{{ $t('products.comparison.headers.starterDesc') }}</div>
              </th>
              <th class="p-6 text-center bg-gradient-to-b from-primary-500/10 to-transparent">
                <div class="inline-flex flex-col">
                  <div class="text-gray-900 dark:text-white font-bold text-sm mb-1">{{ $t('products.comparison.headers.professional') }}</div>
                  <span class="text-xs font-semibold px-2 py-0.5 rounded-full bg-primary-500 text-on-primary inline-block">{{ $t('products.comparison.headers.professionalBadge') }}</span>
                </div>
              </th>
              <th class="p-6 text-center">
                <div class="text-gray-600 dark:text-gray-400 font-semibold text-sm mb-1">{{ $t('products.comparison.headers.enterprise') }}</div>
                <div class="text-xs text-gray-600 dark:text-gray-400">{{ $t('products.comparison.headers.enterpriseDesc') }}</div>
              </th>
            </tr>
          </UiTableHead>

          <UiTableBody class="bg-ui-raised">
            <UiTableRow
              v-for="(product, rowIndex) in pricedProducts"
              :key="product.id"
              group
              :data-testid="`comparison-row-${product.id}`"
              class="animate-reveal-up motion-reduce:animate-none"
              :style="revealStyle(300 + rowIndex * 100)"
            >
              <td class="p-6">
                <UiAccent :color="product.color">
                  <NuxtLink :to="hrefFor(product)" class="flex items-center gap-3 group/link">
                    <UiAccentTile :icon="product.icon" size="sm" class="group-hover/link:scale-110" />
                    <div>
                      <div class="font-bold text-gray-900 dark:text-white group-hover/link:text-primary-700 dark:group-hover/link:text-primary-300 transition-colors">{{ product.name }}</div>
                      <div v-if="product.tagline" class="text-xs text-gray-600 dark:text-gray-400 group-hover/link:text-gray-800 dark:group-hover/link:text-gray-200 transition-colors">{{ product.tagline }}</div>
                    </div>
                  </NuxtLink>
                </UiAccent>
              </td>
              <td class="p-6 text-center">
                <div class="text-2xl font-bold text-gray-900 dark:text-white">{{ product.pricing[0]?.price }}</div>
              </td>
              <td class="p-6 text-center bg-primary-500/5">
                <UiAccent :color="product.color">
                  <div :class="[accent.text, 'text-2xl font-bold']">{{ product.pricing[1]?.price }}</div>
                </UiAccent>
              </td>
              <td class="p-6 text-center">
                <div class="text-2xl font-bold text-gray-900 dark:text-white">{{ product.pricing[2]?.price }}</div>
              </td>
            </UiTableRow>
          </UiTableBody>
        </UiTable>
      </div>

      <UiText size="sm" tone="subtle" class="text-center mt-8">{{ $t('products.comparison.footer') }}</UiText>
    </UiContainer>
  </section>
</template>

<script setup lang="ts">
/**
 * The plan-comparison table of the products page: one row per family that has priced plans, with
 * its first three plans as the Starter, Professional and Enterprise columns.
 */

import { revealStyle } from '~/utils/reveal'
import { ACCENT_CLASSES } from '~/utils/ui/accent'
import type { ProductFamily } from '~/types/productfamily'

/** Props of ProductsComparison. */
interface Props {
  /** Every family; those without priced plans are left out of the table. */
  products: ProductFamily[]
}

/** Props of the component. */
const props = defineProps<Props>()

/** Prefixes a path with the active language. */
const localePath = useLocalePath()

/** Accent classes the price cell reads (the colour itself is set by the surrounding UiAccent). */
const accent = ACCENT_CLASSES

/** The families that have at least one priced plan. */
const pricedProducts = computed(() => props.products.filter((product) => product.pricing.length > 0))

/**
 * The localized address a family links to.
 *
 * @param product The family.
 * @returns Its learn-more page, or its detail page when it has none.
 */
const hrefFor = (product: ProductFamily): string => localePath(product.learnMoreUrl || `/products/${product.id}`)
</script>
