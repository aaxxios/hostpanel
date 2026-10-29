<script setup lang="ts">
/**
 * Support ticket detail: the ticket's admin controls (department, priority, close/reopen,
 * delete), the staff reply form and the conversation thread. A refused load offers a retry; each
 * action shows its outcome (success, or the API's sentence) next to its own controls; deleting is
 * confirmed in a danger prompt and returns to the list.
 */
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useFallbackLabels } from '@/composables/useFallbackLabels'
import SupportTicketStatusBadge from '@/components/support/SupportTicketStatusBadge.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiErrorState from '@/components/ui/UiErrorState.vue'
import UiField from '@/components/ui/UiField.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiLink from '@/components/ui/UiLink.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiStatusMessage from '@/components/ui/UiStatusMessage.vue'
import { useResultBanner } from '@/composables/useResultBanner'
import type { ResultBanner } from '@/composables/useResultBanner'
import { UiDialogVariant } from '@/utils/ui/dialog'
import UiText from '@/components/ui/UiText.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import { TICKET_PRIORITY_OPTIONS } from '@/lib/constants'
import { formatDateTimeCompact } from '@/lib/format'
import { useSupportStore } from '@/stores/support'
import { TicketPriority, TicketStatus } from '@/types/ticket'
import type { UpdateTicketPayload } from '@/types/ticket'
import { Icon } from '@/lib/icons'
import { UiButtonVariant } from '@/utils/ui/button'
import { UiSize } from '@/utils/ui/size'
import { UiTone } from '@/utils/ui/tone'
import { AppRoute } from '@/lib/routes'

const { t } = useI18n()


/** Fallback text for fields the API leaves unresolved. */

const { orUnknown } = useFallbackLabels()

const route = useRoute()
const router = useRouter()
const store = useSupportStore()

/** Ticket ID from route params. */
const ticketId = computed<string>(() => String(route.params.id))

/** Editable department ID for the admin controls (0 = none chosen). */
const editDepartmentId = ref<number>(0)

/** Editable priority for the admin controls. */
const editPriority = ref<TicketPriority>(TicketPriority.Medium)

/** Reply message text. */
const replyMessage = ref('')

/** Whether the delete confirmation dialog is visible. */
const showDeleteDialog = ref(false)

/** Outcome line of the header actions (close, reopen, delete). */
const headerFeedback = useResultBanner()

/** Outcome line of the department and priority save. */
const saveFeedback = useResultBanner()

/** Outcome line of the staff reply. */
const replyFeedback = useResultBanner()

/**
 * Tone of an outcome line: danger for a refusal, success otherwise.
 *
 * @param banner - The outcome line.
 * @returns The tone to draw it in.
 */
const toneOf = (banner: ResultBanner): UiTone => (banner.isError.value ? UiTone.Danger : UiTone.Success)

/**
 * Shows the outcome of a change in its own line: the success sentence, or the API's refusal.
 *
 * @param banner - The outcome line next to the action.
 * @param ok - Whether the API accepted the change.
 * @param successKey - Locale key of the success sentence.
 */
const report = (banner: ResultBanner, ok: boolean, successKey: string): void => {
  if (ok) {
    banner.show(t(successKey), false)
  } else {
    banner.show(store.actionError ?? t('errors.noAnswer'), true)
  }
}

/** Department options mapped for UiSelect. */
const departmentOptions = computed(() => store.departments.map((department) => ({ value: department.id, label: department.name })))

/** Whether the reply form can be sent. */
const canReply = computed<boolean>(() => replyMessage.value.trim() !== '')

/** Last reply timestamp from the ticket's replies, null when there is none. */
const lastReplyAt = computed<string | null>(() => {
  const replies = store.currentTicket?.replies ?? []
  return replies[replies.length - 1]?.createdAt ?? null
})

/**
 * Copies the loaded ticket's department and priority into the editable fields.
 */
const populateForm = (): void => {
  const ticket = store.currentTicket
  if (!ticket) {
    return
  }
  editDepartmentId.value = ticket.department.id ?? 0
  editPriority.value = ticket.priority
}

/**
 * Reloads the ticket after a change went through.
 *
 * @param accepted - Whether the change was accepted.
 */
const reloadIf = async (accepted: boolean): Promise<void> => {
  if (accepted) {
    await store.fetchById(ticketId.value)
  }
}

/**
 * Saves the department and priority chosen in the admin controls.
 */
const handleSaveChanges = async (): Promise<void> => {
  const payload: UpdateTicketPayload = { priority: editPriority.value }
  if (editDepartmentId.value) {
    payload.departmentId = editDepartmentId.value
  }
  saveFeedback.clear()
  const saved = await store.updateTicket(ticketId.value, payload)
  report(saveFeedback, saved, 'shared.changesSavedSuccessfully')
  await reloadIf(saved)
}

