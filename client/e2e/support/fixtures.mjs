/**
 * Fixtures shared by the e2e stub backend (`stub-backend.mjs`) and the specs.
 *
 * One module on purpose: a spec asserts on the very strings the stub serves, so the two can
 * never drift apart. Plain `.mjs` because the stub is started by Playwright's `webServer` with
 * `node` directly, with no TypeScript loader in between.
 *
 * Shapes follow what the C# backend sends (camelCase `ProductDto`, `InvoiceDto`, ...); the Nuxt
 * BFF in `server/api/portal/**` maps them exactly as it does in production.
 *
 * @module e2e/support/fixtures
 */

/** Port the stub backend listens on; the Nuxt server is pointed at it through `API_URL`. */
export const STUB_PORT = Number(process.env.E2E_STUB_PORT || 5999)

/** Base URL of the stub backend, used by specs to read what it received. */
export const STUB_URL = `http://127.0.0.1:${STUB_PORT}`

/** Access token the stub hands out on sign-in; the stub accepts any bearer, so this only needs to be non-empty. */
export const ACCESS_TOKEN = 'e2e-access-token'

/** Password the stub refuses, to exercise the sign-in failure path. */
export const WRONG_PASSWORD = 'wrong-password'

/** The hosting plan the plan-to-cart flow buys. */
export const PLAN = {
  id: 11,
  groupId: 1,
  name: 'E2E Starter',
  description: 'Starter hosting for the end-to-end suite',
  status: 'Active',
  pricing: { monthly: 5, annual: 48 },
  prices: [
    { currencyCode: 'USD', monthly: 5, annual: 48 },
    { currencyCode: 'AMD', monthly: 2000, annual: 19200 },
  ],
}

/** A second, dearer plan so the pricing page has more than one card to choose between. */
export const PLAN_PRO = {
  id: 12,
  groupId: 1,
  name: 'E2E Business',
  description: 'Business hosting for the end-to-end suite',
  status: 'Active',
  pricing: { monthly: 15, annual: 144 },
  // The operator's recommended plan, so the pricing pages draw their badge.
  isFeatured: true,
  prices: [
    { currencyCode: 'USD', monthly: 15, annual: 144 },
    { currencyCode: 'AMD', monthly: 6000, annual: 57600 },
  ],
}

/**
 * The product a domain order hangs on. The storefront finds it by name (a product whose name
 * contains "domain") and adds a domain to the cart only when it exists; it sits in a group of its
 * own so no hosting page lists it.
 */
export const DOMAIN_PRODUCT = {
  id: 90,
  groupId: 9,
  name: 'E2E Domain Registration',
  description: 'The product domain orders are placed on',
  status: 'Active',
  pricing: { monthly: 0, annual: 0 },
  prices: [
    { currencyCode: 'USD', monthly: 0, annual: 0 },
    { currencyCode: 'AMD', monthly: 0, annual: 0 },
  ],
}

/** Every product the catalogue endpoint serves. */
export const PRODUCTS = [PLAN, PLAN_PRO, DOMAIN_PRODUCT]

/**
 * The catalogue's product groups as `GET /api/products/groups` serves them. The hosting group is a
 * product family; the domain group is switched off, so no family page lists the domain product.
 */
export const PRODUCT_GROUPS = [
  { id: 1, name: 'Web Hosting', description: null, isActive: true, productCount: 2 },
  { id: 9, name: 'E2E Domain Registration', description: null, isActive: false, productCount: 1 },
]

/** The enabled billing currencies: USD is the base one, so a guest is priced in it until they pick AMD. */
export const CURRENCIES = [
  { code: 'USD', numeric: '840', prefix: '$', suffix: '', decimals: 2, isBase: true },
  { code: 'AMD', numeric: '051', prefix: '', suffix: ' AMD', decimals: 0, isBase: false },
]

/**
 * The catalogue as `GET /api/products?currency=` prices it: every product's `pricing` is its stored
 * price in the requested currency (the base one when the code is missing or unknown).
 *
 * @param {string | null} code Requested ISO 4217 code, or null for the payer's default.
 * @returns {object[]} The products, priced in that currency.
 */
export const productsIn = (code) =>
  PRODUCTS.map((product) => {
    const price = product.prices.find((p) => p.currencyCode === code) ?? product.prices[0]
    return { ...product, pricing: { monthly: price.monthly, annual: price.annual } }
  })

