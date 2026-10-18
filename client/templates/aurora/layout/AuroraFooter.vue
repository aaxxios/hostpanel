<template>
  <!--
    pb-24 clears the fixed contact button, which otherwise sits on top of the
    bottom row once the page is scrolled to the end.
  -->
  <footer class="relative border-t border-line pb-24 pt-[clamp(40px,6vw,56px)] font-aurora">
    <UiContainer>
      <UiGrid cols="auto" gap="xl">
        <UiStack gap="none" class="min-w-0">
          <UiStack direction="row" align="center" gap="sm">
            <UiTile class="bg-brand font-extrabold text-on-tint" aria-hidden="true">i</UiTile>
            <UiText as="span" size="lg" weight="bold" tone="inherit" class="text-tx">{{ siteName }}</UiText>
          </UiStack>
          <UiText tone="inherit" class="mt-4 max-w-[300px] leading-relaxed text-mut2">
            {{ t('aurora.footer.tagline') }}
          </UiText>

          <!-- Rendered only when an external subscribe endpoint is configured. -->
          <UiForm
            v-if="newsletterUrl"
            native-submit
            spacing="none"
            :action="newsletterUrl"
            method="post"
            target="_blank"
            data-testid="newsletter-form"
            class="mt-[22px] max-w-[340px]"
          >
            <UiStack direction="row" wrap align="end" gap="xs">
              <UiInput
                id="aurora-newsletter"
                v-model="emailInput"
                name="email"
                type="email"
                required
                testid="newsletter-email"
                :label="t('aurora.footer.newsletterTitle')"
                :placeholder="t('aurora.footer.newsletterPlaceholder')"
                class="min-w-[180px] flex-1"
                @blur="emailTouched = true"
              />
              <UiButton
                type="submit"
                variant="bare"
                data-testid="newsletter-submit"
                class="whitespace-nowrap rounded-xl bg-brand px-[18px] py-3 text-sm font-bold text-on-tint hover:brightness-110"
              >
                {{ t('aurora.footer.newsletterCta') }}
              </UiButton>
            </UiStack>
            <UiText v-if="emailInvalid" size="sm" tone="danger" class="mt-2.5 min-h-[18px]" data-testid="newsletter-invalid">
              {{ t('aurora.footer.newsletterInvalid') }}
            </UiText>
          </UiForm>

          <!-- Only the profiles an operator has actually configured. -->
          <UiStack v-if="socials.length" direction="row" gap="xs" class="mt-[18px]">
            <UiButton
              v-for="social in socials"
              :key="social.tag"
              variant="bare"
              :href="social.url"
              :title="social.name"
              :aria-label="social.name"
              target="_blank"
              rel="noopener noreferrer"
              :data-testid="`footer-social-${social.tag}`"
              class="h-9 w-9 rounded-xl border border-line2 text-[13px] font-bold text-mut hover:border-ac1 hover:text-ac1"
            >
              {{ social.tag }}
            </UiButton>
          </UiStack>
        </UiStack>

        <UiStack v-for="col in columns" :key="col.title" gap="none" class="min-w-0">
          <UiHeading :level="3" size="bare" class="text-[13px] font-normal uppercase tracking-[0.1em] text-ac1">
            {{ col.title }}
          </UiHeading>
          <UiStack gap="sm" class="mt-[18px]">
            <UiLink
              v-for="link in col.links"
              :key="link.label"
              data-testid="aurora-footer-link"
              :to="link.to"
              tone="inherit"
              :underline="false"
              class="text-[15px] text-mut hover:text-tx"
            >
              {{ link.label }}
            </UiLink>
          </UiStack>
        </UiStack>
      </UiGrid>

      <UiStack
        direction="row"
        wrap
        align="center"
        justify="between"
        class="mt-11 border-t border-line pt-6"
      >
        <UiText as="span" size="sm" tone="inherit" class="text-mut2">© {{ year }} {{ siteName }}</UiText>
        <UiStack direction="row" wrap align="center" gap="none" class="gap-x-2.5 gap-y-1 text-sm text-mut2">
          <UiText v-if="taxId" as="span" size="sm" tone="inherit">{{ t('aurora.footer.taxId') }} {{ taxId }}</UiText>
          <UiText v-if="taxId && (contactEmail || phone)" as="span" size="sm" tone="inherit">·</UiText>
          <UiLink v-if="contactEmail" data-testid="aurora-footer-email" :href="`mailto:${contactEmail}`" tone="inherit" :underline="false" class="hover:text-tx">
            {{ contactEmail }}
          </UiLink>
          <UiText v-if="contactEmail && phone" as="span" size="sm" tone="inherit">·</UiText>
          <UiLink v-if="phone" data-testid="aurora-footer-phone" :href="phoneHref" tone="inherit" :underline="false" class="hover:text-tx">
            {{ phone }}
          </UiLink>
        </UiStack>
      </UiStack>
    </UiContainer>
  </footer>