/**
 * Closes the ticket.
 */
const handleClose = async (): Promise<void> => {
  headerFeedback.clear()
  const closed = await store.updateTicket(ticketId.value, { status: TicketStatus.Closed })
  report(headerFeedback, closed, 'support.supportTicketDetail.ticketClosed')
  await reloadIf(closed)
}

/**
 * Reopens a closed ticket.
 */
const handleReopen = async (): Promise<void> => {
  headerFeedback.clear()
  const reopened = await store.updateTicket(ticketId.value, { status: TicketStatus.Open })
  report(headerFeedback, reopened, 'support.supportTicketDetail.ticketReopened')
  await reloadIf(reopened)
}

/**
 * Sends the staff reply and clears the message on success.
 */
const handleReply = async (): Promise<void> => {
  if (!canReply.value) {
    return
  }
  replyFeedback.clear()
  const sent = await store.replyToTicket(ticketId.value, replyMessage.value.trim())
  report(replyFeedback, sent, 'support.supportTicketDetail.replySent')
  if (sent) {
    replyMessage.value = ''
    await store.fetchById(ticketId.value)
  }
}

/**
 * Deletes the ticket and returns to the tickets list.
 */
const handleDelete = async (): Promise<void> => {
  headerFeedback.clear()
  const deleted = await store.deleteTicket(ticketId.value)
  showDeleteDialog.value = false
  if (deleted) {
    router.push(AppRoute.SupportTickets)
  } else {
    headerFeedback.show(store.actionError ?? t('errors.noAnswer'), true)
  }
}

/** Closes the delete confirmation; ignored while the delete runs. */
const dismissDelete = (): void => {
  if (!store.loading) {
    showDeleteDialog.value = false
  }
}

/** Loads the ticket of the route again, after a refused load. */
const reload = (): void => {
  void store.fetchById(ticketId.value)
}

// The editable fields follow whichever ticket is loaded.
watch(() => store.currentTicket, populateForm)

onMounted(() => {
  store.fetchById(ticketId.value)
  store.fetchDepartments()
})
</script>

