import { translate } from '@/lib/translate'

/** Closed set of the supported control panel module identifiers. */
export const ServerModule = {
  CPanel: 'CPanel',
  Plesk: 'Plesk',
  DirectAdmin: 'DirectAdmin',
  Centos: 'Centos',
  Cwp7: 'Cwp7',
  Lukittu: 'Lukittu',
  InnovayseWordPressHosting: 'InnovayseWordPressHosting',
  Virtualmin: 'Virtualmin',
  HyperVm: 'HyperVm',
} as const

/** All supported control panel module identifiers. */
export type ServerModule = (typeof ServerModule)[keyof typeof ServerModule]

/** Closed set of account distribution strategies of a server group. */
export const ServerFillType = {
  /** New accounts go to the least full server. */
  LeastFull: 'LeastFull',
  /** The active server is filled before the next one is used. */
  FillUntilFull: 'FillUntilFull',
} as const

/** Account distribution strategy for a server group. */
export type ServerFillType = (typeof ServerFillType)[keyof typeof ServerFillType]

/** DTO for a single provisioning server. */
export interface ServerDto {
  /** Unique server identifier. */
  id: number
  /** Display name. */
  name: string
  /** Hostname or IP used for API access. */
  hostname: string
  /** Primary public IP address, or null. */
  ipAddress: string | null
  /** Additional assigned IPs (newline-separated), or null. */
  assignedIpAddresses: string | null
  /** Control panel module type string. */
  module: ServerModule
  /** API username. */
  username: string
  /** Whether SSL is used for API connections. */
  useSSL: boolean
  /** Account capacity limit, or null for unlimited. */
  maxAccounts: number | null
  /** Whether this is the default server for the module. */
  isDefault: boolean
  /** Whether this server is disabled. */
  isDisabled: boolean
  /** Monthly cost in USD. */
  monthlyCost: number
  /** Datacenter or NOC provider name, or null. */
  datacenter: string | null
  /** Status page URL, or null. */
  serverStatusAddress: string | null
  /** Primary nameserver hostname. */
  ns1Hostname: string | null
  /** Primary nameserver IP. */
  ns1Ip: string | null
  /** Secondary nameserver hostname. */
  ns2Hostname: string | null
  /** Secondary nameserver IP. */
  ns2Ip: string | null
  /** Third nameserver hostname. */
  ns3Hostname: string | null
  /** Third nameserver IP. */
  ns3Ip: string | null
  /** Fourth nameserver hostname. */
  ns4Hostname: string | null
  /** Fourth nameserver IP. */
  ns4Ip: string | null
  /** Fifth nameserver hostname. */
  ns5Hostname: string | null
  /** Fifth nameserver IP. */
  ns5Ip: string | null
  /** ID of the assigned group, or null. */
  serverGroupId: number | null
  /** Name of the assigned group, or null. */
  serverGroupName: string | null
  /** UTC creation timestamp. */
  createdAt: string
  /** Whether the last connection test succeeded, or null if never tested. */
  isOnline: boolean | null
  /** UTC timestamp of the last connection test, or null if never tested. */
  lastTestedAt: string | null
  /** Account count from the last test, or null if unknown. */
  accountsCount: number | null
}

/** Result of a server connection test. */
export interface TestConnectionResultDto {
  /** Whether the connection succeeded. */
  connected: boolean
  /** Total accounts on the server, or null if unavailable. */
  accountsCount: number | null
  /** Server software version, or null. */
  version: string | null
  /** Error message if connection failed, or null. */
  errorMessage: string | null
  /** UTC timestamp of the test. */
  testedAt: string
}

/** DTO for a server group including its assigned servers. */
export interface ServerGroupDto {
  /** Unique group identifier. */
  id: number
  /** Display name. */
  name: string
  /** Fill type string. */
  fillType: ServerFillType
  /** Servers assigned to this group. */
  servers: ServerDto[]
  /** UTC creation timestamp. */
  createdAt: string
}

/** Payload for creating or updating a server. */
export interface ServerPayload {
  /** Display name. */
  name: string
  /** Hostname or IP used for API access. */
  hostname: string
  /** Primary public IP address, or null. */
  ipAddress: string | null
  /** Additional assigned IPs (newline-separated), or null. */
  assignedIpAddresses: string | null
  /** Control panel module type. */
  module: ServerModule
  /** API username. */
  username: string
  /** API password, or null to keep the stored one. */
  password: string | null
  /** API token, or null to keep the stored one. */
  apiToken: string | null
  /** Access hash, or null to keep the stored one. */
  accessHash: string | null
  /** Whether SSL is used for API connections. */
  useSSL: boolean
  /** Account capacity limit, or null for unlimited. */
  maxAccounts: number | null
  /** Whether this is the default server for the module. */
  isDefault: boolean
  /** Whether this server is disabled. */
  isDisabled: boolean
  /** Monthly cost in USD. */
  monthlyCost: number
  /** Datacenter or NOC provider name, or null. */
  datacenter: string | null
  /** Status page URL, or null. */
  serverStatusAddress: string | null
  /** Nameserver 1 hostname, or null. */
  ns1Hostname: string | null
  /** Nameserver 1 IP, or null. */
  ns1Ip: string | null
  /** Nameserver 2 hostname, or null. */
  ns2Hostname: string | null
  /** Nameserver 2 IP, or null. */
  ns2Ip: string | null
  /** Nameserver 3 hostname, or null. */
  ns3Hostname: string | null
  /** Nameserver 3 IP, or null. */
  ns3Ip: string | null
  /** Nameserver 4 hostname, or null. */
  ns4Hostname: string | null
  /** Nameserver 4 IP, or null. */
  ns4Ip: string | null
  /** Nameserver 5 hostname, or null. */
  ns5Hostname: string | null
  /** Nameserver 5 IP, or null. */
  ns5Ip: string | null
}

/** Payload for creating or updating a server group. */
export interface ServerGroupPayload {
  /** Display name. */
  name: string
  /** Fill strategy. */
  fillType: ServerFillType
  /** IDs of servers assigned to this group. */
  serverIds: number[]
}

/** Human-readable labels for each module; product names stay as they are, the in-house hosting product is read from the catalogue. */
export const MODULE_LABELS: Record<ServerModule, string> = {
  CPanel: 'cPanel / WHM',
  Plesk: 'Plesk',
  DirectAdmin: 'DirectAdmin',
  Centos: 'CentOS Web Panel',
  Cwp7: 'CWP7',
  Lukittu: 'Lukittu',
  get InnovayseWordPressHosting() { return translate('servers.moduleLabels.wordPressHosting') },
  Virtualmin: 'Virtualmin',
  HyperVm: 'HyperVM',
}

/** Human-readable labels for fill types, resolved in the operator's language on each read. */
export const FILL_TYPE_LABELS: Record<ServerFillType, string> = {
  get LeastFull() { return translate('servers.fillType.leastFull') },
  get FillUntilFull() { return translate('servers.fillType.fillUntilFull') },
}

/** Closed set of which lists a server mutation reloads afterwards. */
export const ServerRefresh = {
  /** Only the server list. */
  Servers: 'servers',
  /** The server list and the group list. */
  Both: 'both',
} as const

/** One of the {@link ServerRefresh} values. */
export type ServerRefresh = (typeof ServerRefresh)[keyof typeof ServerRefresh]
