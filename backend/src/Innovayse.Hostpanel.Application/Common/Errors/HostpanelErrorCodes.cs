using System.Diagnostics.CodeAnalysis;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Shared.Constants;

namespace Innovayse.Hostpanel.Application.Common.Errors;

/// <summary>
/// Every error code a hostpanel handler can refuse with, declared once. A code is a resource key:
/// the sentence a person reads is <c>ErrorMessages.resx[code]</c>, resolved at the API boundary in
/// the request's language, and <c>ResourceCoverageTests</c> fails the build when a code has no
/// sentence in any language. Never a literal anywhere else.
/// </summary>
/// <remarks>
/// The status each code answers with comes from <c>StatusMapping</c> by its name, so a new code is
/// named for the status it must produce. Existing codes are never renamed: the admin and client
/// frontends branch on them.
/// </remarks>
[SuppressMessage(
    "Naming",
    "CA1707:Identifiers should not contain underscores",
    Justification = "Constant names mirror the SCREAMING_SNAKE wire codes (rules/cqrs.md).")]
public static class HostpanelErrorCodes
{
    /// <summary>An unhandled fault (500); also the fallback sentence for a code with no entry.</summary>
    public const string HOSTPANEL_UNEXPECTED = SharedErrorCodes.Unexpected;

    /// <summary>The edge rate limiter refused the request (429, with <c>Retry-After</c>).</summary>
    public const string HOSTPANEL_RATE_LIMITED = "HOSTPANEL_RATE_LIMITED";

    /// <summary>
    /// A command marked <c>IDemoRestricted</c> was refused because the deployment is a public demo
    /// (403, an exact <c>StatusMapping</c> row). Thrown by the demo guard middleware, not by a handler.
    /// </summary>
    public const string DEMO_ACTION_DISABLED = SharedErrorCodes.DemoActionDisabled;

    // Auth

    /// <summary>
    /// First-run setup was attempted on an installation that already has an Admin (409). Replaces
    /// the thrown <c>SetupAlreadyCompletedException</c>; the code is unchanged.
    /// </summary>
    public const string SETUP_ALREADY_COMPLETED = "SETUP_ALREADY_COMPLETED";

    /// <summary>
    /// First-run setup was attempted without the token this installation issued, or with the wrong
    /// one (403, via the <c>_TOKEN_INVALID</c> suffix). Replaces the thrown
    /// <c>SetupTokenInvalidException</c>; the code is unchanged.
    /// </summary>
    public const string SETUP_TOKEN_INVALID = "SETUP_TOKEN_INVALID";

    /// <summary>The presented TOTP code did not match the account's secret (400).</summary>
    public const string TWO_FACTOR_CODE_INVALID = "TWO_FACTOR_CODE_INVALID";

    /// <summary>The email and password match no account, or a sign-in step could not resolve its account (401).</summary>
    public const string SIGN_IN_UNAUTHORIZED = "SIGN_IN_UNAUTHORIZED";

    /// <summary>The pending token handed out after the password step is malformed (401).</summary>
    public const string PENDING_TOKEN_UNAUTHORIZED = "PENDING_TOKEN_UNAUTHORIZED";

    /// <summary>The TOTP code presented at sign-in did not match (401). Distinct from <see cref="TWO_FACTOR_CODE_INVALID"/>, which answers 400 for a signed-in account.</summary>
    public const string TWO_FACTOR_CODE_UNAUTHORIZED = "TWO_FACTOR_CODE_UNAUTHORIZED";

    /// <summary>The session cannot be refreshed; the caller has to sign in again (401).</summary>
    public const string SESSION_UNAUTHORIZED = "SESSION_UNAUTHORIZED";

    /// <summary>The sign-out fan-out presented a missing or wrong <c>X-Logout-Key</c> (401).</summary>
    public const string BACKCHANNEL_LOGOUT_UNAUTHORIZED = "BACKCHANNEL_LOGOUT_UNAUTHORIZED";

    /// <summary>A password-reset or email-confirmation token is wrong or past its lifetime (400).</summary>
    public const string TOKEN_INVALID_OR_EXPIRED = "TOKEN_INVALID_OR_EXPIRED";

    /// <summary>The local account could not be created, for example because the address is taken or the password is refused (400).</summary>
    public const string REGISTRATION_FAILED = "REGISTRATION_FAILED";

    // Billing — currencies

    /// <summary>The code names no configured currency (404).</summary>
    public const string CURRENCY_NOT_FOUND = "CURRENCY_NOT_FOUND";

    /// <summary>Product prices were asked to be recomputed for the base currency itself (400).</summary>
    public const string CURRENCY_BASE_NOT_RECOMPUTED = "CURRENCY_BASE_NOT_RECOMPUTED";

    /// <summary>No currency is marked as the base, so nothing can be converted (503).</summary>
    public const string BASE_CURRENCY_NOT_CONFIGURED = "BASE_CURRENCY_NOT_CONFIGURED";

    // Billing — invoices

    /// <summary>The id names no invoice (404).</summary>
    public const string INVOICE_NOT_FOUND = "INVOICE_NOT_FOUND";

    // Billing — quotes

    /// <summary>The id names no quote (404).</summary>
    public const string QUOTE_NOT_FOUND = "QUOTE_NOT_FOUND";

    // Billing — billable items

    /// <summary>The id names no billable item (404).</summary>
    public const string BILLABLE_ITEM_NOT_FOUND = "BILLABLE_ITEM_NOT_FOUND";

    /// <summary>A named billable item belongs to a different client than the one billed (400).</summary>
    public const string BILLABLE_ITEM_CLIENT_MISMATCH = "BILLABLE_ITEM_CLIENT_MISMATCH";

    /// <summary>A named billable item has already been placed on an invoice (400).</summary>
    public const string BILLABLE_ITEM_INVOICED = "BILLABLE_ITEM_INVOICED";

    // Billing — transactions and invoice payments

    /// <summary>The id names no transaction (404).</summary>
    public const string TRANSACTION_NOT_FOUND = "TRANSACTION_NOT_FOUND";

    /// <summary>The id names no client (404).</summary>
    public const string CLIENT_NOT_FOUND = "CLIENT_NOT_FOUND";

    /// <summary>The requested bulk action name is not one this endpoint knows (400).</summary>
    public const string BULK_INVOICE_ACTION_UNKNOWN = "BULK_INVOICE_ACTION_UNKNOWN";

