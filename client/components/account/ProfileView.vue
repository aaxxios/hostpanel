<template>
  <UiStack gap="lg" data-testid="account-profile-view">
    <UiGrid cols="2" gap="lg">
      <UiCard :title="$t('client.profile.accountInfo')">
        <dl class="grid grid-cols-1 gap-4">
          <UiDescriptionItem :label="$t('client.profile.firstName')" :value="user.firstname" />
          <UiDescriptionItem :label="$t('client.profile.lastName')" :value="user.lastname" />
          <UiDescriptionItem v-if="user.companyname" :label="$t('client.profile.companyName')" :value="user.companyname" />
          <UiDescriptionItem :label="$t('client.profile.email')" :value="user.email" value-class="break-all" />
          <UiDescriptionItem :label="$t('client.profile.paymentMethod')" :value="paymentMethodLabel" />
          <UiDescriptionItem v-if="ownsLanguage" :label="$t('client.profile.language')" :value="languageLabel" />
        </dl>
      </UiCard>

      <UiCard :title="$t('client.profile.billingAddress')">
        <dl class="grid grid-cols-1 gap-4">
          <UiDescriptionItem :label="$t('client.profile.address1')" :value="user.address1 || undefined" />
          <UiDescriptionItem v-if="user.address2" :label="$t('client.profile.address2')" :value="user.address2" />
          <UiDescriptionItem :label="$t('client.profile.city')" :value="user.city || undefined" />
          <UiDescriptionItem :label="$t('client.profile.state')" :value="user.state || undefined" />
          <UiDescriptionItem :label="$t('client.profile.postcode')" :value="user.postcode || undefined" />
          <UiDescriptionItem :label="$t('client.profile.country')" :value="user.countryname || user.country || undefined" />
          <UiDescriptionItem :label="$t('client.profile.phone')" :value="user.phonenumber || undefined" />
        </dl>
      </UiCard>
    </UiGrid>

    <UiCard :title="$t('client.profile.emailPrefs')">
      <AccountProfileEmailPrefs :model-value="emailPrefs" disabled id-prefix="view" />
    </UiCard>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * ProfileView - the read-only account details: identity, billing address and email preferences.
 *
 * Empty values render the kit's dash; the labels it is given are already worded by the tab.
 *
 * @module components/account/ProfileView
 */

import type { ClientUser } from '~/types/clientuser'
import type { EmailPreferenceFlags } from '~/utils/emailPreferences'

/** Props of {@link ProfileView}. */
interface Props {
  /** The signed-in client's profile. */
  user: ClientUser
  /** The default payment method as a name a person reads. */
  paymentMethodLabel: string
  /** The stored language as a name a person reads. */
  languageLabel: string
  /** Whether this deployment stores the language (false under SSO). */
  ownsLanguage: boolean
  /** The decoded email preferences. */
  emailPrefs: EmailPreferenceFlags
}

defineProps<Props>()
</script>