/** Domain that the stub reports as already registered. */
export const TAKEN_DOMAIN = 'taken-e2e.com'

/** Search term whose `.com` result is available (the stub answers "available" for every name but {@link TAKEN_DOMAIN}). */
export const FREE_TERM = 'fresh-e2e'

/** TLD price table served by `GET /api/domains/tld-pricing`. */
export const TLD_PRICING = {
  currency: { code: 'USD', prefix: '$' },
  pricing: {
    com: {
      sellCurrency: 'USD',
      register: { 1: '10.00' },
      transfer: { 1: '10.00' },
      renew: { 1: '12.00' },
      categories: ['popular'],
    },
    net: {
      sellCurrency: 'USD',
      register: { 1: '12.00' },
      transfer: { 1: '12.00' },
      renew: { 1: '14.00' },
      categories: ['popular'],
    },
  },
}

/** TLD price table priced in AMD, served when `GET /api/domains/tld-pricing?currency=AMD` is asked. */
export const TLD_PRICING_AMD = {
  currency: { code: 'AMD', prefix: '' },
  pricing: {
    com: { ...TLD_PRICING.pricing.com, sellCurrency: 'AMD', register: { 1: '4000' }, transfer: { 1: '4000' }, renew: { 1: '4800' } },
    net: { ...TLD_PRICING.pricing.net, sellCurrency: 'AMD', register: { 1: '4800' }, transfer: { 1: '4800' }, renew: { 1: '5600' } },
  },
}

/** The signed-in customer, as `GET /api/clients/me` returns them. */
export const CLIENT = {
  id: 1,
  firstName: 'Eve',
  lastName: 'Tester',
  companyName: '',
  email: 'e2e@example.com',
  phone: '',
  street: '',
  address2: '',
  city: '',
  state: '',
  postCode: '',
  country: 'US',
  paymentMethod: 'stripe',
  language: '',
  currency: 'USD',
  twoFactorEnabled: false,
  notifyGeneral: true,
  notifyInvoice: true,
  notifySupport: true,
  notifyProduct: true,
  notifyDomain: true,
  notifyAffiliate: false,
}

/** Credentials the stub accepts on sign-in. */
export const CLIENT_CREDENTIALS = { email: CLIENT.email, password: 'correct-horse' }

/** The unpaid invoice the pay flow settles. */
export const INVOICE = {
  id: 501,
  clientId: CLIENT.id,
  clientName: 'Eve Tester',
  status: 'Unpaid',
  currency: 'USD',
  invoiceDate: '2030-01-01T00:00:00Z',
  dueDate: '2030-01-15T00:00:00Z',
  createdAt: '2030-01-01T00:00:00Z',
  paidAt: null,
  total: 5,
  subTotal: 5,
  tax: 0,
  taxRate: 0,
  credit: 0,
  gatewayTransactionId: null,
  notes: null,
  paymentMethod: null,
  items: [{ id: 1, description: 'E2E Starter - monthly', unitPrice: 5, quantity: 1, amount: 5 }],
  transactions: [],
}

/** The saved card the pay page preselects. */
export const SAVED_CARD = {
  id: 'pm_e2e',
  type: 'card',
  description: 'Visa 4242',
  gateway_name: 'stripe',
  card_last_four: '4242',
  card_expiry: '12/30',
  card_type: 'Visa',
}

/** Operator settings the storefront reads; a light default and an uploaded light/dark logo pair. */
export const PUBLIC_SETTINGS = [
  { key: 'portal.template', value: 'aurora' },
  { key: 'portal.theme.default', value: 'light' },
  { key: 'portal.theme.user_toggle', value: 'true' },
  { key: 'portal.logo', value: '/logo.svg' },
  { key: 'portal.logo.dark', value: '/logo-dark.svg' },
]

/** What the contact form submits, reused by the spec to fill the form and to check what arrived. */
export const CONTACT = {
  name: 'Eve Tester',
  email: 'e2e@example.com',
  message: 'Hello from the end-to-end suite.',
}

