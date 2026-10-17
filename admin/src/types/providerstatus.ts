/**
 * Shapes the provisioning-provider status card is fed with, independent of which provider
 * (CWP, CWP7) the figures come from.
 */

/** One labelled figure of the status card (accounts, version, packages...). */
export interface ProviderMetric {
  /** Caption above the figure (already translated). */
  label: string
  /** The figure; a dash is shown when it is null. */
  value: string | number | null
  /** Draws the figure in a large display size (counts) instead of the body size. */
  large?: boolean
  /** Draws the figure in the monospace face (versions). */
  mono?: boolean
}

/** Server info the status card summarises; the CWP and CWP7 DTOs both satisfy it. */
export interface ProviderInfo {
  /** True if the API connection succeeded. */
  connected: boolean
  /** ISO 8601 timestamp of the last successful connection test, null if never tested. */
  lastTestedAt: string | null
  /** Error message if the connection failed, otherwise null. */
  errorMessage: string | null
}

/** Closed set of connection states a provider header can show. */
export const ProviderConnectionState = {
  /** The API connection works. */
  Connected: 'connected',
  /** The API connection failed. */
  Error: 'error',
  /** No server info yet. */
  Unknown: 'unknown',
} as const

/** One of the connection states. */
export type ProviderConnectionState = (typeof ProviderConnectionState)[keyof typeof ProviderConnectionState]
