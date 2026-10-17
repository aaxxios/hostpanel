<template>
  <!-- Spacer in the page flow: the bar is fixed, so without it the bar would cover the last lines and form actions -->
  <div
    v-if="showBanner"
    aria-hidden="true"
    class="h-[var(--cookie-banner-height,0px)] shrink-0"
    data-testid="cookie-banner-spacer"
  />

  <Transition
    enter-active-class="transition-all duration-[400ms] ease-out"
    leave-active-class="transition-all duration-[400ms] ease-in"
    enter-from-class="translate-y-full opacity-0"
    leave-to-class="translate-y-full opacity-0"
  >
    <div
      v-if="showBanner"
      ref="bannerRef"
      role="dialog"
      data-testid="cookie-banner"
      class="fixed bottom-0 left-0 right-0 z-50 p-3 md:p-6 bg-white/95 dark:bg-ui-surface backdrop-blur-xl border-t-2 border-primary-500/30 shadow-2xl"
    >
      <div class="container-custom">
        <div class="flex flex-col md:flex-row items-start md:items-center justify-between gap-3 md:gap-4">
          <!-- Content -->
          <div class="flex-1 flex items-start gap-4">
            <div class="hidden md:block p-3 rounded-xl bg-primary-500/10 border border-primary-500/20">
              <Cookie :size="24" :stroke-width="2" class="text-primary-600 dark:text-primary-400" />
            </div>
            <div class="flex-1">
              <h3 class="hidden md:block text-lg font-bold text-gray-900 dark:text-white mb-2">
                {{ $t('cookieBanner.title') }}
              </h3>
              <p class="text-xs md:text-sm text-gray-600 dark:text-gray-400 leading-snug md:leading-relaxed">
                <span class="md:hidden">{{ $t('cookieBanner.descriptionShort') }}</span>
                <span class="hidden md:inline">{{ $t('cookieBanner.description') }}</span>
                <UiLink :to="localePath('/cookie-policy')" testid="cookie-banner-learn-more">
                  {{ $t('cookieBanner.learnMore') }}
                </UiLink>
              </p>
            </div>
          </div>

          <!-- Buttons -->
          <div class="flex flex-row gap-2 md:gap-3 w-full md:w-auto">
            <UiButton
              variant="subtle"
              size="sm"
              class="flex-1 whitespace-nowrap md:flex-none"
              data-testid="cookie-banner-essential"
              @click="acceptEssential"
            >
              {{ $t('cookieBanner.essentialOnly') }}
            </UiButton>
            <UiButton
              size="sm"
              class="flex-1 whitespace-nowrap md:flex-none"
              data-testid="cookie-banner-accept-all"
              @click="acceptAll"
            >
              {{ $t('cookieBanner.acceptAll') }}
            </UiButton>
          </div>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
/**
 * CookieBanner (`UiCookieBanner`) - the consent bar fixed to the bottom of the page.
 *
 * Mounted by the `default` and `client` layouts only, so auth pages (no layout) never show it.
 * While visible it also renders a spacer so the bar does not cover content.
 *
 * Shown only until a choice is made. The choice itself lives in `useCookieConsent`, where
 * `plugins/tracking.client.ts` reads it: this banner records the answer, the tag loader acts on it.
 */
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { Cookie } from 'lucide-vue-next'
import { CookieConsent } from '~/types/cookieconsent'
import { COOKIE_BANNER_HEIGHT_VAR } from '~/utils/ui/cookieBanner'

/** Localised path builder from the i18n module. */
const localePath = useLocalePath()

/** Whether the banner is showing. */
const showBanner = ref(false)

/** The banner's root element; measured so the spacer and the contact button match its real height. */
const bannerRef = ref<HTMLElement | null>(null)

/** Watches the banner's size; null while it is not shown. */
let resizeObserver: ResizeObserver | null = null

/**
 * Publishes the banner's current height on the root element, or clears it when there is none.
 *
 * @param height The height in pixels, or null to remove the variable.
 */
const publishHeight = (height: number | null): void => {
  if (height === null) {
    document.documentElement.style.removeProperty(COOKIE_BANNER_HEIGHT_VAR)
    return
  }
  document.documentElement.style.setProperty(COOKIE_BANNER_HEIGHT_VAR, `${height}px`)
}

// Follows the banner while it is mounted and stops, clearing the variable, once it is gone.
watch(bannerRef, (element) => {
  resizeObserver?.disconnect()
  resizeObserver = null
  if (!element) {
    publishHeight(null)
    return
  }
  publishHeight(element.offsetHeight)
  if (typeof ResizeObserver !== 'undefined') {
    resizeObserver = new ResizeObserver(() => publishHeight(element.offsetHeight))
    resizeObserver.observe(element)
  }
})

onBeforeUnmount(() => {
  resizeObserver?.disconnect()
  publishHeight(null)
})

/** The visitor's stored cookie choice and the action that records a new one. */
const { consent, accept } = useCookieConsent()

// Mounted rather than computed from `consent` on the server, so a page never renders the banner
// into the HTML for a visitor who already chose.
onMounted(() => {
  if (!consent.value) {
    showBanner.value = true
  }
})

/** Accepts essential cookies only: the tag manager is never loaded. */
const acceptEssential = (): void => {
  accept(CookieConsent.Essential)
  showBanner.value = false
}

/** Accepts all cookies: the tag manager loads if the operator configured one. */
const acceptAll = (): void => {
  accept(CookieConsent.All)
  showBanner.value = false
}
</script>
