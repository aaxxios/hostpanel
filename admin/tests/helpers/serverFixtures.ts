/**
 * Fixtures of the servers and integrations tests: a provisioning server and a group as the admin
 * API returns them.
 */
import type { ServerDto, ServerGroupDto } from '@/types/server'

/**
 * Builds a provisioning server.
 *
 * @param overrides - Fields to change on the default server.
 * @returns The server.
 */
export const serverDto = (overrides: Partial<ServerDto> = {}): ServerDto => ({
  id: 1,
  name: 'host1',
  hostname: 'host1.example.com',
  ipAddress: '203.0.113.10',
  assignedIpAddresses: null,
  module: 'Cwp7',
  username: 'root',
  useSSL: true,
  maxAccounts: 100,
  isDefault: true,
  isDisabled: false,
  monthlyCost: 20,
  datacenter: null,
  serverStatusAddress: null,
  ns1Hostname: null,
  ns1Ip: null,
  ns2Hostname: null,
  ns2Ip: null,
  ns3Hostname: null,
  ns3Ip: null,
  ns4Hostname: null,
  ns4Ip: null,
  ns5Hostname: null,
  ns5Ip: null,
  serverGroupId: null,
  serverGroupName: null,
  createdAt: '2026-01-01T00:00:00Z',
  isOnline: true,
  lastTestedAt: null,
  accountsCount: 12,
  ...overrides,
})

/**
 * Builds a server group.
 *
 * @param overrides - Fields to change on the default group.
 * @returns The group.
 */
export const serverGroupDto = (overrides: Partial<ServerGroupDto> = {}): ServerGroupDto => ({
  id: 3,
  name: 'Shared',
  fillType: 'LeastFull',
  servers: [serverDto()],
  createdAt: '2026-01-01T00:00:00Z',
  ...overrides,
})
