/**
 * Pins the one-variable-name rule for the OIDC client secret: the client portal receives the
 * credential the API already reads, so every compose file feeds `NUXT_SSO_CLIENT_SECRET` from
 * `CLIENT_HOSTPANEL_SECRET`, and no second spelling (`SSO_CLIENT_SECRET`) survives anywhere.
 *
 * The compose files live outside `client/`; the checks are skipped when the repository's `docker/`
 * folder is not mounted next to the client (for example in a client-only image).
 *
 * @module tests/config/clientSecretEnv.test
 */

import { existsSync, readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { describe, expect, it } from 'vitest'

/** The client root. */
const CLIENT_ROOT = resolve(import.meta.dirname, '..', '..')

/** The repository's compose folder. */
const DOCKER_DIR = resolve(CLIENT_ROOT, '..', 'docker')

/** Compose files that start the client portal with the SSO secret. */
const CLIENT_COMPOSE_FILES = ['docker-compose.dev.yml', 'docker-compose.prod.yml', 'docker-compose.hub.yml'] as const

/**
 * Reads a file as text.
 *
 * @param path - Absolute path.
 * @returns The file contents.
 */
const read = (path: string): string => readFileSync(path, 'utf8')

describe('client secret environment mapping', () => {
  it('runtime config leaves the secret to the NUXT_ override instead of a second variable', () => {
    const config = read(resolve(CLIENT_ROOT, 'nuxt.config.ts'))

    // The NUXT_ spelling may appear (in the comment explaining the mapping); the bare one may not.
    expect(config).not.toMatch(/(?<![A-Z_])SSO_CLIENT_SECRET/)
    expect(config).toMatch(/ssoClientSecret:\s*''/)
  })

  it('the client .env.example documents NUXT_SSO_CLIENT_SECRET only', () => {
    const example = read(resolve(CLIENT_ROOT, '.env.example'))

    expect(example).toMatch(/^NUXT_SSO_CLIENT_SECRET=/m)
    expect(example).not.toMatch(/^SSO_CLIENT_SECRET=/m)
  })

  describe.skipIf(!existsSync(DOCKER_DIR))('compose files', () => {
    it.each(CLIENT_COMPOSE_FILES)('%s feeds NUXT_SSO_CLIENT_SECRET from CLIENT_HOSTPANEL_SECRET', (file) => {
      const compose = read(resolve(DOCKER_DIR, file))

      // The production stacks run in local auth mode too, where no sign-on secret exists, so they
      // read the variable with an empty default (`:-`); the name is what this pins, not whether it is set.
      expect(compose).toMatch(/NUXT_SSO_CLIENT_SECRET:\s*\$\{CLIENT_HOSTPANEL_SECRET(:-)?\}/)
      expect(compose).not.toMatch(/(?<![A-Z_])SSO_CLIENT_SECRET/)
    })

    it('docker/.env.example has no separate client secret', () => {
      const example = read(resolve(DOCKER_DIR, '.env.example'))

      expect(example).toMatch(/^CLIENT_HOSTPANEL_SECRET=/m)
      expect(example).not.toMatch(/SSO_CLIENT_SECRET/)
    })
  })
})
