<template>
  <!--
    z-[60] rather than z-50: UiCookieBanner is a full-width bar fixed to the same
    corner at z-50, and being later in the document it took every click meant for
    this button until it was dismissed. The button stayed visible the whole time,
    which is the worst version of the problem: it looked live and was not.
  -->
  <div ref="fabRef" class="fixed bottom-[calc(1.5rem+var(--cookie-banner-height,0px))] right-6 z-[60]" data-testid="floating-actions">

    <!-- Fan-out sub-buttons -->
    <div class="absolute bottom-0 right-0">

      <!-- Live chat: fans up-left -->
      <Transition
        enter-active-class="transition-all duration-300 ease-out"
        leave-active-class="transition-all duration-200 ease-in"
        enter-from-class="opacity-0 scale-0"
        enter-to-class="opacity-100 scale-100"
        leave-from-class="opacity-100 scale-100"
        leave-to-class="opacity-0 scale-0"
      >
        <button
          v-if="isOpen && liveChatEnabled"
          type="button"
          data-testid="floating-live-chat"
          class="group absolute bottom-0 right-0 flex items-center justify-center -translate-x-[5px] -translate-y-[75px] delay-0"
          :aria-label="$t('contact.info.quickConnect.liveChat')"
          @click.stop="openChat"
        >
          <span class="absolute right-full mr-2 px-2.5 py-1 bg-gray-900/90 text-white text-xs font-medium rounded-lg whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none">
            {{ $t('contact.info.quickConnect.liveChat') }}
          </span>
          <span class="w-10 h-10 bg-channel-chat rounded-full flex items-center justify-center shadow-glow-chat hover:scale-110 transition-transform duration-200">
            <MessageSquare :size="18" :stroke-width="2" class="text-white" />
          </span>
        </button>
      </Transition>

      <!-- WhatsApp: fans straight up -->
      <Transition
        enter-active-class="transition-all duration-300 ease-out"
        leave-active-class="transition-all duration-200 ease-in"
        enter-from-class="opacity-0 scale-0"
        enter-to-class="opacity-100 scale-100"
        leave-from-class="opacity-100 scale-100"
        leave-to-class="opacity-0 scale-0"
      >
        <a
          v-if="isOpen && whatsappNumber"
          :href="`https://wa.me/${whatsappNumber}?text=${encodeURIComponent(defaultMessage)}`"
          target="_blank"
          rel="noopener noreferrer"
          data-testid="floating-whatsapp"
          class="group absolute bottom-0 right-0 flex items-center justify-center -translate-x-[55px] -translate-y-[55px] delay-[60ms]"
          :aria-label="$t('whatsapp.contactUs')"
        >
          <span class="absolute right-full mr-2 px-2.5 py-1 bg-gray-900/90 text-white text-xs font-medium rounded-lg whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none">
            {{ $t('contact.info.quickConnect.whatsapp') }}
          </span>
          <span class="w-10 h-10 bg-channel-whatsapp rounded-full flex items-center justify-center shadow-glow-whatsapp hover:scale-110 transition-transform duration-200">
            <MessageCircle :size="18" :stroke-width="2" class="text-white" />
          </span>
        </a>
      </Transition>

      <!-- Telegram: fans left -->
      <Transition
        enter-active-class="transition-all duration-300 ease-out"
        leave-active-class="transition-all duration-200 ease-in"
        enter-from-class="opacity-0 scale-0"
        enter-to-class="opacity-100 scale-100"
        leave-from-class="opacity-100 scale-100"
        leave-to-class="opacity-0 scale-0"
      >
        <a
          v-if="isOpen && telegramHandle"
          :href="`https://t.me/${telegramHandle}`"
          target="_blank"
          rel="noopener noreferrer"
          data-testid="floating-telegram"
          class="group absolute bottom-0 right-0 flex items-center justify-center -translate-x-[75px] -translate-y-[5px] delay-[120ms]"
          :aria-label="$t('telegram.contactUs')"
        >
          <span class="absolute right-full mr-2 px-2.5 py-1 bg-gray-900/90 text-white text-xs font-medium rounded-lg whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none">
            {{ $t('contact.info.quickConnect.telegram') }}
          </span>
          <span class="w-10 h-10 bg-channel-telegram rounded-full flex items-center justify-center shadow-glow-telegram hover:scale-110 transition-transform duration-200">
            <Send :size="18" :stroke-width="2" class="text-white" />
          </span>
        </a>
      </Transition>

    </div>

    <!-- Main toggle button -->
    <button
      v-motion
      type="button"
      data-testid="floating-toggle"
      :initial="{ opacity: 0, scale: 0 }"
      :enter="{ opacity: 1, scale: 1, transition: { delay: 800, duration: 400, type: 'spring' } }"
      class="relative w-12 h-12 rounded-full flex items-center justify-center shadow-2xl transition-all duration-300 hover:scale-110 focus:outline-none"
      :class="isOpen ? 'bg-gray-700 rotate-45' : 'bg-gradient-to-br from-channel-chat to-primary-600 shadow-glow-chat'"
      :aria-expanded="isOpen"
      :aria-label="$t('contact.info.quickConnect.toggle')"
      @click="isOpen = !isOpen"
    >
      <span v-if="!isOpen" class="absolute inset-0 rounded-full bg-channel-chat animate-ping opacity-25" />
      <MessageCircle :size="26" :stroke-width="2" class="relative text-white transition-transform duration-300" :class="isOpen ? 'rotate-[-45deg]' : ''" />
    </button>

  </div>
