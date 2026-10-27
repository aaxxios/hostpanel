/**
 * Reads the Redis connection setting the portal shares with the API.
 *
 * The deployment has one Redis setting, `REDIS_CONNECTION`, written in the comma-separated form the
 * API's Redis client reads (`host:port,password=...,ssl=true,defaultDatabase=0`). The portal is fed
 * the same value (`NUXT_SESSION_REDIS`) so an operator configures Redis once; this module turns it
 * into the options the Node Redis client takes. A `redis://` or `rediss://` URL is accepted too.
 *
 * @module server/utils/redis-connection
 */

/** The Redis client options this setting can carry. */
export interface RedisConnectionOptions {
  /** Host name or address. */
  host: string
  /** TCP port. */
  port: number
  /** ACL user name, when one is configured. */
  username?: string
  /** Password, when one is configured. */
  password?: string
  /** Database index. */
  db?: number
  /** TLS options; present (empty) when the connection must use TLS. */
  tls?: Record<string, never>
}

/** The port Redis listens on when the setting names none. */
const DEFAULT_REDIS_PORT = 6379

/**
 * Splits `host:port` (or `[ipv6]:port`) into its parts.
 *
 * @param endpoint - The endpoint as written in the setting.
 * @returns The host and the port, the default port when none is written.
 */
const parseEndpoint = (endpoint: string): { host: string, port: number } => {
  const bracketed = /^\[([^\]]+)\](?::(\d+))?$/.exec(endpoint)
  if (bracketed) {
    return { host: bracketed[1] ?? '', port: bracketed[2] ? Number(bracketed[2]) : DEFAULT_REDIS_PORT }
  }
  const separator = endpoint.lastIndexOf(':')
  if (separator < 0) {
    return { host: endpoint, port: DEFAULT_REDIS_PORT }
  }
  return { host: endpoint.slice(0, separator), port: Number(endpoint.slice(separator + 1)) || DEFAULT_REDIS_PORT }
}

/**
 * Reads a `redis://` / `rediss://` URL.
 *
 * @param value - The URL.
 * @returns The client options it describes.
 */
const parseUrl = (value: string): RedisConnectionOptions => {
  const url = new URL(value)
  const options: RedisConnectionOptions = {
    host: url.hostname.replace(/^\[|\]$/g, ''),
    port: url.port ? Number(url.port) : DEFAULT_REDIS_PORT,
  }
  if (url.username) {
    options.username = decodeURIComponent(url.username)
  }
  if (url.password) {
    options.password = decodeURIComponent(url.password)
  }
  const database = url.pathname.replace(/^\//, '')
  if (database) {
    options.db = Number(database)
  }
  if (url.protocol === 'rediss:') {
    options.tls = {}
  }
  return options
}

/**
 * Reads the comma-separated form: the first endpoint, then `key=value` options.
 *
 * Only the options that change where and how the portal connects are read (`password`, `user`,
 * `ssl`, `defaultDatabase`); the others tune the API's own client and mean nothing here.
 *
 * @param value - The setting.
 * @returns The client options it describes.
 */
const parseCommaSeparated = (value: string): RedisConnectionOptions => {
  const parts = value.split(',').map((part) => part.trim()).filter((part) => part.length > 0)
  const endpoint = parts.find((part) => !part.includes('=')) ?? ''
  const options: RedisConnectionOptions = parseEndpoint(endpoint)
  for (const part of parts) {
    const separator = part.indexOf('=')
    if (separator < 0) {
      continue
    }
    const key = part.slice(0, separator).trim().toLowerCase()
    const optionValue = part.slice(separator + 1).trim()
    if (key === 'password') {
      options.password = optionValue
    } else if (key === 'user') {
      options.username = optionValue
    } else if (key === 'ssl' && optionValue.toLowerCase() === 'true') {
      options.tls = {}
    } else if (key === 'defaultdatabase') {
      options.db = Number(optionValue)
    }
  }
  return options
}

/**
 * Turns the Redis setting into client options.
 *
 * @param value - The setting; empty or blank means no Redis is configured.
 * @returns The client options, or null when no Redis is configured.
 * @throws Error when a value is given but names no host.
 */
export const parseRedisConnection = (value: string | undefined): RedisConnectionOptions | null => {
  const trimmed = (value ?? '').trim()
  if (!trimmed) {
    return null
  }
  const options = /^rediss?:\/\//i.test(trimmed) ? parseUrl(trimmed) : parseCommaSeparated(trimmed)
  if (!options.host) {
    // Fail at start-up rather than connect to a default host the operator never named.
    throw new Error('The session Redis setting names no host.')
  }
  return options
}
