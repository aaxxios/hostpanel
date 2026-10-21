<template>
  <UiStack gap="none" class="overflow-x-hidden" data-testid="classic-domains">
    <!-- Hero / search -->
    <UiSection :contained="false" padding="lg" class="relative lg:py-32 bg-classic-page overflow-hidden">
      <UiStack gap="none" class="absolute inset-0 pointer-events-none">
        <UiStack gap="none" class="absolute top-0 left-1/4 w-[500px] h-[500px] bg-primary-500/20 rounded-full blur-[150px] animate-blob" />
        <UiStack gap="none" class="absolute bottom-0 right-1/3 w-[400px] h-[400px] bg-secondary-500/15 rounded-full blur-[130px] animate-blob animation-delay-2000" />
      </UiStack>

      <UiContainer class="relative z-10 text-center">
        <UiStack gap="none" class="mx-auto max-w-4xl">
          <!-- Breadcrumbs -->
          <UiBreadcrumbs
            :items="[{ label: $t('domains.badge') }]"
            container-class="mb-10 flex justify-center opacity-60 hover:opacity-100 transition-opacity"
          />

          <UiStack direction="row" align="center" gap="xs" class="inline-flex self-center px-4 py-2 mb-8 rounded-full glass border border-primary-500/20 backdrop-blur-sm">
            <Globe :size="14" :stroke-width="2" class="text-primary-600 dark:text-primary-400" />
            <UiText as="span" size="xs" tone="muted" weight="bold" class="md:text-sm font-black uppercase tracking-[0.2em]">{{ $t('domains.badge') }}</UiText>
          </UiStack>

          <UiHeading :level="UiHeadingLevel.H1" size="title" class="md:text-6xl font-black mb-6 leading-tight uppercase tracking-tighter" data-testid="classic-domains-title">
            {{ $t('domains.title') }}
            <UiGradientText class="block pb-2 bg-gradient-to-r from-primary-400 to-cyan-400">{{ $t('domains.titleHighlight') }}</UiGradientText>
          </UiHeading>

          <!-- Register: search bar -->
          <UiStack gap="none" class="mt-12 max-w-2xl mx-auto w-full">
            <!-- Tab switcher -->
            <UiStack direction="row" justify="center" gap="sm" class="mb-8">
              <UiButton variant="primary" size="sm" class="!rounded-xl shadow-lg shadow-primary-500/20" data-testid="classic-domains-tab-register">
                <Globe :size="14" :stroke-width="2" class="mr-2" />
                {{ $t('domains.tabRegister') }}
              </UiButton>
              <UiButton variant="subtle" size="sm" :to="localePath('/domains/transfer')" class="!rounded-xl" data-testid="classic-domains-tab-transfer">
                <ArrowLeftRight :size="14" :stroke-width="2" class="mr-2" />
                {{ $t('domains.tabTransfer') }}
              </UiButton>
            </UiStack>

            <UiSearchBar
              v-model="searchQuery"
              :placeholder="$t('domains.placeholder')"
              :loading="searching"
              :button-label="$t('domains.search')"
              @search="searchDomain"
            />

            <!-- Availability result -->
            <UiStack v-if="result" gap="none" class="mt-8 transition-all duration-300" data-testid="classic-domains-result">
              <!-- TLD not supported -->
              <UiCard
                v-if="!isTldSupported"
                padding="none"
                class="p-6 rounded-[2rem] border-2 border-amber-500/40 bg-amber-500/5 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-6 backdrop-blur-md"
                data-testid="classic-domains-unsupported"
              >
                <UiStack direction="row" align="center" gap="md">
                  <UiStack direction="row" align="center" justify="center" gap="none" class="w-12 h-12 rounded-2xl border bg-amber-500/10 border-amber-500/30">
                    <AlertCircle :size="24" class="text-amber-600 dark:text-amber-400" />
                  </UiStack>
                  <UiStack gap="none" class="text-left">
                    <UiText weight="bold" class="text-xl font-black uppercase tracking-tight">{{ result.domain }}</UiText>
                    <UiText size="xs" tone="inherit" weight="bold" class="uppercase tracking-widest mt-1 text-amber-600 dark:text-amber-400">
                      {{ $t('domains.tldNotSupported') }}
                    </UiText>
                  </UiStack>
                </UiStack>
              </UiCard>

              <!-- TLD supported: normal result -->
              <UiCard
                v-else
                padding="none"
                class="p-6 rounded-[2rem] border-2 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-6 backdrop-blur-md"
                :class="result.available
                  ? 'border-green-500/40 bg-green-500/5'
                  : 'border-red-500/40 bg-red-500/5'"
                data-testid="classic-domains-availability"
              >
                <UiStack direction="row" align="center" gap="md">
                  <UiStack
                    direction="row"
                    align="center"
                    justify="center"
                    gap="none"
                    class="w-12 h-12 rounded-2xl border transition-all"
                    :class="result.available ? 'bg-green-500/10 border-green-500/30' : 'bg-red-500/10 border-red-500/30'"
                  >
                    <CheckCircle v-if="result.available" :size="24" class="text-green-600 dark:text-green-400" />
                    <XCircle v-else :size="24" class="text-red-600 dark:text-red-400" />
                  </UiStack>
                  <UiStack gap="none" class="text-left">
                    <UiText weight="bold" class="text-xl font-black uppercase tracking-tight">{{ result.domain }}</UiText>
                    <UiText
                      size="xs"
                      :tone="result.available ? 'success' : 'danger'"
                      weight="bold"
                      class="uppercase tracking-widest mt-1"
                    >
                      {{ result.available ? $t('domains.available') : $t('domains.taken') }}
                    </UiText>
                  </UiStack>
                </UiStack>
                <UiStack direction="row" align="center" gap="sm" class="w-full sm:w-auto">
                  <UiButton
                    v-if="result.available"
                    variant="primary"
                    size="lg"
                    class="flex-1 sm:flex-none px-8 rounded-2xl bg-gradient-to-r from-green-500 to-emerald-600 text-white text-xs font-black uppercase tracking-widest hover:shadow-xl hover:shadow-green-500/20 active:scale-95"
                    :disabled="cart.hasDomainItem(result.domain, DomainOrderAction.Register) || !canAddDomain(result.domain)"
                    data-testid="classic-domains-register"
                    @click="addDomainToCart(result.domain)"
                  >
                    {{ cart.hasDomainItem(result.domain, DomainOrderAction.Register) ? $t('domains.alreadyInCart') : $t('domains.registerNow') }}
                  </UiButton>
                  <UiButton
                    v-else
                    variant="outline"
                    full-width
                    class="!rounded-2xl !h-12"
                    :to="localePath('/domains/transfer') + (result?.domain ? `?domain=${result.domain}` : '')"
                    data-testid="classic-domains-transfer"
                  >
                    <ArrowLeftRight :size="15" :stroke-width="2" class="mr-2" />
                    {{ $t('domains.takenTransfer') }}
                  </UiButton>
                </UiStack>
              </UiCard>
            </UiStack>

            <!-- Search error -->
            <UiAlert v-if="searchError" variant="error" class="mt-6 text-left" data-testid="classic-domains-error">
              {{ searchError }}
            </UiAlert>
          </UiStack>
        </UiStack>
      </UiContainer>
    </UiSection>

    <!-- TLD pricing table -->
    <UiSection :contained="false" padding="md" class="relative md:py-24 bg-classic-page overflow-hidden">
      <UiContainer class="relative z-10">
        <UiStack gap="none" class="mx-auto max-w-6xl">
          <UiStack align="center" gap="none" class="text-center mb-16">
            <UiText as="span" size="xs" tone="primary" weight="bold" class="font-black text-primary-500 uppercase tracking-[0.3em] block mb-4 italic">{{ $t('domains.pricing.browseByCategory') }}</UiText>
            <UiHeading :level="UiHeadingLevel.H2" size="title" class="md:text-5xl font-black mb-4 uppercase tracking-tighter">{{ $t('domains.pricing.title') }}</UiHeading>
            <UiStack gap="none" class="w-24 h-px bg-gradient-to-r from-transparent via-primary-500 to-transparent mx-auto mt-6" />
          </UiStack>

          <UiGrid v-if="pricingPending" cols="4" gap="lg" class="py-20" data-testid="classic-domains-loading">
            <UiSkeleton v-for="i in 8" :key="i" shape="block" class="h-32 rounded-3xl" />
          </UiGrid>

          <template v-else-if="tldPricing">
            <!-- Category filter -->
            <UiChipTabs
              v-if="allCategories.length"
              class="mb-12"
              :tabs="categoryTabs"
              :model-value="activeCategory ?? ALL_CATEGORY"
              :label="$t('domains.pricing.browseByCategory')"
              testid="classic-domains-categories"
              @update:model-value="chooseCategory"
            />

            <!-- Desktop: table -->
            <UiCard padding="none" class="hidden md:block rounded-[2.5rem] bg-white/[0.01] overflow-hidden backdrop-blur-3xl shadow-2xl">
              <UiTable class="border-0 rounded-none">
                <UiTableHead>
                  <UiTableRow :hoverable="false" class="bg-white dark:bg-white/5">
                    <UiTableTh class="!py-8 !px-10 font-black text-xs uppercase tracking-widest opacity-50">{{ $t('domains.pricing.extension') }}</UiTableTh>
                    <UiTableTh align="center" class="font-black text-xs uppercase tracking-widest opacity-50">{{ $t('domains.pricing.register') }}</UiTableTh>
                    <UiTableTh align="center" class="font-black text-xs uppercase tracking-widest opacity-50">{{ $t('domains.pricing.transfer') }}</UiTableTh>
                    <UiTableTh align="center" class="font-black text-xs uppercase tracking-widest opacity-50">{{ $t('domains.pricing.renew') }}</UiTableTh>
                    <UiTableTh class="!pr-10" />
                  </UiTableRow>
                </UiTableHead>
                <UiTableBody>
                  <UiTableRow v-for="[ext, tld] in displayedTlds" :key="ext" group class="hover:bg-gray-900/[0.03] dark:hover:bg-white/[0.03]" data-testid="classic-domains-row">
                    <UiTableTd class="!py-8 !px-10">
                      <UiText as="span" weight="bold" class="text-2xl font-black group-hover:text-primary-600 dark:group-hover:text-primary-400 transition-colors tracking-tighter uppercase">.{{ ext }}</UiText>
                    </UiTableTd>
                    <UiTableTd align="center">
                      <UiText as="span" weight="bold" class="block text-xl font-black tabular-nums tracking-tight">{{ formatPrice(tld.register[ONE_YEAR_KEY]) }}</UiText>
                      <UiText as="span" size="xs" tone="subtle" weight="bold" class="text-[10px] uppercase tracking-widest">{{ $t('domains.pricing.perYear') }}</UiText>
                    </UiTableTd>
                    <UiTableTd align="center">
                      <UiText as="span" weight="bold" class="block text-xl font-black tabular-nums tracking-tight">{{ formatPrice(tld.transfer?.[ONE_YEAR_KEY]) }}</UiText>
                      <UiText as="span" size="xs" tone="subtle" weight="bold" class="text-[10px] uppercase tracking-widest">{{ $t('domains.pricing.perYear') }}</UiText>
                    </UiTableTd>
                    <UiTableTd align="center">
                      <UiText as="span" weight="bold" class="block text-xl font-black tabular-nums tracking-tight">{{ formatPrice(tld.renew?.[ONE_YEAR_KEY]) }}</UiText>
                      <UiText as="span" size="xs" tone="subtle" weight="bold" class="text-[10px] uppercase tracking-widest">{{ $t('domains.pricing.perYear') }}</UiText>
                    </UiTableTd>
                    <UiTableTd align="right" class="!pr-10">
                      <UiButton
                        size="sm"
                        variant="outline"
                        class="!rounded-xl group-hover:!bg-primary-500 group-hover:!text-on-primary transition-all uppercase text-[10px] font-black tracking-widest"
                        data-testid="classic-domains-quick-search"
                        @click="quickSearch(ext)"
                      >
                        {{ $t('domains.search') }}
                      </UiButton>
                    </UiTableTd>
                  </UiTableRow>
                  <UiTableRow v-if="!displayedTlds.length" :hoverable="false">
                    <UiTableTd colspan="5" align="center" class="text-gray-600 dark:text-gray-400 py-20 font-medium italic">
                      {{ $t('domains.pricing.noResults') }}
                    </UiTableTd>
                  </UiTableRow>
                </UiTableBody>
              </UiTable>
            </UiCard>

            <!-- Mobile: card layout -->
            <UiGrid cols="1" gap="lg" class="md:hidden">
              <UiCard
                v-for="[ext, tld] in displayedTlds"
                :key="ext"
                padding="none"
                hover
                class="rounded-3xl bg-white/[0.01] p-6 hover:border-primary-500/30 transition-all group active:scale-[0.98]"
                data-testid="classic-domains-card"
              >
                <UiStack direction="row" align="center" justify="between" gap="none" class="mb-6">
                  <UiText as="span" weight="bold" class="text-2xl font-black group-hover:text-primary-600 dark:group-hover:text-primary-400 transition-colors tracking-tighter uppercase">.{{ ext }}</UiText>
                  <UiStack gap="none" class="p-2 rounded-xl bg-white dark:bg-white/5 border border-gray-200 dark:border-white/5 opacity-40">
                    <Globe :size="16" />
                  </UiStack>
                </UiStack>

                <UiStack direction="row" gap="xs" class="mb-8 p-4 rounded-2xl bg-white/[0.03] border border-gray-200 dark:border-white/5">
                  <UiStack gap="none" class="flex-1 text-center">
                    <UiText as="span" size="xs" tone="subtle" weight="bold" class="block font-black uppercase tracking-widest mb-1 opacity-50">{{ $t('domains.pricing.register') }}</UiText>
                    <UiText as="span" size="sm" weight="bold" class="block font-black tabular-nums">{{ formatPrice(tld.register[ONE_YEAR_KEY]) }}</UiText>
                  </UiStack>
                  <UiStack gap="none" class="flex-1 text-center border-x border-gray-200 dark:border-white/5">
                    <UiText as="span" size="xs" tone="subtle" weight="bold" class="block font-black uppercase tracking-widest mb-1 opacity-50">{{ $t('domains.pricing.transfer') }}</UiText>
                    <UiText as="span" size="sm" weight="bold" class="block font-black tabular-nums">{{ formatPrice(tld.transfer?.[ONE_YEAR_KEY]) }}</UiText>
                  </UiStack>
                  <UiStack gap="none" class="flex-1 text-center">
                    <UiText as="span" size="xs" tone="subtle" weight="bold" class="block font-black uppercase tracking-widest mb-1 opacity-50">{{ $t('domains.pricing.renew') }}</UiText>
                    <UiText as="span" size="sm" weight="bold" class="block font-black tabular-nums">{{ formatPrice(tld.renew?.[ONE_YEAR_KEY]) }}</UiText>
                  </UiStack>
                </UiStack>

                <UiButton
                  variant="subtle"
                  full-width
                  class="!h-12 rounded-2xl text-[10px] font-black uppercase tracking-widest hover:bg-primary-500 hover:text-on-primary hover:border-transparent"
                  data-testid="classic-domains-card-search"
                  @click="quickSearch(ext)"
                >
                  {{ $t('domains.pricing.registerNow') }}
                </UiButton>
              </UiCard>

              <UiText v-if="!displayedTlds.length" tone="subtle" class="py-20 text-center italic">
                {{ $t('domains.pricing.noResults') }}
              </UiText>
            </UiGrid>
          </template>

          <!-- Promo boxes -->
          <UiGrid cols="2" gap="xl" class="mt-24">
            <!-- Add web hosting -->
            <UiCard padding="none" class="rounded-[2.5rem] border-gray-200 bg-gradient-to-br from-white/[0.03] to-transparent p-10 hover:border-primary-400/20 transition-all group" data-testid="classic-domains-promo-hosting">
              <UiStack gap="none" class="text-center md:text-left items-center md:items-start">
                <UiStack direction="row" align="center" justify="center" gap="none" class="w-16 h-16 rounded-3xl bg-primary-500/10 border border-primary-500/20 mb-6 group-hover:scale-110 transition-transform">
                  <Server :size="32" class="text-primary-600 dark:text-primary-400" />
                </UiStack>
                <UiHeading :level="UiHeadingLevel.H3" size="section" class="mb-2 font-black uppercase tracking-tighter">{{ $t('domains.promoHostingTitle') }}</UiHeading>
                <UiText size="sm" tone="primary" weight="bold" class="mb-4 uppercase tracking-widest">{{ $t('domains.promoHostingSubtitle') }}</UiText>
                <UiText tone="subtle" class="mb-8 leading-relaxed italic">{{ $t('domains.promoHostingDesc') }}</UiText>
                <UiButton
                  :to="localePath('/hosting')"
                  variant="primary"
                  size="lg"
                  class="gap-3 px-8 !py-4 rounded-2xl bg-primary-500 hover:bg-primary-600 text-xs font-black uppercase tracking-widest shadow-xl shadow-primary-500/20 hover:scale-105 active:scale-95"
                  data-testid="classic-domains-promo-hosting-link"
                >
                  {{ $t('domains.promoHostingBtn') }}
                  <ArrowRight :size="16" />
                </UiButton>
              </UiStack>
            </UiCard>

            <!-- Transfer -->
            <UiCard padding="none" class="rounded-[2.5rem] border-gray-200 bg-gradient-to-bl from-white/[0.03] to-transparent p-10 hover:border-cyan-400/20 transition-all group" data-testid="classic-domains-promo-transfer">
              <UiStack gap="none" class="text-center md:text-left items-center md:items-start h-full">
                <UiStack direction="row" align="center" justify="center" gap="none" class="w-16 h-16 rounded-3xl bg-cyan-500/10 border border-cyan-500/20 mb-6 group-hover:scale-110 transition-transform">
                  <ArrowLeftRight :size="32" class="text-cyan-700 dark:text-cyan-400" />
                </UiStack>
                <UiHeading :level="UiHeadingLevel.H3" size="section" class="mb-2 font-black uppercase tracking-tighter">{{ $t('domains.promoTransferTitle') }}</UiHeading>
                <UiText tone="subtle" class="mb-8 leading-relaxed flex-1 italic">{{ $t('domains.promoTransferDesc') }}</UiText>
                <UiStack gap="none" class="mt-auto items-center md:items-start">
                  <UiButton
                    :to="localePath('/domains/transfer')"
                    variant="outline"
                    size="lg"
                    class="gap-3 px-8 !py-4 rounded-2xl text-xs font-black uppercase tracking-widest shadow-xl hover:border-cyan-500 hover:bg-cyan-500 hover:text-white hover:scale-105 active:scale-95"
                    data-testid="classic-domains-promo-transfer-link"
                  >
                    <ArrowLeftRight :size="16" class="mr-2" />
                    {{ $t('domains.promoTransferBtn') }}
                  </UiButton>
                  <UiText size="xs" tone="subtle" weight="bold" class="text-[10px] mt-4 uppercase tracking-widest">{{ $t('domains.promoTransferNote') }}</UiText>
                </UiStack>
              </UiStack>
            </UiCard>
          </UiGrid>
        </UiStack>
      </UiContainer>
    </UiSection>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * ClassicDomains - the classic template's domains page: the availability search with its result,
 * the TLD price table (desktop table, mobile cards) with a category filter, and the two promo
 * boxes.
 *
 * The availability search and the cart actions go through the stores; the price table is read
 * through the catalogue store in the payer's own currency and never converted.
 */
