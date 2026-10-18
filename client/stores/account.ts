/**
 * Pinia store for the changes made on the client account page: profile, team members,
 * contacts, two-factor and saved payment methods.
 *
 * The page renders this state and branches on the returned outcomes; no API call and no
 * `try/catch` lives in the view. Every refusal is kept as the API's own sentence, read through
 * `lib/errorHandler.ts`. URLs and transport belong to {@link useClientApi}, {@link useBillingApi}
 * and {@link useAuthApi}.
 *
 * @module stores/account
 */

import { defineStore } from 'pinia'
import QRCode from 'qrcode'
import { useAuthApi } from '~/composables/apis/useAuthApi'
import { useBillingApi } from '~/composables/apis/useBillingApi'
import { useClientApi } from '~/composables/apis/useClientApi'
import { useClientStore } from '~/stores/client'
import { ActionOutcome } from '~/types/actionoutcome'
import { ContactSaveOutcome } from '~/types/contactsaveoutcome'
import type { AccountContact } from '~/types/accountcontact'
import type { AccountContactInput } from '~/types/accountcontactinput'
import type { ClientEmailSummary } from '~/types/clientemailsummary'
import type { GatewayMethod } from '~/types/gatewaymethod'
import type { PaymentMethod } from '~/types/payment'
import type { TeamMember } from '~/types/teammember'
import { TwoFactorMode } from '~/types/twofactormode'
import type { TwoFactorSetup } from '~/types/twofactorsetup'
import { apiErrorMessage, apiFieldErrors } from '~/lib/errorHandler'

/**
 * Store behind `pages/client/account/index.vue`.
 *
 * @returns The loading/error state per section and the actions that change the account.
 */