/** Two published knowledge-base articles; the first carries markup the page must neutralise. */
export const KB_ARTICLES = [
  {
    id: 31,
    title: 'How to reset your password',
    content: '<p>Open <strong>account settings</strong> and choose a new password.</p>'
      + '<script>window.__kbXss = 1</script>'
      + '<img src="x" onerror="window.__kbXss = 2">'
      + '<a href="javascript:window.__kbXss = 3">bad link</a>',
    category: 'Account',
    isPublished: true,
  },
  {
    id: 32,
    title: 'Update your billing details',
    content: '<p>Billing details live under the account page.</p>',
    category: 'Billing',
    isPublished: true,
  },
]

/** One published announcement whose body also carries a script, to prove it is stripped. */
export const ANNOUNCEMENT = {
  id: 7,
  title: 'Scheduled maintenance window',
  content: '<p>We will <em>upgrade</em> the platform on Sunday.</p><script>window.__newsXss = 1</script>',
  publishedAt: '2030-03-05T09:00:00Z',
}

/** Support departments a new ticket can be routed to. */
export const DEPARTMENTS = [
  { id: 1, name: 'Technical Support' },
  { id: 2, name: 'Sales' },
]

/** What the new-ticket spec types. */
export const NEW_TICKET = {
  subject: 'E2E: my site is slow',
  message: 'The homepage takes ten seconds to load.',
}

/** Reply the ticket spec posts onto an open ticket. */
export const TICKET_REPLY = 'Thanks, this is still happening.'

/** The open ticket the stub starts with (restored on every reset), as the list endpoint returns it (`TicketListItemDto`). */
export const SEEDED_TICKET = {
  id: 801,
  clientId: CLIENT.id,
  subject: 'Seeded e2e ticket',
  status: 'Open',
  priority: 'Medium',
  createdAt: '2030-02-01T10:00:00Z',
  replyCount: 1,
  departmentName: 'Technical Support',
  lastReplyAt: '2030-02-01T11:00:00Z',
  isFlagged: false,
}

/** The opening message and the staff answer of {@link SEEDED_TICKET} (`TicketDto.Message` and `TicketReplyDto`). */
export const SEEDED_TICKET_THREAD = {
  message: 'Seeded opening message.',
  replies: [
    { id: 1, message: 'Seeded staff answer.', authorName: 'Support Agent', isStaffReply: true, createdAt: '2030-02-01T11:00:00Z' },
  ],
}

/** A hosting service that is already set up (it has a control-panel user). */
export const SERVICE = {
  id: 21,
  productId: PLAN.id,
  productName: PLAN.name,
  domain: 'e2e-site.com',
  status: 'Active',
  price: '5.00',
  billingCycle: 'Monthly',
  nextRenewalAt: '2030-02-01T00:00:00Z',
  username: 'e2euser',
}

/** A registered domain, as `GET /api/me/domains` returns it. */
export const DOMAIN = {
  id: 41,
  clientId: CLIENT.id,
  name: 'e2e-site.com',
  tld: '.com',
  status: 'Active',
  registeredAt: '2029-02-01T00:00:00Z',
  expiresAt: '2030-02-01T00:00:00Z',
  autoRenew: true,
  whoisPrivacy: false,
  isLocked: true,
  registrarRef: null,
  eppCode: null,
  linkedServiceId: null,
  firstPaymentAmount: 10,
  recurringAmount: 12,
  paymentMethod: null,
  promotionCode: null,
  subscriptionId: null,
  adminNotes: null,
  orderId: null,
  orderType: 'Register',
  dnsManagement: false,
  emailForwarding: false,
  priceCurrency: 'USD',
  nextDueDate: '2030-02-01T00:00:00Z',
  registrar: null,
  registrationPeriod: 1,
}

/** Invoice ids the stub treats as the three hosted-gateway outcomes (any other id is unknown to it). */
export const GATEWAY_PAID_INVOICE = '501'

/** The invoice id whose hosted-gateway payment is still being confirmed. */
export const GATEWAY_PENDING_INVOICE = '502'

/** The invoice id whose hosted-gateway payment was declined. */
export const GATEWAY_DECLINED_INVOICE = '503'

/** Email the register spec uses to make the stub refuse the form with a field error. */
export const TAKEN_EMAIL = 'taken@e2e.test'

/** The sentence the stub puts on the refused email field (a server sentence the page must show verbatim). */
export const TAKEN_EMAIL_MESSAGE = 'E2E: this email is already registered.'

/** Email the contact spec uses to make the stub refuse the form with a field error. */
export const BAD_CONTACT_EMAIL = 'bad-contact@e2e.test'