import { Globe, CheckCircle, XCircle, ArrowLeftRight, Server, ArrowRight, AlertCircle } from 'lucide-vue-next'
import { useCartStore } from '~/stores/cart'
import { useCurrencyStore } from '~/stores/currency'
import { useCatalogStore } from '~/stores/catalog'
import { useDomainsStore } from '~/stores/domains'
import { DomainActionOutcome } from '~/types/domainactionoutcome'
import type { TldPricing } from '~/types/tldpricing'
import { formatMoney } from '~/utils/formatMoney'
import { UiHeadingLevel } from '~/utils/ui/enums'
import { DOMAIN_BILLING_CYCLE } from '~/constants/pricingCycle'
import { CartItemType } from '~/types/cartitemtype'
import { DomainOrderAction } from '~/types/domainorderaction'
import type { ChipTab } from '~/types/chiptab'
import { ALL_CATEGORY } from '~/templates/aurora/types/allcategory'
import { tldCategoryLabel, tldCategorySlug } from '~/utils/tldCategory'

/** One extension's entry of the price table. */
type TldEntry = TldPricing['pricing'][string]

/** Registration period, in years, of a quick add and of the one-year price columns. */
const DEFAULT_YEARS = 1

/** Key of the one-year price in a period-keyed price record. */
const ONE_YEAR_KEY = '1'

