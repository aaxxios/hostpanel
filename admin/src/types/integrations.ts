/**
 * Barrel of the integration types, metadata and the server-info DTO, so a screen imports them
 * from one place.
 */
export type {
  IntegrationCategory,
  IntegrationSlug,
  IntegrationDto,
  IntegrationDetailDto,
  IntegrationConfigPayload,
  IntegrationTestResult,
  IntegrationMeta,
  IntegrationField,
} from '@/types/integration'
export { INTEGRATION_META, INTEGRATION_CATEGORIES } from '@/types/integrationmeta'
export type { Cwp7ServerInfoDto } from '@/types/cwp7'
