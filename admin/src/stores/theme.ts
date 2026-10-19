/**
 * The operator's colour scheme: light, dark or following the system.
 *
 * The choice lives in localStorage so the inline pre-paint script (`lib/themeBoot.ts`) can apply it
 * before the app loads. This store owns changing it afterwards, and re-applies "system" whenever
 * the operating system switches scheme.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { bootTheme, THEME_BOOT_CONFIG } from '@/lib/themeBoot'
import { AdminTheme } from '@/types/admintheme'

/**
 * Reads the stored choice, treating a missing, unknown or unreadable value as "system".
 *
 * @returns The operator's choice.
 */
const readStoredTheme = (): AdminTheme => {
  try {
    const stored = window.localStorage.getItem(THEME_BOOT_CONFIG.storageKey)
    const known = Object.values(AdminTheme).find(theme => theme === stored)
    return known ?? AdminTheme.System
  } catch {
    return AdminTheme.System
  }
}

/**
 * Persists a choice. Storage that throws only costs the choice surviving a reload.
 *
 * @param theme - The choice to store.
 */
const writeStoredTheme = (theme: AdminTheme): void => {
  try {
    window.localStorage.setItem(THEME_BOOT_CONFIG.storageKey, theme)
  } catch {
    // Private mode or blocked site data: the choice still applies for this page.
  }
}

/** Pinia store holding the theme choice and applying it to the document. */
export const useThemeStore = defineStore('theme', () => {
  /** The operator's choice. */
  const choice = ref<AdminTheme>(readStoredTheme())

  /** True when the light palette is applied right now. */
  const isLight = ref(false)

  /** The system scheme query, kept so its listener is added only once. */
  let media: MediaQueryList | null = null

  /** Re-applies the stored choice to the document. */
  const apply = (): void => {
    isLight.value = bootTheme(THEME_BOOT_CONFIG)
  }

  /** Applies the choice and starts following system scheme changes. Safe to call twice. */
  const init = (): void => {
    apply()
    if (media === null && typeof window.matchMedia === 'function') {
      media = window.matchMedia(THEME_BOOT_CONFIG.lightQuery)
      // Only "system" depends on it, but bootTheme re-reads the choice, so one listener is enough.
      media.addEventListener('change', apply)
    }
  }

  /**
   * Switches to a theme, stores it and applies it.
   *
   * @param theme - The new choice.
   */
  const setTheme = (theme: AdminTheme): void => {
    choice.value = theme
    writeStoredTheme(theme)
    apply()
  }

  return { choice, isLight, init, setTheme }
})
