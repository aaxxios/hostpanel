<script setup lang="ts">
/**
 * Dialog for one server's SSH key management settings: where customers connect, how HostPanel
 * reaches the key agent installed on the server, and an agent health check.
 *
 * The agent's private key is write-only. A stored key is never shown; leaving the field empty keeps
 * it. Validation belongs to the server: a refusal comes back through the store as the API's own
 * sentence and per-field sentences.
 */
import { computed, onMounted, reactive, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { storeToRefs } from 'pinia'
import { useServerSshStore } from '@/stores/serverssh'
import type { ServerSshSettingsDto, ServerSshSettingsPayload } from '@/types/serverssh'
import { UiTone } from '@/utils/ui/tone'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiField from '@/components/ui/UiField.vue'
import UiForm from '@/components/ui/UiForm.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiLabel from '@/components/ui/UiLabel.vue'
import UiNumberInput from '@/components/ui/UiNumberInput.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import UiToggle from '@/components/ui/UiToggle.vue'

/** Translation function. */
const { t } = useI18n()

/** Props for ServerSshSettingsModal. */
const props = defineProps<{
  /** The server whose settings are edited. */
  serverId: number
  /** Its display name, for the title. */
  serverName: string
}>()

/** Events emitted by ServerSshSettingsModal. */
const emit = defineEmits<{
  /** Emitted when the dialog closes, saved or not. */
  close: []
}>()

/** The SSH settings store. */
const store = useServerSshStore()

/** Reactive state of the store this dialog renders. */
const { settings, loading, saving, testing, lastTest, error, fieldErrors } = storeToRefs(store)

/** The form as the inputs hold it. */
const form = reactive<ServerSshSettingsPayload>({
  isEnabled: false,
  sshHostname: '',
  sshPort: 22,
  agentHost: '',
  agentPort: 22,
  agentUsername: 'hpsshkeys',
  agentPrivateKey: '',
  agentHostKeyFingerprint: '',
  connectTimeoutSeconds: 10,
  operationTimeoutSeconds: 20,
})

/**
 * Copies loaded settings into the form. The private key field stays empty: the stored key is
 * never read back.
 *
 * @param loaded - The settings the API answered with.
 */
const fill = (loaded: ServerSshSettingsDto): void => {
  form.isEnabled = loaded.isEnabled
  form.sshHostname = loaded.sshHostname
  form.sshPort = loaded.sshPort
  form.agentHost = loaded.agentHost
  form.agentPort = loaded.agentPort
  form.agentUsername = loaded.agentUsername || form.agentUsername
  form.agentPrivateKey = ''
  form.agentHostKeyFingerprint = loaded.agentHostKeyFingerprint
  form.connectTimeoutSeconds = loaded.connectTimeoutSeconds
  form.operationTimeoutSeconds = loaded.operationTimeoutSeconds
}

watch(settings, (loaded) => {
  if (loaded) {
    fill(loaded)
  }
})

/** The badge of the last recorded health check. */
const health = computed(() => {
  if (settings.value?.isHealthy === true) {
    return { tone: UiTone.Success, label: t('servers.ssh.healthy') }
  }
  if (settings.value?.isHealthy === false) {
    return { tone: UiTone.Danger, label: t('servers.ssh.unhealthy') }
  }
  return { tone: UiTone.Neutral, label: t('servers.ssh.notTested') }
})

/** The body to send: an empty private key field means "keep the stored one". */
const payload = computed<ServerSshSettingsPayload>(() => ({
  ...form,
  agentPrivateKey: form.agentPrivateKey?.trim() ? form.agentPrivateKey : null,
}))

/**
 * Saves the settings and closes the dialog on success.
 *
 * @returns Resolves when the attempt has ended.
 */
const handleSave = async (): Promise<void> => {
  if (await store.save(props.serverId, payload.value)) {
    emit('close')
  }
}

/**
 * Runs the agent health check against the stored settings.
 *
 * @returns Resolves when the check has ended.
 */
const handleTest = (): Promise<void> => store.test(props.serverId)

onMounted(() => {
  store.reset()
  return store.load(props.serverId)
})
</script>

<template>
  <UiDialog :title="t('servers.ssh.title', { name: props.serverName })" size="lg" @close="emit('close')">
    <UiStack gap="md">
      <UiAlert v-if="error" variant="error" data-testid="server-ssh-error">{{ error }}</UiAlert>
      <UiSkeleton v-if="loading && !settings" :lines="4" data-testid="server-ssh-loading" />

      <template v-else-if="settings">
        <UiAlert v-if="!settings.isSupported" variant="warning" data-testid="server-ssh-unsupported">
          {{ t('servers.ssh.unsupported') }}
        </UiAlert>

        <UiStack direction="row" gap="sm" wrap>
          <UiBadge :tone="health.tone" dot data-testid="server-ssh-health">{{ health.label }}</UiBadge>
          <UiText v-if="lastTest?.agentVersion" size="sm" tone="muted" data-testid="server-ssh-agent-version">
            {{ t('servers.ssh.agentVersion', { version: lastTest.agentVersion }) }}
          </UiText>
          <UiText v-if="lastTest?.error" size="sm" tone="muted" data-testid="server-ssh-agent-error">
            {{ t('servers.ssh.agentError', { code: lastTest.error }) }}
          </UiText>
        </UiStack>

        <UiForm stacked @submit.prevent="handleSave">
          <UiLabel variant="inline">
            <UiToggle v-model="form.isEnabled" :disabled="!settings.isSupported" data-testid="server-ssh-enabled" />
            {{ t('servers.ssh.enabled') }}
          </UiLabel>
          <UiText size="sm" tone="muted">{{ t('servers.ssh.enabledHint') }}</UiText>

          <UiGrid :columns="2" gap="sm">
            <UiField :label="t('servers.ssh.sshHostname')" :hint="t('servers.ssh.sshHostnameHint')" :error="fieldErrors.sshHostname">
              <template #default="{ id, invalid }">
                <UiInput :id="id" v-model="form.sshHostname" :invalid="invalid" :placeholder="t('servers.ssh.hostnamePlaceholder')" data-testid="server-ssh-hostname" />
              </template>
            </UiField>
            <UiField :label="t('servers.ssh.sshPort')" :error="fieldErrors.sshPort">
              <UiNumberInput v-model="form.sshPort" :min="1" :max="65535" :placeholder="t('servers.ssh.portPlaceholder')" data-testid="server-ssh-port" />
            </UiField>
          </UiGrid>

          <UiGrid :columns="2" gap="sm">
            <UiField :label="t('servers.ssh.agentHost')" :hint="t('servers.ssh.agentHostHint')" :error="fieldErrors.agentHost">
              <template #default="{ id, invalid }">
                <UiInput :id="id" v-model="form.agentHost" :invalid="invalid" :placeholder="t('servers.ssh.hostnamePlaceholder')" data-testid="server-ssh-agent-host" />
              </template>
            </UiField>
            <UiField :label="t('servers.ssh.agentPort')" :error="fieldErrors.agentPort">
              <UiNumberInput v-model="form.agentPort" :min="1" :max="65535" :placeholder="t('servers.ssh.portPlaceholder')" data-testid="server-ssh-agent-port" />
            </UiField>
          </UiGrid>

          <UiGrid :columns="2" gap="sm">
            <UiField :label="t('servers.ssh.agentUsername')" :error="fieldErrors.agentUsername">
              <template #default="{ id, invalid }">
                <UiInput :id="id" v-model="form.agentUsername" :invalid="invalid" :placeholder="t('servers.ssh.agentUsernamePlaceholder')" data-testid="server-ssh-agent-username" />
              </template>
            </UiField>
            <UiField :label="t('servers.ssh.fingerprint')" :hint="t('servers.ssh.fingerprintHint')" :error="fieldErrors.agentHostKeyFingerprint">
              <template #default="{ id, invalid }">
                <UiInput :id="id" v-model="form.agentHostKeyFingerprint" :invalid="invalid" :placeholder="t('servers.ssh.fingerprintPlaceholder')" data-testid="server-ssh-fingerprint" />
              </template>
            </UiField>
          </UiGrid>

          <UiField
            :label="t('servers.ssh.privateKey')"
            :hint="settings.hasAgentPrivateKey ? t('servers.ssh.privateKeyStoredHint') : t('servers.ssh.privateKeyHint')"
            :error="fieldErrors.agentPrivateKey"
          >
            <template #default="{ id, invalid }">
              <UiTextarea
                :id="id"
                v-model="form.agentPrivateKey"
                rows="4"
                :invalid="invalid"
                :placeholder="settings.hasAgentPrivateKey ? t('servers.ssh.privateKeyKeepPlaceholder') : t('servers.ssh.privateKeyPlaceholder')"
                autocomplete="off"
                spellcheck="false"
                data-testid="server-ssh-private-key"
              />
            </template>
          </UiField>

          <UiGrid :columns="2" gap="sm">
            <UiField :label="t('servers.ssh.connectTimeout')" :error="fieldErrors.connectTimeoutSeconds">
              <UiNumberInput v-model="form.connectTimeoutSeconds" :min="1" :max="120" :placeholder="t('servers.ssh.secondsPlaceholder')" data-testid="server-ssh-connect-timeout" />
            </UiField>
            <UiField :label="t('servers.ssh.operationTimeout')" :error="fieldErrors.operationTimeoutSeconds">
              <UiNumberInput v-model="form.operationTimeoutSeconds" :min="1" :max="300" :placeholder="t('servers.ssh.secondsPlaceholder')" data-testid="server-ssh-operation-timeout" />
            </UiField>
          </UiGrid>
        </UiForm>
      </template>
    </UiStack>

    <template #footer>
      <UiButton
        type="button"
        variant="secondary"
        :loading="testing"
        :disabled="!settings?.hasAgentPrivateKey"
        data-testid="server-ssh-test"
        @click="handleTest"
      >
        {{ t('servers.ssh.test') }}
      </UiButton>
      <UiButton type="button" variant="secondary" data-testid="server-ssh-cancel" @click="emit('close')">
        {{ t('shared.cancel') }}
      </UiButton>
      <UiButton type="button" variant="primary" :loading="saving" :disabled="!settings" data-testid="server-ssh-save" @click="handleSave">
        {{ t('shared.saveChanges') }}
      </UiButton>
    </template>
  </UiDialog>
</template>
