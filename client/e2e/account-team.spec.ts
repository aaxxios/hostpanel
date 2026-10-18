/**
 * Flow: the account's team.
 *
 * The tab lists the owner (who cannot be removed) and the members; removing a member asks first and
 * then reaches the backend; an invitation carries the invitee's name, address and permissions, and
 * a refused invitation shows the backend's sentence.
 */
import { expect, test } from './support/test'
import { INVITE, TEAM_MEMBER, TEAM_OWNER } from './support/fixtures.mjs'
import { resetStub, scriptStub, signIn, stubRequests, waitForHydration } from './support/helpers'

test.beforeEach(async ({ page, context, request, baseURL }) => {
  await resetStub(request)
  await signIn(context, baseURL ?? 'http://127.0.0.1:3100')

  await page.goto('/client/account')
  await waitForHydration(page)
  await page.getByTestId('ui-tab-users').click()
  await expect(page.getByTestId('account-team-tab')).toBeVisible()
})

test('the owner is listed without a remove action and a member has one', async ({ page }) => {
  await expect(page.getByTestId(`account-team-member-${TEAM_OWNER.userId}`)).toContainText(TEAM_OWNER.email)
  await expect(page.getByTestId(`account-team-remove-${TEAM_OWNER.userId}`)).toHaveCount(0)

  await expect(page.getByTestId(`account-team-member-${TEAM_MEMBER.userId}`)).toContainText(TEAM_MEMBER.email)
  await expect(page.getByTestId(`account-team-remove-${TEAM_MEMBER.userId}`)).toBeVisible()
})

test('removing a member asks first, then reaches the backend and the member leaves the list', async ({ page, request }) => {
  await page.getByTestId(`account-team-remove-${TEAM_MEMBER.userId}`).click()

  // Nothing is removed until the dialog is confirmed.
  await expect(page.getByTestId('ui-confirm-dialog')).toBeVisible()
  expect((await stubRequests(request)).some(r => r.method === 'DELETE')).toBe(false)

  await page.getByTestId('ui-confirm-confirm').click()

  await expect(page.getByTestId(`account-team-member-${TEAM_MEMBER.userId}`)).toHaveCount(0)
  await expect(page.getByTestId(`account-team-member-${TEAM_OWNER.userId}`)).toBeVisible()
  expect((await stubRequests(request)).some(r => r.method === 'DELETE' && r.path === `/api/me/users/${TEAM_MEMBER.userId}`)).toBe(true)
})

test('an invitation is sent with the name, address and full access, and the form empties', async ({ page, request }) => {
  await page.getByTestId('account-invite-firstname').fill(INVITE.firstName)
  await page.getByTestId('account-invite-lastname').fill(INVITE.lastName)
  await page.getByTestId('account-invite-email').fill(INVITE.email)
  await page.getByTestId('account-invite-submit').click()

  await expect(page.getByTestId('account-invite-form').getByTestId('ui-form-success')).toBeVisible()
  await expect(page.getByTestId('account-invite-email')).toHaveValue('')

  const sent = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === '/api/me/users/invite')
  expect(sent?.body).toMatchObject({ email: INVITE.email, firstName: INVITE.firstName, lastName: INVITE.lastName, permissions: 'all' })
})

test('choosing the permissions offers the list and sends only the chosen ones', async ({ page, request }) => {
  await page.getByTestId('account-invite-perm-choose').click()
  await expect(page.getByTestId('account-invite-perms')).toBeVisible()

  await page.getByTestId('account-invite-firstname').fill(INVITE.firstName)
  await page.getByTestId('account-invite-lastname').fill(INVITE.lastName)
  await page.getByTestId('account-invite-email').fill(INVITE.email)
  await page.getByTestId('account-invite-submit').click()

  await expect(page.getByTestId('account-invite-form').getByTestId('ui-form-success')).toBeVisible()
  const sent = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === '/api/me/users/invite')
  // No permission was ticked, so the list is empty -- not "all".
  expect(sent?.body).toMatchObject({ email: INVITE.email, permissions: '' })
})

test('a refused invitation shows the backend sentence and keeps what was typed', async ({ page, request }) => {
  await scriptStub(request, { method: 'POST', path: '/api/me/users/invite', status: 400, code: 'INVITE_REFUSED', detail: 'E2E: this person is already invited.' })

  await page.getByTestId('account-invite-firstname').fill(INVITE.firstName)
  await page.getByTestId('account-invite-lastname').fill(INVITE.lastName)
  await page.getByTestId('account-invite-email').fill(INVITE.email)
  await page.getByTestId('account-invite-submit').click()

  await expect(page.getByTestId('account-invite-form').getByTestId('ui-form-error')).toContainText('E2E: this person is already invited.')
  await expect(page.getByTestId('account-invite-email')).toHaveValue(INVITE.email)
})