/** Base name used when a quick search starts from an empty search box. */
const FALLBACK_BASE_NAME = 'example'

/** Translate function and the active locale. */
const { t, locale } = useI18n()

/** Prefixes a path with the active locale. */
const localePath = useLocalePath()

/** Current route; `?domain=` pre-fills the search. */
const route = useRoute()

/** Cart store. */
const cart = useCartStore()

/** Domains store: the availability search and the domain product id. */
const domainsStore = useDomainsStore()
domainsStore.reset()

/** Domain product ID fetched from the backend products endpoint. */
const domainProductId = ref<number | null>(null)

// SEO and structured data live in pages/domains/index.vue so both templates emit
// the same head. Nothing template-specific was removed here.

/** The text in the search box. */
const searchQuery = ref(typeof route.query.domain === 'string' ? route.query.domain : '')

/** Whether an availability search is in flight (owned by the store). */
const searching = computed(() => domainsStore.searching)

/** The availability answer (owned by the store). */
const result = computed(() => domainsStore.searchResult)

/** The page's own validation sentence, shown before any request is made. */
const validationError = ref('')

/** The sentence to show: local validation first, else the store's API refusal. */
const searchError = computed(() => validationError.value || domainsStore.searchError)

/** Checks domain availability through the store. */
const searchDomain = async (): Promise<void> => {
  const domain = searchQuery.value.trim()
  if (!domain) {
    return
  }
  validationError.value = ''
  if (!domain.includes('.')) {
    validationError.value = t('domains.invalidDomain')
    return
  }
  const outcome = await domainsStore.checkAvailability(domain)
  if (outcome === DomainActionOutcome.Failed && !domainsStore.searchError) {
    validationError.value = t('domains.checkFailed')
  }
}

