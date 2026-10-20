<template>
  <footer class="relative mt-20 overflow-hidden bg-classic-footer" data-testid="layout-footer">
    <!-- Background decor -->
    <UiStack gap="none" class="absolute inset-0 pointer-events-none overflow-hidden">
      <UiStack gap="none" class="absolute -top-24 -left-24 w-96 h-96 bg-primary-600/5 dark:bg-primary-600/10 rounded-full blur-[120px]" />
      <UiStack gap="none" class="absolute -bottom-24 -right-24 w-96 h-96 bg-secondary-600/5 dark:bg-secondary-600/10 rounded-full blur-[120px]" />
      <UiStack gap="none" class="absolute inset-x-0 top-0 h-px bg-gradient-to-r from-transparent via-gray-300 dark:via-white/10 to-transparent" />
    </UiStack>

    <UiContainer class="relative z-10 pt-20 pb-10">
      <!-- Main footer content -->
      <UiGrid cols="1" gap="xl" class="sm:grid-cols-2 lg:grid-cols-4 mb-16">
        <!-- Column 1: logo and vision -->
        <UiStack gap="none">
          <UiStack direction="row" align="center" gap="none" class="mb-8 group cursor-pointer">
            <!-- Follows the colour mode, like the footer surface behind it. -->
            <AppLogo
              :surface="LogoSurface.Auto"
              :width="240"
              :height="80"
              loading="lazy"
              img-class="h-12 md:h-14 w-auto max-w-[240px] object-contain transition-transform duration-500 group-hover:scale-105"
            />
          </UiStack>
          <UiText tone="muted" class="leading-relaxed mb-10 max-w-sm">
            {{ $t('footer.description') }}
          </UiText>

          <!-- Social icons with the magnetic effect -->
          <UiStack v-if="socialLinks.length" direction="row" align="center" gap="lg">
            <UiMagnetic v-for="social in socialLinks" :key="social.name" :strength="0.4">
              <UiLink
                :href="social.href"
                external
                tone="muted"
                :underline="false"
                class="w-12 h-12 rounded-xl bg-white dark:bg-white/[0.03] border border-gray-200 dark:border-white/5 flex items-center justify-center hover:text-primary-700 dark:hover:text-on-primary hover:bg-primary-500/20 hover:border-primary-500/50 transition-all duration-300 group/social"
                :aria-label="social.name"
                :title="social.name"
                data-testid="footer-social-link"
              >
                <Icon :name="social.icon" class="text-2xl transition-transform group-hover/social:scale-110" />
              </UiLink>
            </UiMagnetic>
          </UiStack>
        </UiStack>

        <!-- Column 2: navigation -->
        <UiStack gap="none">
          <UiHeading :level="UiHeadingLevel.H3" size="subsection" class="mb-8 tracking-tight flex items-center gap-3 font-bold">
            <UiStack gap="none" class="w-2 h-2 rounded-full bg-primary-500 shadow-dot-primary" />
            {{ $t('footer.columns.navigation') }}
          </UiHeading>
          <UiList list-style="plain" class="space-y-4">
            <UiListItem v-for="link in navigationLinks" :key="link.href">
              <UiFooterLink
                :to="link.href"
                :variant="UiFooterLinkVariant.Classic"
                :underline="false"
                class="text-sm duration-300 group"
                testid="footer-nav-link"
              >
                <UiStack gap="none" as="span" class="w-0 group-hover:w-4 h-px bg-primary-500 mr-0 group-hover:mr-3 transition-all duration-300 opacity-0 group-hover:opacity-100" />
                {{ link.label }}
              </UiFooterLink>
            </UiListItem>
          </UiList>
        </UiStack>

        <!-- Column 3: services -->
        <UiStack gap="none">
          <UiHeading :level="UiHeadingLevel.H3" size="subsection" class="mb-8 tracking-tight flex items-center gap-3 font-bold">
            <UiStack gap="none" class="w-2 h-2 rounded-full bg-secondary-500 shadow-dot-secondary" />
            {{ $t('footer.columns.services') }}
          </UiHeading>
          <UiList list-style="plain" class="space-y-4">
            <UiListItem v-for="service in serviceLinks" :key="service.href">
              <UiFooterLink
                :to="service.href"
                :variant="UiFooterLinkVariant.Classic"
                :underline="false"
                class="text-sm duration-300 group"
                testid="footer-service-link"
              >
                <UiStack gap="none" as="span" class="w-0 group-hover:w-4 h-px bg-secondary-500 mr-0 group-hover:mr-3 transition-all duration-300 opacity-0 group-hover:opacity-100" />
                {{ service.label }}
              </UiFooterLink>
            </UiListItem>
          </UiList>
        </UiStack>

        <!-- Column 4: contact and back to top -->
        <UiStack gap="none">
          <UiHeading :level="UiHeadingLevel.H3" size="subsection" class="mb-8 tracking-tight flex items-center gap-3 font-bold">
            <UiStack gap="none" class="w-2 h-2 rounded-full bg-cyan-500 shadow-dot-cyan" />
            {{ $t('footer.columns.contact') }}
          </UiHeading>
          <UiList list-style="plain" class="space-y-6">
            <UiListItem v-if="contactEmail" class="group">
              <UiFooterLink
                :href="`mailto:${contactEmail}`"
                :variant="UiFooterLinkVariant.Classic"
                :underline="false"
                class="gap-4 text-sm"
                testid="footer-email"
              >
                <UiStack direction="row" align="center" justify="center" gap="none" class="w-10 h-10 rounded-lg bg-white dark:bg-white/[0.03] border border-gray-200 dark:border-white/5 group-hover:border-primary-500/30 group-hover:bg-primary-500/10">
                  <Mail :size="18" class="text-primary-600 dark:text-primary-400 transition-transform group-hover:scale-110" />
                </UiStack>
                {{ contactEmail }}
              </UiFooterLink>
            </UiListItem>
            <UiListItem v-if="contactPhone" class="group">
              <UiFooterLink
                :href="`tel:${contactPhoneLink}`"
                :variant="UiFooterLinkVariant.Classic"
                :underline="false"
                class="gap-4 text-sm"
                testid="footer-phone"
              >
                <UiStack direction="row" align="center" justify="center" gap="none" class="w-10 h-10 rounded-lg bg-white dark:bg-white/[0.03] border border-gray-200 dark:border-white/5 group-hover:border-secondary-500/30 group-hover:bg-secondary-500/10">
                  <Phone :size="18" class="text-secondary-600 dark:text-secondary-400 transition-transform group-hover:scale-110" />
                </UiStack>
                {{ contactPhone }}
              </UiFooterLink>
            </UiListItem>
            <UiListItem v-if="contactAddress" class="group">
              <UiStack direction="row" align="center" gap="lg">
                <UiStack direction="row" align="center" justify="center" gap="none" class="w-10 h-10 rounded-lg bg-white dark:bg-white/[0.03] border border-gray-200 dark:border-white/5 group-hover:border-cyan-500/30 group-hover:bg-cyan-500/10">
                  <MapPin :size="18" class="text-cyan-600 dark:text-cyan-400 transition-transform group-hover:scale-110" />
                </UiStack>
                <UiText as="span" size="sm" tone="muted" data-testid="footer-address">{{ contactAddress }}</UiText>
              </UiStack>
            </UiListItem>
          </UiList>

          <!-- Back to top button -->
          <UiStack direction="row" justify="end" gap="none" class="mt-12">
            <UiMagnetic :strength="0.5">
              <UiIconButton
                :icon="ChevronUp"
                :label="$t('footer.backToTop')"
                size="lg"
                class="w-14 h-14 rounded-full bg-white dark:bg-white/[0.03] shadow-drop-badge"
                testid="footer-back-to-top"
                @click="scrollToTop"
              />
            </UiMagnetic>
          </UiStack>
        </UiStack>
      </UiGrid>

      <!-- Bottom bar -->
      <UiCard padding="xl" class="relative rounded-[2rem] bg-white dark:bg-white/[0.02] backdrop-blur-xl">
        <UiStack align="center" justify="between" gap="lg" class="md:flex-row">
          <UiText size="sm" tone="subtle" weight="medium">
            © {{ currentYear }} <UiText as="span" size="sm" weight="medium">{{ BRAND_NAME }}</UiText>. {{ $t('footer.bottom.rights') }}
          </UiText>

          <UiStack direction="row" align="center" justify="center" wrap gap="lg">
            <UiFooterLink
              v-for="legal in legalLinks"
              :key="legal.to"
              :to="legal.to"
              :variant="UiFooterLinkVariant.Classic"
              class="text-sm font-medium"
              testid="footer-legal-link"
            >
              {{ legal.label }}
            </UiFooterLink>
          </UiStack>

          <!-- Trusted badge -->
          <UiStack direction="row" align="center" gap="sm" class="px-4 py-2 rounded-full bg-white dark:bg-white/[0.03] border border-gray-200 dark:border-white/5">
            <UiStack gap="none" class="w-2 h-2 rounded-full bg-green-500 animate-pulse" />
            <UiText as="span" size="xs" tone="muted" weight="bold" class="uppercase tracking-widest">{{ $t('footer.bottom.secure') }}</UiText>
          </UiStack>
        </UiStack>
      </UiCard>
    </UiContainer>
  </footer>
