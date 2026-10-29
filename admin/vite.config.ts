import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'
import type { Plugin } from 'vite'
// The extension is spelled out because Vite's native config loader runs this file under Node,
// which resolves a relative import only by its full file name.
import { themeBootScript } from './src/lib/themeBoot.ts'

/**
 * Inlines the theme pre-paint script at the end of `<head>`, after the `theme-color` metas it
 * rewrites, so the stored light/dark choice is on `<html>` before the first paint.
 *
 * @returns The Vite plugin.
 */
const themeBoot = (): Plugin => ({
  name: 'admin-theme-boot',
  transformIndexHtml: () => [{ tag: 'script', children: themeBootScript(), injectTo: 'head' }],
})

export default defineConfig({
  // Relative: the deployed base path is set at container start (<base href> + config.js).
  base: './',
  plugins: [
    vue(),
    tailwindcss(),
    themeBoot(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  build: {
    // Every route is already a lazy chunk and the country atlas is imported from its
    // `/lite` entry, so the only chunk over the default 500 kB is the world-map outline
    // data (@svg-maps/world): one 1.2 MB object literal of SVG paths that cannot be cut
    // into pieces. It loads only on the clients-by-country report, in its own named chunk.
    // The limit sits just above it so any NEW oversized chunk still warns.
    chunkSizeWarningLimit: 1300,
    rolldownOptions: {
      output: {
        codeSplitting: {
          groups: [{ name: 'world-map', test: /@svg-maps.world/ }],
        },
      },
    },
  },
  server: {
    // Extra names the dev server answers to besides localhost, from the environment (ADMIN_DEV_ALLOWED_HOSTS,
    // comma separated): a deployment's own host names are not written into the repository.
    allowedHosts: (process.env.ADMIN_DEV_ALLOWED_HOSTS ?? '').split(',').map((h) => h.trim()).filter(Boolean),
    proxy: {
      '/api': {
        target: process.env.API_PROXY_TARGET || 'http://localhost:5148',
        changeOrigin: true,
      },
      '/uploads': {
        target: process.env.API_PROXY_TARGET || 'http://localhost:5148',
        changeOrigin: true,
      },
    },
  },
})
