/**
 * Unit tests for `utils/controlPanel.ts`.
 *
 * @module tests/utils/controlPanel.test
 */

import { describe, expect, it } from 'vitest'
import { controlPanelUrl, webmailUrl, websiteUrl } from '~/utils/controlPanel'

describe('control panel addresses', () => {
  it('builds the secure panel sign-in address with the panel port', () => {
    expect(controlPanelUrl('srv.test')).toBe('https://srv.test:2083')
  })

  it('builds the secure webmail address with the webmail port', () => {
    expect(webmailUrl('srv.test')).toBe('https://srv.test:2096')
  })

  it('builds the secure website address without a port', () => {
    expect(websiteUrl('example.com')).toBe('https://example.com')
  })
})