</template>

<script setup lang="ts">
/**
 * LayoutFooter - the site footer with four columns: logo and social icons, navigation links,
 * service links, and contact details with a back-to-top button, above a bar of legal links.
 */
import { PortalSettingKey } from '~/constants/portalSettingKey'
import { ChevronUp, Mail, MapPin, Phone } from 'lucide-vue-next'
import type { SocialLink } from '~/types/sociallink'
import { LogoSurface } from '~/utils/appLogo'
import { UiFooterLinkVariant, UiHeadingLevel } from '~/utils/ui/enums'

/** Translate function. */
const { t } = useI18n()

/** Prefixes a path with the active locale. */
const localePath = useLocalePath()

/** Company name shown in the copyright line: the operator's site name. */
const { name: BRAND_NAME } = useSiteIdentity()

/** Reads an operator setting by its key, falling back to the matching runtime config value. */
const { get } = usePortalSettings()

/** Contact address shown and linked in the contact column; empty hides the row. */
const contactEmail = computed(() => get(PortalSettingKey.ContactEmail, 'portalContactEmail'))

/** Contact phone as the operator wrote it, and in the form a `tel:` link needs; empty hides the row. */
const { phone: contactPhone, phoneLink: contactPhoneLink, address: contactAddress } = useContactDetails()

