/**
 * The query parameter that carries a typed domain name from a search bar to the domains page.
 *
 * One name for the writer (the home page search bar) and every reader (the domains page and the
 * classic domains template), so the link a search builds is always the one the page understands.
 *
 * @module constants/domainSearchQuery
 */

/** Query parameter of `/domains` holding the name to look up, e.g. `/domains?domain=example.com`. */
export const DOMAIN_SEARCH_QUERY_PARAM = 'domain'
