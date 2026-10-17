<template>
  <UiAccent :color="product.color">
    <section class="py-10 md:py-24 bg-ui-page relative overflow-hidden min-h-dvh" data-testid="product-detail">
      <!-- Animated background -->
      <UiDecor class="inset-0">
        <UiDecor class="inset-0 bg-gradient-to-br from-primary-500/10 via-transparent to-primary-500/10 dark:to-secondary-500/10" />
        <UiDecor class="top-0 right-1/4 w-[600px] h-[600px] bg-primary-500/25 dark:bg-secondary-500/25 rounded-full blur-[150px] animate-blob" />
        <UiDecor class="bottom-0 left-1/3 w-[500px] h-[500px] bg-cyan-500/20 rounded-full blur-[140px] animate-blob animation-delay-2000" />
        <UiDecor class="top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[400px] h-[400px] bg-primary-500/15 rounded-full blur-[130px] animate-blob animation-delay-4000" />
        <UiDecor class="inset-0 opacity-[0.025] pattern-grid-80" />
      </UiDecor>

      <UiContainer class="relative z-10">
        <div class="max-w-6xl mx-auto">
          <slot name="breadcrumbs" />

          <div class="space-y-8">
            <div class="space-y-6">
              <!-- Header -->
              <div>
                <div class="flex items-start gap-4 mb-4">
                  <UiAccentTile :icon="`mdi:${product.icon}`" size="lg" />
                  <div class="flex-1">
                    <UiHeading :level="1" size="subsection" class="text-2xl" data-testid="product-name">{{ product.name }}</UiHeading>
                  </div>
                </div>
                <!-- Interpolated, never HTML: the description is operator-authored and `parseDescription` has already stripped its tags. -->
                <UiText v-if="product.description" tone="muted" class="leading-relaxed" data-testid="product-description">{{ product.description }}</UiText>
              </div>

              <!-- Key features -->
              <div v-if="product.features.length">
                <UiHeading :level="2" size="subsection" class="mb-3 flex items-center gap-2">
                  <span :class="[accent.tintStrong, 'w-6 h-6 rounded-lg flex items-center justify-center']">
                    <Sparkle :size="14" :stroke-width="2" :class="accent.text" />
                  </span>
                  {{ $t('products.products.keyFeatures') }}
                </UiHeading>

                <div class="grid md:grid-cols-2 gap-x-6 gap-y-3">
                  <div
                    v-for="(feature, index) in product.features"
                    :key="index"
                    v-motion
                    :initial="{ opacity: 0, x: -20 }"
                    :visible-once="{ opacity: 1, x: 0, transition: { delay: index * 50, duration: 300 } }"
                    class="flex items-center gap-2 group"
                  >
                    <CheckCircle :size="18" :stroke-width="2" :class="[accent.text, 'flex-shrink-0 group-hover:scale-110 transition-transform']" />
                    <span class="text-sm text-gray-700 dark:text-gray-300 group-hover:text-gray-900 dark:group-hover:text-white transition-colors">{{ feature }}</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Pricing plans -->
            <div>
              <UiHeading :level="2" size="subsection" class="text-xl mb-6 flex items-center gap-2">
                <DollarSign :size="24" :stroke-width="2" :class="accent.text" />
                {{ $t('products.pricing.title') }}
              </UiHeading>

              <!-- Billing cycle choice -->
              <div v-if="cycles.length > 1" class="flex flex-wrap gap-2 mb-6">
                <UiAccentButton
                  v-for="cycle in cycles"
                  :key="cycle"
                  size="sm"
                  pill
                  :appearance="selectedCycle === cycle ? 'solid' : 'outline'"
                  :data-testid="`product-cycle-${cycle}`"
                  @click="emit('update:selectedCycle', cycle)"
                >
                  {{ $t(`products.pricing.cycles.${PRICING_CYCLE_I18N_KEY[cycle]}`) }}
                </UiAccentButton>
              </div>

              <div class="grid md:grid-cols-3 gap-6 mb-8">
                <div
                  v-for="(plan, index) in product.plans"
                  :key="plan.tier"
                  v-motion
                  :initial="{ opacity: 0, y: 30 }"
                  :visible-once="{ opacity: 1, y: 0, transition: { delay: index * 100, duration: 500 } }"
                  class="relative group"
                  :data-testid="`product-plan-${plan.pid}`"
                >
                  <UiAccentPanel :highlighted="index === POPULAR_PLAN_INDEX">
                    <span v-if="index === POPULAR_PLAN_INDEX" class="absolute -top-[7px] left-1/2 -translate-x-1/2 z-10">
                      <span :class="[accent.fill, 'px-4 py-1 text-xs font-bold rounded-full text-white shadow-lg whitespace-nowrap']">{{ $t('products.pricing.popular') }}</span>
                    </span>

                    <span :class="[index === POPULAR_PLAN_INDEX ? accent.text : 'text-gray-400', 'block text-xs uppercase tracking-wider mb-2 font-bold']">{{ plan.tier }}</span>

                    <div class="text-4xl font-bold text-gray-900 dark:text-white mb-6" data-testid="product-plan-price">{{ priceFor(plan) || '—' }}</div>

                    <UiList list-style="plain" class="mb-6 flex-1">
                      <UiListItem v-for="feature in plan.features" :key="feature" class="flex items-start gap-2">
                        <CheckCircle :size="16" :stroke-width="2" :class="[accent.text, 'flex-shrink-0 mt-0.5']" />
                        <span class="text-xs leading-relaxed">{{ feature }}</span>
                      </UiListItem>
                    </UiList>

                    <div class="flex flex-col gap-2">
                      <UiButton
                        :variant="isInCart(plan) ? 'subtle' : 'outline'"
                        size="sm"
                        full-width
                        :data-testid="`product-add-${plan.pid}`"
                        @click="emit('addToCart', plan)"
                      >
                        <Check v-if="isInCart(plan)" :size="13" :stroke-width="2.5" class="mr-1.5" />
                        <ShoppingCart v-else :size="13" :stroke-width="2" class="mr-1.5" />
                        {{ isInCart(plan) ? $t('hosting.inCart') : $t('hosting.addToCart') }}
                      </UiButton>

                      <!-- Only when this deployment has an external order page to send the visitor to. -->
                      <UiAccentButton
                        v-if="trialUrlFor(plan)"
                        size="sm"
                        full-width
                        :appearance="index === POPULAR_PLAN_INDEX ? 'solid' : 'outline'"
                        :data-testid="`product-trial-${plan.pid}`"
                        @click="emit('trial', plan)"
                      >
                        {{ $t('products.pricing.trial') }}
                      </UiAccentButton>
                    </div>
                  </UiAccentPanel>
                </div>
              </div>

              <!-- Guarantee and calls to action -->
              <UiCard padding="md" class="flex flex-col sm:flex-row items-center justify-between gap-3 sm:gap-4 w-full">
                <div class="flex items-center gap-2 sm:gap-3 text-center sm:text-left">
                  <ShieldCheck :size="28" :stroke-width="2" class="text-green-600 dark:text-green-400 flex-shrink-0" />
                  <div>
                    <div class="text-xs sm:text-sm font-semibold text-gray-900 dark:text-white break-words">{{ $t('products.pricing.guarantee.title') }}</div>
                    <div class="text-[10px] sm:text-xs text-gray-500 break-words">{{ $t('products.pricing.guarantee.subtitle') }}</div>
                  </div>
                </div>

                <div class="flex flex-col sm:flex-row gap-2 sm:gap-3 w-full sm:w-auto">
                  <UiAccentButton class="w-full sm:w-auto" data-testid="product-demo" @click="emit('demo')">
                    <PlayCircle :size="20" :stroke-width="2" />
                    {{ $t('products.pricing.cta.viewDemo') }}
                  </UiAccentButton>
                  <UiAccentButton appearance="outline" :to="localePath('/contact')" class="w-full sm:w-auto" data-testid="product-contact">
                    {{ $t('products.pricing.contactSales') }}
                  </UiAccentButton>
                </div>
              </UiCard>
            </div>
          </div>
        </div>
      </UiContainer>

      <!-- Corner decorations -->
      <UiDecor class="top-0 right-0 w-48 h-48 border-r-2 border-t-2 border-cyan-500/30" />
      <UiDecor class="bottom-0 left-0 w-48 h-48 border-l-2 border-b-2 border-primary-500/30 dark:border-secondary-500/30" />
    </section>
  </UiAccent>