/** Scrolls the page back to the top. */
const scrollToTop = (): void => {
  window.scrollTo({
    top: 0,
    behavior: 'smooth'
  })
}

/** Navigation column links. */
const navigationLinks = computed(() => [
  { label: t('footer.nav.home'), href: localePath('/') },
  { label: t('footer.nav.announcements'), href: localePath('/announcements') },
  { label: t('footer.nav.knowledgebase'), href: localePath('/knowledgebase') },
  { label: t('footer.nav.faq'), href: localePath('/faq') },
  { label: t('footer.nav.contact'), href: localePath('/contact') }
])

/** Services column links. */
const serviceLinks = computed(() => [
  { label: t('footer.services.plans'), href: localePath('/products') }
])

/** Legal links of the bottom bar. */
const legalLinks = computed(() => [
  { label: t('footer.bottom.privacy'), to: localePath('/privacy') },
  { label: t('footer.bottom.cookies'), to: localePath('/cookie-policy') },
  { label: t('footer.bottom.terms'), to: localePath('/terms') },
  { label: t('footer.bottom.refund'), to: localePath('/refund-policy') }
])

/**
 * Social media profiles the operator configured; the name doubles as the accessible label.
 * A profile with no URL is dropped, so an install that sets none renders no icons at all.
 */
const socialLinks = computed<SocialLink[]>(() => [
  { name: 'Twitter', href: get(PortalSettingKey.SocialTwitter, 'portalSocialTwitter'), icon: 'mdi:twitter' },
  { name: 'LinkedIn', href: get(PortalSettingKey.SocialLinkedin, 'portalSocialLinkedin'), icon: 'mdi:linkedin' },
  { name: 'Facebook', href: get(PortalSettingKey.SocialFacebook, 'portalSocialFacebook'), icon: 'mdi:facebook' },
].filter(entry => entry.href))

/** Current year for the copyright line. */
const currentYear = new Date().getFullYear()
</script>
