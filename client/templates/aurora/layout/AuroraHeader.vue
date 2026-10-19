<template>
  <header class="relative border-b border-line font-aurora">
    <UiContainer class="flex flex-wrap items-center justify-between gap-x-6 gap-y-4 py-[18px]">
      <UiLink
        :to="localePath('/')"
        tone="inherit"
        :underline="false"
        testid="site-home"
        class="flex items-center gap-3 text-tx"
      >
        <!-- Both wordmarks, swapped by the dark: variant — see useBrandLogo for why not one mode-aware src. -->
        <template v-if="logoLight">
          <UiImage
            unoptimised
            loading="eager"
            fit="contain"
            testid="site-logo-light"
            :src="logoLight"
            :alt="logoAlt"
            class="h-[34px] w-auto max-w-[140px]"
            :class="{ 'dark:hidden': hasDarkLogo }"
          />
          <UiImage
            v-if="hasDarkLogo"
            unoptimised
            loading="eager"
            fit="contain"
            testid="site-logo-dark"
            :src="logoDark"
            :alt="logoAlt"
            class="hidden h-[34px] w-auto max-w-[140px] dark:block"
          />
        </template>
        <template v-else>
          <UiTile class="bg-brand font-extrabold text-on-tint" aria-hidden="true">{{ siteName.charAt(0).toLowerCase() }}</UiTile>
          <UiText as="span" size="lg" weight="bold" tone="inherit">{{ siteName }}</UiText>
        </template>
      </UiLink>

      <UiStack
        as="nav"
        direction="row"
        align="center"
        wrap
        class="hidden min-w-0 flex-auto gap-x-[26px] gap-y-3 text-[15px] text-mut xl:flex"
      >
        <UiLink
          v-for="link in navLinks"
          :key="link.key"
          :to="link.to"
          tone="inherit"
          :underline="false"
          :testid="`nav-${link.key}`"
          class="rounded-lg px-2 py-1 hover:text-ac1"
          :class="active === link.key ? 'font-semibold text-tx' : 'font-normal'"
        >
          {{ link.label }}
        </UiLink>
      </UiStack>

      <UiStack direction="row" align="center" gap="xs" class="shrink-0">
        <AuroraAppLauncher />
        <UiLanguageSwitcher />
        <UiCurrencySwitcher />
        <UiThemeToggle />

        <UiButton
          variant="bare"
          :to="localePath('/cart')"
          :title="t('aurora.nav.cart')"
          :aria-label="t('aurora.nav.cart')"
          data-testid="nav-cart"
          class="relative h-10 w-10 rounded-xl border border-line2 text-tx2"
        >
          <Icon name="lucide:shopping-cart" class="h-[19px] w-[19px]" />
          <UiTile
            v-if="cartCount > 0"
            size="sm"
            rounded
            class="absolute -right-1.5 -top-1.5 bg-brand font-extrabold text-on-tint"
            data-testid="nav-cart-count"
          >
            {{ cartCount }}
          </UiTile>
        </UiButton>

        <!--
          Signed in and signed out are two different headers, not one header with a
          different label. The session is known during server rendering (`authed`
          is a plain, readable cookie), so this needs no ClientOnly wrapper.
        -->
        <AuroraAccountMenu v-if="isLoggedIn" />

        <template v-else>
          <!--
            A plain anchor, not NuxtLink: under AUTH_MODE=sso this resolves to the
            Nitro route that starts the OIDC flow, and the router would try to
            match it against the client bundle instead of leaving the page.
          -->
          <UiLink
            :href="signInHref"
            tone="inherit"
            :underline="false"
            testid="nav-sign-in"
            class="hidden px-1.5 text-[15px] text-mut hover:text-ac1 sm:inline-flex"
          >
            {{ t('aurora.nav.login') }}
          </UiLink>

          <UiButton
            variant="bare"
            :to="localePath('/hosting')"
            data-testid="nav-start"
            class="hidden whitespace-nowrap rounded-xl bg-brand px-5 py-[11px] text-[15px] font-semibold text-on-tint hover:brightness-110 sm:inline-flex"
          >
            {{ t('aurora.nav.start') }}
          </UiButton>
        </template>

        <UiButton
          variant="bare"
          :title="t('aurora.nav.menu')"
          :aria-label="t('aurora.nav.menu')"
          :aria-expanded="menuOpen"
          data-testid="nav-menu-toggle"
          class="h-10 w-10 rounded-xl border border-line2 text-tx2 xl:hidden"
          @click="menuOpen = !menuOpen"
        >
          <Icon name="lucide:menu" class="h-[19px] w-[19px]" />
        </UiButton>
      </UiStack>
    </UiContainer>
  </header>

  <div
    v-if="menuOpen"
    data-testid="nav-mobile-menu"
    class="relative z-30 border-b border-line bg-panel font-aurora xl:hidden"
  >
    <UiContainer class="pb-5 pt-3">
      <UiStack as="nav" gap="none">
        <UiLink
          v-for="link in navLinks"
          :key="link.key"
          :to="link.to"
          tone="inherit"
          :underline="false"
          :testid="`nav-mobile-${link.key}`"
          class="rounded-xl px-2 py-3 text-base text-tx"
          @click="menuOpen = false"
        >
          {{ link.label }}
        </UiLink>
      </UiStack>
    </UiContainer>
  </div>
