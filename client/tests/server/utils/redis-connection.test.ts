/**
 * Tests for `server/utils/redis-connection.ts`: the one Redis setting the deployment shares with the
 * API, in its comma-separated form or as a URL, becomes the options the Node client takes.
 *
 * @module tests/server/utils/redis-connection.test
 */

import { describe, expect, it } from 'vitest'
import { parseRedisConnection } from '~/server/utils/redis-connection'

describe('parseRedisConnection', () => {
  it('answers null when no Redis is configured', () => {
    expect(parseRedisConnection(undefined)).toBeNull()
    expect(parseRedisConnection('')).toBeNull()
    expect(parseRedisConnection('   ')).toBeNull()
  })

  it('reads host and port', () => {
    expect(parseRedisConnection('cache:6380')).toEqual({ host: 'cache', port: 6380 })
  })

  it('uses the default port when none is written', () => {
    expect(parseRedisConnection('cache')).toEqual({ host: 'cache', port: 6379 })
  })

  it('reads the password, user, TLS and database options and ignores the rest', () => {
    expect(parseRedisConnection('cache:6379,password=s3cret,user=portal,ssl=true,defaultDatabase=2,abortConnect=false'))
      .toEqual({ host: 'cache', port: 6379, password: 's3cret', username: 'portal', tls: {}, db: 2 })
  })

  it('keeps an equals sign inside a password', () => {
    expect(parseRedisConnection('cache:6379,password=a=b')?.password).toBe('a=b')
  })

  it('reads a bracketed IPv6 endpoint', () => {
    expect(parseRedisConnection('[::1]:6390')).toEqual({ host: '::1', port: 6390 })
  })

  it('reads a redis URL with credentials and database', () => {
    expect(parseRedisConnection('redis://portal:p%40ss@cache:6381/3'))
      .toEqual({ host: 'cache', port: 6381, username: 'portal', password: 'p@ss', db: 3 })
  })

  it('turns TLS on for a rediss URL', () => {
    expect(parseRedisConnection('rediss://cache')).toEqual({ host: 'cache', port: 6379, tls: {} })
  })

  it('refuses a setting that names no host', () => {
    expect(() => parseRedisConnection('password=only')).toThrow()
  })
})
