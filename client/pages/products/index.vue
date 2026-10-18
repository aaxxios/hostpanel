<template>
  <UiStack gap="none" data-testid="products-page">
    <CatalogProductsHero />
    <CatalogProductFamilyGrid :products="products" />
    <CatalogProductsComparison :products="products" />
    <CatalogProductsBenefits />
    <CatalogProductsTestimonials />
    <SectionsCTA />
  </UiStack>
</template>

<script setup lang="ts">
/**
 * Products page - the SaaS and hosting families with their plans and prices.
 *
 * The page reads the catalogue through the catalogue store (prices are in the payer's currency, so
 * the read runs again when the payer's currency or the language changes), groups it into
 * families with `buildProductFamilies`, and hands the result to the catalogue components. It owns
 * the head (title, canonical, hreflang, schema.org) and nothing visual.
 */
import { APP_LOCALES } from '~/constants/appLocales'
import { useCartStore } from '~/stores/cart'
import { useCatalogStore } from '~/stores/catalog'
import { useCurrencyStore } from '~/stores/currency'
import { formatMoney } from '~/utils/formatMoney'
import { productGids } from '~/utils/catalogGroups'
import { buildProductFamilies } from '~/utils/productFamilies'
import type { ProductFamily } from '~/types/productfamily'
import { SeoPageType } from '~/types/seopagetype'

/** Translation helpers and the active language. */
const { t, locale } = useI18n()

/** Operator-set site name that the SEO copy interpolates as `{site}`. */
const { name: siteName } = useSiteIdentity()
/** The cart: its lines, totals and persistence. */
const cart = useCartStore()
/** The payer's currency and the currencies prices can be shown in. */
const currencyStore = useCurrencyStore()
/** The catalogue: products, plans, slides and TLD prices. */
const catalogStore = useCatalogStore()

onMounted(() => {
  cart.init()
  currencyStore.init()
  currencyStore.load()
})

/** The payer's currency to format prices with, never chosen by the page's language. */
const payerCurrency = computed(() => currencyStore.moneyCurrencyFor(currencyStore.payerCode))

/** Which products the page reads: hosting plus every SaaS group. */
const catalogFilter = { gids: productGids.join(',') }

await useAsyncData('products:families', () => catalogStore.loadProducts(catalogFilter), {
  watch: [() => currencyStore.payerCode, locale],
})

/** The static domains card, the one family that is not a catalogue group. */
const domainsFamily = computed<ProductFamily>(() => ({
  id: 'domains',
  gid: null,
  icon: 'earth',
  color: '#3b82f6',
  demoUrl: '',
  learnMoreUrl: '/domains',
  name: t('domains.card.name'),
  tagline: t('domains.card.tagline'),
  description: t('domains.card.description'),
  features: [t('domains.card.f1'), t('domains.card.f2'), t('domains.card.f3')],
  pricing: [],
}))

/** The domains card followed by one family per catalogue group. */
const products = computed<ProductFamily[]>(() =>
  buildProductFamilies(
    catalogStore.productsFor(catalogFilter),
    (amount) => formatMoney(amount, payerCurrency.value),
    currencyStore.payerCode,
    [domainsFamily.value],
  ))

/** Public site address, from the SEO helper that also sets the head. */
const { baseUrl: productsBaseUrl } = useSeo({
  title: t('seo.products.title', { site: siteName.value }),
  description: t('seo.products.description', { site: siteName.value }),
  keywords: t('seo.products.keywords', { site: siteName.value }),
  type: SeoPageType.Website,
  path: '/products',
})

/** Schema.org builders and the head injector. */
const { organizationSchema: productsOrgSchema, injectSchema: productsInjectSchema } = useSchemaOrg()
productsInjectSchema([
  productsOrgSchema(),
  {
    '@context': 'https://schema.org',
    '@type': 'CollectionPage',
    '@id': `${productsBaseUrl}/products#productspage`,
    url: `${productsBaseUrl}/products`,
    name: t('seo.products.title'),
    description: t('seo.products.description', { site: siteName.value }),
    inLanguage: [...APP_LOCALES],
    publisher: { '@id': `${productsBaseUrl}/#organization` },
  },
])
</script>