    /// <summary>
    /// The invoice was paid through a hosted-gateway module with no configured plugin, so the
    /// refund cannot be executed against it (503).
    /// </summary>
    public const string PAYMENT_PLUGIN_NOT_CONFIGURED = "PAYMENT_PLUGIN_NOT_CONFIGURED";

    // Billing — gateway payments and saved payment methods

    /// <summary>The invoice is not in a status a payment may be started or charged for (400).</summary>
    public const string INVOICE_NOT_PAYABLE = "INVOICE_NOT_PAYABLE";

    /// <summary>The invoice already has a gateway session inside the bank's live window (409).</summary>
    public const string GATEWAY_SESSION_ALREADY_STARTED = "GATEWAY_SESSION_ALREADY_STARTED";

    /// <summary>The invoice's billing currency does not match the resolved gateway plugin's (400).</summary>
    public const string GATEWAY_CURRENCY_MISMATCH = "GATEWAY_CURRENCY_MISMATCH";

    /// <summary>The invoice carries no hosted-gateway session to verify (404).</summary>
    public const string GATEWAY_SESSION_NOT_FOUND = "GATEWAY_SESSION_NOT_FOUND";

    /// <summary>The card gateway payment method id does not belong to the caller's account (400).</summary>
    public const string PAYMENT_METHOD_NOT_OWNED = "PAYMENT_METHOD_NOT_OWNED";

    /// <summary>The caller's account has no saved payment method yet (400).</summary>
    public const string NO_SAVED_PAYMENT_METHODS = "NO_SAVED_PAYMENT_METHODS";

    /// <summary>
    /// The gateway return URL is not an absolute URL, or its origin is not one this deployment
    /// is configured to hand a payer back to (400).
    /// </summary>
    public const string RETURN_URL_NOT_ALLOWED = "RETURN_URL_NOT_ALLOWED";

    /// <summary>The payment gateway declined or otherwise failed to charge the client (502).</summary>
    public const string GATEWAY_CHARGE_UPSTREAM_FAILED = "GATEWAY_CHARGE_UPSTREAM_FAILED";

    /// <summary>
    /// The payment gateway reported the charge as successful but returned no transaction id to
    /// record against the invoice (502).
    /// </summary>
    public const string GATEWAY_TRANSACTION_MISSING_UPSTREAM_FAILED = "GATEWAY_TRANSACTION_MISSING_UPSTREAM_FAILED";

    /// <summary>The caller has no row in the clients table, so there is no account to act on (404).</summary>
    public const string CLIENT_PROFILE_NOT_FOUND = "CLIENT_PROFILE_NOT_FOUND";

    // Clients

    /// <summary>
    /// The client has no contact with that id -- another account's or none at all (404). One code
    /// for both so every contact route refuses alike, and neither answer tells a caller which of
    /// the two happened.
    /// </summary>
    public const string MY_CONTACT_NOT_FOUND = "MY_CONTACT_NOT_FOUND";

    /// <summary>The token names no invitation (404).</summary>
    public const string INVITATION_NOT_FOUND = "INVITATION_NOT_FOUND";

    /// <summary>The invitation's window to accept it has passed (400).</summary>
    public const string INVITATION_EXPIRED = "INVITATION_EXPIRED";

    /// <summary>The invitation has already been accepted (409).</summary>
    public const string INVITATION_ALREADY_ACCEPTED = "INVITATION_ALREADY_ACCEPTED";

    /// <summary>The invited address already has a pending invitation on this account (409).</summary>
    public const string INVITATION_ALREADY_PENDING = "INVITATION_ALREADY_PENDING";

    /// <summary>The id names no identity account (404).</summary>
    public const string IDENTITY_USER_NOT_FOUND = "IDENTITY_USER_NOT_FOUND";

    /// <summary>The account's owner has no identity record to read (404).</summary>
    public const string ACCOUNT_OWNER_NOT_FOUND = "ACCOUNT_OWNER_NOT_FOUND";

    /// <summary>The address invited names the account's own owner (400).</summary>
    public const string CANNOT_INVITE_ACCOUNT_OWNER = "CANNOT_INVITE_ACCOUNT_OWNER";

    /// <summary>The invited address is already linked to this account (409).</summary>
    public const string USER_ALREADY_LINKED = "USER_ALREADY_LINKED";

    /// <summary>The existing user named already has a client account of their own (409).</summary>
    public const string EXISTING_USER_ALREADY_HAS_CLIENT = "EXISTING_USER_ALREADY_HAS_CLIENT";

    /// <summary>The client has already been invoiced, so its currency is fixed (409).</summary>
    public const string CLIENT_CURRENCY_ALREADY_INVOICED = "CLIENT_CURRENCY_ALREADY_INVOICED";

    /// <summary>
    /// The requested client status names no member of <c>ClientStatus</c> (400). The validator
    /// already refuses this for a caller that reached the handler through the API; answered here
    /// too, defensively, for a command dispatched some other way.
    /// </summary>
    public const string CLIENT_STATUS_INVALID = "CLIENT_STATUS_INVALID";

    // Notifications

    /// <summary>
    /// The id names no email log entry sent to the caller's own account (404). Answered alike
    /// whether the entry does not exist or was sent to somebody else, so
    /// <c>/api/me/emails/{id}</c> cannot be used to enumerate ids. Added alongside the Clients
    /// batch because <c>MyEmailsController</c> had to move onto <c>BaseApiController</c> once
    /// <c>GetMyProfileQuery</c> started answering <c>Result&lt;ClientDto&gt;</c>.
    /// </summary>
    public const string EMAIL_LOG_NOT_FOUND = "EMAIL_LOG_NOT_FOUND";

    /// <summary>
    /// The id names no email template (404): <c>GetEmailTemplateHandler</c>,
    /// <c>UpdateEmailTemplateHandler</c>, <c>DeleteEmailTemplateHandler</c>,
    /// <c>ToggleEmailTemplateHandler</c> and <c>SendEmailHandler</c>'s slug lookup all answer with
    /// it — the last one is never seen on the wire today, because nothing binds
    /// <c>SendEmailCommand</c> to a route, but the handler still answers with a code rather than
    /// throwing so a future endpoint would not need to change it.
    /// </summary>
    public const string EMAIL_TEMPLATE_NOT_FOUND = "EMAIL_TEMPLATE_NOT_FOUND";

    // Support — tickets

    /// <summary>
    /// The id names no ticket (404). Shared by the staff surface and by <c>ITicketOwnership</c>'s
    /// client-facing refusal, so the whole tickets surface refuses alike.
    /// </summary>
    public const string TICKET_NOT_FOUND = "TICKET_NOT_FOUND";

    // Support — departments

