/**
 * GET /uploads/**
 *
 * Proxies uploaded file requests to the backend API server.
 * Slide images are stored on the API server's wwwroot/uploads/ directory
 * and need to be accessible from the client portal's domain.
 */
export default defineEventHandler(async (event) => {
  const path = getRouterParam(event, 'path')
  return proxyRequest(event, `${getApiUrl()}/uploads/${path}`)
})
