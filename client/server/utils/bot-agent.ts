/**
 * Recognises requests that are not a person's browser: crawlers, link previews, uptime monitors
 * and command-line clients.
 *
 * Such a client keeps no cookies, so a sign-in probe aimed at it can never finish; it would only
 * be redirected round and round. The silent sign-on middleware leaves them alone.
 *
 * @module server/utils/bot-agent
 */

/** Fragments of the user agents of crawlers, previews, monitors and scripted clients (matched case-insensitively). */
const BOT_AGENT_PATTERN = new RegExp(
  [
    'bot',
    'crawl',
    'spider',
    'slurp',
    'preview',
    'monitor',
    'uptime',
    'pingdom',
    'lighthouse',
    'headless',
    'facebookexternalhit',
    'embedly',
    'curl',
    'wget',
    'python-requests',
    'go-http-client',
    'httpclient',
    'axios',
    'node-fetch',
  ].join('|'),
  'i'
)

/**
 * Whether a user agent belongs to a bot, a preview fetcher, a monitor or a scripted client.
 *
 * @param userAgent - The `User-Agent` header; absent or empty counts as not a browser.
 * @returns True when the client is not a person's browser.
 */
export const isBotUserAgent = (userAgent: string | undefined): boolean =>
  !userAgent || BOT_AGENT_PATTERN.test(userAgent)
