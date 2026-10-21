<template>
  <section class="py-14 md:py-20 relative overflow-hidden" data-testid="sections-cta">
    <!-- Background cover -->
    <UiDecor class="inset-0">
      <UiDecor class="inset-0 bg-gradient-to-br from-blue-50 dark:from-gray-900 via-white dark:via-ui-page to-blue-50 dark:to-gray-900" />
      <UiDecor class="inset-0 bg-gradient-to-r from-primary-600/20 via-transparent to-secondary-600/20" />

      <!-- Glowing orbs -->
      <UiDecor class="top-0 left-1/4 w-96 h-96 bg-primary-500/30 rounded-full blur-[150px] animate-blob" />
      <UiDecor class="bottom-0 right-1/4 w-80 h-80 bg-secondary-500/30 rounded-full blur-[150px] animate-blob animation-delay-2000" />

      <!-- Floating particles -->
      <UiDecor class="top-1/3 left-1/4 w-2 h-2 bg-primary-400/70 rounded-full animate-float [animation-delay:0.5s]" />
      <UiDecor class="top-1/2 right-1/3 w-3 h-3 bg-secondary-600/60 dark:bg-secondary-400/60 rounded-full animate-float [animation-delay:1.2s]" />
      <UiDecor class="bottom-1/3 left-1/3 w-2 h-2 bg-cyan-400/70 rounded-full animate-float [animation-delay:2s]" />

      <!-- Border lines -->
      <UiDecor class="top-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-primary-500/50 to-transparent" />
      <UiDecor class="bottom-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-secondary-500/50 to-transparent" />

      <!-- Grid overlay -->
      <UiDecor class="inset-0 opacity-[0.02] pattern-grid-60" />
    </UiDecor>

    <UiContainer class="relative z-10">
      <div class="max-w-3xl mx-auto text-center animate-reveal-up motion-reduce:animate-none">
        <span class="inline-block px-4 py-2 mb-6 bg-white dark:bg-white/5 backdrop-blur-sm border border-gray-200 dark:border-white/10 rounded-full text-sm text-gray-700 dark:text-gray-300">{{ $t('cta.badge') }}</span>

        <UiHeading :level="2" class="text-4xl md:text-5xl lg:text-6xl mb-4">{{ $t('cta.title') }}</UiHeading>

        <UiText tone="muted" size="lg" class="mb-10 max-w-xl mx-auto">{{ $t('cta.subtitle') }}</UiText>

        <!-- Email form -->
        <UiForm spacing="none" class="max-w-xl mx-auto" data-testid="cta-form" @submit="handleSubmit">
          <div class="flex flex-col sm:flex-row gap-4 relative">
            <UiInput
              v-model="email"
              type="email"
              :placeholder="$t('cta.placeholder')"
              required
              class="flex-1 relative"
              size="lg"
              testid="cta-email"
            />

            <UiButton
              type="submit"
              variant="primary"
              size="lg"
              :loading="isSubmitting"
              class="relative hover:shadow-lg hover:shadow-primary-500/50 transition-all duration-300"
              data-testid="cta-submit"
            >
              <span>{{ $t('cta.button') }}</span>
              <ArrowRight v-if="!isSubmitting" :size="18" :stroke-width="2" class="ml-2" />
            </UiButton>
          </div>
        </UiForm>

        <!-- Success message -->
        <Transition
          enter-active-class="transition-opacity duration-300 ease-out"
          leave-active-class="transition-opacity duration-300 ease-out"
          enter-from-class="opacity-0"
          leave-to-class="opacity-0"
        >
          <UiAlert v-if="showSuccess" variant="success" class="mt-6 max-w-xl mx-auto text-left" data-testid="cta-success">
            {{ $t('cta.success') }}
          </UiAlert>
        </Transition>
      </div>
    </UiContainer>

    <!-- Decorative corner accents -->
    <UiDecor class="top-0 right-0 w-48 h-48 border-r-2 border-t-2 border-secondary-500/30" />
    <UiDecor class="bottom-0 left-0 w-48 h-48 border-l-2 border-b-2 border-primary-500/30" />
  </section>
</template>

<script setup lang="ts">
/**
 * Call-to-action band that closes a marketing page: a headline and an e-mail field.
 *
 * The submit is simulated (a one second wait, then a confirmation that disappears after three
 * seconds); the field is cleared afterwards. The block enters with a CSS keyframe, not a
 * scroll-triggered directive, so it is visible in server HTML and in both themes.
 */
import { ArrowRight } from 'lucide-vue-next'

/** How long the simulated submit takes, in ms. */
const SUBMIT_DELAY = 1000

/** How long the confirmation stays on screen, in ms. */
const SUCCESS_VISIBLE = 3000

/** The e-mail address typed into the field. */
const email = ref('')

/** Whether the simulated submit is running. */
const isSubmitting = ref(false)

/** Whether the confirmation is showing. */
const showSuccess = ref(false)

/** Submits the address: waits, confirms, clears the field and hides the confirmation again. */
const handleSubmit = async (): Promise<void> => {
  if (!email.value) {
    return
  }

  isSubmitting.value = true
  await new Promise((resolve) => setTimeout(resolve, SUBMIT_DELAY))

  showSuccess.value = true
  email.value = ''
  isSubmitting.value = false

  setTimeout(() => {
    showSuccess.value = false
  }, SUCCESS_VISIBLE)
}
</script>
