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
          500: 'rgb(var(--brand-primary-500, 14 165 233) / <alpha-value>)',
          600: 'rgb(var(--brand-primary-600, 2 132 199) / <alpha-value>)',
          700: 'rgb(var(--brand-primary-700, 3 105 161) / <alpha-value>)',
          800: 'rgb(var(--brand-primary-800, 7 89 133) / <alpha-value>)',
          900: 'rgb(var(--brand-primary-900, 12 74 110) / <alpha-value>)',
          950: 'rgb(var(--brand-primary-950, 8 47 73) / <alpha-value>)',
        },
        // Text on a filled primary (buttons in classic and the shared components) and on
        // the lighter tint aurora's gradient and nova's dark-mode brand are built from. The
        // plugin decides white or near-black per colour; unset keeps today's white / near-black.
        'on-primary': 'rgb(var(--brand-on-primary, 255 255 255) / <alpha-value>)',
        'on-tint': 'rgb(var(--brand-on-tint, 8 9 15) / <alpha-value>)',
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
        surf: 'var(--surf)',
        panel: 'var(--panel)',
        input: 'var(--input)',
        ac1: 'var(--ac1)',
        ac2: 'var(--ac2)',
        ac3: 'var(--ac3)',
        acbg: 'var(--ac-bg)',
        ok: 'var(--ok)',
        danger: 'var(--danger)',

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
        // Constant across both colour modes — the brand mark, not a theme token. Reads the
        // operator's accent and primary, falling back to the original purple → cyan.
        brand: 'linear-gradient(135deg, rgb(var(--brand-accent-500, 93 63 255)), rgb(var(--brand-primary-400, 0 209 255)))',
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
        'glow-whatsapp': '0 4px 20px rgba(37, 211, 102, 0.5)',
        'glow-telegram': '0 4px 20px rgba(0, 136, 204, 0.5)',
        'glow-chat': '0 4px 24px rgba(6, 182, 212, 0.5)',
        // Status dots follow the operator's brand colours (fallback: today's literals).
        'dot-primary': '0 0 10px rgb(var(--brand-primary-500, 14 165 233) / 0.5)',
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
