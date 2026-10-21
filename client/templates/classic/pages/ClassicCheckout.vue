<template>
  <UiStack gap="none" class="bg-classic-page text-gray-900 dark:text-white selection:bg-cyan-500/30" data-testid="classic-checkout">
    <!-- Hero header -->
    <UiStack gap="none" class="relative py-12 md:py-20 overflow-hidden border-b border-gray-200 dark:border-white/5">
      <UiStack gap="none" class="absolute inset-0 pointer-events-none">
        <UiStack gap="none" class="absolute top-0 left-1/4 w-[500px] h-[500px] bg-primary-500/10 rounded-full blur-[140px] animate-pulse" />
        <UiStack gap="none" class="absolute bottom-0 right-1/4 w-[500px] h-[500px] bg-cyan-500/10 rounded-full blur-[140px] animate-pulse animation-delay-2000" />
      </UiStack>

      <UiContainer class="relative z-10">
        <UiStack
          align="center"
          gap="none"
          class="max-w-4xl mx-auto text-center animate-reveal-up motion-reduce:animate-none"
        >
          <UiStack direction="row" align="center" gap="xs" class="inline-flex px-3 py-1 mb-6 rounded-full glass border border-gray-200 dark:border-white/10">
            <ShieldCheck :size="14" class="text-cyan-700 dark:text-cyan-400" />
            <UiText as="span" size="xs" tone="inherit" weight="semibold" class="tracking-wider uppercase text-cyan-700 dark:text-cyan-400">{{ $t('checkout.secureCheckout') }}</UiText>
          </UiStack>
          <UiHeading :level="UiHeadingLevel.H1" size="display" class="mb-6 font-black tracking-tight">
            <UiGradientText class="bg-gradient-to-r from-gray-900 via-gray-900 to-gray-600 dark:from-white dark:via-white dark:to-white/60">{{ $t('checkout.title') }}</UiGradientText>
          </UiHeading>
          <UiText tone="muted" size="lg" weight="medium" class="max-w-xl mx-auto">
            {{ $t('checkout.subtitle') }}
          </UiText>
        </UiStack>
      </UiContainer>
    </UiStack>

    <!-- Main content -->
    <UiSection :contained="false" padding="md" class="relative md:py-20">
      <UiContainer class="relative z-10">
        <!-- Empty cart state -->
        <UiCard
          v-if="cart.isEmpty"
          padding="none"
          class="max-w-2xl mx-auto py-8 rounded-3xl backdrop-blur-sm animate-reveal-up motion-reduce:animate-none"
          data-testid="checkout-empty"
        >
          <UiEmptyState :icon="ShoppingCart" :title="$t('cart.empty')" :description="$t('cart.emptyDesc')">
            <UiButton :to="localePath('/hosting')" variant="primary" size="lg" data-testid="checkout-browse">
              {{ $t('cart.browseHosting') }}
            </UiButton>
          </UiEmptyState>
        </UiCard>

        <!-- Checkout grid -->
        <UiGrid v-else cols="1" gap="xl" class="lg:grid-cols-12 items-start" data-testid="checkout-grid">
          <!-- Left column: steps -->
          <UiStack gap="xl" class="lg:col-span-7">
            <!-- Account section -->
            <UiCard padding="none" class="p-8 md:p-10 rounded-[32px] backdrop-blur-xl relative group transition-all duration-500 hover:border-gray-300 dark:hover:border-white/20" data-testid="checkout-account">
              <UiStack direction="row" align="center" gap="md" class="mb-8">
                <UiStack direction="row" align="center" justify="center" gap="none" class="w-12 h-12 rounded-2xl bg-primary-500/20 border border-primary-500/30 text-primary-600 dark:text-primary-400 group-hover:scale-110 transition-transform duration-500">
                  <User :size="24" :stroke-width="2" />
                </UiStack>
                <UiStack gap="none">
                  <UiHeading :level="UiHeadingLevel.H3" size="subsection" class="text-xl font-bold">{{ $t('checkout.account') }}</UiHeading>
                  <UiText size="sm" tone="muted">{{ $t('checkout.identifyPrompt') }}</UiText>
                </UiStack>
              </UiStack>

              <!-- Signed-in state -->
              <UiStack
                v-if="isLoggedIn"
                direction="row"
                align="center"
                justify="between"
                gap="md"
                class="p-5 rounded-2xl bg-green-500/5 border border-green-500/20"
                data-testid="checkout-signed-in"
              >
                <UiStack direction="row" align="center" gap="md">
                  <UiStack direction="row" align="center" justify="center" gap="none" class="w-10 h-10 rounded-full bg-green-400/20 text-green-600 dark:text-green-400">
                    <CheckCircle :size="18" />
                  </UiStack>
                  <UiStack gap="none">
                    <UiText size="sm" weight="semibold">{{ $t('checkout.loggedInAs') }}</UiText>
                    <UiText size="sm" tone="success" data-testid="checkout-user-email">{{ userEmail }}</UiText>
                  </UiStack>
                </UiStack>
                <UiButton variant="ghost" size="sm" class="text-xs font-bold text-gray-600 dark:text-gray-400" data-testid="checkout-not-you" @click="handleLogout">
                  {{ $t('checkout.notYou') }}
                </UiButton>
              </UiStack>

              <!-- Guest form -->
              <UiStack v-else gap="lg" data-testid="checkout-guest-form">
                <UiStack direction="row" align="center" gap="sm" class="p-4 rounded-2xl bg-cyan-500/5 border border-cyan-500/10 mb-2">
                  <Info :size="16" class="text-cyan-700 dark:text-cyan-400 shrink-0" />
                  <UiText size="sm" tone="muted">
                    {{ $t('checkout.guestNotice') }}
                    <UiLink :to="localePath(CLIENT_LOGIN_PATH)" tone="inherit" class="text-cyan-700 dark:text-cyan-400 font-bold hover:underline ml-1" :underline="false" data-testid="checkout-sign-in">
                      {{ $t('checkout.signIn') }}
                    </UiLink>
                  </UiText>
                </UiStack>

                <UiGrid cols="2" gap="lg">
                  <UiInput
                    v-model="form.firstname"
                    :label="$t('checkout.fields.firstname')"
                    :placeholder="$t('checkout.placeholders.firstname')"
                    required
                    testid="checkout-firstname"
                  />
                  <UiInput
                    v-model="form.lastname"
                    :label="$t('checkout.fields.lastname')"
                    :placeholder="$t('checkout.placeholders.lastname')"
                    required
                    testid="checkout-lastname"
                  />
                </UiGrid>

                <UiInput
                  v-model="form.email"
                  type="email"
                  :label="$t('checkout.fields.email')"
                  :placeholder="$t('checkout.placeholders.email')"
                  required
                  testid="checkout-email"
                />

                <UiGrid cols="2" gap="lg">
                  <UiInput
                    v-model="form.password"
                    type="password"
                    :label="$t('checkout.fields.password')"
                    :placeholder="$t('checkout.placeholders.password')"
                    required
                    testid="checkout-password"
                  />
                  <UiPhoneInput
                    v-model="form.phonenumber"
                    :label="$t('checkout.fields.phonenumber')"
                    required
                  />
                </UiGrid>

                <UiSelect
                  :model-value="currencyStore.payerCode ?? undefined"
                  :label="$t('cart.currency.label')"
                  :placeholder="$t('cart.currency.placeholder')"
                  :options="currencyOptions"
                  testid="checkout-currency"
                  @update:model-value="value => changeGuestCurrency(String(value))"
                />
              </UiStack>
            </UiCard>

            <!-- Payment method section -->
            <UiCard padding="none" class="p-8 md:p-10 rounded-[32px] backdrop-blur-xl group transition-all duration-500 hover:border-gray-300 dark:hover:border-white/20" data-testid="checkout-payment">
              <UiStack direction="row" align="center" gap="md" class="mb-8">
                <UiStack direction="row" align="center" justify="center" gap="none" class="w-12 h-12 rounded-2xl bg-cyan-500/20 border border-cyan-500/30 text-cyan-700 dark:text-cyan-400 group-hover:scale-110 transition-transform duration-500">
                  <CreditCard :size="24" :stroke-width="2" />
                </UiStack>
                <UiStack gap="none">
                  <UiHeading :level="UiHeadingLevel.H3" size="subsection" class="text-xl font-bold">{{ $t('checkout.paymentMethod') }}</UiHeading>
                  <UiText size="sm" tone="muted">{{ $t('checkout.choosePayment') }}</UiText>
                </UiStack>
              </UiStack>

              <UiGrid v-if="methodsPending" cols="2" gap="sm" data-testid="checkout-methods-loading">
                <UiSkeleton v-for="i in 3" :key="i" shape="block" class="h-14 rounded-2xl" />
              </UiGrid>

              <UiText v-else-if="!paymentMethods.length" tone="subtle" class="italic p-6 text-center border border-dashed border-gray-200 dark:border-white/10 rounded-2xl" data-testid="checkout-no-methods">
                {{ $t('checkout.noPaymentMethods') }}
              </UiText>

              <UiGrid v-else cols="2" gap="md" data-testid="checkout-methods">
                <UiCard
                  v-for="method in paymentMethods"
                  :key="method.module"
                  padding="sm"
                  class="cursor-pointer transition-all duration-300"
                  :class="selectedMethod === method.module
                    ? 'border-cyan-500/60 bg-cyan-500/10 shadow-halo-cyan-sm'
                    : 'hover:border-gray-300 dark:hover:border-white/20'"
                  :data-testid="`checkout-method-${method.module}`"
                  @click="selectedMethod = method.module"
                >
                  <UiStack gap="sm">
                    <UiStack direction="row" align="center" justify="center" gap="none" class="w-8 h-8 rounded-lg bg-white dark:bg-white/5 border border-gray-200 dark:border-white/10">
                      <CreditCard :size="16" :class="selectedMethod === method.module ? 'text-cyan-700 dark:text-cyan-400' : 'text-gray-600 dark:text-gray-400'" />
                    </UiStack>
                    <UiRadio
                      v-model="selectedMethod"
                      name="payment-method"
                      :value="method.module"
                      :label="method.displayName"
                      :testid="`checkout-method-radio-${method.module}`"
                    />
                  </UiStack>
                </UiCard>
              </UiGrid>

              <!-- Card form -->
              <Transition
                enter-active-class="transition-all duration-300"
                enter-from-class="opacity-0 max-h-0"
                enter-to-class="opacity-100 max-h-40"
                leave-active-class="transition-all duration-300"
                leave-from-class="opacity-100 max-h-40"
                leave-to-class="opacity-0 max-h-0"
              >
                <UiStack v-if="selectedMethod === PAYMENT_MODULE_STRIPE" gap="none" class="mt-6 overflow-hidden" data-testid="checkout-card-form">
                  <CheckoutStripeCardForm ref="stripeCardFormRef" />
                </UiStack>
              </Transition>
            </UiCard>
          </UiStack>

          <!-- Right column: summary -->
          <UiStack as="aside" gap="lg" class="lg:col-span-5 sticky top-24">
            <UiCard
              padding="none"
              class="p-8 bg-gradient-to-br from-classic-summary-from to-classic-summary-to border-white/10 rounded-[32px] shadow-2xl relative overflow-hidden ring-1 ring-white/10 text-white"
              data-testid="checkout-summary"
            >
              <!-- Decorative background -->
              <UiStack gap="none" class="absolute -top-24 -right-24 w-48 h-48 bg-cyan-500/10 rounded-full blur-[80px]" />

              <UiHeading :level="UiHeadingLevel.H3" size="subsection" class="relative text-xl font-bold mb-8 flex items-center gap-3 !text-white">
                <Receipt :size="20" class="text-cyan-400" />
                {{ $t('checkout.yourOrder') }}
              </UiHeading>

              <UiAlert v-if="staleItems.length" variant="warning" class="relative mb-4" data-testid="checkout-stale">
                {{ $t('cart.staleCurrencyNotice') }}
              </UiAlert>

              <UiStack gap="md" class="relative mb-8">
                <UiCard
                  v-for="item in cart.itemsInCurrency(currencyStore.payerCode)"
                  :key="item.pid"
                  padding="sm"
                  class="bg-white/5 border-white/10 dark:border-white/5"
                  data-testid="checkout-item"
                >
                  <UiStack direction="row" align="start" justify="between" gap="md">
                    <UiStack direction="row" align="center" gap="sm">
                      <UiStack direction="row" align="center" justify="center" gap="none" class="w-10 h-10 rounded-xl bg-cyan-500/10 text-cyan-400 shrink-0">
                        <Server :size="18" />
                      </UiStack>
                      <UiStack gap="none">
                        <UiText size="sm" tone="inherit" weight="bold" truncate class="w-32 md:w-full">{{ item.name }}</UiText>
                        <UiText size="xs" tone="inherit" weight="bold" class="text-[10px] uppercase tracking-widest text-cyan-400/60 mt-0.5">{{ item.cycleLabel }}</UiText>
                      </UiStack>
                    </UiStack>
                    <UiText size="sm" tone="inherit" weight="bold" class="font-black whitespace-nowrap">{{ itemPrice(item) }}</UiText>
                  </UiStack>
                </UiCard>
              </UiStack>

              <!-- Total divider -->
              <UiStack gap="none" class="relative h-px bg-white/10 my-6" />

              <UiStack direction="row" align="center" justify="between" gap="none" class="relative mb-8">
                <UiText tone="inherit" weight="medium" class="text-gray-400">{{ $t('cart.total') }}</UiText>
                <UiText as="span" tone="inherit" weight="bold" class="text-3xl font-black text-white" data-testid="checkout-total">{{ totalLabel }}</UiText>
              </UiStack>

              <!-- Error box -->
              <UiAlert v-if="orderError" variant="error" class="relative mb-6 animate-shake" data-testid="checkout-error">
                {{ orderError }}
              </UiAlert>

              <!-- Submit. Disabled with no method selected: the gateway list came back
                   empty because every gateway is switched off in the admin. -->
              <UiButton
                variant="primary"
                size="lg"
                full-width
                :loading="submitting"
                :disabled="submitting || cart.isEmpty || !selectedMethod"
                class="relative !h-16 rounded-2xl text-lg font-bold shadow-2xl shadow-cyan-500/20 bg-gradient-to-tr from-cyan-600 to-primary-600 hover:from-cyan-500 hover:to-primary-500 transition-all duration-300"
                data-testid="checkout-submit"
                @click="submitOrder"
              >
                <Lock v-if="!submitting" :size="20" class="mr-2" />
                {{ submitting ? submittingLabel : $t('checkout.placeOrder') }}
              </UiButton>

              <UiText size="xs" tone="inherit" weight="medium" class="relative mt-6 flex items-center justify-center gap-2 text-gray-400">
                <ShieldCheck :size="14" class="text-green-500" />
                {{ $t('cart.secureCheckout') }}
              </UiText>
            </UiCard>

            <!-- Help card -->
            <UiCard padding="md" class="rounded-3xl" data-testid="checkout-help">
              <UiStack direction="row" align="center" gap="md">
                <UiStack direction="row" align="center" justify="center" gap="none" class="w-10 h-10 rounded-full bg-white dark:bg-white/5 text-gray-600 dark:text-gray-400">
                  <HelpCircle :size="20" />
                </UiStack>
                <UiStack gap="none">
                  <UiText size="sm" weight="bold">{{ $t('checkout.needHelp') }}</UiText>
                  <UiText size="xs" tone="subtle">{{ $t('checkout.supportAlways') }}</UiText>
                </UiStack>
                <UiLink :to="localePath('/contact')" :underline="false" class="ml-auto text-xs font-bold" data-testid="checkout-contact">
                  {{ $t('checkout.contactUs') }}
                </UiLink>
              </UiStack>
            </UiCard>
          </UiStack>
        </UiGrid>
      </UiContainer>
    </UiSection>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * ClassicCheckout - the classic template's checkout: the account step (signed-in summary or
 * guest form), the payment-method step with the card form, and the order summary with the
 * submit button.
 *
 * The order flow lives in `stores/checkout.ts`, shared with the other checkout template; this
 * glue around it (guest form, card element reference, router) is `composables/useCheckoutPage.ts`,
 * shared with the other checkout template.
 */
import {
  CreditCard, ShoppingCart, Server, CheckCircle, User,
  Lock, ShieldCheck, Info, Receipt, HelpCircle
} from 'lucide-vue-next'
import { CLIENT_LOGIN_PATH } from '~/constants/clientRoutes'
import { PAYMENT_MODULE_STRIPE } from '~/utils/paymentModules'
import { UiHeadingLevel } from '~/utils/ui/enums'
import { useCheckoutPage } from '~/composables/useCheckoutPage'

/** State and actions of the checkout flow, shared with the other checkout template. */
const {
  localePath,
  cart,
  currencyStore,
  isLoggedIn,
  staleItems,
  totalLabel,
  submittingLabel,
  paymentMethods,
  selectedMethod,
  submitting,
  orderError,
  itemPrice,
  currencyOptions,
  changeGuestCurrency,
  methodsPending,
  userEmail,
  handleLogout,
  form,
  stripeCardFormRef,
  submitOrder
} = await useCheckoutPage()
</script>
