<template>
  <!-- Floating Contact Buttons -->
  <div class="fixed bottom-6 right-6 z-50 flex flex-col gap-3">
    <!-- Telegram Button -->
    <a
      v-if="telegramHandle"
      :href="`${TELEGRAM_LINK_BASE}${telegramHandle}`"
      target="_blank"
      rel="noopener noreferrer"
      class="group relative"
      data-testid="contact-telegram"
      :aria-label="$t('telegram.contactUs')"
    >
      <div
        v-motion
        :initial="{ opacity: 0, scale: 0, rotate: -180 }"
        :enter="{ opacity: 1, scale: 1, rotate: 0, transition: { delay: 1100, duration: 500, type: 'spring' } }"
        class="relative w-12 h-12 md:w-14 md:h-14 bg-channel-telegram rounded-full flex items-center justify-center shadow-glow-telegram hover:scale-110 transition-all duration-300 cursor-pointer"
      >
        <!-- Pulse animation -->
        <div class="absolute inset-0 rounded-full bg-channel-telegram animate-ping opacity-75" />

        <Send
          :size="24"
          :stroke-width="2"
          fill="currentColor"
          class="relative text-white group-hover:scale-110 transition-transform md:hidden"
        />
        <Send
          :size="28"
          :stroke-width="2"
          fill="currentColor"
          class="relative text-white group-hover:scale-110 transition-transform hidden md:block"
        />
      </div>

      <!-- Tooltip (hidden on mobile) -->
      <div
        class="absolute right-full mr-3 top-1/2 -translate-y-1/2 px-4 py-2 bg-gray-900 text-white text-sm font-medium rounded-lg shadow-lg whitespace-nowrap opacity-0 md:group-hover:opacity-100 transition-opacity duration-300 pointer-events-none hidden md:block"
      >
        {{ $t('telegram.tooltip') }}
        <div class="absolute right-0 top-1/2 -translate-y-1/2 translate-x-full">
          <div class="w-0 h-0 border-t-8 border-t-transparent border-b-8 border-b-transparent border-l-8 border-l-gray-900" />
        </div>
      </div>
    </a>

    <!-- WhatsApp Button -->
    <a
      v-if="phoneNumber"
      :href="`${WHATSAPP_LINK_BASE}${phoneNumber.replace(/[^0-9]/g, '')}?text=${encodeURIComponent(defaultMessage)}`"
      target="_blank"
      rel="noopener noreferrer"
      class="group relative"
      data-testid="contact-whatsapp"
      :aria-label="$t('whatsapp.contactUs')"
    >
      <div
        v-motion
        :initial="{ opacity: 0, scale: 0, rotate: -180 }"
        :enter="{ opacity: 1, scale: 1, rotate: 0, transition: { delay: 1000, duration: 500, type: 'spring' } }"
        class="relative w-12 h-12 md:w-14 md:h-14 bg-channel-whatsapp rounded-full flex items-center justify-center shadow-glow-whatsapp hover:scale-110 transition-all duration-300 cursor-pointer"
      >
        <!-- Pulse animation -->
        <div class="absolute inset-0 rounded-full bg-channel-whatsapp animate-ping opacity-75" />

        <MessageCircle
          :size="24"
          :stroke-width="2"
          fill="currentColor"
          class="relative text-white group-hover:scale-110 transition-transform md:hidden"
        />
        <MessageCircle
          :size="28"
          :stroke-width="2"
          fill="currentColor"
          class="relative text-white group-hover:scale-110 transition-transform hidden md:block"
        />
      </div>

      <!-- Tooltip (hidden on mobile) -->
      <div
        class="absolute right-full mr-3 top-1/2 -translate-y-1/2 px-4 py-2 bg-gray-900 text-white text-sm font-medium rounded-lg shadow-lg whitespace-nowrap opacity-0 md:group-hover:opacity-100 transition-opacity duration-300 pointer-events-none hidden md:block"
      >
        {{ $t('whatsapp.tooltip') }}
        <div class="absolute right-0 top-1/2 -translate-y-1/2 translate-x-full">
          <div class="w-0 h-0 border-t-8 border-t-transparent border-b-8 border-b-transparent border-l-8 border-l-gray-900" />
        </div>
      </div>
    </a>
  </div>
</template>

<script setup lang="ts">
/**
 * WhatsAppButton (`UiWhatsAppButton`) - floating Telegram and WhatsApp contact buttons.
 *
 * Both channels come from the operator's portal settings, so a self-hosted install shows its own
 * contacts; a channel with no value is not rendered. The glow and brand colours are theme tokens
 * (`channel-*`, `shadow-glow-*`), and the pulse is Tailwind's `animate-ping`.
 */
import { PortalSettingKey } from '~/constants/portalSettingKey'
import { TELEGRAM_LINK_BASE, WHATSAPP_LINK_BASE } from '~/constants/contactChannel'
import { computed } from 'vue'
import { Send, MessageCircle } from 'lucide-vue-next'

/** Translator for the strings this component builds itself. */
const { t } = useI18n()
/** Reads an operator setting, falling back to its environment value. */
const { get: getContactSetting } = usePortalSettings()

/** WhatsApp number in international format; empty hides the button. */
const phoneNumber = computed(() => getContactSetting(PortalSettingKey.ContactWhatsapp, 'portalWhatsapp'))

/** Telegram handle; empty hides the button. */
const telegramHandle = computed(() => getContactSetting(PortalSettingKey.ContactTelegram, 'portalTelegram'))

/** Message pre-filled when WhatsApp opens. */
const defaultMessage = computed(() => t('whatsapp.defaultMessage'))
</script>
