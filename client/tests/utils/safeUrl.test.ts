/**
 * Unit tests for the shared URL check: the scheme allow-list `isSafeUrl` applies and the
 * http/https-or-relative rule of `safeHref`.
 *
 * @module tests/utils/safeUrl.test
 */
import { describe, expect, it } from 'vitest'
import { isSafeUrl, safeHref } from '~/utils/safeUrl'

describe('isSafeUrl', () => {
  const links = new Set(['http', 'https', 'mailto', 'tel'])

  it.each([['https://example.com'], ['http://example.com'], ['mailto:a@b.co'], ['tel:+123'], ['/path'], ['#anchor'], ['?q=1'], ['page.html']])(
    'accepts %s',
    (url) => {
      expect(isSafeUrl(url, links)).toBe(true)
    },
  )

  it.each([['javascript:alert(1)'], ['JavaScript:alert(1)'], [' java\tscript:alert(1)'], ['vbscript:x'], ['data:text/html,x'], ['file:///etc/passwd'], ['']])(
    'rejects %j',
    (url) => {
      expect(isSafeUrl(url, links)).toBe(false)
    },
  )

  it('applies the scheme set it is given', () => {
    expect(isSafeUrl('mailto:a@b.co', new Set(['https']))).toBe(false)
  })
})

describe('safeHref', () => {
  it.each([['https://demo.example.com/x'], ['http://demo.example.com'], ['/demo'], ['//demo.example.com']])('keeps %s', (url) => {
    expect(safeHref(url)).toBe(url)
  })

  it.each([['javascript:alert(1)'], ['data:text/html,x'], ['mailto:a@b.co'], ['']])('drops %j', (url) => {
    expect(safeHref(url)).toBeUndefined()
  })

  it('drops an absent address', () => {
    expect(safeHref(null)).toBeUndefined()
    expect(safeHref(undefined)).toBeUndefined()
  })
})