export const useAccountStore = defineStore('account', () => {
  const clientApi = useClientApi()
  const billingApi = useBillingApi()
  const authApi = useAuthApi()

  /**
   * Field-level messages of the last refused form post (profile, contact, payment method), keyed by camelCase field name (`UiField` renders
   * `fieldErrors[name]`); empty when the refusal was not about fields or nothing was refused.
   */
  const fieldErrors = ref<Record<string, string>>({})

  // ── Reads ──────────────────────────────────────────────────────────────────
  // Each list below is an optional part of the page: a failed read leaves it empty and its
  // error sentence set, so a tab can say "we could not ask" instead of "you have none".

  /** The team members with access to the account. */
  const users = ref<TeamMember[]>([])
  /** Whether the team list is being read. */
  const usersLoading = ref(false)

  /** The account's sub-contacts. */
  const contacts = ref<AccountContact[]>([])
  /** Whether the contact list is being read. */
  const contactsLoading = ref(false)

  /** The mail the system has sent the account. */
  const emails = ref<ClientEmailSummary[]>([])
  /** Whether the sent-mail list is being read. */
  const emailsLoading = ref(false)

  /** The saved cards and bank accounts. */
  const paymentMethods = ref<PaymentMethod[]>([])
  /** Whether the saved payment methods are being read. */
  const paymentLoading = ref(false)
  /** The API's sentence when the saved payment methods could not be read; null when they could. */
  const paymentLoadError = ref<string | null>(null)

  /** The gateways the profile can name as its default payment method. */
  const gatewayMethods = ref<GatewayMethod[]>([])

  /**
   * Reads the team members.
   *
   * @returns Nothing; a failure leaves the list empty.
   */
  const loadUsers = async (): Promise<void> => {
    usersLoading.value = true
    try {
      users.value = await clientApi.fetchUsers()
    } catch {
      users.value = []
    } finally {
      usersLoading.value = false
    }
  }

  /**
   * Reads the sub-contacts.
   *
   * @returns Nothing; a failure leaves the list empty.
   */
  const loadContacts = async (): Promise<void> => {
    contactsLoading.value = true
    try {
      contacts.value = await clientApi.fetchContacts()
    } catch {
      contacts.value = []
    } finally {
      contactsLoading.value = false
    }
  }

  /**
   * Reads the sent-mail list.
   *
   * @returns Nothing; a failure leaves the list empty.
   */
  const loadEmails = async (): Promise<void> => {
    emailsLoading.value = true
    try {
      emails.value = await clientApi.fetchEmails()
    } catch {
      emails.value = []
    } finally {
      emailsLoading.value = false
    }
  }

  /**
   * Reads the saved payment methods. A failure is kept as {@link paymentLoadError}, because an
   * unreadable list and an account with no saved cards must not look the same.
   *
   * @returns Nothing.
   */
  const loadPaymentMethods = async (): Promise<void> => {
    paymentLoading.value = true
    paymentLoadError.value = null
    try {
      paymentMethods.value = await billingApi.fetchPaymentMethods()
    } catch (e: unknown) {
      paymentMethods.value = []
      paymentLoadError.value = apiErrorMessage(e)
    } finally {
      paymentLoading.value = false
    }
  }

  /**
   * Reads the gateways the profile can name as its default; a failure leaves the list empty.
   *
   * @returns Nothing.
   */
  const loadGatewayMethods = async (): Promise<void> => {
    try {
      gatewayMethods.value = await billingApi.fetchGatewayMethods()
    } catch {
      gatewayMethods.value = []
    }
  }

  // ── Profile ────────────────────────────────────────────────────────────────

  /** Whether the profile save is in flight. */
  const saving = ref(false)

  /** The API's sentence for the last refused profile save; empty when there is none. */
  const saveError = ref('')

  /**
   * Saves the profile and reloads it so the client store agrees with the server.
   *
   * @param profile - The form fields plus the decoded email preferences.
   * @returns How the save ended; on `Failed`, {@link saveError} holds the sentence.
   */
  const saveProfile = async (profile: Record<string, unknown>): Promise<ActionOutcome> => {
    saving.value = true
    saveError.value = ''
    fieldErrors.value = {}
    try {
      await clientApi.updateMe(profile)
      await useClientStore().fetchUser(true)
      return ActionOutcome.Succeeded
    } catch (e: unknown) {
      saveError.value = apiErrorMessage(e)
      fieldErrors.value = apiFieldErrors(e)
      return ActionOutcome.Failed
    } finally {
      saving.value = false
    }
  }

  // ── Team members ───────────────────────────────────────────────────────────

  /** Whether an invitation is being sent. */
  const inviting = ref(false)

  /** The API's sentence for the last refused invite or removal; empty when there is none. */
  const inviteError = ref('')

  /** Id of the team member being removed, or null. */
  const removingUserId = ref<string | null>(null)

  /**
   * Sends a team invitation.
   *
   * @param invite - Email, names and permissions of the invitee.
   * @returns How it ended; on `Failed`, {@link inviteError} holds the sentence.
   */
  const inviteUser = async (invite: Record<string, unknown>): Promise<ActionOutcome> => {
    inviting.value = true
    inviteError.value = ''
    try {
      await clientApi.inviteUser(invite)
      return ActionOutcome.Succeeded
    } catch (e: unknown) {
      inviteError.value = apiErrorMessage(e)
      return ActionOutcome.Failed
    } finally {
      inviting.value = false
    }
  }

  /**
   * Removes a team member's access.
   *
   * @param id - Id of the member.
   * @returns How it ended; on `Failed`, {@link inviteError} holds the sentence.
   */
  const removeUser = async (id: string): Promise<ActionOutcome> => {
    removingUserId.value = id
    inviteError.value = ''
    try {
      await clientApi.removeUser(id)
      return ActionOutcome.Succeeded
    } catch (e: unknown) {
      inviteError.value = apiErrorMessage(e)
      return ActionOutcome.Failed
    } finally {
      removingUserId.value = null
    }
  }

  // ── Contacts ───────────────────────────────────────────────────────────────

  /** Whether a contact save is in flight. */
  const contactSaving = ref(false)

  /** The API's sentence for the last refused contact save; empty when there is none. */
  const contactError = ref('')

  /**
   * Creates a contact, or updates one when an id is given.
   *
   * @param id - Contact id, or null to create.
   * @param body - The contact, in the backend's request shape.
   * @returns `Invalid` when a name is missing (no request made), otherwise how the call ended.
   */
  const saveContact = async (id: number | null, body: AccountContactInput): Promise<ContactSaveOutcome> => {
    contactError.value = ''
    fieldErrors.value = {}
    if (!body.firstName || !body.lastName) {
      return ContactSaveOutcome.Invalid
    }
    contactSaving.value = true
    try {
      if (id === null) {
        await clientApi.createContact(body)
      } else {
        await clientApi.updateContact(id, body)
      }
      return ContactSaveOutcome.Saved
    } catch (e: unknown) {
      contactError.value = apiErrorMessage(e)
      fieldErrors.value = apiFieldErrors(e)
      return ContactSaveOutcome.Failed
    } finally {
      contactSaving.value = false
    }
  }

  /**
   * Deletes a contact.
   *
   * @param id - Contact id.
   * @returns How it ended; a failure is not surfaced beyond the outcome.
   */
  const removeContact = async (id: number): Promise<ActionOutcome> => {
    try {
      await clientApi.removeContact(id)
      return ActionOutcome.Succeeded
    } catch {
      return ActionOutcome.Failed
    }
  }

  // ── Two-factor ─────────────────────────────────────────────────────────────

  /** Whether enrolment is being started. */
  const tfaLoading = ref(false)

  /** The sentence for a failed enrolment start; null when there is none. */
  const tfaError = ref<string | null>(null)

  /** Whether the dialog's code is being submitted. */
  const tfaSaving = ref(false)

  /** The sentence for a refused code; empty when there is none. */
  const tfaModalError = ref('')

  /**
   * Starts two-factor enrolment and renders the QR code locally.
   *
   * @returns The secret, URI and QR data URL, or null on failure ({@link tfaError} then holds
   * the sentence).
   */
  const startTwoFactorSetup = async (): Promise<TwoFactorSetup | null> => {
    tfaLoading.value = true
    tfaError.value = null
    try {
      const data = await authApi.setupTwoFactor()
      const qrDataUrl = await QRCode.toDataURL(data.qrCodeUri, { width: 180, margin: 2 })
      return { secret: data.secret, qrCodeUri: data.qrCodeUri, qrDataUrl }
    } catch (e: unknown) {
      tfaError.value = apiErrorMessage(e)
      return null
    } finally {
      tfaLoading.value = false
    }
  }

  /**
   * Submits the dialog's code for the given direction and refreshes the profile on success.
   *
   * @param mode - Whether to switch two-factor on or off.
   * @param code - The authenticator code.
   * @returns How it ended; on `Failed`, {@link tfaModalError} holds the sentence.
   */
  const submitTwoFactor = async (mode: TwoFactorMode, code: string): Promise<ActionOutcome> => {
    tfaSaving.value = true
    tfaModalError.value = ''
    try {
      await (mode === TwoFactorMode.Enable ? authApi.enableTwoFactor(code) : authApi.disableTwoFactor(code))
      // The flag lives on the profile, so refresh it rather than leaving the store disagreeing.
      await useClientStore().fetchUser(true)
      return ActionOutcome.Succeeded
    } catch (e: unknown) {
      tfaModalError.value = apiErrorMessage(e)
      return ActionOutcome.Failed
    } finally {
      tfaSaving.value = false
    }
  }

  // ── Payment methods ───────────────────────────────────────────────────────

  /** The sentence for the last refused set-default or removal; empty when there is none. */
  const paymentActionError = ref('')

  /** Id of the method being made default, or null. */
  const settingDefaultId = ref<string | null>(null)

  /** Id of the method being removed, or null. */
  const removingId = ref<string | null>(null)

  /** Whether the expiry edit is in flight. */
  const editSaving = ref(false)

  /** The sentence for a refused expiry edit; empty when there is none. */
  const editError = ref('')

  /**
   * Updates a saved payment method.
   *
   * @param id - Method id.
   * @param body - The changed fields.
   * @returns How it ended; on `Failed`, {@link editError} holds the sentence.
   */
  const updatePaymentMethod = async (id: string, body: Record<string, unknown>): Promise<ActionOutcome> => {
    editSaving.value = true
    editError.value = ''
    fieldErrors.value = {}
    try {
      await billingApi.updatePaymentMethod(id, body)
      return ActionOutcome.Succeeded
    } catch (e: unknown) {
      editError.value = apiErrorMessage(e)
      fieldErrors.value = apiFieldErrors(e)
      return ActionOutcome.Failed
    } finally {
      editSaving.value = false
    }
  }

  /**
   * Makes a saved method the default.
   *
   * @param id - Method id.
   * @returns How it ended; on `Failed`, {@link paymentActionError} holds the sentence.
   */
  const setDefaultPaymentMethod = async (id: string): Promise<ActionOutcome> => {
    settingDefaultId.value = id
    paymentActionError.value = ''
    try {
      await billingApi.setDefaultPaymentMethod(id)
      return ActionOutcome.Succeeded
    } catch (e: unknown) {
      paymentActionError.value = apiErrorMessage(e)
      return ActionOutcome.Failed
    } finally {
      settingDefaultId.value = null
    }
  }

  /**
   * Removes a saved method.
   *
   * @param id - Method id.
   * @returns How it ended; on `Failed`, {@link paymentActionError} holds the sentence.
   */
  const removePaymentMethod = async (id: string): Promise<ActionOutcome> => {
    removingId.value = id
    paymentActionError.value = ''
    try {
      await billingApi.removePaymentMethod(id)
      return ActionOutcome.Succeeded
    } catch (e: unknown) {
      paymentActionError.value = apiErrorMessage(e)
      return ActionOutcome.Failed
    } finally {
      removingId.value = null
    }
  }

  /**
   * Clears every modal-level error, called when a modal is (re)opened so a stale sentence
   * from the previous attempt is not shown.
   *
   * @returns Nothing.
   */
  const clearModalErrors = (): void => {
    saveError.value = ''
    contactError.value = ''
    tfaModalError.value = ''
    editError.value = ''
    fieldErrors.value = {}
  }

  return {
    users,
    usersLoading,
    loadUsers,
    contacts,
    contactsLoading,
    loadContacts,
    emails,
    emailsLoading,
    loadEmails,
    paymentMethods,
    paymentLoading,
    paymentLoadError,
    loadPaymentMethods,
    gatewayMethods,
    loadGatewayMethods,
    fieldErrors,
    saving,
    saveError,
    saveProfile,
    inviting,
    inviteError,
    removingUserId,
    inviteUser,
    removeUser,
    contactSaving,
    contactError,
    saveContact,
    removeContact,
    tfaLoading,
    tfaError,
    tfaSaving,
    tfaModalError,
    startTwoFactorSetup,
    submitTwoFactor,
    paymentActionError,
    settingDefaultId,
    removingId,
    editSaving,
    editError,
    updatePaymentMethod,
    setDefaultPaymentMethod,
    removePaymentMethod,
    clearModalErrors
  }
})