</template>

<script setup lang="ts">
/**
 * The body of a product's detail page: its header, key features, billing-cycle choice, one panel
 * per plan and the guarantee strip with the demo and contact calls to action.
 *
 * It is presentational. The page owns the data and the cart: it passes the product, the cycles on
 * offer, the chosen cycle and three lookups (`priceFor`, `isInCart`, `trialUrlFor`), and reacts
 * to the events. The whole section is painted in the product's own accent colour.
 */
import { Check, CheckCircle, DollarSign, PlayCircle, ShieldCheck, ShoppingCart, Sparkle } from 'lucide-vue-next'
import { PRICING_CYCLE_I18N_KEY } from '~/constants/pricingCycle'
import { ACCENT_CLASSES } from '~/utils/ui/accent'
import type { PricingCycle } from '~/types/pricingcycle'
import type { ProductDetail } from '~/types/productdetail'
import type { ProductPlan } from '~/types/productplan'

/** Props of ProductDetail. */
interface Props {
  /** The product to show. */
  product: ProductDetail
  /** The billing cycles at least one plan is priced for. */
  cycles: PricingCycle[]
  /** The cycle the prices are shown for. */
  selectedCycle: PricingCycle
  /** The price of a plan for the selected cycle, formatted; empty when it has none. */
  priceFor: (plan: ProductPlan) => string
  /** Whether a plan is already in the cart for the selected cycle. */
  isInCart: (plan: ProductPlan) => boolean
  /** The external order page of a plan, or null when this deployment has none. */
  trialUrlFor: (plan: ProductPlan) => string | null
}

defineProps<Props>()

/** Events of ProductDetail. */
const emit = defineEmits<{
  /** The visitor chose another billing cycle. */
  'update:selectedCycle': [cycle: PricingCycle]
  /** The visitor asked to add a plan to the cart. */
  addToCart: [plan: ProductPlan]
  /** The visitor asked to start a trial of a plan. */
  trial: [plan: ProductPlan]
  /** The visitor asked to see the demo. */
  demo: []
}>()

/** The plan that is drawn as the recommended one (the middle tier). */
const POPULAR_PLAN_INDEX = 1

/** Prefixes a path with the active language. */
const localePath = useLocalePath()

/** Accent classes the section reads (the colour itself is set by the surrounding UiAccent). */
const accent = ACCENT_CLASSES
</script>