</template>

<script setup lang="ts">
/**
 * aurora template site footer.
 *
 * The newsletter block posts to an external provider's form endpoint and is
 * omitted entirely when none is configured, so an unconfigured install never
 * shows a subscribe button that goes nowhere. Live chat is not duplicated here —
 * the layout already mounts UiFloatingActions.
 */
const { t } = useI18n()
/** Prefixes a path with the active locale. */
const localePath = useLocalePath()
/** Reads an operator setting by its key. */
const { get } = usePortalSettings()
/** The operator's site name, shown beside the mark and in the copyright line. */
const { name: siteName } = useSiteIdentity()

/** Subscribe endpoint of the newsletter provider; empty hides the block. */
const newsletterUrl = computed(() => get('portal.newsletter.action_url', 'portalNewsletterUrl'))
// Was a hard-coded support@innovayse.com — every self-hosted install published
// this operator's address as its own. Hidden when unset, like the other channels.
/** Public contact e-mail; empty hides it. */
const contactEmail = computed(() => get('portal.contact.email', 'portalContactEmail'))
/** Public phone number; empty hides it. */
const phone = computed(() => get('portal.contact.phone', 'portalContactPhone'))
/** Legal tax identifier; empty hides it. */
const taxId = computed(() => get('portal.legal.tax_id', 'portalLegalTaxId'))
/** Current year for the copyright line. */
const year = new Date().getFullYear()

/** `tel:` link target: the phone number with spacing and punctuation removed. */
const phoneHref = computed(() => `tel:${phone.value.replace(/[^0-9+]/g, '')}`)

/** Telegram is stored as a handle, not a URL, because the chat actions need it that way. */
const telegramUrl = computed(() => {
  const handle = get('portal.contact.telegram', 'portalTelegram')
  return handle ? `https://t.me/${handle}` : ''
})

/**
 * Social profiles the operator has configured. Each is omitted when unset, so an
 * install that fills in none renders no row at all rather than a line of dead
 * icons pointing at this project's own accounts.
 */
const socials = computed(() => [
  { tag: 'fb', name: 'Facebook', url: get('portal.social.facebook', 'portalSocialFacebook') },
  { tag: 'ig', name: 'Instagram', url: get('portal.social.instagram', 'portalSocialInstagram') },
  { tag: 'in', name: 'LinkedIn', url: get('portal.social.linkedin', 'portalSocialLinkedin') },
  { tag: 'tg', name: 'Telegram', url: telegramUrl.value },
  { tag: 'yt', name: 'YouTube', url: get('portal.social.youtube', 'portalSocialYoutube') },
].filter(entry => entry.url))

/** What the visitor typed in the newsletter field. */
const emailInput = ref('')

/** Whether the newsletter field has been left once, so a half-typed address is not nagged about. */
const emailTouched = ref(false)

/**
 * Whether to warn about the address. The form posts to an external provider, so
 * success cannot be confirmed here — only an obviously wrong address can be
 * caught before the visitor is sent away.
 */
const emailInvalid = computed(() =>
  emailTouched.value && emailInput.value.length > 0 && !emailInput.value.includes('@'))

/** Link columns, in display order. */
const columns = computed(() => [
  {
    title: t('aurora.footer.company'),
    // No /about route exists in this portal — the company link points at contact.
    links: [
      { label: t('aurora.footer.about'), to: localePath('/contact') },
      { label: t('aurora.footer.blog'), to: localePath('/knowledgebase') },
      { label: t('aurora.footer.contact'), to: localePath('/contact') },
    ],
  },
  {
    title: t('aurora.footer.support'),
    links: [
      { label: t('aurora.footer.helpCenter'), to: localePath('/knowledgebase') },
      { label: t('aurora.footer.faq'), to: localePath('/faq') },
      { label: t('aurora.footer.domainTransfer'), to: localePath('/domains/transfer') },
    ],
  },
  {
    title: t('aurora.footer.legal'),
    links: [
      { label: t('aurora.footer.terms'), to: localePath('/terms') },
      { label: t('aurora.footer.privacy'), to: localePath('/privacy') },
      { label: t('aurora.footer.cookies'), to: localePath('/cookie-policy') },
      { label: t('aurora.footer.refund'), to: localePath('/refund-policy') },
    ],
  },
])
</script>
