/**
 * GET /api/portal/client/services/:id
 * Returns details for a single service.
 * The C# backend only has a list endpoint, so we fetch all and filter.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const services = await internalApiCall<Record<string, unknown>[]>(event, '/me/services')
  const s = services.find((svc) => String(svc.id) === id)

  if (!s) {
    // The backend has no single-service read, so this refusal is the BFF's own: a code, no sentence.
    throw createError({ statusCode: 404, data: { code: 'SERVICE_NOT_FOUND' } })
  }

  return {
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
    serverid: s.serverId ?? 0,
    servername: s.serverName ?? '',
    serverip: s.serverIp ?? '',
    serverhostname: s.serverHostname ?? '',
    firstpaymentamount: s.firstPaymentAmount ?? '0.00',
    recurringamount: s.price ?? '0.00',
    paymentmethod: s.paymentMethod ?? '',
    paymentmethodname: s.paymentMethod ?? '',
    billingcycle: s.billingCycle ?? '',
    nextduedate: s.nextRenewalAt ?? '',
    status: s.status ?? 'Active',
    username: s.username ?? '',
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
  }
})