/**
 * Currency store. TLD pricing is priced in the payer's own currency, never the page's language;
 * see `docs/superpowers/specs/2026-09-13-multi-currency-pricing-design.md` section 5.
 */
const currencyStore = useCurrencyStore()

// Auto-search if a domain query param is present (coming from /products)
onMounted(async () => {
  currencyStore.init()
  currencyStore.load()

  if (searchQuery.value) {
    searchDomain()
  }

  // Fetch the domain product PID for the cart; null disables cart add
  domainProductId.value = await domainsStore.fetchDomainProductId()
})

/** Catalogue store: the TLD price table. */
const catalogStore = useCatalogStore()

// The price table is read in the payer's currency; the read runs again once `payerCode`
// resolves or changes.
/** Whether the price table is still loading. */
const { pending: pricingPending } = await useAsyncData('domains:classic-tld-pricing', () => catalogStore.loadTldPricing(), {
  watch: [() => currencyStore.payerCode, locale]
})

/** The price table (owned by the catalogue store). */
const tldPricing = computed(() => catalogStore.tldPricing)

/** The category the table is filtered to; null shows every extension. */
const activeCategory = ref<string | null>(null)

/**
 * The extension part of a domain name.
 *
 * @param domain - Full domain name (e.g. "example.com").
 * @returns Everything after the first dot (e.g. "com").
 */