    /// <summary>The id names no support department (404).</summary>
    public const string DEPARTMENT_NOT_FOUND = "DEPARTMENT_NOT_FOUND";

    // Support — downloads

    /// <summary>The id names no download (404).</summary>
    public const string DOWNLOAD_NOT_FOUND = "DOWNLOAD_NOT_FOUND";

    /// <summary>The id names no download category (404).</summary>
    public const string DOWNLOAD_CATEGORY_NOT_FOUND = "DOWNLOAD_CATEGORY_NOT_FOUND";

    // Support — predefined replies

    /// <summary>The id names no predefined reply (404).</summary>
    public const string PREDEFINED_REPLY_NOT_FOUND = "PREDEFINED_REPLY_NOT_FOUND";

    /// <summary>The id names no predefined reply category (404).</summary>
    public const string PREDEFINED_REPLY_CATEGORY_NOT_FOUND = "PREDEFINED_REPLY_CATEGORY_NOT_FOUND";

    // Support — contact form

    /// <summary>This deployment has no configured enquiry inbox for the contact form (503).</summary>
    public const string CONTACT_NOT_CONFIGURED = "CONTACT_NOT_CONFIGURED";

    /// <summary>The SMTP relay refused to send the contact form's message (502).</summary>
    public const string CONTACT_SEND_FAILED = "CONTACT_SEND_FAILED";

    // Domains

    /// <summary>
    /// The id names no domain (404). Shared by the admin surface and by
    /// <see cref="IDomainOwnership"/>'s client-facing refusal, so the whole domains surface
    /// refuses alike under one code.
    /// </summary>
    public const string DOMAIN_NOT_FOUND = "DOMAIN_NOT_FOUND";

    /// <summary>The name is already registered through this system (409).</summary>
    public const string DOMAIN_ALREADY_REGISTERED = "DOMAIN_ALREADY_REGISTERED";

    /// <summary>The registrar rejected the registration request (502).</summary>
    public const string DOMAIN_REGISTRATION_UPSTREAM_FAILED = "DOMAIN_REGISTRATION_UPSTREAM_FAILED";

    /// <summary>The registrar rejected the renewal request (502).</summary>
    public const string DOMAIN_RENEWAL_UPSTREAM_FAILED = "DOMAIN_RENEWAL_UPSTREAM_FAILED";

    /// <summary>The registrar rejected the incoming transfer request (502).</summary>
    public const string DOMAIN_TRANSFER_UPSTREAM_FAILED = "DOMAIN_TRANSFER_UPSTREAM_FAILED";

    /// <summary>The registrar rejected cancellation of a pending incoming transfer (502).</summary>
    public const string DOMAIN_TRANSFER_CANCEL_UPSTREAM_FAILED = "DOMAIN_TRANSFER_CANCEL_UPSTREAM_FAILED";

    /// <summary>
    /// The domain carries no registrar reference, so an outgoing transfer or a renewal cannot be
    /// placed against it (400).
    /// </summary>
    public const string DOMAIN_NO_REGISTRAR_REF = "DOMAIN_NO_REGISTRAR_REF";

    /// <summary>The registrar rejected the outgoing transfer initiation request (502).</summary>
    public const string DOMAIN_OUTGOING_TRANSFER_UPSTREAM_FAILED = "DOMAIN_OUTGOING_TRANSFER_UPSTREAM_FAILED";

    /// <summary>The registrar accepted the outgoing transfer but returned no EPP code (502).</summary>
    public const string DOMAIN_EPP_CODE_UPSTREAM_FAILED = "DOMAIN_EPP_CODE_UPSTREAM_FAILED";

    /// <summary>The registrar rejected the auto-renew update request (502).</summary>
    public const string DOMAIN_AUTO_RENEW_UPSTREAM_FAILED = "DOMAIN_AUTO_RENEW_UPSTREAM_FAILED";

    /// <summary>The registrar returned no WHOIS record for the name (404).</summary>
    public const string DOMAIN_WHOIS_NOT_FOUND = "DOMAIN_WHOIS_NOT_FOUND";

    /// <summary>
    /// This installation has no active product of type <c>Domain</c> configured, so a domain
    /// renewal or transfer-in cannot be sold as an order (503). A deployment gap, not a caller
    /// error.
    /// </summary>
    public const string DOMAIN_PRODUCT_NOT_CONFIGURED = "DOMAIN_PRODUCT_NOT_CONFIGURED";

    /// <summary>
    /// A client renew order was refused because a renewal invoice for the domain cannot be released:
    /// it is overdue and bills other items too (no longer editable), or it is PaymentPending or in
    /// Collections (a payment may be in flight); the client settles that invoice first (409).
    /// </summary>
    public const string RENEWAL_INVOICE_OPEN = "RENEWAL_INVOICE_OPEN";

    // Domains — DNS and forwarding

    /// <summary>The id names no DNS record on the domain's zone (404).</summary>
    public const string DNS_RECORD_NOT_FOUND = "DNS_RECORD_NOT_FOUND";

    /// <summary>The registrar rejected adding, updating or deleting a DNS record (502).</summary>
    public const string DNS_RECORD_UPSTREAM_FAILED = "DNS_RECORD_UPSTREAM_FAILED";

    /// <summary>The registrar rejected the nameserver update request (502).</summary>
    public const string NAMESERVERS_UPSTREAM_FAILED = "NAMESERVERS_UPSTREAM_FAILED";

    /// <summary>The registrar rejected the DNS management toggle request (502).</summary>
    public const string DNS_MANAGEMENT_UPSTREAM_FAILED = "DNS_MANAGEMENT_UPSTREAM_FAILED";

    /// <summary>The registrar rejected the transfer-lock update request (502).</summary>
    public const string REGISTRAR_LOCK_UPSTREAM_FAILED = "REGISTRAR_LOCK_UPSTREAM_FAILED";

    /// <summary>The registrar rejected the WHOIS privacy update request (502).</summary>
    public const string WHOIS_PRIVACY_UPSTREAM_FAILED = "WHOIS_PRIVACY_UPSTREAM_FAILED";

    /// <summary>The registrar holds no registrant contact record for the name (404).</summary>
    public const string DOMAIN_CONTACT_NOT_FOUND = "DOMAIN_CONTACT_NOT_FOUND";

    /// <summary>The registrar rejected the registrant contact update request (502).</summary>
    public const string DOMAIN_CONTACT_UPSTREAM_FAILED = "DOMAIN_CONTACT_UPSTREAM_FAILED";