</template>

<script setup lang="ts">
/**
 * aurora template site header.
 *
 * Presentation only. The locale menu, colour-mode toggle, apps launcher and
 * account menu are the portal's existing components; this restyles their
 * surroundings rather than reimplementing them.
 *
 * Two things it does read: whether there is a session, which decides between the
 * account menu and the sign-in pair, and the configured sign-in destination.
 */
import AuroraAppLauncher from '~/templates/aurora/layout/AppLauncher.vue'
import AuroraAccountMenu from '~/templates/aurora/layout/AccountMenu.vue'
import { useCartStore } from '~/stores/cart'

/** Props of the header. */
const props = withDefaults(defineProps<{
  /** Which navigation entry to mark as current. */
  active?: string
}>(), { active: '' })

/** The current navigation key as a ref. */
const { active } = toRefs(props)
/** Translator for the active locale. */
const { t } = useI18n()
/** Prefixes a path with the active locale. */
const localePath = useLocalePath()
/** The cart store. */
const cart = useCartStore()

// Where "Sign in" goes is a deployment decision, not a constant: `sso` starts the
// OIDC flow against the platform identity provider, `local` opens this product's
// own form. This header used to hard-code the latter, so an SSO deployment sent
// every visitor to a login page its users have no credentials for.
/** Where the sign-in link goes under the configured auth mode. */
const { signInHref } = useAuthMode()
/** Whether a session exists. */
const { isLoggedIn } = storeToRefs(useAuthStore())

/** Whether the narrow-screen navigation panel is open. */
const menuOpen = ref(false)

/** Number of lines in the cart, shown on the cart button. */
const cartCount = computed(() => cart.items.length)

// Operator-uploaded logo for the current colour mode, with the site name as alt text.
// Empty renders the built-in mark and wordmark below.
/** Uploaded wordmarks and their alt text. */
const { light: logoLight, dark: logoDark, hasDarkVariant: hasDarkLogo, alt: logoAlt } = useBrandLogo()
/** The operator's site name. */
const { name: siteName } = useSiteIdentity()

/** Navigation entries, in display order. */
const navLinks = computed(() => [
  { key: 'hosting', label: t('aurora.nav.hosting'), to: localePath('/hosting') },
  { key: 'domains', label: t('aurora.nav.domains'), to: localePath('/domains') },
  { key: 'services', label: t('aurora.nav.services'), to: localePath('/products') },
  { key: 'process', label: t('aurora.nav.process'), to: localePath('/contact') },
  { key: 'faq', label: t('aurora.nav.faq'), to: localePath('/faq') },
])
</script>