const tldOf = (domain: string): string => domain.substring(domain.indexOf('.') + 1)

/**
 * Performs a quick domain search using the current base name with the given TLD extension.
 *
 * @param ext - TLD extension (e.g. "com", "net").
 */
const quickSearch = (ext: string): void => {
  const base = searchQuery.value.trim().split('.')[0] || FALLBACK_BASE_NAME
  searchQuery.value = `${base}.${ext}`
  nextTick(() => searchDomain())
}

/**
 * Whether a domain can be added to the cart: its TLD has a price table entry. The entry is
 * already converted to the payer's currency by the API, whatever currency the TLD sells in.
 *
 * @param domain - Full domain name.
 * @returns True when the TLD is priced.
 */
const canAddDomain = (domain: string): boolean => {
  const entry = tldPricing.value?.pricing?.[tldOf(domain)]
  return !!entry
}

/**
 * Adds a domain registration to the cart.
 *
 * Refuses when the TLD has no one-year registration price. The price is the API's, already
 * converted to the payer's currency. Callers gate the button on {@link canAddDomain} first;
 * this is the second line of defence.
 *
 * @param domain - Full domain name (e.g. "example.com").
 * @param years - Registration period in years.
 */
const addDomainToCart = (domain: string, years: number = DEFAULT_YEARS): void => {
  if (!domainProductId.value || !currencyStore.payerCode) {
    return
  }

  const tld = tldOf(domain)
  const entry = tldPricing.value?.pricing?.[tld]
  if (!entry?.register?.[ONE_YEAR_KEY]) {
    return
  }

  cart.addItem({
    pid: domainProductId.value,
    name: domain,
    billingcycle: DOMAIN_BILLING_CYCLE,
    cycleLabel: t('domains.years', years),
    amount: parseFloat(entry.register[ONE_YEAR_KEY]),
    currency: currencyStore.payerCode,
    domain,
    itemType: CartItemType.Domain,
    domainAction: DomainOrderAction.Register,
    tld,
    years,
  })
}

