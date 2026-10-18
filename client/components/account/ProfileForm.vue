<template>
  <UiForm spacing="lg" :error="saveError" data-testid="account-profile-form" @submit="onSubmit">
    <UiGrid cols="2" gap="lg">
      <UiCard :title="$t('client.profile.accountInfo')">
        <UiGrid cols="2" gap="md">
          <UiInput
            v-model="form.firstname"
            :label="$t('client.profile.firstName')"
            :placeholder="$t('client.account.placeholders.firstName')"
            :error="fieldErrors.firstname"
            size="sm"
            required
            testid="account-firstname"
          />
          <UiInput
            v-model="form.lastname"
            :label="$t('client.profile.lastName')"
            :placeholder="$t('client.account.placeholders.lastName')"
            :error="fieldErrors.lastname"
            size="sm"
            required
            testid="account-lastname"
          />
          <UiInput
v-model="form.companyname"
            data-testid="profile-company"
            :label="$t('client.profile.companyName')"
            :placeholder="$t('client.account.placeholders.companyName')"
            :error="fieldErrors.companyname"
            size="sm"
            class="sm:col-span-2"
          />
          <UiInput
            v-model="form.email"
            type="email"
            :label="$t('client.profile.emailAddress')"
            :placeholder="$t('client.account.placeholders.email')"
            :error="fieldErrors.email"
            size="sm"
            required
            class="sm:col-span-2"
            testid="account-email"
          />
          <UiSelect
v-model="form.paymentmethod"
            data-testid="profile-payment-method"
            :label="$t('client.profile.paymentMethod')"
            :placeholder="$t('ui.selectPlaceholder')"
            :options="paymentOptions"
            size="sm"
            class="sm:col-span-2"
          />
          <UiSelect
            v-if="ownsLanguage" v-model="form.language"
            data-testid="profile-language"
            :label="$t('client.profile.language')"
            :placeholder="$t('ui.selectPlaceholder')"
            :options="languageOptions"
            size="sm"
            class="sm:col-span-2"
          />
        </UiGrid>
      </UiCard>

      <UiCard :title="$t('client.profile.billingAddress')">
        <UiGrid cols="2" gap="md">
          <UiInput
v-model="form.address1"
            data-testid="profile-address1"
            :label="$t('client.profile.address1')"
            :placeholder="$t('client.account.placeholders.address1')"
            :error="fieldErrors.address1"
            size="sm"
            class="sm:col-span-2"
          />
          <UiInput
v-model="form.address2"
            data-testid="profile-address2"
            :label="$t('client.profile.address2')"
            :placeholder="$t('client.account.placeholders.address2')"
            :error="fieldErrors.address2"
            size="sm"
            class="sm:col-span-2"
          />
          <UiInput
v-model="form.city"
            data-testid="profile-city"
            :label="$t('client.profile.city')"
            :placeholder="$t('client.account.placeholders.city')"
            :error="fieldErrors.city"
            size="sm"
          />
          <UiInput
v-model="form.state"
            data-testid="profile-state"
            :label="$t('client.profile.state')"
            :placeholder="$t('client.account.placeholders.state')"
            :error="fieldErrors.state"
            size="sm"
          />
          <UiInput
v-model="form.postcode"
            data-testid="profile-postcode"
            :label="$t('client.profile.postcode')"
            :placeholder="$t('client.account.placeholders.postcode')"
            :error="fieldErrors.postcode"
            size="sm"
          />
          <UiSelect
v-model="form.country"
            data-testid="profile-country"
            :label="$t('client.profile.country')"
            :placeholder="$t('ui.selectPlaceholder')"
            :options="countries"
            size="sm"
          />
          <UiInput
