<script setup lang="ts">
/**
 * Guided SSH key management setup for one server:
 *
 * 1. Connection — where customers connect, and how HostPanel reaches the server's agent.
 * 2. Install — HostPanel generates a per-server ED25519 identity and shows a one-command installer
 *    that verifies the bundle's SHA256 before running anything.
 * 3. Host key — HostPanel reads the key the endpoint presents; the administrator compares it with
 *    what the installer printed on the server and pins it.
 * 4. Test — the agent must answer, and must have been installed for this server (binding).
 * 5. Enable — refused by the API until a test with the saved settings succeeded.
 *
 * The private key is write-only: generated and kept by HostPanel, or pasted under "advanced", and
 * never shown. Requests and refusals belong to the server SSH store.
 */
import { computed, onMounted, reactive, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { storeToRefs } from 'pinia'
import { buildInstallCommand } from '@/lib/sshAgentInstall'
import { useServerSshStore } from '@/stores/serverssh'
import type { ServerSshSettingsDto, ServerSshSettingsPayload } from '@/types/serverssh'
import { UiTone } from '@/utils/ui/tone'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCodeBlock from '@/components/ui/UiCodeBlock.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiField from '@/components/ui/UiField.vue'
import UiForm from '@/components/ui/UiForm.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
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
const { settings, setup, loading, saving, testing, generating, scanning, lastTest, lastScan, error, fieldErrors } = storeToRefs(store)

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

/** The one-command installer, once an identity exists. */
const installCommand = computed(() => (setup.value ? buildInstallCommand(window.location.origin, setup.value) : null))

/** Whether the saved settings point at an agent host, so a scan and a test can run. */
const hasEndpoint = computed(() => Boolean(settings.value?.agentHost))

/** The body to send: an empty private key field means "keep the stored one". */
const payload = computed<ServerSshSettingsPayload>(() => ({
  ...form,
  agentPrivateKey: form.agentPrivateKey?.trim() ? form.agentPrivateKey : null,
}))

/**
 * Saves the settings. The dialog stays open, so the next step can use what was saved.
 *
 * @returns Resolves when the attempt has ended.
 */
const handleSave = async (): Promise<void> => {
  await store.save(props.serverId, payload.value)
}

/**
 * Generates (or rotates) the server's management identity.
 *
 * @returns Resolves when the attempt has ended.
 */
const handleGenerate = async (): Promise<void> => {
  await store.generateIdentity(props.serverId)
}

/**
 * Reads the host key the saved endpoint presents.
 *
 * @returns Resolves when the scan has ended.
 */
const handleScan = (): Promise<void> => store.scanHostKey(props.serverId)

/** Whether the scanned key is the one in the fingerprint field, i.e. the one that is or will be pinned. */
const scanMatches = computed(() => Boolean(lastScan.value) && lastScan.value?.fingerprint === form.agentHostKeyFingerprint.trim())

/** Puts the scanned fingerprint into the form; it is pinned when the settings are saved. */
const useScannedFingerprint = (): void => {
  if (lastScan.value) {
    form.agentHostKeyFingerprint = lastScan.value.fingerprint
  }
}

/**
 * Runs the agent health check against the saved settings.
 *
 * @returns Resolves when the check has ended.
 */
const handleTest = (): Promise<void> => store.test(props.serverId)

onMounted(() => {
  store.reset()
  return store.loadAll(props.serverId)
})
</script>

<template>
  <UiDialog :title="t('servers.ssh.title', { name: props.serverName })" size="lg" @close="emit('close')">
    <UiStack gap="lg">
      <UiAlert v-if="error" variant="error" data-testid="server-ssh-error">{{ error }}</UiAlert>
      <UiSkeleton v-if="loading && !settings" :lines="4" data-testid="server-ssh-loading" />

      <template v-else-if="settings">
        <UiAlert v-if="!settings.isSupported" variant="warning" data-testid="server-ssh-unsupported">
          {{ t('servers.ssh.unsupported') }}
        </UiAlert>

        <!-- 1. Connection -->
        <UiStack gap="sm" data-testid="server-ssh-step-connection">
          <UiHeading :level="4">{{ t('servers.ssh.stepConnection') }}</UiHeading>
          <UiForm stacked @submit.prevent="handleSave">
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
              <UiField :label="t('servers.ssh.connectTimeout')" :error="fieldErrors.connectTimeoutSeconds">
                <UiNumberInput v-model="form.connectTimeoutSeconds" :min="1" :max="120" :placeholder="t('servers.ssh.secondsPlaceholder')" data-testid="server-ssh-connect-timeout" />
              </UiField>
            </UiGrid>
            <UiText size="sm" tone="muted">{{ t('servers.ssh.connectionHint') }}</UiText>
          </UiForm>
        </UiStack>

        <!-- 2. Install -->
        <UiStack gap="sm" data-testid="server-ssh-step-install">
          <UiHeading :level="4">{{ t('servers.ssh.stepInstall') }}</UiHeading>
          <template v-if="setup?.profile">
            <UiText size="sm" tone="muted">{{ t('servers.ssh.installIntro', { profile: setup.profile, version: setup.bundleVersion }) }}</UiText>
            <UiStack direction="row" gap="sm" wrap>
              <UiButton type="button" variant="secondary" :loading="generating" data-testid="server-ssh-generate" @click="handleGenerate">
                {{ setup.publicKey ? t('servers.ssh.rotateIdentity') : t('servers.ssh.generateIdentity') }}
              </UiButton>
            </UiStack>
            <UiText v-if="setup.publicKey" size="sm" tone="muted">{{ t('servers.ssh.rotateWarning') }}</UiText>
            <template v-if="installCommand">
              <UiText size="sm">{{ t('servers.ssh.runAsRoot') }}</UiText>
              <UiCodeBlock :code="installCommand" :copy-label="t('servers.ssh.copy')" :copied-label="t('servers.ssh.copied')" data-testid="server-ssh-install-command" />
              <UiText size="sm" tone="muted">{{ t('servers.ssh.installVerifies', { sha: setup.bundleSha256 }) }}</UiText>
            </template>
          </template>
        </UiStack>

        <!-- 3. Host key -->
        <UiStack gap="sm" data-testid="server-ssh-step-hostkey">
          <UiHeading :level="4">{{ t('servers.ssh.stepHostKey') }}</UiHeading>
          <UiText size="sm" tone="muted">{{ t('servers.ssh.hostKeyIntro') }}</UiText>
          <UiStack direction="row" gap="sm" wrap>
            <UiButton type="button" variant="secondary" :loading="scanning" :disabled="!hasEndpoint" data-testid="server-ssh-scan" @click="handleScan">
              {{ t('servers.ssh.scanHostKey') }}
            </UiButton>
          </UiStack>
          <template v-if="lastScan">
            <UiCodeBlock :code="`${lastScan.algorithm} ${lastScan.fingerprint}`" :copy-label="t('servers.ssh.copy')" :copied-label="t('servers.ssh.copied')" data-testid="server-ssh-scanned" />
            <UiStack direction="row" gap="sm" wrap>
              <UiBadge :tone="scanMatches ? UiTone.Success : UiTone.Warning" dot data-testid="server-ssh-scan-match">
                {{ scanMatches ? t('servers.ssh.matchesPinned') : t('servers.ssh.notPinned') }}
              </UiBadge>
              <UiButton v-if="!scanMatches" type="button" variant="ghost" size="sm" data-testid="server-ssh-use-scanned" @click="useScannedFingerprint">
                {{ t('servers.ssh.useScanned') }}
              </UiButton>
            </UiStack>
          </template>
          <UiField :label="t('servers.ssh.fingerprint')" :hint="t('servers.ssh.fingerprintHint')" :error="fieldErrors.agentHostKeyFingerprint">
            <template #default="{ id, invalid }">
              <UiInput :id="id" v-model="form.agentHostKeyFingerprint" :invalid="invalid" :placeholder="t('servers.ssh.fingerprintPlaceholder')" data-testid="server-ssh-fingerprint" />
            </template>
          </UiField>
        </UiStack>

        <!-- 4. Test -->
        <UiStack gap="sm" data-testid="server-ssh-step-test">
          <UiHeading :level="4">{{ t('servers.ssh.stepTest') }}</UiHeading>
          <UiStack direction="row" gap="sm" wrap>
            <UiButton
              type="button"
              variant="secondary"
              :loading="testing"
              :disabled="!settings.hasAgentPrivateKey || !hasEndpoint"
              data-testid="server-ssh-test"
              @click="handleTest"
            >
              {{ t('servers.ssh.test') }}
            </UiButton>
            <UiBadge :tone="health.tone" dot data-testid="server-ssh-health">{{ health.label }}</UiBadge>
            <UiText v-if="lastTest?.agentVersion" size="sm" tone="muted" data-testid="server-ssh-agent-version">
              {{ t('servers.ssh.agentVersion', { version: lastTest.agentVersion, provider: lastTest.provider ?? '—' }) }}
            </UiText>
          </UiStack>
          <UiAlert v-if="lastTest?.error === 'binding_mismatch'" variant="error" data-testid="server-ssh-binding-mismatch">
            {{ t('servers.ssh.bindingMismatch') }}
          </UiAlert>
          <UiText v-else-if="lastTest?.error" size="sm" tone="muted" data-testid="server-ssh-agent-error">
            {{ t('servers.ssh.agentError', { code: lastTest.error }) }}
          </UiText>
        </UiStack>

        <!-- 5. Enable -->
        <UiStack gap="sm" data-testid="server-ssh-step-enable">
          <UiHeading :level="4">{{ t('servers.ssh.stepEnable') }}</UiHeading>
          <UiLabel variant="inline">
            <UiToggle v-model="form.isEnabled" :disabled="!settings.isSupported" data-testid="server-ssh-enabled" />
            {{ t('servers.ssh.enabled') }}
          </UiLabel>
          <UiText size="sm" tone="muted">{{ t('servers.ssh.enabledHint') }}</UiText>
        </UiStack>

        <!-- Advanced: an existing key instead of a generated one -->
        <UiStack gap="sm" data-testid="server-ssh-step-advanced">
          <UiHeading :level="4">{{ t('servers.ssh.advanced') }}</UiHeading>
          <UiField
            :label="t('servers.ssh.privateKey')"
            :hint="settings.hasAgentPrivateKey ? t('servers.ssh.privateKeyStoredHint') : t('servers.ssh.privateKeyHint')"
            :error="fieldErrors.agentPrivateKey"
          >
            <template #default="{ id, invalid }">
              <UiTextarea
                :id="id"
                v-model="form.agentPrivateKey"
                rows="3"
                :invalid="invalid"
                :placeholder="settings.hasAgentPrivateKey ? t('servers.ssh.privateKeyKeepPlaceholder') : t('servers.ssh.privateKeyPlaceholder')"
                autocomplete="off"
                spellcheck="false"
                data-testid="server-ssh-private-key"
              />
            </template>
          </UiField>
          <UiField :label="t('servers.ssh.operationTimeout')" :error="fieldErrors.operationTimeoutSeconds">
            <UiNumberInput v-model="form.operationTimeoutSeconds" :min="1" :max="300" :placeholder="t('servers.ssh.secondsPlaceholder')" data-testid="server-ssh-operation-timeout" />
          </UiField>
        </UiStack>
      </template>
    </UiStack>

    <template #footer>
      <UiButton type="button" variant="secondary" data-testid="server-ssh-cancel" @click="emit('close')">
        {{ t('shared.close') }}
      </UiButton>
      <UiButton type="button" variant="primary" :loading="saving" :disabled="!settings" data-testid="server-ssh-save" @click="handleSave">
        {{ t('shared.saveChanges') }}
      </UiButton>
    </template>
  </UiDialog>
</template>