    /// <summary>The registrar rejected the email forwarding toggle request (502).</summary>
    public const string EMAIL_FORWARDING_UPSTREAM_FAILED = "EMAIL_FORWARDING_UPSTREAM_FAILED";

    /// <summary>The id names no email forwarding rule on the domain (404).</summary>
    public const string EMAIL_FORWARDING_RULE_NOT_FOUND = "EMAIL_FORWARDING_RULE_NOT_FOUND";

    /// <summary>
    /// The registrar rejected adding, updating or deleting an email forwarding rule (502).
    /// </summary>
    public const string EMAIL_FORWARDING_RULE_UPSTREAM_FAILED = "EMAIL_FORWARDING_RULE_UPSTREAM_FAILED";

    /// <summary>The id names no TLD configuration (404).</summary>
    public const string TLD_CONFIG_NOT_FOUND = "TLD_CONFIG_NOT_FOUND";

    /// <summary>A TLD configuration for the extension already exists (409).</summary>
    public const string TLD_CONFIG_ALREADY_EXISTS = "TLD_CONFIG_ALREADY_EXISTS";

    /// <summary>The registrar module named does not match a known provider (400).</summary>
    public const string TLD_REGISTRAR_MODULE_UNKNOWN = "TLD_REGISTRAR_MODULE_UNKNOWN";

    // Support — knowledge base and content

    /// <summary>The id names no knowledge base article (404).</summary>
    public const string KBARTICLE_NOT_FOUND = "KBARTICLE_NOT_FOUND";

    /// <summary>The id names no knowledge base category (404).</summary>
    public const string KBCATEGORY_NOT_FOUND = "KBCATEGORY_NOT_FOUND";

    /// <summary>The id names no announcement (404).</summary>
    public const string ANNOUNCEMENT_NOT_FOUND = "ANNOUNCEMENT_NOT_FOUND";

    /// <summary>The id names no network issue (404).</summary>
    public const string NETWORKISSUE_NOT_FOUND = "NETWORKISSUE_NOT_FOUND";

    /// <summary>
    /// The public knowledge base article read cannot serve the id (400): it either names no row
    /// at all or names an unpublished draft. Deliberately not a <c>*_NOT_FOUND</c> code -- ids are
    /// sequential and this route is anonymous, so a caller telling "missing" apart from "exists but
    /// is a draft" could map the unpublished backlog one guess at a time.
    /// </summary>
    public const string KB_PUBLISHED_ARTICLE_UNAVAILABLE = "KB_PUBLISHED_ARTICLE_UNAVAILABLE";

    // Orders

    /// <summary>
    /// The id names no order, or -- for the four checkout-payment actions a guest reaches with no
    /// credential -- the id and the payment token presented for it do not go together (404). Both
    /// causes answer alike on purpose: order ids are sequential, and a caller who could tell
    /// "no such order" apart from "not your order" could map out how many orders exist.
    /// </summary>
    public const string ORDER_NOT_FOUND = "ORDER_NOT_FOUND";

    /// <summary>
    /// The order is not Pending or Cancelled, so it may not be permanently deleted (400).
    /// </summary>
    public const string ORDER_NOT_DELETABLE = "ORDER_NOT_DELETABLE";

    /// <summary>
    /// The order carries no linked invoice yet, so there is nothing to pay or fulfil (400). Every
    /// order is linked to its invoice in the same unit of work that creates it, so this is a
    /// defensive answer for a state <c>PlaceOrder</c> never actually leaves behind.
    /// </summary>
    public const string ORDER_NOT_INVOICED = "ORDER_NOT_INVOICED";

    /// <summary>
    /// The checkout named a payment method that is not configured and switched on right now
    /// (400). The list a checkout shows and the methods this refusal checks against come from the
    /// same query, so what is shown and what is accepted never disagree.
    /// </summary>
    public const string ORDER_PAYMENT_METHOD_UNAVAILABLE = "ORDER_PAYMENT_METHOD_UNAVAILABLE";

    /// <summary>
    /// The checkout chose a currency this deployment does not offer, or has switched off (400).
    /// </summary>
    public const string ORDER_CURRENCY_UNAVAILABLE = "ORDER_CURRENCY_UNAVAILABLE";

    /// <summary>
    /// A guest checkout (no signed-in credential) is missing a field its registration needs
    /// (400). The validator already requires these of an anonymous caller, so this is the
    /// handler's own defensive answer for a command that reached it some other way.
    /// </summary>
    public const string ORDER_GUEST_DETAILS_REQUIRED = "ORDER_GUEST_DETAILS_REQUIRED";

    /// <summary>
    /// The ordered product has no price for the requested billing cycle in the payer's currency
    /// (400). The order refuses rather than converting from another currency, because a converted
    /// figure is not a price the operator set.
    /// </summary>
    public const string ORDER_PRODUCT_PRICE_UNAVAILABLE = "ORDER_PRODUCT_PRICE_UNAVAILABLE";

    /// <summary>
    /// A domain line in the order could not be priced (400): the name has no TLD, the TLD is not
    /// one this deployment sells, it is sold in a currency other than the payer's, the requested
    /// action is not one of register/transfer/renew, or the TLD has no published price for the
    /// requested period. The five causes are logged, never returned, so probing the checkout does
    /// not map out which TLDs this deployment carries.
    /// </summary>
    public const string ORDER_DOMAIN_PRICING_INVALID = "ORDER_DOMAIN_PRICING_INVALID";

    // Services

    /// <summary>
    /// The id names no client service (404). Shared by the admin surface and, now that
    /// <c>IServiceOwnership</c> answers a <see cref="Shared.Models.Result"/> instead of throwing,
    /// by every client-facing <c>My*</c> refusal too: a service belonging to somebody else, one
    /// that does not exist, and a caller with no client record all answer this code alike.
    /// </summary>
    public const string SERVICE_NOT_FOUND = "SERVICE_NOT_FOUND";

    // Services — SSH key management

    /// <summary>The pasted text is not a single, valid ED25519 public key (400).</summary>
    public const string SSH_KEY_INVALID = "SSH_KEY_INVALID";

    /// <summary>The pasted text is a private key; it is never stored or echoed (400).</summary>
    public const string SSH_PRIVATE_KEY_REJECTED = "SSH_PRIVATE_KEY_REJECTED";

    /// <summary>The key is already on the service, recorded here or present on the server (409).</summary>
    public const string SSH_KEY_ALREADY_INSTALLED = "SSH_KEY_ALREADY_INSTALLED";

    /// <summary>The service already holds the maximum number of SSH keys (409).</summary>
    public const string SSH_KEY_LIMIT_REACHED = "SSH_KEY_LIMIT_REACHED";