/**
 * Whether the searched domain's TLD is present and enabled in the pricing data.
 * False when the TLD is not offered by the platform.
 */
const isTldSupported = computed<boolean>(() => {
  if (!result.value) {
    return true
  }
  return !!tldPricing.value?.pricing?.[tldOf(result.value.domain)]
})

/** Unique TLD categories with their counts, from the pricing data. */
const allCategories = computed(() => {
  if (!tldPricing.value?.pricing) {
    return []
  }
  const cats = new Map<string, number>()
  for (const tld of Object.values(tldPricing.value.pricing)) {
    for (const cat of (tld.categories ?? [])) {
      cats.set(cat, (cats.get(cat) ?? 0) + 1)
    }
  }
  return Array.from(cats.entries()).map(([name, count]) => ({ name, count }))
})

/** The category chips: "all" first (with the full count), then one per category the data carries. */
const categoryTabs = computed<ChipTab[]>(() => [
  { value: ALL_CATEGORY, label: t('domains.pricing.catAll'), count: totalTldCount.value, testid: 'classic-domains-category-all' },
  ...allCategories.value.map(cat => ({
    value: cat.name,
    label: tldCategoryLabel(cat.name, t),
    count: cat.count,
    testid: `classic-domains-category-${tldCategorySlug(cat.name)}`,
  })),
])

