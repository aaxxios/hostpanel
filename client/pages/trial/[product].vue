<template>
  <UiStack gap="none" class="min-h-dvh bg-ui-page" data-testid="trial-page">
    <CatalogTrialHero
      :name="config.name"
      :icon="config.icon"
      :color="config.color"
      :trial-url="trialUrl"
      :learn-more-url="config.learnMoreUrl"
    />
    <CatalogTrialDetails
      :name="config.name"
      :color="config.color"
      :trial-url="trialUrl"
      :features="features"
    />
  </UiStack>
</template>

<script setup lang="ts">
/**
 * Free-trial page of one product.
 * Route: /trial/[product]
 *
 * The page reads the product's catalogue group through the catalogue store (in the payer's
 * currency, like every other caller, so the cache key is shared), takes the product's visual
 * config from `productConfig` and hands both to the trial components. It owns the head only.
 */
import { useCatalogStore } from '~/stores/catalog'
import { useCurrencyStore } from '~/stores/currency'
import { parseDescription, productConfig, productGidToKey } from '~/utils/catalogGroups'

/** Accent colour of a product the config table does not know. */
const UNKNOWN_PRODUCT_COLOR = '#6366f1'

/** The current route. */
const route = useRoute()
/** Translation helpers and the active language. */
const { t, locale } = useI18n()
/** The payer's currency and the currencies prices can be shown in. */
const currencyStore = useCurrencyStore()
/** The catalogue: products, plans, slides and TLD prices. */
const catalogStore = useCatalogStore()

onMounted(() => {
  currencyStore.init()
  currencyStore.load()
})

/** Route param, e.g. `smartlearn-system`. */
const productSlug = route.params.product as string

/** Display name, icon, accent colour and catalogue page of the product. */
const config = computed(() => productConfig[productSlug] ?? {
  name: productSlug,
  icon: 'cube',
  color: UNKNOWN_PRODUCT_COLOR,
  learnMoreUrl: '/products',
  demoUrl: undefined,
})

/** The catalogue group the product belongs to, when it is a known family. */
const trialGid = Object.entries(productGidToKey).find(([, key]) => key === productSlug)?.[0]

/** Which products the page reads: the product's own group. */
const trialFilter = { gid: trialGid ? Number(trialGid) : undefined }

await useAsyncData(`trial:${productSlug}`, () => catalogStore.loadProducts(trialFilter), {
  watch: [() => currencyStore.payerCode, locale],
})

/** `ProductDto` has no pay type or order URL, so the first product stands in for the trial plan. */
const trialPlan = computed(() => catalogStore.productsFor(trialFilter)[0])

/** Where the trial call to action leads: always the family's own page. */
const trialUrl = computed(() => config.value.learnMoreUrl)

/** Feature lines parsed out of the trial plan's description. */
const features = computed(() => (trialPlan.value ? parseDescription(trialPlan.value.description || '').features : []))

useSeo({
  title: t('trial.seo.title', { product: config.value.name }),
  description: t('trial.seo.description', { product: config.value.name }),
  path: `/trial/${productSlug}`,
})
</script>