</template>

<script setup lang="ts">
/**
 * FloatingActions (`UiFloatingActions`) - the floating contact button that fans out three actions
 * (live chat, WhatsApp, Telegram) up and to the left of the bottom-right corner.
 *
 * Contact channels come from the operator's settings rather than being baked in, so a self-hosted
 * install shows its own contacts; each action hides when its setting is empty. Escape or a click
 * outside closes the fan.
 */
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { Send, MessageCircle, MessageSquare } from 'lucide-vue-next'

/** Translator for the strings this component builds itself. */
const { t } = useI18n()

/** The root element, used to detect outside clicks. */
const fabRef = ref<HTMLElement | null>(null)

/** Whether the fan is open. */
const isOpen = ref(false)

/**
 * Closes the fan when a click lands outside the component.
 *
 * @param event The document click.
 */
const handleOutsideClick = (event: MouseEvent): void => {
  if (fabRef.value && !fabRef.value.contains(event.target as Node)) {
    isOpen.value = false
  }
}

/**
 * Escape closes the fan, the keyboard equivalent of clicking away from it.
 *
 * @param event The document key press.
 */
const handleEscape = (event: KeyboardEvent): void => {
  if (event.key === 'Escape') {
    isOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleOutsideClick)
  document.addEventListener('keydown', handleEscape)
})
onUnmounted(() => {
  document.removeEventListener('click', handleOutsideClick)
  document.removeEventListener('keydown', handleEscape)
})

/** Message pre-filled when WhatsApp opens. */
const defaultMessage = computed(() => t('whatsapp.defaultMessage'))

/** Reads an operator setting, falling back to its environment value. */
const { get: getPortalSetting } = usePortalSettings()

/** WhatsApp number from configuration; empty hides the action. */
const whatsappNumber = computed(() => getPortalSetting('portal.contact.whatsapp', 'portalWhatsapp'))

/** Telegram handle from configuration; empty hides the action. */
const telegramHandle = computed(() => getPortalSetting('portal.contact.telegram', 'portalTelegram'))

/** Chat provider from configuration; empty hides the chat bubble. */
const chatProvider = computed(() => getPortalSetting('portal.chat.provider', 'portalChatProvider'))

/**
 * Whether to offer the live chat action.
 *
 * Both spellings are accepted. The provider is Innochat, which is what an operator configuring
 * this today would reasonably write, but the setting has shipped seeded with `chatwoot` since the
 * first release and existing installs have that value stored. Refusing it would have switched
 * the widget off for every one of them on upgrade.
 */
const liveChatEnabled = computed(() =>
  ['chatwoot', 'innochat'].includes(chatProvider.value.toLowerCase()))

/**
 * Opens the live chat widget.
 *
 * The widget is started by the loader in app.vue and exposes itself on the window; `openLiveChat`
 * knows which global that is. It reports back rather than failing silently, and a failure is
 * logged: a visitor clicking a chat button that does nothing has no way to tell that anything
 * went wrong, and neither did anyone reading the console.
 */
const openChat = (): void => {
  isOpen.value = false
  if (!import.meta.client) {
    return
  }

  if (!openLiveChat(window)) {
    console.warn('[live chat] widget is not running; the SDK has not started it yet')
  }
}
</script>
