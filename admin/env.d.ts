/// <reference types="vite/client" />

import type { AppRuntimeConfig } from '@/types/runtimeConfig'

declare global {
  interface Window {
    /** Runtime configuration written by `config.js`; absent when the file is not loaded. */
    __APP_CONFIG__?: Partial<AppRuntimeConfig>
  }
}

export {}
