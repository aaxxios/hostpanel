<template>
  <section class="py-20 bg-ui-page relative overflow-hidden" data-testid="sections-products">
    <!-- Background decorations -->
    <UiDecor class="inset-0">
      <UiDecor class="inset-0 bg-gradient-to-tr from-primary-500/5 dark:from-secondary-500/5 via-transparent to-cyan-500/5" />
      <UiDecor class="top-0 left-0 w-1/2 h-1/2 bg-primary-500/10 dark:bg-secondary-500/10 rounded-full blur-[150px]" />
      <UiDecor class="bottom-0 right-0 w-1/3 h-1/3 bg-cyan-500/10 rounded-full blur-[120px]" />
      <UiDecor class="top-1/4 left-1/3 w-2 h-2 bg-primary-400/50 dark:bg-secondary-400/50 rounded-full animate-float [animation-delay:0.3s]" />
      <UiDecor class="bottom-1/3 right-1/4 w-3 h-3 bg-cyan-400/40 rounded-full animate-float [animation-delay:1.8s]" />
      <UiDecor class="top-1/2 right-1/3 w-2 h-2 bg-primary-400/50 rounded-full animate-float [animation-delay:2.5s]" />
      <UiDecor class="top-1/3 left-0 w-full h-px bg-gradient-to-r from-transparent via-primary-500/10 to-transparent" />
    </UiDecor>

    <UiContainer class="relative z-10">
      <MarketingSectionHeader
        class="mb-16"
        :badge="$t('products.products.badge')"
        :title="$t('products.products.title')"
        :subtitle="$t('products.products.subtitle')"
      />

      <!-- Products grid -->
      <UiGrid cols="2" gap="lg" class="lg:grid-cols-4">
        <UiAccent v-for="(product, index) in products" :key="product.id" :color="product.color">
          <!-- The card sits on a dark photograph, so it always uses the dark palette. -->
          <div
            v-motion
            :initial="{ opacity: 0, y: 50 }"
            :visible-once="{ opacity: 1, y: 0, transition: { delay: index * 100, duration: 500 } }"
            class="dark group relative rounded-2xl overflow-hidden min-h-[450px]"
            :data-testid="`home-product-${product.id}`"
          >
            <UiImage
              v-if="product.image"
              :src="product.image"
              :alt="product.name"
              width="800"
              height="600"
              format="webp"
              quality="80"
              sizes="xs:100vw sm:100vw md:50vw lg:33vw"
              class="absolute inset-0 w-full h-full transition-transform duration-700 group-hover:scale-110 pointer-events-none"
            />

            <!-- Dark gradient overlay -->
            <UiDecor class="inset-0 bg-gradient-to-t from-black via-black/70 to-black/30 group-hover:via-black/80 transition-all duration-500" />
            <UiDecor class="inset-0 opacity-0 group-hover:opacity-30 transition-opacity duration-500 bg-[radial-gradient(circle_at_50%_100%,color-mix(in_srgb,var(--accent)_25%,transparent)_0%,transparent_60%)]" />
            <UiDecor class="inset-0 opacity-0 group-hover:opacity-10 transition-opacity duration-500 pattern-dots-20" />

            <div class="relative h-full p-6 flex flex-col">
              <div v-if="index === 0" class="absolute top-4 left-4">
                <span class="px-3 py-1 bg-gradient-to-r from-cyan-500 to-primary-500 text-white text-xs font-bold rounded-full shadow-lg shadow-cyan-500/30 animate-pulse-glow">
                  {{ $t('products.products.featured') }}
                </span>
              </div>

              <!-- Clickable content -->
              <NuxtLink :to="localePath(product.learnMoreUrl)" class="flex flex-col flex-1 cursor-pointer" :data-testid="`home-product-link-${product.id}`">
                <UiAccentTile :icon="`mdi:${product.icon}`" class="mb-4 mt-8 backdrop-blur-sm group-hover:scale-110 group-hover:rotate-3" />

                <UiHeading :level="3" size="subsection" class="text-xl mb-2 group-hover:text-cyan-300 transition-colors duration-300">{{ product.name }}</UiHeading>

                <!-- Only the static domains card has a tagline; families have no such field. -->
                <span v-if="product.tagline" :class="[accent.tintStrong, accent.text, 'inline-block px-3 py-1 rounded-full text-xs font-medium mb-3 w-fit backdrop-blur-sm']">{{ product.tagline }}</span>

                <UiText tone="muted" size="sm" class="mb-4 line-clamp-3 group-hover:text-gray-300 transition-colors duration-300">{{ product.description }}</UiText>

                <div class="flex-1 space-y-2 mb-4 opacity-0 group-hover:opacity-100 transition-all duration-300 delay-100">
                  <div v-for="(feature, featureIndex) in product.features" :key="featureIndex" class="flex items-center gap-2">
                    <CheckCircle :size="14" :stroke-width="2" :class="[accent.text, 'flex-shrink-0']" />
                    <span class="text-xs text-gray-400">{{ feature }}</span>
                  </div>
                </div>
              </NuxtLink>

              <!-- CTA -->
              <div class="mt-auto">
                <UiAccentButton
                  appearance="soft"
                  full-width
                  :href="product.demoUrl || undefined"
                  :to="product.demoUrl ? undefined : localePath(product.learnMoreUrl)"
                  class="group-hover:shadow-lg"
                  :data-testid="`home-product-cta-${product.id}`"
                >
                  <span>{{ product.demoUrl ? $t('products.products.viewDemo') : $t('products.pricing.cta.learnMore') }}</span>
                  <ArrowRight :size="18" :stroke-width="2" class="group-hover:translate-x-1 transition-transform" />
                </UiAccentButton>
              </div>
            </div>

            <!-- Corner accents on hover -->
            <UiDecor :class="[accent.borderMedium, 'top-0 left-0 w-16 h-16 border-l-2 border-t-2 rounded-tl-2xl scale-0 group-hover:scale-100 transition-transform duration-500 origin-top-left']" />
            <UiDecor :class="[accent.borderMedium, 'bottom-0 right-0 w-16 h-16 border-r-2 border-b-2 rounded-br-2xl scale-0 group-hover:scale-100 transition-transform duration-500 origin-bottom-right']" />
          </div>
        </UiAccent>
      </UiGrid>

      <!-- View all -->
      <div class="text-center mt-12 sm:mt-14 md:mt-16">
        <UiButton
          variant="outline"
          :to="localePath('/products')"
          class="group hover:shadow-lg hover:shadow-primary-500/20 dark:hover:shadow-secondary-500/20 transition-all duration-300 md:px-6 md:py-3 md:text-lg"
          data-testid="home-products-all"
        >
          <span>{{ $t('products.products.viewAll') }}</span>
          <ArrowRight :size="20" :stroke-width="2" class="ml-2 group-hover:translate-x-1 transition-transform" />
        </UiButton>
      </div>
    </UiContainer>

    <!-- Section corner decorations -->
    <UiDecor class="top-0 right-0 w-40 h-40 border-r-2 border-t-2 border-cyan-500/20" />
    <UiDecor class="bottom-0 left-0 w-40 h-40 border-l-2 border-b-2 border-primary-500/20 dark:border-secondary-500/20" />
  </section>
