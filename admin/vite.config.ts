import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  // Relative: the deployed base path is set at container start (<base href> + config.js).
  base: './',
  plugins: [
    vue(),
    tailwindcss(),
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
    allowedHosts: ['panel-admin.innovayse.local', 'stage-panel-admin.innovayse.com'],
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