    /// <summary>The key id names no live key of this service; a key of another service answers the same (404).</summary>
    public const string SSH_KEY_NOT_FOUND = "SSH_KEY_NOT_FOUND";

    /// <summary>The key was issued on the service's previous server or account; nothing is sent anywhere for it (409).</summary>
    public const string SSH_KEY_STALE = "SSH_KEY_STALE";

    /// <summary>The service's state (inactive, unsupported panel, SSH not enabled on its server, shell disabled) does not allow key changes (409).</summary>
    public const string SSH_ACCESS_UNAVAILABLE = "SSH_ACCESS_UNAVAILABLE";

    /// <summary>The server's key agent failed or did not answer; the outcome is recorded on the key (503).</summary>
    public const string SSH_SERVER_UNAVAILABLE = "SSH_SERVER_UNAVAILABLE";

    /// <summary>The server's control panel module has no SSH key implementation (400).</summary>
    public const string SSH_MODULE_NOT_SUPPORTED = "SSH_MODULE_NOT_SUPPORTED";

    /// <summary>The server's SSH settings lack a value an enabled configuration or an agent call needs (400).</summary>
    public const string SSH_SETTINGS_INCOMPLETE = "SSH_SETTINGS_INCOMPLETE";

    /// <summary>An open cancellation request already exists for the service (409).</summary>
    public const string CANCELLATION_ALREADY_PENDING = "CANCELLATION_ALREADY_PENDING";

    /// <summary>The id names no cancellation request (404).</summary>
    public const string CANCELLATION_REQUEST_NOT_FOUND = "CANCELLATION_REQUEST_NOT_FOUND";

    /// <summary>The id names no product (404).</summary>
    public const string PRODUCT_NOT_FOUND = "PRODUCT_NOT_FOUND";

    /// <summary>
    /// The product is not active, or an amount was left for the handler to price and the product
    /// has no price for that cycle in the ordering client's currency (400).
    /// </summary>
    public const string PRODUCT_NOT_AVAILABLE = "PRODUCT_NOT_AVAILABLE";

    /// <summary>The service is not Pending, so it cannot be set up (400).</summary>
    public const string SERVICE_NOT_PENDING = "SERVICE_NOT_PENDING";

    /// <summary>
    /// No server of the product's module has room to provision the service right now (503) — a
    /// capacity gap, not a caller error.
    /// </summary>
    public const string NO_ELIGIBLE_SERVER = "NO_ELIGIBLE_SERVER";

    /// <summary>The provisioning provider rejected the setup request (502).</summary>
    public const string PROVISIONING_UPSTREAM_FAILED = "PROVISIONING_UPSTREAM_FAILED";

    /// <summary>The status named on an update does not match a known service status (400).</summary>
    public const string INVALID_SERVICE_STATUS = "INVALID_SERVICE_STATUS";

    // Provisioning

    /// <summary>
    /// The service exists but has never been given a provisioning reference, so there is no
    /// hosting account on any provider to act on (400).
    /// </summary>
    public const string SERVICE_NOT_PROVISIONED = "SERVICE_NOT_PROVISIONED";

    /// <summary>
    /// The service has a provisioning reference but no assigned server, or the server it names no
    /// longer exists, so no provider can be resolved for it (400).
    /// </summary>
    public const string SERVICE_NO_SERVER_ASSIGNED = "SERVICE_NO_SERVER_ASSIGNED";

    /// <summary>The provisioning provider rejected the password change (502).</summary>
    public const string SERVICE_PASSWORD_UPSTREAM_FAILED = "SERVICE_PASSWORD_UPSTREAM_FAILED";

    /// <summary>The provisioning provider rejected the package change (502).</summary>
    public const string SERVICE_PACKAGE_UPSTREAM_FAILED = "SERVICE_PACKAGE_UPSTREAM_FAILED";

    /// <summary>The provisioning provider rejected the suspension (502).</summary>
    public const string SERVICE_SUSPEND_UPSTREAM_FAILED = "SERVICE_SUSPEND_UPSTREAM_FAILED";

    /// <summary>The provisioning provider rejected the termination (502).</summary>
    public const string SERVICE_TERMINATE_UPSTREAM_FAILED = "SERVICE_TERMINATE_UPSTREAM_FAILED";

    /// <summary>The provisioning provider rejected the reactivation (502).</summary>
    public const string SERVICE_UNSUSPEND_UPSTREAM_FAILED = "SERVICE_UNSUSPEND_UPSTREAM_FAILED";

    /// <summary>The provisioning provider could not issue a control-panel single-sign-on URL (502).</summary>
    public const string SERVICE_CPANEL_SSO_UPSTREAM_FAILED = "SERVICE_CPANEL_SSO_UPSTREAM_FAILED";

    /// <summary>The provisioning provider could not return the account's credentials (502).</summary>
    public const string SERVICE_CREDENTIALS_UPSTREAM_FAILED = "SERVICE_CREDENTIALS_UPSTREAM_FAILED";

    // Admin — servers

    /// <summary>The id names no configured provisioning server (404).</summary>
    public const string SERVER_NOT_FOUND = "SERVER_NOT_FOUND";

    /// <summary>The id names no configured server group (404).</summary>
    public const string SERVER_GROUP_NOT_FOUND = "SERVER_GROUP_NOT_FOUND";

    // Admin — plugins

    /// <summary>
    /// The uploaded archive has no <c>plugin.json</c> at its root, the manifest could not be
    /// parsed, or it is missing the plugin id (400).
    /// </summary>
    public const string PLUGIN_MANIFEST_INVALID = "PLUGIN_MANIFEST_INVALID";

    /// <summary>The plugin was built against an SDK version this deployment does not support (400).</summary>
    public const string PLUGIN_SDK_VERSION_UNSUPPORTED = "PLUGIN_SDK_VERSION_UNSUPPORTED";

    /// <summary>
    /// One of the archive's entries would write outside the plugin's own directory (400). The
    /// zip-slip guard's refusal, named for what a caller can act on rather than the attack it
    /// blocks.
    /// </summary>
    public const string PLUGIN_ARCHIVE_UNSAFE = "PLUGIN_ARCHIVE_UNSAFE";

    /// <summary>The plugin id is not a bare directory name and cannot be resolved safely (400).</summary>
    public const string PLUGIN_ID_INVALID = "PLUGIN_ID_INVALID";

    /// <summary>No plugin with that id is installed (404).</summary>
    public const string PLUGIN_NOT_FOUND = "PLUGIN_NOT_FOUND";

