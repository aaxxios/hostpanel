<template>
  <UiStack gap="lg">
    <UiBackLink :to="CLIENT_DOMAINS_PATH" :label="$t('client.domains.backTo')" testid="domain-back" />

    <UiStack v-if="pending">
      <UiSkeleton shape="block" class="h-20" />
      <UiSkeleton shape="block" class="h-80" />
    </UiStack>

    <UiEmptyState v-else-if="!domain" :icon="AlertCircle" :title="$t('client.domains.notFound')" />

    <template v-else>
      <ClientDomainsDomainHeader :domain="domain" :full-name="fullDomainName" />

      <UiSplit ratio="end-wide" gap="lg">
        <template #start>
          <ClientDomainsDomainSidebar v-model:active-tab="activeTab" :is-pending="isPending" />
        </template>

        <template #end>
          <UiStack>
            <UiAlert
              v-if="isPending"
              variant="info"
              :icon-size="16"
              :title="pendingTitle"
              data-testid="domain-pending"
            >
              {{ pendingDescription }}
            </UiAlert>

            <ClientDomainsDomainOverview
              v-if="activeTab === DomainTab.Overview"
              :domain="domain"
              :full-name="fullDomainName"
              :lock-enabled="lockEnabled"
              :is-pending="isPending"
              @navigate="activeTab = $event"
            />
            <ClientDomainsDomainRenewPanel
              v-else-if="activeTab === DomainTab.Renew"
              :domain="domain"
              :domain-id="domainId"
              :full-name="fullDomainName"
            />
            <ClientDomainsDomainAutoRenewPanel
              v-else-if="activeTab === DomainTab.AutoRenew"
              v-model:enabled="autoRenewEnabled"
              :domain-id="domainId"
            />
            <ClientDomainsDomainNameserversPanel v-else-if="activeTab === DomainTab.Nameservers" :domain-id="domainId" />
            <ClientDomainsDomainLockPanel
              v-else-if="activeTab === DomainTab.Lock"
              v-model:lock-enabled="lockEnabled"
              v-model:id-protect-enabled="idProtectEnabled"
              :domain-id="domainId"
            />
            <ClientDomainsDomainContactPanel v-else-if="activeTab === DomainTab.Contact" :domain-id="domainId" />
          </UiStack>
        </template>
      </UiSplit>
    </template>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * Domain detail: header, a side menu and the panel it selects (overview, renewal, auto-renew,
 * nameservers, registrar lock with ID protection and EPP code, WHOIS contacts). The domain is
 * read through the domains store, which also owns the in-flight flags and refusal sentences the
 * panels render. The three switches are kept here so they survive switching panels.
 */
import { CLIENT_DOMAINS_PATH } from '~/constants/clientRoutes'
import { AlertCircle } from 'lucide-vue-next'
import { useDomainsStore } from '~/stores/domains'
import { DomainStatus } from '~/types/domainstatus'
import { DomainTab } from '~/types/domaintab'

definePageMeta({ layout: 'client', middleware: 'client-auth' })

/** Translates a key into the active locale. */
const { t } = useI18n()
/** The current route; its params name the record shown. */
const route = useRoute()

/** The id of the domain in the route. */
const domainId = String(route.params.id)

/** The domains store; reset so a previous visit's results and refusals do not linger. */
const domainsStore = useDomainsStore()
domainsStore.reset()
/** Loading flag of the data this page reads. */
const { pending } = await useAsyncData(`client-domain-${domainId}`, () => domainsStore.loadDomain(domainId))

/** The domain being shown; null until loaded or after a failed read. */
const domain = computed(() => domainsStore.domain)

/** Full domain name (the backend's Name field is already the FQDN). */
const fullDomainName = computed(() => domain.value?.name ?? '')

/** The panel shown beside the side menu, a `DomainTab` value (a string so the menu can v-model it). */
const activeTab = ref<string>(DomainTab.Overview)

/** Whether the domain is still being processed by the registrar (not yet live). */
const isPending = computed(() => domain.value?.status === DomainStatus.PendingRegistration || domain.value?.status === DomainStatus.PendingTransfer)

/** Headline of the pending banner: a transfer or a registration. */
const pendingTitle = computed(() => (domain.value?.status === DomainStatus.PendingTransfer ? t('client.domains.pendingTransferTitle') : t('client.domains.pendingRegistrationTitle')))

/** Body of the pending banner: a transfer or a registration. */
const pendingDescription = computed(() => (domain.value?.status === DomainStatus.PendingTransfer ? t('client.domains.pendingTransferDesc') : t('client.domains.pendingRegistrationDesc')))

/** Whether auto-renew is on; follows the domain, then the switch once the API accepts a change. */
const autoRenewEnabled = ref(false)

/** Whether the registrar lock is on; follows the domain, then the switch. */
const lockEnabled = ref(false)

/** Whether ID protection is on; follows the domain, then the switch. */
const idProtectEnabled = ref(false)

watch(domain, (current) => {
  if (current) {
    autoRenewEnabled.value = current.autoRenew
    lockEnabled.value = current.isLocked
    idProtectEnabled.value = current.whoisPrivacy
  }
}, { immediate: true })
</script>