</template>

<script setup lang="ts">
/**
 * Products section of the home page: one image card per product family.
 *
 * Data: the catalogue API (name, plus description and features parsed out of one `description`
 * field), `lib/data.ts` (image) and `productConfig` (icon, accent colour, demo link). There is no
 * tagline: the catalogue sends none, so only the static domains card shows one. Each card is
 * painted in its family's accent colour and always uses the dark palette, because it sits on a
 * photograph under a dark overlay.
 */
import { ArrowRight, CheckCircle } from 'lucide-vue-next'
import { products as visualData } from '~/lib/data'
import { useCatalogStore } from '~/stores/catalog'
import { useCurrencyStore } from '~/stores/currency'
import type { ProductCard } from '~/types/productcard'
import { nameToKey, parseDescription, productConfig, productGidToKey, productGids } from '~/utils/catalogGroups'
import { ACCENT_CLASSES } from '~/utils/ui/accent'

/** Catalogue group of the hosting plans, which has its own visual config. */
const HOSTING_GID = 1

/** Accent classes the cards read (the colour itself is set by each card's UiAccent). */
const accent = ACCENT_CLASSES

/** Translation helpers and the active language. */
const { t, locale } = useI18n()
/** Prefixes a path with the active language. */
const localePath = useLocalePath()
/** The payer's currency and the currencies prices can be shown in. */
const currencyStore = useCurrencyStore()
/** The catalogue: products, plans, slides and TLD prices. */
const catalogStore = useCatalogStore()

onMounted(() => {
  currencyStore.init()
  currencyStore.load()
})

/** Image of each family by id, from the static visual data. */
const imageMap = Object.fromEntries(visualData.map((product) => [product.id, product.image]))

/** Operator-configured hosting demo address; empty hides the demo link (the card links to the learn-more page). */
const hostingDemoUrl = (useRuntimeConfig().public.portalHostingDemoUrl as string) || ''

/** Hosting's visual config; it is not in `productConfig`. */
const hostingVisual = {
  icon: 'cloud-check',
  color: '#0ea5e9',
  image: '/images/products/hosting.jpg',
  demoUrl: hostingDemoUrl,
  learnMoreUrl: '/hosting',
}

/** Which products the section reads: hosting plus every SaaS group, in one request. */
const catalogFilter = { gids: productGids.join(',') }

await useAsyncData('products:families', () => catalogStore.loadProducts(catalogFilter), {
  watch: [() => currencyStore.payerCode, locale],
})

/** The cards: the static domains entry, then one per catalogue family that has products. */
const products = computed<ProductCard[]>(() => {
  const raw = catalogStore.productsFor(catalogFilter)
  const items: ProductCard[] = [{
    id: 'domains',
    name: t('domains.card.name'),
    tagline: t('domains.card.tagline'),
    description: t('domains.card.description'),
    features: [t('domains.card.f1'), t('domains.card.f2'), t('domains.card.f3')],
    icon: 'earth',
    color: '#3b82f6',
    image: imageMap.domains || '/images/products/domains.jpg',
    demoUrl: '',
    learnMoreUrl: '/domains',
  }]

  for (const gid of productGids) {
    const key = productGidToKey[gid]
    // `productGidToKey` does not cover every gid in `productGids`.
    if (!key) {
      continue
    }
    const config = productConfig[key]
    if (!config) {
      continue
    }

    const group = raw.filter((product) => Number(product.groupId) === gid)
    const parent = group.find((product) => product.slug?.trim() === key)
      ?? group.find((product) => nameToKey(product.name) === key)
      ?? group[0]
    if (!parent) {
      continue
    }

    const { summary, features } = parseDescription(parent.description || '')
    const visual = gid === HOSTING_GID
      ? hostingVisual
      : { icon: config.icon, color: config.color, image: imageMap[key] || '', demoUrl: config.demoUrl ?? '', learnMoreUrl: config.learnMoreUrl }

    items.push({
      id: key,
      name: parent.name || config.name,
      // Empty on purpose: the catalogue endpoint sends no tagline, and the card guards on it.
      tagline: '',
      description: summary,
      features: features.slice(0, 3),
      ...visual,
    })
  }

  return items
})
</script>