    // Admin — integrations

    /// <summary>The slug names neither a built-in integration nor a loaded plugin (404).</summary>
    public const string INTEGRATION_NOT_FOUND = "INTEGRATION_NOT_FOUND";

    // Products

    /// <summary>The id names no product group (404).</summary>
    public const string PRODUCT_GROUP_NOT_FOUND = "PRODUCT_GROUP_NOT_FOUND";

    /// <summary>The id names no product specification line (404).</summary>
    public const string PRODUCT_FEATURE_NOT_FOUND = "PRODUCT_FEATURE_NOT_FOUND";

    /// <summary>
    /// The product already has a specification line under this label; the storefront's comparison
    /// table keys its rows on the label and would render only the first one (400).
    /// </summary>
    public const string PRODUCT_FEATURE_LABEL_DUPLICATE = "PRODUCT_FEATURE_LABEL_DUPLICATE";

    // Slides

    /// <summary>The id names no homepage slide (404).</summary>
    public const string SLIDE_NOT_FOUND = "SLIDE_NOT_FOUND";

    /// <summary>An uploaded slide image is not one of the media types this deployment accepts (400).</summary>
    public const string SLIDE_IMAGE_TYPE_INVALID = "SLIDE_IMAGE_TYPE_INVALID";

    /// <summary>An uploaded slide image is over the byte ceiling this deployment enforces (400).</summary>
    public const string SLIDE_IMAGE_EXCEEDS_LIMIT = "SLIDE_IMAGE_EXCEEDS_LIMIT";

    // Storefront content

    /// <summary>The id names no testimonial (404).</summary>
    public const string TESTIMONIAL_NOT_FOUND = "TESTIMONIAL_NOT_FOUND";

    /// <summary>The id names no partner (404).</summary>
    public const string PARTNER_NOT_FOUND = "PARTNER_NOT_FOUND";

    /// <summary>The id names no server location (404).</summary>
    public const string LOCATION_NOT_FOUND = "LOCATION_NOT_FOUND";

    /// <summary>The id names no portfolio project (404).</summary>
    public const string PROJECT_NOT_FOUND = "PROJECT_NOT_FOUND";

    /// <summary>The id names no frequently asked question (404).</summary>
    public const string FAQ_NOT_FOUND = "FAQ_NOT_FOUND";

    /// <summary>Another portfolio project already uses the slug (409, via the <c>_EXISTS</c> suffix).</summary>
    public const string PROJECT_SLUG_EXISTS = "PROJECT_SLUG_EXISTS";

    // Migration

    /// <summary>The id or key names no migration job (404).</summary>
    public const string MIGRATION_JOB_NOT_FOUND = "MIGRATION_JOB_NOT_FOUND";

    /// <summary>
    /// The job is not <c>Pending</c>, so a pull cannot be started for it — it is already running,
    /// already finished, or already failed (400).
    /// </summary>
    public const string MIGRATION_JOB_NOT_PENDING = "MIGRATION_JOB_NOT_PENDING";

    /// <summary>
    /// The job has already completed or failed, so it cannot accept another batch (409, via the
    /// <c>ALREADY</c> token).
    /// </summary>
    public const string MIGRATION_JOB_ALREADY_FINISHED = "MIGRATION_JOB_ALREADY_FINISHED";

    /// <summary>
    /// The migration plugin's files are not present on this deployment, so there is nothing to
    /// download (404).
    /// </summary>
    public const string MIGRATION_PLUGIN_NOT_FOUND = "MIGRATION_PLUGIN_NOT_FOUND";

    /// <summary>
    /// The source system — a port this deployment does not own — could not be reached or refused
    /// the connection test (502). No detail from the source ever reaches the response.
    /// </summary>
    public const string MIGRATION_CONNECTION_UPSTREAM_FAILED = "MIGRATION_CONNECTION_UPSTREAM_FAILED";

    // Admin — settings

    /// <summary>The id names no configuration setting (404). Replaces a bare thrown
    /// <c>InvalidOperationException</c> (400); the admin not-found ruling moves it to 404.</summary>
    public const string SETTING_NOT_FOUND = "SETTING_NOT_FOUND";

    /// <summary>
    /// The written value is outside the key's fixed vocabulary, or does not match the shape the
    /// key requires (400). <c>detail</c> carries a fixed sentence; the per-key wording — the
    /// values or the shape actually accepted — travels in <c>errors[key]</c> instead, because it
    /// depends on the row being written and cannot be a single resx sentence.
    /// </summary>
    public const string INVALID_SETTING_VALUE = "INVALID_SETTING_VALUE";

    // Admin — branding

    /// <summary>
    /// The uploaded bytes are not an image this deployment will store — not an image at all, an
    /// unsupported format, or a declared type that disagrees with the real bytes (400). The
    /// precise cause is logged, never returned.
    /// </summary>
    public const string BRANDING_IMAGE_INVALID = "BRANDING_IMAGE_INVALID";

    /// <summary>
    /// The upload is over the byte ceiling, or decodes to more pixels than this deployment will
    /// rasterise (413, via the <c>_TOO_LARGE</c> suffix). Which ceiling was hit is logged, never
    /// returned.
    /// </summary>
    public const string BRANDING_IMAGE_TOO_LARGE = "BRANDING_IMAGE_TOO_LARGE";

    // Auth — account provisioning refused by an admin action

    /// <summary>
    /// A guest checkout, an admin "create client" or a migration pull tried to create an account
    /// where an SSO owns the accounts (400). Answered from <c>PlaceOrderHandler</c>'s guest-checkout
    /// branch and <c>AdminCreateClientHandler</c>'s new-account branch; a migration pull logs the
    /// same refusal against the job's own log instead of answering an HTTP caller.
    /// </summary>
    public const string USER_PROVISIONING_NOT_ALLOWED_CREATE_ACCOUNT = "USER_PROVISIONING_NOT_ALLOWED_CREATE_ACCOUNT";

    /// <summary>
    /// An admin tried to rename a person where an SSO owns the accounts (400). Answered from
    /// <c>UpdateUserHandler</c>'s own call.
    /// </summary>
    public const string USER_PROVISIONING_NOT_ALLOWED_CHANGE_NAME = "USER_PROVISIONING_NOT_ALLOWED_CHANGE_NAME";

