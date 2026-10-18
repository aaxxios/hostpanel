<template>
  <component
    :is="domains"
    :price-rows="priceRows"
    :results="results"
    :pending="pending"
    :categories="categories"
    :active-category="activeCategory"
    :in-cart="cartDomains"
    @search="search"
    @update:category="(value: string) => (activeCategory = value)"
    @add="addDomain"
  />
</template>

<script setup lang="ts">
/**
 * Domains page.
 *
 * SEO and structured data belong here so both templates emit the same head, and
 * so does everything that touches the network or the cart — the template only
 * renders and reports what the visitor clicked.
 */
import { APP_LOCALES } from '~/constants/appLocales'
import { useCartStore } from '~/stores/cart'
import { useCatalogStore } from '~/stores/catalog'
import { useCurrencyStore } from '~/stores/currency'
import { useDomainsStore } from '~/stores/domains'
import { ALL_CATEGORY } from '~/templates/aurora/types/allcategory'
import { SeoPageType } from '~/types/seopagetype'
import { TemplateSlot } from '~/types/templateslot'
import { DOMAIN_BILLING_CYCLE } from '~/constants/pricingCycle'
import { CartItemType } from '~/types/cartitemtype'
import { DomainOrderAction } from '~/types/domainorderaction'

/** Translation helpers and the active language. */
const { t, locale } = useI18n()

/** Operator-set site name that the SEO copy interpolates as `{site}`. */
const { name: siteName } = useSiteIdentity()
/** The template registry: resolves the component the active template provides for a slot. */
const { slot } = useTemplate()
/** The cart: its lines, totals and persistence. */
const cart = useCartStore()
/** The payer's currency and the currencies prices can be shown in. */
const currencyStore = useCurrencyStore()
/** The catalogue: products, plans, slides and TLD prices. */
const catalogStore = useCatalogStore()
/** The domains store: price table, categories and name lookup. */
const domainsStore = useDomainsStore()

/** The template's domains component (classic or aurora), resolved by the registry. */
const domains = slot(TemplateSlot.Domains)

/** Public site address, from the SEO helper that also sets the head. */
const { baseUrl } = useSeo({
  title: t('seo.domains.title', { site: siteName.value }),
  description: t('seo.domains.description', { site: siteName.value }),
  keywords: t('seo.domains.keywords', { site: siteName.value }),
  type: SeoPageType.Website,
  path: '/domains',
})

/** Schema.org builders and the head injector. */
const { organizationSchema, injectSchema } = useSchemaOrg()
injectSchema([
  organizationSchema(),
  {
    '@context': 'https://schema.org',
    '@type': 'CollectionPage',
    '@id': `${baseUrl}/domains#domainspage`,
    url: `${baseUrl}/domains`,
    name: t('seo.domains.title'),
    description: t('seo.domains.description', { site: siteName.value }),
    inLanguage: [...APP_LOCALES],
    publisher: { '@id': `${baseUrl}/#organization` },
  },
])

// Prices follow the payer's currency, so the table is read again when it resolves or changes.
await useAsyncData('domains:tld-pricing', () => catalogStore.loadTldPricing(), {
  watch: [() => currencyStore.payerCode, locale],
})

/** Price table, categories and name lookup state of the domains store. */
const { priceRows, categories, lookupResults: results, lookupPending: pending } = storeToRefs(domainsStore)

/** Runs a lookup for the typed name across the offered extensions. */
const search = domainsStore.searchOfferedTlds

/** The extension category the price table is filtered to. */
const activeCategory = ref(ALL_CATEGORY)

onMounted(() => {
  cart.init()
  currencyStore.init()
  currencyStore.load()
})

// The cart needs a product to hang a domain order on; the catalogue store finds it.
await useAsyncData('domains:catalogue', () => catalogStore.loadProducts(), {
  watch: [() => currencyStore.payerCode],
})

/** The id of the product a domain cart line hangs on; null when the operator has none. */
const domainProductId = computed(() => catalogStore.domainProductId)

/** Domains already in the cart, so their row shows as added rather than repeating. */
const cartDomains = computed(() =>
  cart.items.filter((item) => item.itemType === CartItemType.Domain).map((item) => item.domain ?? ''))

/**
 * Adds a domain registration to the cart.
 *
 * Silently does nothing when no domain product exists — the button is only shown
 * for available names, and an operator with no domain product configured has
 * nothing to sell here. The price row is already converted to the payer's currency by the
 * API (the TLD's own sell currency does not matter), so the cart line carries that amount.
 *
 * @param domain Fully qualified domain the visitor chose.
 */
const addDomain = (domain: string): void => {
  if (!domainProductId.value || cartDomains.value.includes(domain) || !currencyStore.payerCode) {
    return
  }

  const tld = domain.slice(domain.indexOf('.') + 1)
  const row = priceRows.value.find((priceRow) => priceRow.tld === `.${tld}`)
  if (!row) {
    return
  }

  cart.addItem({
    pid: domainProductId.value,
    name: domain,
    billingcycle: DOMAIN_BILLING_CYCLE,
    cycleLabel: '1 Year',
    amount: row.registerAmount,
    currency: currencyStore.payerCode,
    domain,
    itemType: CartItemType.Domain,
    domainAction: DomainOrderAction.Register,
    tld,
    years: 1,
  })
}
</script>
