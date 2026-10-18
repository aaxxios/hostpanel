/**
 * Fixtures for the servers screen: one provisioning server as `GET /api/admin/servers` returns it.
 */
import type { ServerDto, TestConnectionResultDto } from '../../src/types/server'

/** A reachable control-panel server that is the default for its module. */
export const hostServer: ServerDto = {
  id: 1,
  name: 'host1',
  hostname: 'host1.example.test',
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
  isOnline: null,
  lastTestedAt: null,
  accountsCount: 12,
}

/** `POST /api/admin/servers/1/test-connection` when the panel answers. */
export const connectionOk: TestConnectionResultDto = {
  connected: true,
  accountsCount: 12,
  version: '7.4.2',
  errorMessage: null,
  testedAt: '2026-10-01T00:00:00Z',
}
