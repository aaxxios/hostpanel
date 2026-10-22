/**
 * Tailwind configuration: dark mode by class, the brand colour scales and the content globs.
 *
 * @module tailwind.config
 */

import type { Config } from 'tailwindcss'
import plugin from 'tailwindcss/plugin'

export default <Partial<Config>>{
  darkMode: 'class',
  // The Nuxt Tailwind module scans components/, layouts/, pages/ and friends by
  // default. Portal templates deliberately live outside those directories, so
  // without listing templates/ here every utility used only inside a template
  // is silently dropped from the bundle — the markup renders, unstyled.
  content: [
    './components/**/*.{vue,js,ts}',
    './layouts/**/*.{vue,js,ts}',
    './pages/**/*.{vue,js,ts}',
    './plugins/**/*.{js,ts}',
    './composables/**/*.{js,ts}',
    './utils/**/*.{js,ts}',
    './templates/**/*.{vue,js,ts}',
    './app.vue',
    './error.vue',
  ],
  theme: {
    screens: {
      xs: '480px',
      sm: '640px',
      md: '768px',
      lg: '1024px',
      xl: '1280px',
      '2xl': '1536px',
    },
    extend: {
      colors: {
        // The brand scales read the :root variables plugins/brand-tokens.ts emits from
        // portal.brand.primary / .accent, with today's literal as the fallback — an
        // install that sets neither renders exactly what it did. The `r g b` form is
        // what lets the opacity modifier (bg-primary-500/30) keep working.
        primary: {
          50: 'rgb(var(--brand-primary-50, 240 249 255) / <alpha-value>)',
          100: 'rgb(var(--brand-primary-100, 224 242 254) / <alpha-value>)',
          200: 'rgb(var(--brand-primary-200, 186 230 253) / <alpha-value>)',
          300: 'rgb(var(--brand-primary-300, 125 211 252) / <alpha-value>)',
          400: 'rgb(var(--brand-primary-400, 56 189 248) / <alpha-value>)',
          // 500 and up are the contrast-safe fallbacks: white on 500 is 4.9:1, and 600 / 700 as
          // text on white are 5.9:1 / 7.6:1 (the sky ramp's own 500 and 600 were 2.8:1 and 4.1:1).
          // aurora.css repeats the 500 and 600 literals; tests/assets/css/templates/aurora.test.ts
          // fails when the two drift.
          500: 'rgb(var(--brand-primary-500, 2 119 182) / <alpha-value>)',
          600: 'rgb(var(--brand-primary-600, 3 105 161) / <alpha-value>)',
          700: 'rgb(var(--brand-primary-700, 7 89 133) / <alpha-value>)',
          800: 'rgb(var(--brand-primary-800, 12 74 110) / <alpha-value>)',
          900: 'rgb(var(--brand-primary-900, 10 62 94) / <alpha-value>)',
          950: 'rgb(var(--brand-primary-950, 8 47 73) / <alpha-value>)',
        },
        // Text on a filled primary (buttons in classic and the shared components). The plugin
        // decides white or near-black per colour; unset keeps today's white.
        'on-primary': 'rgb(var(--brand-on-primary, 255 255 255) / <alpha-value>)',
        // Text on the brand gradient (`bg-brand`): always white, because the gradient is built from the
        // saturated 600 shades in both colour modes. Not operator-computed, unlike on-primary.
        'on-brand': 'rgb(255 255 255 / <alpha-value>)',
        secondary: {
          50: 'rgb(var(--brand-accent-50, 250 245 255) / <alpha-value>)',
          100: 'rgb(var(--brand-accent-100, 243 232 255) / <alpha-value>)',
          200: 'rgb(var(--brand-accent-200, 233 213 255) / <alpha-value>)',
          300: 'rgb(var(--brand-accent-300, 216 180 254) / <alpha-value>)',
          400: 'rgb(var(--brand-accent-400, 192 132 252) / <alpha-value>)',
          500: 'rgb(var(--brand-accent-500, 168 85 247) / <alpha-value>)',
          600: 'rgb(var(--brand-accent-600, 147 51 234) / <alpha-value>)',
          700: 'rgb(var(--brand-accent-700, 126 34 206) / <alpha-value>)',
          800: 'rgb(var(--brand-accent-800, 107 33 168) / <alpha-value>)',
          900: 'rgb(var(--brand-accent-900, 88 28 135) / <alpha-value>)',
          950: 'rgb(var(--brand-accent-950, 59 7 100) / <alpha-value>)',
        },
        // aurora template design tokens. Each holds a complete colour value,
        // several of them rgba, so Tailwind's opacity modifier cannot decompose
        // them: use border-line, never border-line/50.
        // Kit surfaces (components/ui): a white or dark panel for cards/tables/dialogs and a
        // slightly different one for floating menus. Values live in assets/css/ui.css so the
        // light/dark switch is one place.
        ui: {
          surface: 'var(--ui-surface)',
          popover: 'var(--ui-popover)',
          // The marketing page background (light grey / near-black) and the raised panel that
          // sits on it (white / slightly lighter near-black), so a section never carries a hex.
          page: 'var(--ui-page)',
          raised: 'var(--ui-raised)',
        },
        // classic template surfaces (values in assets/css/templates/classic.css). `r g b`
        // triplets, so the opacity modifier works: bg-classic-surface/95.
        classic: {
          page: 'rgb(var(--classic-page) / <alpha-value>)',
          surface: 'rgb(var(--classic-surface) / <alpha-value>)',
          footer: 'rgb(var(--classic-footer) / <alpha-value>)',
          // Raised or muted panel inside a surface: table heads, alternating bands, input wells.
          'surface-2': 'rgb(var(--classic-surface-2) / <alpha-value>)',
          // Hairlines and card borders.
          border: 'rgb(var(--classic-border) / <alpha-value>)',
          // Body and heading text, and the quieter supporting text.
          ink: 'rgb(var(--classic-ink) / <alpha-value>)',
          muted: 'rgb(var(--classic-muted) / <alpha-value>)',
          // The one solid brand colour, its hover shade, its soft tint and the text drawn on it.
          brand: 'rgb(var(--classic-brand) / <alpha-value>)',
          'brand-hover': 'rgb(var(--classic-brand-hover) / <alpha-value>)',
          'brand-soft': 'rgb(var(--classic-brand-soft) / <alpha-value>)',
          'on-brand': 'rgb(var(--classic-on-brand) / <alpha-value>)',
          'summary-from': 'rgb(var(--classic-summary-from) / <alpha-value>)',
          'summary-to': 'rgb(var(--classic-summary-to) / <alpha-value>)',
        },
        // Third-party channel brand colours used by the floating contact actions.
        channel: {
          whatsapp: '#25D366',
          telegram: '#0088cc',
          chat: '#06b6d4',
        },
        // Text on an aurora tile gradient: always near-black, whatever the colour mode.
        'tile-ink': '#08090F',
        page: 'var(--page)',
        tx: 'var(--tx)',
        tx2: 'var(--tx2)',
        mut: 'var(--mut)',
        mut2: 'var(--mut2)',
        line: 'var(--line)',
        line2: 'var(--line2)',
        // Featured-card ring, a full colour value per mode: use border-line-brand, never border-ac1/25
        // (ac1 is a var(), so the opacity modifier emits no CSS and the border falls back to grey).
        'line-brand': 'var(--line-brand)',
        surf: 'var(--surf)',
        panel: 'var(--panel)',
        input: 'var(--input)',
        ac1: 'var(--ac1)',
        ac2: 'var(--ac2)',
        ac3: 'var(--ac3)',
        acbg: 'var(--ac-bg)',
        ok: 'var(--ok)',
        danger: 'var(--danger)',
        chip: 'var(--chip-bg)',

        // nova template design tokens. Prefixed --n-* and scoped to .tpl-nova
        // (and html[data-template='nova']) so they cannot collide with the
        // aurora tokens above. --n-border is rgba, so the opacity modifier
        // cannot decompose it: use border-nova-border, never /50.
        nova: {
          bg: 'var(--n-bg)',
          surface: 'var(--n-surface)',
          'surface-2': 'var(--n-surface-2)',
          ink: 'var(--n-ink)',
          muted: 'var(--n-muted)',
          brand: 'var(--n-brand)',
          'brand-hover': 'var(--n-brand-hover)',
          'on-brand': 'var(--n-on-brand)',
          'on-accent': 'var(--n-on-accent)',
          accent: 'var(--n-accent)',
          border: 'var(--n-border)',
          success: 'var(--n-success)',
          danger: 'var(--n-danger)',
        },
      },
      backgroundImage: {
        card: 'var(--card)',
        'card-hi': 'var(--card-hi)',
        'hero-grad': 'var(--hero-grad)',
        // The brand gradient, defined per colour mode in assets/css/templates/aurora.css: light
        // tints on the dark theme, the saturated 600 shades on the light one. Reads the operator's
        // accent and primary, falling back to the original purple → blue.
        brand: 'var(--brand-grad)',
        // The closing call-to-action card wash, also per colour mode.
        'cta-card': 'var(--cta-card)',
        glow1: 'radial-gradient(closest-side, var(--glow1), transparent)',
        glow2: 'radial-gradient(closest-side, var(--glow2), transparent)',
        // A radial fade driven by the from/via/to stops of the element, e.g. the vignette of a hero.
        'gradient-radial': 'radial-gradient(circle at center, var(--tw-gradient-stops))',
        // Aurora service and app tiles: fixed two-stop gradients named for the hues they blend,
        // constant across colour modes (they are marks, not surfaces).
        'tile-sky': 'linear-gradient(135deg, #7DE3FF, #5D3FFF)',
        'tile-lilac': 'linear-gradient(135deg, #B9A6FF, #5D3FFF)',
        'tile-mint': 'linear-gradient(135deg, #38E8A0, #00D1FF)',
        'tile-pink': 'linear-gradient(135deg, #FF9CE0, #B9A6FF)',
        'tile-cyan': 'linear-gradient(135deg, #00D1FF, #38E8A0)',
        'tile-indigo': 'linear-gradient(135deg, #5D3FFF, #00D1FF)',
        'tile-ice': 'linear-gradient(135deg, #7DE3FF, #B9A6FF)',
        'tile-rose': 'linear-gradient(135deg, #FF9CE0, #5D3FFF)',
      },
      boxShadow: {
        panel: 'var(--sh)',
        // The resting depth of an aurora content card (assets/css/templates/aurora.css).
        card: 'var(--sh-card)',
        'glow-whatsapp': '0 4px 20px rgba(37, 211, 102, 0.5)',
        'glow-telegram': '0 4px 20px rgba(0, 136, 204, 0.5)',
        'glow-chat': '0 4px 24px rgba(6, 182, 212, 0.5)',
        // Status dots follow the operator's brand colours (fallback: today's literals).
        'dot-primary': '0 0 10px rgb(var(--brand-primary-500, 2 119 182) / 0.5)',
        'dot-secondary': '0 0 10px rgb(var(--brand-accent-500, 139 92 246) / 0.5)',
        'dot-cyan': '0 0 10px rgb(6 182 212 / 0.5)',
        // Cyan halos around featured classic-template panels, smallest to widest.
        'halo-cyan-sm': '0 0 20px rgb(6 182 212 / 0.15)',
        'halo-cyan-md': '0 0 60px rgb(6 182 212 / 0.1)',
        'halo-cyan-lg': '0 0 80px rgb(6 182 212 / 0.15)',
        'halo-violet': '0 0 100px rgb(139 92 246 / 0.15)',
        // Dark drop shadows: a round badge, a glass panel, a floating bar, and the two nova cards.
        'drop-badge': '0 0 30px rgb(0 0 0 / 0.5)',
        'drop-glass': '0 0 50px rgb(0 0 0 / 0.5)',
        'drop-bar': '0 20px 50px rgb(0 0 0 / 0.5)',
        'nova-window': '0 28px 60px -30px rgb(0 0 0 / 0.55)',
        'nova-card': '0 24px 50px -30px rgb(0 0 0 / 0.5)',
      },
      // Marketing motion. `float`, `blob` and `gradient` already exist in assets/styles/animations.css;
      // these are the ones that used to live in a scoped <style> of a single section.
      keyframes: {
        // Entrance reveal: runs in CSS so the content is in the markup and visible without any script.
        'reveal-up': { '0%': { opacity: '0', transform: 'translateY(24px)' }, '100%': { opacity: '1', transform: 'translateY(0)' } },
        sheen: { '0%': { transform: 'translateX(-100%)' }, '100%': { transform: 'translateX(100%)' } },
        marquee: { '0%': { transform: 'translateX(0)' }, '100%': { transform: 'translateX(-50%)' } },
        'ken-burns': { '0%, 100%': { transform: 'scale(1)' }, '50%': { transform: 'scale(1.1)' } },
        flare: {
          '0%': { transform: 'translate(-50%, -50%) rotate(0deg) scale(1)', opacity: '0' },
          '50%': { opacity: '0.5' },
          '100%': { transform: 'translate(50%, 50%) rotate(45deg) scale(2)', opacity: '0' },
        },
        'scroll-hint': {
          '0%, 100%': { transform: 'translateY(0)', opacity: '1' },
          '50%': { transform: 'translateY(15px)', opacity: '0.2' },
        },
        'float-particle': {
          '0%, 100%': { transform: 'translate(0, 0)' },
          '33%': { transform: 'translate(30px, -20px)' },
          '66%': { transform: 'translate(-20px, 40px)' },
        },
      },
      animation: {
        'reveal-up': 'reveal-up 0.5s ease-out backwards',
        sheen: 'sheen 3s linear infinite',
        marquee: 'marquee 30s linear infinite',
        'ken-burns': 'ken-burns 20s ease-in-out infinite',
        flare: 'flare 1.5s ease-out infinite',
        'scroll-hint': 'scroll-hint 2s ease-in-out infinite',
        'float-particle': 'float-particle 10s ease-in-out infinite',
      },
      fontFamily: {
        // Each family reads the operator's typeface from :root first (see
        // plugins/brand-tokens.ts) and falls back to the template's own — sans stays Inter
        // so classic is untouched, aurora stays Noto Sans Armenian. --font-body drives the
        // body families and --font-heading the display ones.
        sans: ['var(--font-body, "Inter Variable", Inter, system-ui, sans-serif)'],
        aurora: ['var(--font-body, "Noto Sans Armenian", system-ui, sans-serif)'],
        display: ['var(--font-heading, "Inter Variable", Inter, "Noto Sans Armenian", system-ui, sans-serif)'],
        // nova is Latin-first, but Inter carries no Armenian coverage, so the
        // hy locale would fall through to whatever the system picked. Noto Sans
        // Armenian sits behind it to cover those glyphs.
        nova: ['var(--font-body, "Inter Variable", Inter, "Noto Sans Armenian", system-ui, sans-serif)'],
        // JetBrains Mono carries no Armenian coverage, so an .հայ domain and the
        // dram sign both fall through to whatever the system picks and render as
        // the wrong glyphs. Noto Sans Armenian sits in the fallback chain to
        // cover them; Latin and digits still come from JetBrains Mono.
        mono: ['JetBrains Mono', 'Noto Sans Armenian', 'ui-monospace', 'monospace'],
      }
    }
  },
  plugins: [
    plugin(function({ addComponents }) {
      addComponents({
        '.container-custom': {
          width: '100%',
          marginLeft: 'auto',
          marginRight: 'auto',
          paddingLeft: '1rem',
          paddingRight: '1rem',
          '@screen sm': {
            maxWidth: '640px',
            paddingLeft: '1.5rem',
            paddingRight: '1.5rem',
          },
          '@screen md': {
            maxWidth: '768px',
          },
          '@screen lg': {
            maxWidth: '1024px',
            paddingLeft: '2rem',
            paddingRight: '2rem',
          },
          '@screen xl': {
            maxWidth: '1280px',
          },
        }
      })
    })
  ]
}
