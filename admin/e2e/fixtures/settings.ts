/**
 * Fixtures for the system settings page and the branding upload.
 */
import type { Setting } from '../../src/types/setting'

/** URL the stubbed branding upload answers with. */
export const uploadedLogoUrl = '/uploads/branding/logo-e2e.png'

/** The seeded rows: only the branding keys are needed for the upload flow. */
export const brandingSettings: Setting[] = [
  { id: 11, key: 'portal.logo', value: '', description: 'Logo', isSecret: false, hasValue: false },
  { id: 12, key: 'portal.logo.dark', value: '', description: 'Logo for dark backgrounds', isSecret: false, hasValue: false },
  { id: 13, key: 'portal.logo.mark', value: '', description: 'Square mark', isSecret: false, hasValue: false },
  { id: 14, key: 'portal.favicon', value: '', description: 'Favicon', isSecret: false, hasValue: false },
]

/** One-pixel transparent PNG, the smallest valid upload. */
export const tinyPng = Buffer.from(
  'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==',
  'base64',
)
