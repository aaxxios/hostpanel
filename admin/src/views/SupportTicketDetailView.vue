<script setup lang="ts">
/**
 * Support ticket detail: the ticket's admin controls (department, priority, close/reopen,
 * delete), the staff reply form and the conversation thread.
 */
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import SupportTicketStatusBadge from '@/components/support/SupportTicketStatusBadge.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiField from '@/components/ui/UiField.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiLink from '@/components/ui/UiLink.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
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

const { t } = useI18n()

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
  editDepartmentId.value = ticket.departmentId ?? 0
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
  await reloadIf(await store.updateTicket(ticketId.value, payload))
}

/**
 * Closes the ticket.
 */
const handleClose = async (): Promise<void> => {
  await reloadIf(await store.updateTicket(ticketId.value, { status: TicketStatus.Closed }))
}

/**
 * Reopens a closed ticket.
 */
const handleReopen = async (): Promise<void> => {
  await reloadIf(await store.updateTicket(ticketId.value, { status: TicketStatus.Open }))
}

/**
 * Sends the staff reply and clears the message on success.
 */
const handleReply = async (): Promise<void> => {
  if (!canReply.value) {
    return
  }
  const sent = await store.replyToTicket(ticketId.value, replyMessage.value.trim())
  if (sent) {
    replyMessage.value = ''
    await store.fetchById(ticketId.value)
  }
}

/**
 * Deletes the ticket and returns to the tickets list.
 */
const handleDelete = async (): Promise<void> => {
  showDeleteDialog.value = false
  if (await store.deleteTicket(ticketId.value)) {
    router.push('/support/tickets')
  }
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

    <UiAlert v-else-if="store.error && !store.currentTicket" variant="error" data-testid="ticket-error">
      {{ store.error }}
    </UiAlert>

    <template v-else-if="store.currentTicket">
      <UiStack direction="row" justify="between" align="center" wrap>
        <UiStack direction="row" align="center" wrap>
          <UiLink
            to="/support/tickets"
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

      <UiAlert v-if="store.error" variant="error" data-testid="ticket-action-error">{{ store.error }}</UiAlert>

      <UiCard padding="md">
        <UiStack>
          <UiHeading :level="2" variant="card">{{ t('support.supportTicketDetail.ticketSettings') }}</UiHeading>
          <UiGrid :columns="4">
            <UiField :label="t('shared.department')">
              <UiSelect
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
                  <UiText tone="primary" size="md">{{ reply.authorName }}</UiText>
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
      variant="confirm"
      :open="showDeleteDialog"
      :title="t('support.supportTicketDetail.deleteTicket')"
      :message="t('support.supportTicketDetail.deleteTicketMessage', { id: ticketId })"
      :confirm-label="t('shared.delete')"
      :loading-label="t('shared.deleting')"
      :loading="store.loading"
      tone="danger"
      @confirm="handleDelete"
      @close="showDeleteDialog = false"
    />
  </UiStack>
</template>