    /// <summary>
    /// An admin tried to change a person's sign-in address where an SSO owns the accounts (400).
    /// Answered from <c>UpdateUserHandler</c> and from <c>UpdateClientHandler</c>'s own call to the
    /// same provisioning method.
    /// </summary>
    public const string USER_PROVISIONING_NOT_ALLOWED_CHANGE_EMAIL = "USER_PROVISIONING_NOT_ALLOWED_CHANGE_EMAIL";

    /// <summary>An admin tried to delete an account where an SSO owns the accounts (400).</summary>
    public const string USER_PROVISIONING_NOT_ALLOWED_DELETE_ACCOUNT = "USER_PROVISIONING_NOT_ALLOWED_DELETE_ACCOUNT";

    /// <summary>An admin tried to set a person's password directly where an SSO owns the accounts (400).</summary>
    public const string USER_PROVISIONING_NOT_ALLOWED_SET_PASSWORD = "USER_PROVISIONING_NOT_ALLOWED_SET_PASSWORD";

    /// <summary>
    /// An admin tried to issue a password-reset link where an SSO owns the accounts (400).
    /// </summary>
    public const string USER_PROVISIONING_NOT_ALLOWED_ISSUE_PASSWORD_RESET =
        "USER_PROVISIONING_NOT_ALLOWED_ISSUE_PASSWORD_RESET";

    /// <summary>
    /// Something tried to confirm an email address where an SSO owns the accounts (400).
    /// </summary>
    public const string USER_PROVISIONING_NOT_ALLOWED_CONFIRM_EMAIL =
        "USER_PROVISIONING_NOT_ALLOWED_CONFIRM_EMAIL";

    /// <summary>
    /// Something tried to issue an email-confirmation token where an SSO owns the accounts (400).
    /// </summary>
    public const string USER_PROVISIONING_NOT_ALLOWED_ISSUE_EMAIL_CONFIRMATION =
        "USER_PROVISIONING_NOT_ALLOWED_ISSUE_EMAIL_CONFIRMATION";

    // Domain aggregate refusals (A6 Task 8). The literal string is declared once, on the
    // Domain-side <c>&lt;Feature&gt;Errors</c> class each aggregate throws with — Domain may not
    // reference this Application-layer class, so these constants alias that value rather than
    // duplicating it, purely so ResourceCoverageTests (which reflects over this class only) still
    // requires a sentence for each one in every language.

    /// <summary>The base currency cannot be disabled (400) — <see cref="Domain.Billing.Currency.Disable"/>.</summary>
    public const string CURRENCY_BASE_DISABLE_NOT_ALLOWED = Domain.Billing.BillingErrors.CurrencyBaseDisableNotAllowed;

    /// <summary>Only a Draft invoice may be published (400) — <see cref="Domain.Billing.Invoice.Publish"/>.</summary>
    public const string INVOICE_NOT_DRAFT = Domain.Billing.BillingErrors.InvoiceNotDraft;

    /// <summary>The invoice is no longer editable (400) — <see cref="Domain.Billing.Invoice.AddItem"/>.</summary>
    public const string INVOICE_NOT_EDITABLE = Domain.Billing.BillingErrors.InvoiceNotEditable;

    /// <summary>
    /// The id names no line item on this invoice (404) — <see cref="Domain.Billing.Invoice.RemoveItem"/>
    /// and <see cref="Domain.Billing.Invoice.UpdateItem"/>.
    /// </summary>
    public const string INVOICE_ITEM_NOT_FOUND = Domain.Billing.BillingErrors.InvoiceItemNotFound;

    /// <summary>
    /// The invoice is not eligible to be marked overdue (400) — <see cref="Domain.Billing.Invoice.MarkOverdue"/>.
    /// </summary>
    public const string INVOICE_NOT_UNPAID = Domain.Billing.BillingErrors.InvoiceNotUnpaid;

    /// <summary>A paid invoice cannot be cancelled (400) — <see cref="Domain.Billing.Invoice.Cancel"/>.</summary>
    public const string INVOICE_CANCEL_NOT_ALLOWED = Domain.Billing.BillingErrors.InvoiceCancelNotAllowed;

    /// <summary>
    /// The invoice must be Paid for this action (400) — <see cref="Domain.Billing.Invoice.MarkUnpaid"/>
    /// and <see cref="Domain.Billing.Invoice.Refund"/>.
    /// </summary>
    public const string INVOICE_NOT_PAID = Domain.Billing.BillingErrors.InvoiceNotPaid;

    /// <summary>The quote is no longer editable (400) — <see cref="Domain.Billing.Quote.AddItem"/>.</summary>
    public const string QUOTE_NOT_EDITABLE = Domain.Billing.BillingErrors.QuoteNotEditable;

    /// <summary>The quote's stage does not allow acceptance (400) — <see cref="Domain.Billing.Quote.Accept"/>.</summary>
    public const string QUOTE_NOT_ACCEPTABLE = Domain.Billing.BillingErrors.QuoteNotAcceptable;

    /// <summary>
    /// The id names no line item on this quote (404) — <see cref="Domain.Billing.Quote.RemoveItem"/>
    /// and <see cref="Domain.Billing.Quote.UpdateItem"/>.
    /// </summary>
    public const string QUOTE_ITEM_NOT_FOUND = Domain.Billing.BillingErrors.QuoteItemNotFound;

    /// <summary>
    /// The id names no contact on this client (404) — <see cref="Domain.Clients.Client.RemoveContact"/>
    /// and <see cref="Domain.Clients.Client.UpdateContact"/>.
    /// </summary>
    public const string CLIENT_CONTACT_NOT_FOUND = Domain.Clients.ClientsErrors.ClientContactNotFound;

    /// <summary>
    /// The account owner cannot also be added as an additional user (400) —
    /// <see cref="Domain.Clients.Client.AddUser"/>.
    /// </summary>
    public const string CLIENT_USER_IS_OWNER = Domain.Clients.ClientsErrors.ClientUserIsOwner;

    /// <summary>The user is already linked to this client (400) — <see cref="Domain.Clients.Client.AddUser"/>.</summary>
    public const string CLIENT_USER_LINK_DUPLICATE = Domain.Clients.ClientsErrors.ClientUserLinkDuplicate;

    /// <summary>
    /// The user is not linked to this client (400) — <see cref="Domain.Clients.Client.RemoveUser"/> and
    /// <see cref="Domain.Clients.Client.UpdateUserPermissions"/>.
    /// </summary>
    public const string CLIENT_USER_NOT_LINKED = Domain.Clients.ClientsErrors.ClientUserNotLinked;

    /// <summary>
    /// The signed-in user lacks the permission a client portal action needs, or has no account
    /// relationship at all (403, an exact <c>StatusMapping</c> row). Raised by the permission filter.
    /// </summary>
    public const string CLIENT_PERMISSION_DENIED = Domain.Clients.ClientsErrors.ClientPermissionDenied;

