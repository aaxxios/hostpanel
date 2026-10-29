/**
 * Tests for `server/utils/bot-agent.ts`: crawlers, previews, monitors, scripted clients and a
 * missing user agent are not browsers; ordinary browsers are.
 *
 * @module tests/server/utils/bot-agent.test
 */

import { describe, expect, it } from 'vitest'
import { isBotUserAgent } from '~/server/utils/bot-agent'

describe('isBotUserAgent', () => {
  it.each([
    'Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)',
    'Mozilla/5.0 (compatible; bingbot/2.0)',
    'facebookexternalhit/1.1',
    'Slackbot-LinkExpanding 1.0',
    'Mozilla/5.0 Chrome-Lighthouse',
    'UptimeRobot/2.0',
    'Mozilla/5.0 HeadlessChrome/120.0',
    'curl/8.4.0',
    'Wget/1.21',
    'python-requests/2.31',
  ])('treats %s as not a browser', (agent) => {
    expect(isBotUserAgent(agent)).toBe(true)
  })

  it('treats a missing or empty user agent as not a browser', () => {
    expect(isBotUserAgent(undefined)).toBe(true)
    expect(isBotUserAgent('')).toBe(true)
  })

  it.each([
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
    'Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1',
    'Mozilla/5.0 (X11; Linux x86_64; rv:121.0) Gecko/20100101 Firefox/121.0',
  ])('treats a browser (%s) as a browser', (agent) => {
    expect(isBotUserAgent(agent)).toBe(false)
  })
})