v-model="form.phonenumber"
            data-testid="profile-phone"
            type="tel"
            :label="$t('client.profile.phone')"
            :placeholder="$t('client.account.placeholders.phone')"
            :error="fieldErrors.phonenumber"
            size="sm"
            class="sm:col-span-2"
          />
        </UiGrid>
      </UiCard>
    </UiGrid>

    <UiCard :title="$t('client.profile.emailPrefs')">
      <AccountProfileEmailPrefs v-model="emailPrefs" id-prefix="edit" />
    </UiCard>

    <template #actions>
      <UiButton type="button" size="sm" variant="subtle" data-testid="account-profile-cancel" @click="emit('cancel')">
        {{ $t('client.profile.cancel') }}
      </UiButton>
      <UiButton type="submit" size="sm" :loading="saving" data-testid="account-profile-save">
        <Check v-if="!saving" :size="14" :stroke-width="2" class="mr-1.5" />
        {{ saving ? $t('client.profile.saving') : $t('client.profile.save') }}
      </UiButton>
    </template>
  </UiForm>
</template>

<script setup lang="ts">
/**
 * ProfileForm - the edit form for the profile, billing address and email preferences.
 *
 * Owns only the draft; saving goes through the account store, which keeps the refusal sentence
 * and the per-field messages this form renders. A refused save leaves the form open.
 *
 * @module components/account/ProfileForm
 */

import { Check } from 'lucide-vue-next'
import { storeToRefs } from 'pinia'
import { useAccountStore } from '~/stores/account'
import { useCatalogStore } from '~/stores/catalog'
import type { ClientUser } from '~/types/clientuser'
import { ActionOutcome } from '~/types/actionoutcome'
import { decodeEmailPrefs } from '~/utils/emailPreferences'

/** One option of a select. */
interface Option {
  /** Value stored when chosen. */
  value: string
  /** Text shown for it. */
  label: string
}

/** Props of {@link ProfileForm}. */
interface Props {
  /** The profile being edited; the draft starts from it. */
  user: ClientUser
  /** Whether this deployment stores the language (false under SSO). */
  ownsLanguage: boolean
  /** The default-payment-method choices. */
  paymentOptions: Option[]
  /** The language choices. */
  languageOptions: Option[]
}

/** The props. */
const props = defineProps<Props>()

/** Events of {@link ProfileForm}. */
const emit = defineEmits<{
  /** The profile was saved. */
  saved: []
  /** The edit was abandoned. */
  cancel: []
}>()

/** The account store: save action and its refusal state. */
const account = useAccountStore()
/** Save progress, the API's sentence and the per-field messages. */
const { saving, saveError, fieldErrors } = storeToRefs(account)
/** The catalogue's country list. */
const { countries: storedCountries } = storeToRefs(useCatalogStore())

/** The country choices; empty until the catalogue has loaded them. */
const countries = computed<Option[]>(() => storedCountries.value ?? [])

/** The draft of the editable fields, seeded from the profile. */
const form = reactive({
  firstname: props.user.firstname,
  lastname: props.user.lastname,
  companyname: props.user.companyname ?? '',
  email: props.user.email,
  phonenumber: props.user.phonenumber ?? '',
  address1: props.user.address1 ?? '',
  address2: props.user.address2 ?? '',
  city: props.user.city ?? '',
  state: props.user.state ?? '',
  postcode: props.user.postcode ?? '',
  country: props.user.country ?? '',
  paymentmethod: props.user.defaultgateway ?? '',
  language: props.user.language ?? ''
})

/** The draft of the email preferences. */
const emailPrefs = ref(decodeEmailPrefs(props.user.email_preferences))

// A stale sentence from an earlier attempt must not greet a fresh edit.
account.clearModalErrors()

/** Saves the draft; on success tells the tab to leave edit mode. */
const onSubmit = async (): Promise<void> => {
  // A refusal stays in the store as `saveError`, worded by the API.
  if (await account.saveProfile({ ...form, emailprefs: emailPrefs.value }) === ActionOutcome.Succeeded) {
    emit('saved')
  }
}
</script>