/** The sentence the stub puts on the refused contact email field. */
export const BAD_CONTACT_EMAIL_MESSAGE = 'E2E: this email address is not accepted.'

/** A new profile name the account spec saves. */
export const NEW_FIRST_NAME = 'Evelyn'

/**
 * A hosting service that is already set up and has NO domain.
 *
 * The BFF resolves the live nameservers and certificate of a service's domain (a DNS lookup and
 * a TLS handshake, `hosting-info.get.ts`), which the stub cannot answer and which would make a
 * spec depend on the network. The BFF already has a seam for that: a service without a domain
 * returns an empty answer without looking anything up. Every spec that opens a service's detail
 * page therefore uses this one (or {@link SERVICE_PENDING}), never {@link SERVICE}.
 */
export const SERVICE_MANAGED = {
  id: 22,
  productId: PLAN_PRO.id,
  productName: PLAN_PRO.name,
  domain: '',
  status: 'Active',
  price: '15.00',
  billingCycle: 'Monthly',
  nextRenewalAt: '2030-02-01T00:00:00Z',
  username: 'e2emgd',
}

/** A paid hosting service that has no control-panel account yet, so it needs the setup wizard (no domain, for the same reason as {@link SERVICE_MANAGED}). */
export const SERVICE_PENDING = {
  id: 23,
  productId: PLAN.id,
  productName: PLAN.name,
  domain: '',
  status: 'Pending',
  price: '5.00',
  billingCycle: 'Monthly',
  nextRenewalAt: '2030-02-01T00:00:00Z',
  username: '',
}

/** What the setup wizard types: a valid hostname, a username of 3-8 letters/digits, a password of 8+ characters. */
export const SETUP = {
  domain: 'wizard-e2e.com',
  username: 'wizuser',
  password: 'wizard-pass-1',
}

/** The new control-panel password the service spec sets. */
export const NEW_SERVICE_PASSWORD = 'fresh-pass-123'

/** The reason the cancellation spec gives. */
export const CANCEL_REASON = 'E2E: moving to another plan.'

/** The invoice raised against {@link SERVICE_MANAGED}, as `GET /api/me/services/:id/invoices` lists it. */
export const SERVICE_INVOICE = {
  id: INVOICE.id,
  invoiceDate: INVOICE.invoiceDate,
  dueDate: INVOICE.dueDate,
  total: INVOICE.total,
  status: INVOICE.status,
}

/** The nameservers the domain starts with, as `GET /api/me/domains/:id/nameservers` returns them. */
export const DOMAIN_NAMESERVERS = [
  { id: 1, host: 'ns1.e2e-site.test' },
  { id: 2, host: 'ns2.e2e-site.test' },
]

/** The custom nameservers the domain spec saves. */
export const NEW_NAMESERVERS = ['ns1.fresh-e2e.test', 'ns2.fresh-e2e.test']

/** The authorisation code the stub issues on request. */
export const EPP_CODE = 'EPP-E2E-CODE'

/** What the domain transfer spec types. */
export const TRANSFER = {
  domain: 'moved-e2e.com',
  eppCode: 'EPP-E2E-1',
}

/** Order the stub answers every order placement with (order, its invoice and the payment token). */
export const ORDER = {
  orderId: 301,
  invoiceId: INVOICE.id,
  paymentToken: 'tok-e2e',
}

/** Order raised by a domain renewal or transfer; the invoice is the unpaid one. */
export const DOMAIN_ORDER = {
  orderId: 701,
  invoiceId: INVOICE.id,
}

/** Payment gateways enabled for a new order: bank transfer only, because it needs no card element or bank redirect. */
export const GATEWAY_METHODS = [
  { module: 'bank_transfer', displayName: 'Bank transfer' },
]

/** The enrolment the stub answers `POST /api/me/2fa/setup` with. */
export const TWO_FACTOR = {
  secret: 'JBSWY3DPEHPK3PXP',
  qrCodeUri: 'otpauth://totp/E2E:e2e@example.com?secret=JBSWY3DPEHPK3PXP&issuer=E2E',
  code: '123456',
}

/** The sentence the stub puts on a refused two-factor code. */
export const TWO_FACTOR_REFUSED = 'E2E: this code is not valid.'

/*
 * NOTE for the contacts, team and WHOIS fixtures below: they are the backend's own wire shapes
 * (`ContactDto`, `ClientUserDto`, `WhoisDto`, camelCase), which the BFF forwards unmapped, so the
 * client area reads exactly these member names.
 */

