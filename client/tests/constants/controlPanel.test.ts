/**
 * Unit tests for `constants/controlPanel.ts`.
 *
 * @module tests/constants/controlPanel.test
 */

import { describe, expect, it } from 'vitest'
import { CONTROL_PANEL_PORT, WEBMAIL_PORT } from '~/constants/controlPanel'

describe('control panel constants', () => {
  it('are valid TCP ports', () => {
    for (const port of [CONTROL_PANEL_PORT, WEBMAIL_PORT]) {
      expect(Number.isInteger(port)).toBe(true)
      expect(port).toBeGreaterThan(0)
      expect(port).toBeLessThan(65536)
    }
  })

  it('give the panel and the webmail different ports', () => {
    expect(CONTROL_PANEL_PORT).not.toBe(WEBMAIL_PORT)
  })
})
