<template>
  <!-- pb-24 clears the fixed contact button the layout mounts over this corner. -->
  <footer class="tpl-nova border-t border-nova-border bg-nova-bg pb-24 pt-14 font-nova text-nova-ink">
    <UiContainer>
      <div class="grid gap-10 lg:grid-cols-[minmax(0,1.4fr)_repeat(3,minmax(0,1fr))]">
        <div>
          <UiStack direction="row" align="center" gap="sm">
            <AppLogoLockup img-class="h-8 w-auto max-w-[120px]" mark-class="h-8 w-8" name-class="text-base font-bold" />
          </UiStack>
          <UiText size="inherit" tone="inherit" class="mt-4 max-w-[320px] text-[15px] leading-relaxed text-nova-muted">
            {{ t('nova.footer.tagline') }}
          </UiText>

          <!-- Only the profiles the operator configured; an unset one is not a dead link. -->
          <div v-if="socials.length" class="mt-6">
            <UiHeading
              :level="2"
              size="bare"
              class="text-[13px] font-semibold uppercase tracking-[0.08em] text-nova-muted"
            >
              {{ t('nova.footer.social') }}
            </UiHeading>
            <UiList list-style="bare" class="mt-3 flex flex-wrap gap-2">
              <li v-for="social in socials" :key="social.key">
                <UiLink
                  :href="social.url"
                  external
                  tone="inherit"
                  :underline="false"
                  :testid="`nova-footer-social-${social.key}`"
                  class="grid h-11 w-11 place-items-center rounded-xl border border-nova-border text-nova-muted hover:border-nova-brand hover:text-nova-ink"
                >
                  <Icon :name="social.icon" class="h-[18px] w-[18px]" aria-hidden="true" />
                  <span class="sr-only">{{ social.name }}</span>
                </UiLink>
              </li>
            </UiList>
          </div>
        </div>

        <div v-for="column in FOOTER_COLUMNS" :key="column.key" class="min-w-0">
          <UiHeading
            :level="2"
            size="bare"
            class="text-[13px] font-semibold uppercase tracking-[0.08em] text-nova-muted"
          >
            {{ t(column.titleKey) }}
          </UiHeading>
          <UiList list-style="bare" class="mt-4 flex flex-col gap-1">
            <li v-for="link in column.links" :key="link.key">
              <UiFooterLink
                :to="localePath(link.to)"
                :variant="UiFooterLinkVariant.Nova"
                :testid="`nova-footer-${column.key}-${link.key}`"
                class="min-h-[40px] text-[15px]"
              >
                {{ t(link.labelKey) }}
              </UiFooterLink>
            </li>
          </UiList>
        </div>
      </div>

      <UiStack
        direction="row"
        wrap
        align="center"
        justify="between"
        gap="md"
        class="mt-12 border-t border-nova-border pt-6 text-sm text-nova-muted"
      >
        <UiText size="inherit" tone="inherit">© {{ year }} {{ BRAND_NAME }}. {{ t('nova.footer.rights') }}</UiText>

        <!-- Each channel is hidden when the operator has not filled it in. -->
        <UiList list-style="bare" class="flex flex-wrap items-center gap-x-4 gap-y-1">
          <li v-if="taxId">{{ t('nova.footer.taxId') }} {{ taxId }}</li>
          <li v-if="contactEmail">
            <UiFooterLink
              :href="`mailto:${contactEmail}`"
              :variant="UiFooterLinkVariant.Nova"
              testid="nova-footer-email"
            >
              {{ contactEmail }}
            </UiFooterLink>
          </li>
          <li v-if="phone">
            <UiFooterLink
              :href="`tel:${phoneHref}`"
              :variant="UiFooterLinkVariant.Nova"
              testid="nova-footer-phone"
            >
              {{ phone }}
            </UiFooterLink>
          </li>
        </UiList>
      </UiStack>
    </UiContainer>
  </footer>
</template>

<script setup lang="ts">
/**
 * nova template site footer.
 *
 * The columns come from `content.ts` and point only at routes this portal has.
 * The contact line and the social row are read from operator settings and each
 * entry disappears when unset, so a fresh install publishes neither an address
 * it does not own nor a link to somebody else's account.
 *
 * Live chat is not repeated here: the layout already mounts UiFloatingActions.
 */
import AppLogoLockup from '~/components/AppLogoLockup.vue'
import { UiFooterLinkVariant } from '~/utils/ui/enums'
import { PortalSettingKey } from '~/constants/portalSettingKey'
import { TELEGRAM_LINK_BASE } from '~/constants/contactChannel'
import { FOOTER_COLUMNS } from '~/templates/nova/content'

/** Name the footer signs with: the operator's site name. */
const { name: BRAND_NAME } = useSiteIdentity()

/** Translator for the copy. */
const { t } = useI18n()
/** Prefixes a route with the active locale. */
const localePath = useLocalePath()
/** Reads the operator settings. */
const { get } = usePortalSettings()

/** Current year, for the copyright line. */
const year = new Date().getFullYear()

/** Operator contact email; empty hides the entry. */
const contactEmail = computed(() => get(PortalSettingKey.ContactEmail, 'portalContactEmail'))
/** Operator contact phone; empty hides the entry. */
const phone = computed(() => get(PortalSettingKey.ContactPhone, 'portalContactPhone'))
/** Operator tax id; empty hides the entry. */
const taxId = computed(() => get(PortalSettingKey.LegalTaxId, 'portalLegalTaxId'))

/** `tel:` accepts digits and a leading plus; everything else is presentation. */
const phoneHref = computed(() => phone.value.replace(/[^0-9+]/g, ''))

/** Telegram is stored as a handle rather than a URL — the chat actions need it that way. */
const telegramUrl = computed(() => {
  const handle = get(PortalSettingKey.ContactTelegram, 'portalTelegram')
  return handle ? `${TELEGRAM_LINK_BASE}${handle}` : ''
})

/** The social profiles the operator configured, in display order. */
const socials = computed(() => [
  { key: 'facebook', icon: 'lucide:facebook', name: 'Facebook', url: get(PortalSettingKey.SocialFacebook, 'portalSocialFacebook') },
  { key: 'instagram', icon: 'lucide:instagram', name: 'Instagram', url: get(PortalSettingKey.SocialInstagram, 'portalSocialInstagram') },
  { key: 'linkedin', icon: 'lucide:linkedin', name: 'LinkedIn', url: get(PortalSettingKey.SocialLinkedin, 'portalSocialLinkedin') },
  { key: 'telegram', icon: 'lucide:send', name: 'Telegram', url: telegramUrl.value },
  { key: 'youtube', icon: 'lucide:youtube', name: 'YouTube', url: get(PortalSettingKey.SocialYoutube, 'portalSocialYoutube') },
].filter(entry => entry.url))
</script>