/** The contact the account starts with, as `GET /api/clients/me/contacts` lists it (`ContactDto`). */
export const SEEDED_CONTACT = {
  id: 61,
  firstName: 'Carl',
  lastName: 'Contact',
  companyName: null,
  email: 'carl@e2e.test',
  phone: null,
  type: 'General',
  street: null,
  address2: null,
  city: null,
  state: null,
  postCode: null,
  country: null,
  notifyGeneral: true,
  notifyInvoice: true,
  notifySupport: true,
  notifyProduct: true,
  notifyDomain: false,
  notifyAffiliate: false,
}

/** The contact the spec adds. */
export const NEW_CONTACT = {
  firstName: 'Nina',
  lastName: 'Newcontact',
  email: 'nina@e2e.test',
}

/** The first name the contact edit saves. */
export const EDITED_CONTACT_FIRST_NAME = 'Carla'

/** The account owner, listed first in the team and never removable (`ClientUserDto`). */
export const TEAM_OWNER = {
  userId: 'u-owner',
  firstName: 'Eve',
  lastName: 'Tester',
  email: CLIENT.email,
  isOwner: true,
  permissions: 8191,
  lastLoginAt: null,
  createdAt: '2029-01-01T00:00:00Z',
}

/** A team member the owner can remove (`ClientUserDto`). */
export const TEAM_MEMBER = {
  userId: 'u-member',
  firstName: 'Mia',
  lastName: 'Member',
  email: 'member@e2e.test',
  isOwner: false,
  permissions: 8191,
  lastLoginAt: null,
  createdAt: '2029-03-01T00:00:00Z',
}

/** The registry record of {@link DOMAIN}, as `GET /api/me/domains/:id/whois` answers (`WhoisDto`). */
export const WHOIS_RECORD = {
  registrar: 'E2E Registrar Ltd',
  registrant: 'Eve Tester',
  createdAt: '2029-02-01T00:00:00Z',
  expiresAt: '2030-02-01T00:00:00Z',
}

/** The registrant contact of {@link DOMAIN}, as `GET /api/me/domains/:id/contact` answers (`DomainContactDto`). */
export const REGISTRANT_CONTACT = {
  firstName: 'Reggie',
  lastName: 'Registrant',
  organization: null,
  email: 'registrant@e2e.test',
  phone: '+15550100001',
  address1: '1 Registry Street',
  address2: null,
  city: 'Yerevan',
  state: 'Yerevan',
  postalCode: '0001',
  country: 'AM',
}

/** What the invitation form submits. */
export const INVITE = {
  firstName: 'Ivy',
  lastName: 'Invitee',
  email: 'ivy@e2e.test',
}

/** How far ahead the stub reports the next demo reset, in milliseconds (two and a half hours). */
export const DEMO_RESET_IN_MS = 150 * 60 * 1000

/** The demo accounts the stub publishes; the admin one must never reach the customer sign-in page. */
export const DEMO_ACCOUNTS = [
  { role: 'Admin', email: 'admin@demo.test', password: 'demo-admin-pass' },
  { role: 'Client', email: 'client@demo.test', password: 'demo-client-pass' },
]

/** Questions the public FAQ feed serves; the topic is spelled by its enum name, as the API does. */
export const FAQS = [
  { id: 1, category: 'General', question: 'Example question one?', answer: 'Example answer one.', sortOrder: 1 },
  { id: 2, category: 'General', question: 'Example question two?', answer: 'Example answer two.', sortOrder: 2 },
  { id: 3, category: 'Products', question: 'Example question three?', answer: 'Example answer three.', sortOrder: 3 },
]

/** Testimonials the public feed serves. */
export const TESTIMONIALS = [
  { id: 1, name: 'Example Person', company: 'Example Co', role: 'Founder', avatarUrl: null, quote: 'Example quote.', rating: 5, sortOrder: 1 },
]

/** Partners the public feed serves. */
export const PARTNERS = [
  { id: 1, name: 'Example Co', logoUrl: null, url: 'https://example.test', sortOrder: 1 },
]

/** Projects the public feed serves. */
export const PROJECTS = [
  { id: 1, slug: 'example-project', title: 'Example project', summary: null, description: null, imageUrls: [], url: 'https://example.test', sortOrder: 1 },
]
