/**
 * Nuxt configuration: rendering rules per route, runtime config, modules and the head defaults.
 *
 * @see https://nuxt.com/docs/api/configuration/nuxt-config
 * @module nuxt.config
 */

export default defineNuxtConfig({
  /**
   * Development only: render in the browser instead of on the server.
   *
   * The dev server's SSR transform runs in a vite-node worker that talks to Nitro over a
   * unix socket, and on this Docker-for-Windows setup that exchange never completes for this
   * app: every server-rendered route either hangs or answers 500 "IPC connection closed",
   * while API routes, static assets and the `ssr: false` pages under /client all work. It is
   * not the application code — the same routes render correctly in production, and the
   * failure reproduces on an unmodified checkout.
   *
   * Turning SSR off in development trades away one thing — you cannot see server-rendered
   * output locally — to get back every page, which otherwise cannot be opened at all.
   * Production is untouched: `$development` applies only to `nuxt dev`.
   *
   * Remove this once the dev-server issue is fixed, and verify by loading any public page.
   */
  $development: {
    ssr: false,
  },

  compatibilityDate: '2026-02-04',
  devtools: { enabled: true },

  // Client area pages are auth-gated and fetch data client-side only — disable SSR
  // to avoid hydration mismatches (server renders empty state, client renders content)
  // X-Robots-Tag: noindex prevents search engines from indexing private pages
  routeRules: {
    // Immutable hashed assets — cache for 1 year
    '/_nuxt/**': { headers: { 'Cache-Control': 'public, max-age=31536000, immutable' } },
    // Public static assets — cache for 30 days
    '/images/**': { headers: { 'Cache-Control': 'public, max-age=2592000, stale-while-revalidate=86400' } },
    '/fonts/**': { headers: { 'Cache-Control': 'public, max-age=31536000, immutable' } },
    // Auth-gated SPA pages — no SSR, no indexing
    '/client/**': { ssr: false, headers: { 'X-Robots-Tag': 'noindex, nofollow' } },
    '/cart/**': { ssr: false, headers: { 'X-Robots-Tag': 'noindex, nofollow' } },
    '/checkout/**': { ssr: false, headers: { 'X-Robots-Tag': 'noindex, nofollow' } },
    // The hosted-gateway returnUrl landing page. It belongs to this group for the same two
    // reasons: it has nothing to server-render (the payment outcome is only known after the
    // on-mount verification call), and a page whose URL carries an invoice/order id must
    // never be indexed. Omitting it is not cosmetic — it was the one page left server-
    // rendered, and SSR of it returned 500 in production, breaking every payer's return
    // from the bank. Development runs with `ssr: false` globally (see the note at the top
    // of this file), so this can only ever be caught against a production build.
    '/payment/**': { ssr: false, headers: { 'X-Robots-Tag': 'noindex, nofollow' } },
    // The live-chat SDK (app.vue, chat.innovayse.com) draws its launcher bubble in
    // *this* page and points the inbox icon at a root-relative `/brand-assets/...`, so
    // the icon was requested from this origin, 404'd into the Vue Router, and left a
    // broken bubble plus a router warning on every public page. Proxy the path to the
    // chat host the widget is configured against — the same rule innovayse-main carries
    // — so the icon stays whatever is set there instead of a copy here that would drift.
    // Caching is whatever the chat host sends; a `headers` override here would be dead
    // config, since a proxied response keeps the upstream's.
    '/brand-assets/**': { proxy: 'https://chat.innovayse.com/brand-assets/**' },
  },

  nitro: {
    // Compress static assets with gzip and brotli at build time
    compressPublicAssets: { gzip: true, brotli: true },
    // Minify server-side HTML output
    minify: true,
    // The error handlers are assembled in the `nitro:config` hook below: setting
    // `errorHandler` here would make Nuxt skip its own handler, the one that renders error.vue.
  },

  hooks: {
    // Nitro's built-in handler drops an H3Error's `data`, which is where the C# backend's
    // machine-readable error code travels. server/error.ts adds it back for /api/* and
    // answers nothing else. See the file header for why the code has to reach the browser at all.
    //
    // It goes in FRONT of the handler Nuxt registers (the one that renders error.vue) rather
    // than replacing it: Nitro runs the chain in order and stops at the first handler that
    // responds, so a page error falls through to Nuxt's handler and the branded error page is
    // rendered. Replacing it left every 404 on Nitro's bare fallback template, which also pulls
    // its stylesheet from a public CDN.
    'nitro:config': (nitroConfig) => {
      nitroConfig.errorHandler = ['~/server/error', ...[nitroConfig.errorHandler ?? []].flat()]
    }
  },
  modules: [
    /**
     * Inline module: drops the `vue-router/volar/sfc-route-blocks` Volar plugin that Nuxt adds to the
     * generated tsconfig. vue-router 4.6 no longer exports that path, so vue-tsc printed
     * `Resolve plugin path failed ... ERR_PACKAGE_PATH_NOT_EXPORTED` on every typecheck. The plugin only
     * type-checks `<route>` blocks, which this app does not use.
     */
    (_options, nuxt) => {
      // Registered once every module is installed, so this handler runs after the one that adds the plugin.
      nuxt.hook('modules:done', () => {
        nuxt.hook('prepare:types', ({ tsConfig }) => {
          const options = tsConfig.vueCompilerOptions
          if (options?.plugins) {
            options.plugins = options.plugins.filter((plugin: unknown) => plugin !== 'vue-router/volar/sfc-route-blocks')
          }
        })
      })
    },
    '@nuxt/eslint', '@nuxt/icon', '@nuxt/image', '@nuxtjs/tailwindcss', '@nuxtjs/i18n', '@nuxtjs/sitemap', 'nuxt-swiper', '@pinia/nuxt'],

  image: {
    quality: 80,
    format: ['webp'],
    // No `loading: 'lazy'` here. It sat in this block claiming to set a default for every
    // NuxtImg, but `@nuxt/image`'s ModuleOptions has no such key -- the module ignored it and
    // the comment described behaviour nobody had. Lazy loading is per-component: a `<NuxtImg>`
    // that should defer needs its own `loading="lazy"`, as the eager ones already spell out
    // `loading="eager"`.
    screens: {
      xs: 320,
      sm: 640,
      md: 768,
      lg: 1024,
      xl: 1280,
      '2xl': 1536
    }
  },

  css: ['~/assets/css/main.css', '@fontsource-variable/inter', '~/assets/styles/animations.css', '~/assets/styles/global.css'],

  runtimeConfig: {
    // Server-side only. No credential for an external service belongs here — only this
    // BFF's own OIDC client credential, internal addresses, and the auth-mode switch.
    authMode: process.env.AUTH_MODE || 'sso',
    apiUrl: process.env.API_URL || 'http://localhost:5000',
    ssoUrl: process.env.SSO_URL || 'http://sso-api:8080',
    ssoClientId: process.env.SSO_CLIENT_ID || 'hostpanel',
    ssoClientSecret: process.env.SSO_CLIENT_SECRET || '',
    ssoCallbackUrl: process.env.SSO_CALLBACK_URL || 'http://panel.innovayse.local/api/portal/auth/sso/callback',
    // Internal URL of the main portal, used for backchannel logout delegation; empty skips the call
    mainApiUrl: process.env.NUXT_MAIN_API_URL || '',

    // Public runtime config (exposed to client)
    public: {
      authMode: process.env.AUTH_MODE || 'sso',
      baseUrl: process.env.NUXT_PUBLIC_BASE_URL || '',
      ssoPublicUrl: process.env.SSO_PUBLIC_URL || 'http://sso.innovayse.local',
      tasksUrl: process.env.NUXT_PUBLIC_TASKS_URL || 'http://tasks.innovayse.local',
      erpUrl: process.env.NUXT_PUBLIC_ERP_URL || 'http://erp.innovayse.local',
      sheetsUrl: process.env.NUXT_PUBLIC_SHEETS_URL || 'http://sheets.innovayse.local',
      emailUrl: process.env.NUXT_PUBLIC_EMAIL_URL || 'http://email.innovayse.local',
      driveUrl: process.env.NUXT_PUBLIC_DRIVE_URL || 'http://drive.innovayse.local',
      // Address of the billing-portal install this panel hands visitors off to (card
      // management, invoice payment, domain transfer). An address only — the legacy billing
      // API credentials are the C# backend's and deliberately absent here. Empty
      // hides every link that needs it rather than rendering a broken one.
      //
      // The trailing-slash strip below only runs where nuxt.config is evaluated: a
      // dev server, or the `docker build` that freezes the production defaults. A
      // deployed container overrides this through NUXT_PUBLIC_BILLING_PORTAL_URL, which Nitro
      // substitutes verbatim, so that value must carry no trailing slash of its own.
      billingPortalUrl: (process.env.BILLING_PORTAL_URL || '').replace(/\/+$/, ''),
      // Stripe.js's publishable key, read by useStripe(). Public by design; the
      // secret key is the backend's. Overridden at runtime with
      // NUXT_PUBLIC_STRIPE_PUBLISHABLE_KEY — empty leaves the card form uninitialised.
      stripePublishableKey: process.env.STRIPE_PUBLISHABLE_KEY || '',
      mainUrl: process.env.NUXT_PUBLIC_MAIN_URL || 'http://app.innovayse.local',
      // Active portal template. Phase 4 lets an admin override this from settings;
      // until then it is the only switch. Unknown values fall back to 'aurora'.
      portalTemplate: process.env.NUXT_PUBLIC_PORTAL_TEMPLATE || 'aurora',
      // The header logos and browser tab icon, settable from the admin panel.
      // Empty renders the template's built-in mark/wordmark and static favicon;
      // useBrandLogo() picks the dark variant in dark mode and falls back to the
      // light one, and the mark where a wordmark does not fit.
      portalLogo: process.env.NUXT_PUBLIC_PORTAL_LOGO || '',
      portalLogoDark: process.env.NUXT_PUBLIC_PORTAL_LOGO_DARK || '',
      portalLogoMark: process.env.NUXT_PUBLIC_PORTAL_LOGO_MARK || '',
      portalFavicon: process.env.NUXT_PUBLIC_PORTAL_FAVICON || '',
      // Colour mode for a visitor with no saved choice (light | dark | system) and
      // whether the sun/moon switch is shown. 'dark' is what the site did before the
      // setting existed. Resolution lives in utils/colorMode.ts.
      portalThemeDefault: process.env.NUXT_PUBLIC_PORTAL_THEME_DEFAULT || 'dark',
      portalThemeUserToggle: process.env.NUXT_PUBLIC_PORTAL_THEME_USER_TOGGLE || '',
      // What the site calls itself: og:site_name, logo alt text, structured data.
      // Empty falls back to the built-in name in useSiteIdentity().
      portalSiteName: process.env.NUXT_PUBLIC_PORTAL_SITE_NAME || '',
      portalSiteTagline: process.env.NUXT_PUBLIC_PORTAL_SITE_TAGLINE || '',
      // Brand colours (#rrggbb) and typefaces; empty keeps every template's own. Read by
      // plugins/brand-tokens.ts, which derives the whole scale from the one colour.
      portalBrandPrimary: process.env.NUXT_PUBLIC_PORTAL_BRAND_PRIMARY || '',
      portalBrandAccent: process.env.NUXT_PUBLIC_PORTAL_BRAND_ACCENT || '',
      portalBrandFontHeading: process.env.NUXT_PUBLIC_PORTAL_BRAND_FONT_HEADING || '',
      portalBrandFontBody: process.env.NUXT_PUBLIC_PORTAL_BRAND_FONT_BODY || '',
      // Tracking and live chat. Nothing loads until these are set — and the tag only after
      // the visitor accepts all cookies (plugins/tracking.client.ts).
      portalGtmId: process.env.NUXT_PUBLIC_PORTAL_GTM_ID || '',
      portalChatBaseUrl: process.env.NUXT_PUBLIC_PORTAL_CHAT_BASE_URL || '',
      portalChatWebsiteToken: process.env.NUXT_PUBLIC_PORTAL_CHAT_WEBSITE_TOKEN || '',
      portalChatWebsiteTokenRu: process.env.NUXT_PUBLIC_PORTAL_CHAT_WEBSITE_TOKEN_RU || '',
      portalChatWebsiteTokenHy: process.env.NUXT_PUBLIC_PORTAL_CHAT_WEBSITE_TOKEN_HY || '',
      // Header app launcher. Off unless a deployment actually runs the sibling
      // apps it links to; every app URL below has a development default, so
      // presence of a URL cannot decide this on its own.
      portalAppsEnabled: process.env.NUXT_PUBLIC_PORTAL_APPS_ENABLED || '',
      // Contact and widget configuration. Each is hidden when empty — a fresh
      // install with none of these set still renders a complete site.
      portalWhatsapp: process.env.NUXT_PUBLIC_PORTAL_WHATSAPP || '',
      portalTelegram: process.env.NUXT_PUBLIC_PORTAL_TELEGRAM || '',
      portalChatProvider: process.env.NUXT_PUBLIC_PORTAL_CHAT_PROVIDER || '',
      portalNewsletterUrl: process.env.NUXT_PUBLIC_PORTAL_NEWSLETTER_URL || '',
      portalContactEmail: process.env.NUXT_PUBLIC_PORTAL_CONTACT_EMAIL || '',
      portalSocialFacebook: process.env.NUXT_PUBLIC_PORTAL_SOCIAL_FACEBOOK || '',
      portalSocialInstagram: process.env.NUXT_PUBLIC_PORTAL_SOCIAL_INSTAGRAM || '',
      portalSocialLinkedin: process.env.NUXT_PUBLIC_PORTAL_SOCIAL_LINKEDIN || '',
      portalSocialYoutube: process.env.NUXT_PUBLIC_PORTAL_SOCIAL_YOUTUBE || '',
      portalSocialTwitter: process.env.NUXT_PUBLIC_PORTAL_SOCIAL_TWITTER || '',
      // Address of the hosting product demo, linked from the home products section; empty hides the link.
      portalHostingDemoUrl: process.env.NUXT_PUBLIC_PORTAL_HOSTING_DEMO_URL || '',
      portalContactPhone: process.env.NUXT_PUBLIC_PORTAL_CONTACT_PHONE || '',
      // Support working hours and their time zone, shown on the contact page; each is hidden when empty.
      portalContactHours: process.env.NUXT_PUBLIC_PORTAL_CONTACT_HOURS || '',
      portalContactTimezone: process.env.NUXT_PUBLIC_PORTAL_CONTACT_TIMEZONE || '',
      portalLegalTaxId: process.env.NUXT_PUBLIC_PORTAL_LEGAL_TAX_ID || '',
    }
  },

  i18n: {
    // The module resolves `vueI18n` inside its restructure directory (`i18n/` by default); this app keeps
    // the file at the project root, so the directory is the root itself.
    restructureDir: '.',
    vueI18n: './i18n.config.ts',
    // Locale messages are loaded dynamically by plugins/i18n.ts via setLocaleMessage.
    // No langDir/files needed here — the plugin handles all locale JSON imports.
    locales: [
      { code: 'en', name: 'English', language: 'en-US' },
      { code: 'ru', name: 'Русский', language: 'ru-RU' },
      { code: 'hy', name: 'Հայերեն', language: 'hy-AM' }
    ],
    defaultLocale: 'en',
    strategy: 'prefix_except_default',
    detectBrowserLanguage: false
  },

  // Canonical origin for generated URLs. @nuxtjs/sitemap v7 reads this rather
  // than its own `hostname` option, which earlier versions used and which was
  // still set here — silently ignored, so the sitemap advertised whatever host
  // the request arrived on, down to the container's own address.
  //
  // Deliberately no default: with nothing set, nuxt-site-config keeps falling
  // back to the request host, which is at least the operator's own domain. A
  // literal here would put this deployment's domain in every other operator's
  // sitemap.
  site: {
    url: process.env.NUXT_PUBLIC_BASE_URL,
  },

  // Sitemap configuration
  sitemap: {
    exclude: [
      '/admin/**',
      '/_nuxt/**'
    ],
    defaults: {
      changefreq: 'weekly' as const,
      priority: 0.7,
      lastmod: new Date().toISOString()
    },
    urls: async () => {
      // The module accepts only these priorities and change frequencies, so the list is typed
      // rather than left to widen to `number`.
      const routes: Array<{
        loc: string
        changefreq: 'daily' | 'weekly' | 'monthly' | 'yearly'
        priority: 0 | 0.1 | 0.2 | 0.3 | 0.4 | 0.5 | 0.6 | 0.7 | 0.8 | 0.9 | 1
      }> = [
        // Main pages
        { loc: '/', changefreq: 'daily' as const, priority: 1.0 },
        { loc: '/products', changefreq: 'weekly' as const, priority: 0.9 },
        // The sitemap module discovers both automatically; these entries only
        // raise them from the default 0.7 to the 0.9 the storefront deserves.
        { loc: '/hosting', changefreq: 'weekly' as const, priority: 0.9 },
        { loc: '/domains', changefreq: 'weekly' as const, priority: 0.9 },
        { loc: '/contact', changefreq: 'monthly' as const, priority: 0.7 },
        { loc: '/faq', changefreq: 'monthly' as const, priority: 0.6 },
        { loc: '/terms', changefreq: 'yearly' as const, priority: 0.3 },
        { loc: '/privacy', changefreq: 'yearly' as const, priority: 0.3 },
        { loc: '/cookie-policy', changefreq: 'yearly' as const, priority: 0.3 },
        { loc: '/refund-policy', changefreq: 'yearly' as const, priority: 0.3 },
        { loc: '/acceptable-use', changefreq: 'yearly' as const, priority: 0.3 }
      ]

      // Individual product pages (SaaS families)
      const productSlugs = ['smartlearn-system', 'propsystem-pro', 'shopkit-pro', 'metricskit-pro', 'quickbite', 'elpida-ai', 'taskero']
      productSlugs.forEach(slug => {
        routes.push({ loc: `/products/${slug}`, changefreq: 'weekly' as const, priority: 0.8 })
      })

      return routes
    }
  },

  build: {
    transpile: ['lucide-vue-next']
  },

  vite: {
    server: {
      allowedHosts: ['panel.innovayse.local', 'panel-admin.innovayse.local']
    },
    ssr: {
      // Force lucide-vue-next to be bundled into the SSR output (not treated as external)
      // This prevents named export resolution issues (e.g. "Server is not defined")
      noExternal: ['lucide-vue-next']
    },
    build: {
      rolldownOptions: {
        // Production builds drop console.* calls and debugger statements. Vite 8 minifies with the Oxc
        // minifier and ignores the old `esbuild.drop` option (it printed a warning saying so), so the
        // same rule is stated for the minifier itself.
        output: process.env.NODE_ENV === 'production'
          ? { minify: { compress: { dropConsole: true, dropDebugger: true } } }
          : {},
        checks: {
          // The "plugin timings" notice reports that Nuxt's own transform plugins (keyed-functions,
          // imports-transform, the Vue compiler) take most of the build time. They are Nuxt internals this
          // project cannot make faster, so the notice carries no action and is turned off.
          bundlerTimings: false
        }
      }
    }
  },

  postcss: {
    plugins: {
      // The Tailwind module adds this nesting plugin for Tailwind 3, but Nuxt cannot load it as an ES
      // module (the package exposes it as CommonJS only) and printed a warning on every build while
      // skipping it. Nothing in the stylesheets uses nested rules, so it is switched off explicitly.
      'tailwindcss/nesting': false
    }
  },

  app: {
    // Set NUXT_APP_CDN_URL env var in production to serve assets from CDN
    cdnURL: process.env.NUXT_APP_CDN_URL || '',
    head: {
      htmlAttrs: {
        lang: 'en'
      },
      // Brand-free defaults: app.vue adds the operator's site name (og:site_name, author) at request time.
      title: 'Web Hosting, Domains and Servers | Client Portal',
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1, viewport-fit=cover' },
        { name: 'description', content: 'Web hosting, domains, VPS and dedicated servers with online billing and a self-service client portal to manage services, invoices and support.' },
        { name: 'keywords', content: 'web hosting, domain registration, VPS, dedicated servers, SSL certificates, client portal, online billing' },
        { name: 'robots', content: 'index, follow' },

        // Open Graph / Facebook
        { property: 'og:type', content: 'website' },
        { property: 'og:title', content: 'Web Hosting, Domains and Servers' },
        { property: 'og:description', content: 'Hosting, domains and servers with online billing and a client portal' },
        { property: 'og:image', content: '/og-image.jpg' },
        { property: 'og:locale', content: 'en_US' },
        { property: 'og:locale:alternate', content: 'ru_RU' },
        { property: 'og:locale:alternate', content: 'hy_AM' },
        // og:site_name and author come from app.vue, where the operator's site name is known.

        // Twitter Card
        { name: 'twitter:card', content: 'summary_large_image' },
        { name: 'twitter:title', content: 'Web Hosting, Domains and Servers' },
        { name: 'twitter:description', content: 'Hosting, domains and servers with online billing and a client portal' },
        { name: 'twitter:image', content: '/og-image.jpg' },

        // Additional SEO
        { name: 'format-detection', content: 'telephone=no' },
        { name: 'theme-color', content: '#0ea5e9' }
      ],
      // The portal header widget's <script> is NOT here: process.env is read at build time,
      // so it would bake the builder's URL into the image. plugins/header-widget.ts adds it
      // from runtimeConfig instead.
      link: [
        // No preconnect to a tag or analytics host here: whether either is contacted at
        // all is the operator's setting plus the visitor's consent (plugins/tracking.client.ts).
        // The favicon links are NOT here. app.vue emits them through brandingIcons(),
        // built-in defaults included, because unhead only dedupes link tags that share an
        // explicit key -- so a static link here and an uploaded one there both render, and
        // Chrome picks the SVG. An operator who uploaded a favicon would keep seeing the
        // built-in one. One owner instead of two.
        { rel: 'manifest', href: '/site.webmanifest' }
      ]
    }
  }
})