    /// <summary>
    /// The named user is already the account owner (400) — <see cref="Domain.Clients.Client.TransferOwnership"/>.
    /// </summary>
    public const string CLIENT_TRANSFER_SAME_OWNER = Domain.Clients.ClientsErrors.ClientTransferSameOwner;

    /// <summary>
    /// The deduction exceeds the client's credit balance (400) — <see cref="Domain.Clients.Client.DeductCredit"/>.
    /// </summary>
    public const string CLIENT_CREDIT_INSUFFICIENT = Domain.Clients.ClientsErrors.ClientCreditInsufficient;

    /// <summary>The account is not in a state that can be suspended (400) — <see cref="Domain.Clients.Client.Suspend"/>.</summary>
    public const string CLIENT_SUSPEND_NOT_ALLOWED = Domain.Clients.ClientsErrors.ClientSuspendNotAllowed;

    /// <summary>The account is already closed (400) — <see cref="Domain.Clients.Client.Close"/>.</summary>
    public const string CLIENT_CLOSE_NOT_ALLOWED = Domain.Clients.ClientsErrors.ClientCloseNotAllowed;

    /// <summary>A closed account cannot be reactivated this way (400) — <see cref="Domain.Clients.Client.Activate"/>.</summary>
    public const string CLIENT_ACTIVATE_NOT_ALLOWED = Domain.Clients.ClientsErrors.ClientActivateNotAllowed;

    /// <summary>
    /// The domain's status does not allow cancellation (400) — <see cref="Domain.Domains.RegisteredDomain.Cancel"/>.
    /// </summary>
    public const string DOMAIN_CANCEL_NOT_ALLOWED = Domain.Domains.DomainsErrors.DomainCancelNotAllowed;

    /// <summary>
    /// The domain's status does not allow renewal (400) — <see cref="Domain.Domains.RegisteredDomain.Renew"/>.
    /// </summary>
    public const string DOMAIN_RENEW_NOT_ALLOWED = Domain.Domains.DomainsErrors.DomainRenewNotAllowed;

    /// <summary>
    /// The id names no DNS record on this domain (404) —
    /// <see cref="Domain.Domains.RegisteredDomain.UpdateDnsRecord"/> and
    /// <see cref="Domain.Domains.RegisteredDomain.RemoveDnsRecord"/>.
    /// </summary>
    public const string DOMAIN_DNS_RECORD_NOT_FOUND = Domain.Domains.DomainsErrors.DomainDnsRecordNotFound;

    /// <summary>
    /// The id names no email forwarding rule on this domain (404) —
    /// <see cref="Domain.Domains.RegisteredDomain.UpdateForwardingRule"/> and
    /// <see cref="Domain.Domains.RegisteredDomain.RemoveForwardingRule"/>.
    /// </summary>
    public const string DOMAIN_FORWARDING_RULE_NOT_FOUND = Domain.Domains.DomainsErrors.DomainForwardingRuleNotFound;

    /// <summary>
    /// The order is not Pending (400) — <see cref="Domain.Orders.Order.Accept"/>,
    /// <see cref="Domain.Orders.Order.Cancel"/> and <see cref="Domain.Orders.Order.MarkFraud"/>.
    /// </summary>
    public const string ORDER_NOT_PENDING = Domain.Orders.OrdersErrors.OrderNotPending;

    /// <summary>The service is not Active (400) — <see cref="Domain.Services.ClientService.Suspend"/>.</summary>
    public const string SERVICE_NOT_ACTIVE = Domain.Services.ServicesErrors.ServiceNotActive;

    /// <summary>The service is already terminated (400) — <see cref="Domain.Services.ClientService.Terminate"/>.</summary>
    public const string SERVICE_TERMINATE_NOT_ALLOWED = Domain.Services.ServicesErrors.ServiceTerminateNotAllowed;

    /// <summary>The service is not Suspended (400) — <see cref="Domain.Services.ClientService.Unsuspend"/>.</summary>
    public const string SERVICE_NOT_SUSPENDED = Domain.Services.ServicesErrors.ServiceNotSuspended;

    /// <summary>A closed ticket does not accept new replies (400) — <see cref="Domain.Support.Ticket.AddReply"/>.</summary>
    public const string TICKET_REPLY_NOT_ALLOWED = Domain.Support.SupportErrors.TicketReplyNotAllowed;

    /// <summary>The ticket is already closed (400) — <see cref="Domain.Support.Ticket.Close"/>.</summary>
    public const string TICKET_CLOSE_NOT_ALLOWED = Domain.Support.SupportErrors.TicketCloseNotAllowed;

    /// <summary>The sort order of a question is negative (400).</summary>
    public const string FAQ_SORT_ORDER_NEGATIVE = Domain.Faqs.FaqErrors.SortOrderNegative;

    /// <summary>The sort order of a partner is negative (400).</summary>
    public const string PARTNER_SORT_ORDER_NEGATIVE = Domain.Partners.PartnerErrors.SortOrderNegative;

    /// <summary>The sort order of a server location is negative (400).</summary>
    public const string LOCATION_SORT_ORDER_NEGATIVE = Domain.Locations.ServerLocationErrors.SortOrderNegative;

    /// <summary>The country code of a server location is not two letters (400).</summary>
    public const string LOCATION_COUNTRY_CODE_INVALID = Domain.Locations.ServerLocationErrors.CountryCodeInvalid;

    /// <summary>The sort order of a project is negative (400).</summary>
    public const string PROJECT_SORT_ORDER_NEGATIVE = Domain.Projects.ProjectErrors.SortOrderNegative;

    /// <summary>A project's slug is empty (400).</summary>
    public const string PROJECT_SLUG_REQUIRED = Domain.Projects.ProjectErrors.SlugRequired;

    /// <summary>The sort order of a testimonial is negative (400).</summary>
    public const string TESTIMONIAL_SORT_ORDER_NEGATIVE = Domain.Testimonials.TestimonialErrors.SortOrderNegative;

    /// <summary>A testimonial's name is empty (400).</summary>
    public const string TESTIMONIAL_NAME_REQUIRED = Domain.Testimonials.TestimonialErrors.NameRequired;

    /// <summary>A testimonial's rating is outside 1 to 5 (400).</summary>
    public const string TESTIMONIAL_RATING_OUT_OF_RANGE = Domain.Testimonials.TestimonialErrors.RatingOutOfRange;
}
