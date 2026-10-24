/**
 * GET /api/portal/client/services
 * Returns all services for the authenticated client from the C# backend,
 * mapped to the legacy-style field names the frontend expects.
 */
export default defineEventHandler(async (event) => {
  const services = await internalApiCall<Record<string, unknown>[]>(event, '/me/services')

  return services.map((s) => ({
    id: s.id,
    clientid: 0,
    pid: s.productId,
    regdate: s.createdAt ?? '',
    // Null stays null: the page renders the fallback sentence in the visitor's language.
    name: s.productName ?? null,
    translated_name: s.productName ?? null,
    groupname: '',
    domain: s.domain ?? '',
    dedicatedip: '',
    serverid: 0,
    servername: '',
    serverip: '',
    serverhostname: '',
    firstpaymentamount: '0.00',
    recurringamount: s.price ?? '0.00',
    paymentmethod: '',
    paymentmethodname: '',
    billingcycle: s.billingCycle ?? '',
    nextduedate: s.nextRenewalAt ?? '',
    status: s.status ?? 'Active',
    username: s.username ?? 'provisioned',
    password: '',
    subscriptionid: '',
    promoid: 0,
    overideautosuspend: 0,
    overidesuspenduntil: '',
    ns1: '', ns2: '',
    assignedips: '',
    notes: '',
    diskusage: 0, disklimit: 0,
    bwusage: 0, bwlimit: 0,
    lastupdate: '',
    customfields: { customfield: [] },
    configoptions: { configoption: [] },
  }))
})