/**
 * Applies a chip choice to the filter.
 *
 * @param value - The chosen category, or the "all" sentinel to show every extension.
 */
const chooseCategory = (value: string): void => {
  activeCategory.value = value === ALL_CATEGORY ? null : value
}

/** How many extensions the table holds before filtering. */
const totalTldCount = computed(() => Object.keys(tldPricing.value?.pricing ?? {}).length)

/** The extensions that pass the category filter, as `[extension, entry]` pairs. */
const displayedTlds = computed<Array<[string, TldEntry]>>(() => {
  if (!tldPricing.value?.pricing) {
    return []
  }
  const entries = Object.entries(tldPricing.value.pricing)
  const category = activeCategory.value
  if (!category) {
    return entries
  }
  return entries.filter(([, tld]) => (tld.categories ?? []).includes(category))
})

/**
 * Formats a price string with the payer's currency.
 *
 * @param price - Price value as a string, or undefined when the extension has none.
 * @returns Formatted price, or the "not available" label for a missing, free-marker or -1 price.
 */
const formatPrice = (price: string | undefined): string => {
  if (!price || price === '-1' || price === '0.00') {
    return t('domains.pricing.notAvailable')
  }
  const code = tldPricing.value?.currency?.code
  return formatMoney(parseFloat(price), currencyStore.moneyCurrencyFor(code))
}
</script>