<template>
  <UiStack>
    <UiSkeleton v-if="store.loading && !store.currentTicket" :lines="4" data-testid="ticket-loading" />

    <UiErrorState
      v-else-if="store.error && !store.currentTicket"
      :message="store.error"
      :loading="store.loading"
      testid="ticket-error"
      @retry="reload"
    />

    <template v-else-if="store.currentTicket">
      <UiStack direction="row" justify="between" align="center" wrap>
        <UiStack direction="row" align="center" wrap>
          <UiLink
            :to="AppRoute.SupportTickets"
            :variant="UiButtonVariant.Secondary"
            :size="UiSize.Sm"
            data-testid="ticket-back"
          >
            <UiIcon :markup="Icon.Back" :size="UiSize.Sm" />
            {{ t('shared.back') }}
          </UiLink>
          <UiHeading :level="1" variant="card" data-testid="ticket-title">
            {{ t('shared.tktIdSubject', { id: store.currentTicket.id, subject: store.currentTicket.subject }) }}
          </UiHeading>
          <SupportTicketStatusBadge :status="store.currentTicket.status" />
        </UiStack>
        <UiStack direction="row" gap="sm">
          <UiButton
            v-if="store.currentTicket.status !== TicketStatus.Closed"
            :variant="UiButtonVariant.Secondary"
            data-testid="ticket-close"
            @click="handleClose"
          >
            {{ t('shared.close') }}
          </UiButton>
          <UiButton
            v-else
            :variant="UiButtonVariant.Secondary"
            data-testid="ticket-reopen"
            @click="handleReopen"
          >
            {{ t('support.supportTicketDetail.reopen') }}
          </UiButton>
          <UiButton :variant="UiButtonVariant.Danger" data-testid="ticket-delete" @click="showDeleteDialog = true">
            {{ t('shared.delete') }}
          </UiButton>
        </UiStack>
      </UiStack>

      <UiStatusMessage :message="headerFeedback.message.value" :tone="toneOf(headerFeedback)" data-testid="ticket-action-feedback" />

      <UiCard padding="md">
        <UiStack>
          <UiHeading :level="2" variant="card">{{ t('support.supportTicketDetail.ticketSettings') }}</UiHeading>
          <UiGrid :columns="4">
            <UiField :label="t('shared.department')">
              <UiErrorState
                v-if="store.departmentsError !== null"
                :message="store.departmentsError"
                testid="ticket-departments-error"
                @retry="store.fetchDepartments"
              />
              <UiSelect
                v-else
                v-model="editDepartmentId"
                :options="departmentOptions"
                :placeholder="t('shared.selectDepartment')"
                data-testid="ticket-department"
              />
            </UiField>
            <UiField :label="t('shared.priority')">
              <UiSelect
                v-model="editPriority"
                :options="TICKET_PRIORITY_OPTIONS"
                :placeholder="t('shared.selectPriority')"
                data-testid="ticket-priority"
              />
            </UiField>
            <UiField :label="t('shared.status')">
              <SupportTicketStatusBadge :status="store.currentTicket.status" />
            </UiField>
            <UiField :label="t('shared.lastReply')">
              <UiText tone="secondary" size="md" data-testid="ticket-last-reply">
                {{ lastReplyAt ? formatDateTimeCompact(lastReplyAt) : t('support.supportTicketDetail.noRepliesYet') }}
              </UiText>
            </UiField>
          </UiGrid>
          <UiStack direction="row">
            <UiButton
              :variant="UiButtonVariant.Primary"
              :loading="store.loading"
              data-testid="ticket-save"
              @click="handleSaveChanges"
            >
              {{ store.loading ? t('shared.saving') : t('shared.saveChanges') }}
            </UiButton>
          </UiStack>
          <UiStatusMessage :message="saveFeedback.message.value" :tone="toneOf(saveFeedback)" data-testid="ticket-save-feedback" />
        </UiStack>
      </UiCard>

      <UiCard padding="md">
        <UiStack>
          <UiHeading :level="2" variant="card">{{ t('shared.addReply') }}</UiHeading>
          <UiField :label="t('shared.message')">
            <UiTextarea
              v-model="replyMessage"
              rows="5"
              :placeholder="t('support.supportTicketDetail.typeYourReply')"
              data-testid="ticket-reply-message"
            />
          </UiField>
          <UiStack direction="row">
            <UiButton
              :variant="UiButtonVariant.Primary"
              :disabled="store.loading || !canReply"
              data-testid="ticket-reply-submit"
              @click="handleReply"
            >
              {{ store.loading ? t('shared.sending') : t('support.supportTicketDetail.reply') }}
            </UiButton>
          </UiStack>
          <UiStatusMessage :message="replyFeedback.message.value" :tone="toneOf(replyFeedback)" data-testid="ticket-reply-feedback" />
        </UiStack>
      </UiCard>

      <UiCard padding="md">
        <UiStack>
          <UiHeading :level="2" variant="card">{{ t('support.supportTicketDetail.conversation') }}</UiHeading>

          <UiCard padding="sm" data-testid="ticket-initial-message">
            <UiStack gap="sm">
              <UiStack direction="row" justify="between">
                <UiText tone="primary" size="md">{{ t('shared.client') }}</UiText>
                <UiText tone="muted" size="sm">{{ formatDateTimeCompact(store.currentTicket.createdAt) }}</UiText>
              </UiStack>
              <UiText tone="secondary" size="md" class="whitespace-pre-wrap">{{ store.currentTicket.message }}</UiText>
            </UiStack>
          </UiCard>

          <UiCard
            v-for="reply in store.currentTicket.replies"
            :key="reply.id"
            padding="sm"
            :elevated="reply.isStaffReply"
            data-testid="ticket-reply"
          >
            <UiStack gap="sm">
              <UiStack direction="row" justify="between">
                <UiStack direction="row" gap="sm" align="center">
                  <UiText tone="primary" size="md">{{ orUnknown(reply.authorName) }}</UiText>
                  <UiBadge v-if="reply.isStaffReply" :tone="UiTone.Primary" small>
                    {{ t('support.supportTicketDetail.staff') }}
                  </UiBadge>
                </UiStack>
                <UiText tone="muted" size="sm">{{ formatDateTimeCompact(reply.createdAt) }}</UiText>
              </UiStack>
              <UiText tone="secondary" size="md" class="whitespace-pre-wrap">{{ reply.message }}</UiText>
            </UiStack>
          </UiCard>

          <UiEmptyState
            v-if="store.currentTicket.replies.length === 0"
            :title="t('support.supportTicketDetail.noRepliesYet')"
            data-testid="ticket-no-replies"
          />
        </UiStack>
      </UiCard>
    </template>

    <UiDialog
      :variant="UiDialogVariant.Confirm"
      :open="showDeleteDialog"
      :title="t('support.supportTicketDetail.deleteTicket')"
      :message="t('support.supportTicketDetail.deleteTicketMessage', { id: ticketId })"
      :confirm-label="t('shared.delete')"
      :loading-label="t('shared.deleting')"
      :loading="store.loading"
      :tone="UiTone.Danger"
      @confirm="handleDelete"
      @close="dismissDelete"
    />
  </UiStack>
</template>
