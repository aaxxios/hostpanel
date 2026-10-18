<template>
  <section id="products-section" class="py-24 bg-ui-page relative" data-testid="products-catalog">
    <UiContainer>
      <!-- Product navigation -->
      <div class="flex flex-wrap justify-center gap-2 sm:gap-3 mb-12 sm:mb-14 md:mb-16">
        <UiButton
          v-for="(product, index) in products"
          :key="product.id"
          v-motion
          :initial="{ opacity: 0, y: 20 }"
          :visible-once="{ opacity: 1, y: 0, transition: { delay: index * 100, duration: 400 } }"
          variant="subtle"
          size="sm"
          :icon="`mdi:${product.icon}`"
          :to="hrefFor(product)"
          class="font-semibold whitespace-nowrap"
          :data-testid="`product-nav-${product.id}`"
        >
          {{ product.name }}
        </UiButton>
      </div>

      <!-- Catalogue grid -->
      <UiGrid cols="3" gap="lg" class="md:gap-8">
        <UiAccent v-for="(product, index) in products" :key="product.id" :color="product.color">
          <UiCard
            v-motion
            :initial="{ opacity: 0, y: 30 }"
            :visible-once="{ opacity: 1, y: 0, transition: { delay: index * 100, duration: 500 } }"
            hover
            class="group relative h-full sm:p-8 hover:-translate-y-2 transition-all duration-300"
            :data-testid="`product-card-${product.id}`"
          >
            <NuxtLink :to="hrefFor(product)" class="flex flex-col h-full" :data-testid="`product-link-${product.id}`">
              <!-- Radial glow on hover -->
              <UiDecor class="inset-0 rounded-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-500 bg-[radial-gradient(circle_at_top_right,color-mix(in_srgb,var(--accent)_8%,transparent),transparent_70%)]" />

              <!-- Header -->
              <div class="relative flex items-start gap-4 mb-6">
                <UiAccentTile :icon="`mdi:${product.icon}`" class="group-hover:scale-110" />
                <div class="flex-1">
                  <UiHeading :level="3" class="text-xl font-bold group-hover:text-primary-700 dark:group-hover:text-primary-300 transition-colors">
                    {{ product.name }}
                  </UiHeading>
                  <!-- Only the static domains card carries a tagline; families have no such field. -->
                  <span v-if="product.tagline" :class="[accent.text, 'block text-xs uppercase tracking-wider font-bold mb-2']">{{ product.tagline }}</span>
                </div>
              </div>

              <!-- Summary -->
              <UiText tone="muted" size="sm" class="relative leading-relaxed mb-6 flex-1">{{ product.description }}</UiText>

              <!-- Feature highlights -->
              <div v-if="product.features.length" class="relative space-y-2 mb-8">
                <div v-for="(feature, featureIndex) in product.features.slice(0, 3)" :key="featureIndex" class="flex items-center gap-2">
                  <CheckCircle :size="16" :stroke-width="2" :class="accent.text" />
                  <span class="text-xs text-gray-700 dark:text-gray-300 line-clamp-1">{{ feature }}</span>
                </div>
              </div>
              <div v-else class="relative space-y-2 mb-8">
                <div class="flex items-center gap-2">
                  <CheckCircle :size="16" :stroke-width="2" :class="accent.text" />
                  <span class="text-xs text-gray-700 dark:text-gray-300">{{ $t('products.products.badge') }}</span>
                </div>
              </div>

              <!-- Action (the whole card is the link) -->
              <span :class="[accent.tintStrong, accent.borderMedium, 'relative inline-flex items-center justify-center gap-2 px-6 py-3 rounded-xl font-bold text-gray-900 dark:text-white border transition-all duration-300 overflow-hidden group/btn']">
                <span class="relative z-10 flex items-center gap-2 group-hover/btn:translate-x-1 transition-transform">
                  {{ $t('products.pricing.cta.learnMore') }}
                  <Icon name="mdi:arrow-right" :size="18" />
                </span>
                <UiDecor as="span" class="inset-0 opacity-0 group-hover/btn:opacity-100 transition-opacity duration-300 bg-[linear-gradient(to_right,color-mix(in_srgb,var(--accent)_25%,transparent),color-mix(in_srgb,var(--accent)_12%,transparent))]" />
              </span>
            </NuxtLink>
          </UiCard>
        </UiAccent>
      </UiGrid>
    </UiContainer>

    <!-- Decorative rule -->
    <UiDecor class="bottom-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-primary-500/30 dark:via-secondary-500/30 to-transparent" />
  </section>
</template>

<script setup lang="ts">
/**
 * The catalogue of the products page: a row of family shortcuts and one card per family.
 *
 * Every family is coloured by its own accent (`UiAccent`), so a card, its icon tile and its
 * check marks share one colour without the page knowing it. A family that has a `learnMoreUrl`
 * links there; one without links to its detail page.
 */
import { CheckCircle } from 'lucide-vue-next'
import { ACCENT_CLASSES } from '~/utils/ui/accent'
import type { ProductFamily } from '~/types/productfamily'

/** Props of ProductFamilyGrid. */
interface Props {
  /** The families to show, in order. */
  products: ProductFamily[]
}

defineProps<Props>()

/** Prefixes a path with the active language. */
const localePath = useLocalePath()

/** Accent classes the card reads (the colour itself is set by the surrounding UiAccent). */
const accent = ACCENT_CLASSES

/**
 * The localized address a family links to.
 *
 * @param product The family.
 * @returns Its learn-more page, or its detail page when it has none.
 */
const hrefFor = (product: ProductFamily): string => localePath(product.learnMoreUrl || `/products/${product.id}`)
</script>
