/**
 * Playwright flows against `vite preview` of a real production build. The Hostpanel API is
 * never running: every spec stubs it with `page.route` (see `e2e/support/stubApi.ts`), so the
 * panel is exercised exactly as the browser sees it — same-origin `/api/*` calls — and the suite
 * runs without a backend, a database or a sign-on service.
 */
import { defineConfig, devices } from '@playwright/test'

/** Where `vite preview` serves the built SPA during the run. */
const previewUrl = 'http://127.0.0.1:4173'

export default defineConfig({
  testDir: 'e2e',
  fullyParallel: true,
  forbidOnly: Boolean(process.env['CI']),
  retries: 0,
  // The first navigation to a cold preview inside a loaded container is slow; the 30 s default
  // fails runs that share the machine with the unit suite.
  timeout: 90_000,
  // Same reason: the first assertion of a worker waits for a cold lazy route chunk to load, which
  // the 5 s default cannot cover on a busy machine.
  expect: { timeout: 20_000 },
  reporter: 'list',
  use: {
    baseURL: previewUrl,
    trace: 'retain-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: {
    // `build-only` skips the type-check half of `npm run build`: the type gate is its own CI step.
    command: 'npm run build-only && npm run preview -- --host 127.0.0.1 --port 4173 --strictPort',
    url: previewUrl,
    // Locally a preview that is already running is reused; CI always builds fresh.
    reuseExistingServer: !process.env['CI'],
    // A production build inside a busy container can take minutes.
    timeout: 600_000,
  },
})
