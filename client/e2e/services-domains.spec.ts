/**
 * Flow: the customer's services and domains.
 *
 * The lists show what the customer owns. A service that is already set up links to its detail
 * page; one that still needs its control-panel account says so and links to the setup wizard. A
 * domain row offers its management link. (The detail pages have their own specs.)
 */
import { expect, test } from './support/test'
import { DOMAIN, SERVICE, SERVICE_PENDING } from './support/fixtures.mjs'
import { resetStub, signIn, waitForHydration } from './support/helpers'

test.beforeEach(async ({ context, request, baseURL }) => {
  await resetStub(request)
  await signIn(context, baseURL ?? 'http://127.0.0.1:3100')
})

test('the services list shows a set-up service with no setup call to action', async ({ page }) => {
  await page.goto('/client/services')
  await waitForHydration(page)

  await expect(page.getByTestId('services-list')).toBeVisible()
  await expect(page.getByTestId(`service-card-${SERVICE.id}`)).toBeVisible()
  await expect(page.getByTestId(`service-card-${SERVICE.id}`).getByTestId('service-needs-setup')).toHaveCount(0)
  await expect(page.getByTestId(`service-card-${SERVICE.id}`)).toHaveAttribute('href', `/client/services/${SERVICE.id}`)
})

test('a service without a control-panel account is marked and leads to the setup wizard', async ({ page }) => {
  await page.goto('/client/services')
  await waitForHydration(page)

  await expect(page.getByTestId(`service-card-${SERVICE_PENDING.id}`).getByTestId('service-needs-setup')).toBeVisible()
  await expect(page.getByTestId(`service-card-${SERVICE_PENDING.id}`)).toHaveAttribute('href', `/client/services/${SERVICE_PENDING.id}/setup`)
})

test('the domains list shows the domain and its management link', async ({ page }) => {
  await page.goto('/client/domains')
  await waitForHydration(page)

  await expect(page.getByTestId('domains-table')).toBeVisible()
  await expect(page.getByTestId(`domain-row-${DOMAIN.id}`)).toContainText(DOMAIN.name)
  await expect(page.getByTestId(`domain-manage-${DOMAIN.id}`)).toHaveAttribute('href', `/client/domains/${DOMAIN.id}`)
})
