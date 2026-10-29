import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vitest/config'
import type { Plugin } from 'vite'
import { fileURLToPath } from 'node:url'

/**
 * Stands in for the build flags Nuxt injects: the unit environment is a browser (happy-dom), so
 * `import.meta.client` is true, `import.meta.server` false and `import.meta.dev` false. A plain `define` entry is not applied to
 * `import.meta.*` members while vitest serves modules, hence a transform.
 *
 * @returns The Vite plugin.
 */
const nuxtBuildFlags = (): Plugin => ({
  name: 'hostpanel:nuxt-build-flags',
  enforce: 'pre',
  transform(code, id) {
    if (id.includes('node_modules') || !code.includes('import.meta.')) {
      return null
    }
    return code.replace(/import\.meta\.client/g, 'true').replace(/import\.meta\.server/g, 'false').replace(/import\.meta\.dev/g, 'false')
  },
})

export default defineConfig({
  // Compiles `.vue` files so a component's unit test can import it directly. Template comments are
  // dropped as in a production build: kept, a comment above the root element turns the root into a
  // fragment and a wrapper's root-element assertions land on the comment node.
  plugins: [nuxtBuildFlags(), vue({ template: { compilerOptions: { comments: false } } })],
  resolve: {
    alias: {
      '~': fileURLToPath(new URL('.', import.meta.url)),
    },
  },
  test: {
    // Vue prints a one-off notice that `<Suspense>` is experimental whenever a test mounts an async page
    // under it, which is how every page test mounts. It is not a defect and cannot be turned off in Vue, so
    // exactly that line is dropped; every other console message still reaches the report.
    onConsoleLog: (log) => !log.includes('<Suspense> is an experimental feature'),
    setupFiles: ['./vitest.setup.ts'],
    // Two projects over one tree. Tests that cross a real socket (the BFF routes, the transport and the api composables, which
    // talk to a stub server) run in plain Node: under happy-dom the same `fetch` echoes every refused reply and
    // every unreachable host to the console as a browser would, which is noise for a test that provokes them.
    // Everything else mounts components and needs the DOM.
    projects: [
      {
        extends: true,
        test: {
          name: 'dom',
          // happy-dom so component tests can mount; `vitest.setup.ts` stubs the Nuxt auto-imports.
          environment: 'happy-dom',
          // A unit test never fetches a third-party script: a `<script src>` a plugin appends (the tag
          // manager) settles as loaded, as it would in a browser, instead of happy-dom printing a
          // "file loading is disabled" DOMException for a request it was never going to make.
          environmentOptions: { happyDOM: { settings: { handleDisabledFileLoadingAsSuccess: true } } },
          // Tests live in `tests/`, mirroring the source tree; e2e specs belong to Playwright.
          include: ['tests/**/*.test.ts'],
          exclude: ['node_modules/**', '.nuxt/**', '.output/**', 'dist/**', 'e2e/**', 'tests/server/**', 'tests/composables/apis/**', 'tests/composables/useApi.test.ts'],
        },
      },
      {
        extends: true,
        test: {
          name: 'node',
          environment: 'node',
          include: ['tests/server/**/*.test.ts', 'tests/composables/apis/**/*.test.ts', 'tests/composables/useApi.test.ts'],
          exclude: ['node_modules/**', '.nuxt/**', '.output/**', 'dist/**', 'e2e/**'],
        },
      },
    ],
  },
})
