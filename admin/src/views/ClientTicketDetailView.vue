<script setup lang="ts">
/**
 * Client ticket detail page -- displays full ticket data with admin controls,
 * reply form, and conversation thread. Supports status changes, priority/department
 * updates, and ticket deletion.
 */
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useClientTicketsStore } from '@/stores/clientTickets'
import { formatDate, formatDateTimeCompact } from '@/lib/format'
import { TICKET_PRIORITY_OPTIONS } from '@/lib/constants'
import { ticketStatusLabel, ticketStatusTone } from '@/lib/tones'
import { TicketPriority, TicketStatus } from '@/types/ticket'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
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
import { UiButtonVariant } from '@/utils/ui/button'
import { UiAlignItems, UiJustify } from '@/utils/ui/layout'
import { UiSize } from '@/utils/ui/size'
import { UiTextSize, UiTextTone } from '@/utils/ui/text'
import { UiTone } from '@/utils/ui/tone'

const { t } = useI18n()

/** Icon markup (shape tags) of the back link. */
const BACK_ICON = '<polyline points="15 18 9 12 15 6"/>'

/** Current route, source of the client and ticket ids. */
const route = useRoute()

/** Router, used to leave after a delete. */
const router = useRouter()

/** Client ticket store, the only caller of the ticket endpoints. */
const store = useClientTicketsStore()

/** Client ID from route params. */
const clientId = computed(() => route.params.id as string)

/** Ticket ID from route params. */
const ticketId = computed(() => route.params.ticketId as string)

/** Editable department ID for admin controls. */
const editDepartmentId = ref<number>(0)

/** Editable priority for admin controls. */
const editPriority = ref<TicketPriority>(TicketPriority.Medium)

/** Reply message text. */
const replyMessage = ref('')

/** Whether the delete confirmation modal is visible. */
const showDeleteModal = ref(false)

/** Department options mapped for UiSelect. */
const departmentOptions = computed(() =>
  store.departments.map(d => ({ value: d.id, label: d.name })),
)

/** Priority options for UiSelect. */
const priorityOptions = TICKET_PRIORITY_OPTIONS

/** True when the reply form cannot be sent yet. */
const replyIncomplete = computed<boolean>(() => !replyMessage.value.trim())

/** Last reply timestamp from the ticket's replies. */
const lastReplyAt = computed(() => {
  const ticket = store.currentTicket
  if (!ticket || ticket.replies.length === 0) {
    return null
  }
  // The length check above guarantees a last reply; `?.` only narrows the index access.
  return ticket.replies[ticket.replies.length - 1]?.createdAt ?? null
})

/**
 * Populates editable fields from the loaded ticket.
 *
 * @returns Nothing.
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
 * Reloads the open ticket after a successful change.
 *
 * @param ok - Whether the change was accepted.
 * @returns Promise that resolves when the ticket is reloaded.
 */
const reloadIf = async (ok: boolean): Promise<void> => {
  if (ok) {
    await store.fetchById(ticketId.value)
  }
}

/**
 * Saves admin control changes (department, priority).
 *
 * @returns Promise that resolves when update completes.
 */
const handleSaveChanges = async (): Promise<void> => {
  const payload: { departmentId?: number; priority?: TicketPriority } = {}
  if (editDepartmentId.value) {
    payload.departmentId = editDepartmentId.value
  }
  payload.priority = editPriority.value

  await reloadIf(await store.updateTicket(ticketId.value, payload))
}

/**
 * Closes an open ticket by setting status to Closed.
 *
 * @returns Promise that resolves when update completes.
 */
const handleClose = async (): Promise<void> => {
  await reloadIf(await store.updateTicket(ticketId.value, { status: TicketStatus.Closed }))
}

/**
 * Reopens a closed ticket by setting status to Open.
 *
 * @returns Promise that resolves when update completes.
 */
const handleReopen = async (): Promise<void> => {
  await reloadIf(await store.updateTicket(ticketId.value, { status: TicketStatus.Open }))
}

/**
 * Submits a staff reply to the ticket.
 *
 * @returns Promise that resolves when reply is sent.
 */
const handleReply = async (): Promise<void> => {
  if (replyIncomplete.value) {
    return
  }

  const ok = await store.replyToTicket(ticketId.value, replyMessage.value.trim())
  if (ok) {
    replyMessage.value = ''
    await store.fetchById(ticketId.value)
  }
}

/**
 * Deletes the ticket and navigates back to the tickets list.
 *
 * @returns Promise that resolves when deletion completes.
 */
const handleDelete = async (): Promise<void> => {
  showDeleteModal.value = false
  const ok = await store.deleteTicket(ticketId.value)
  if (ok) {
    router.push(`/clients/${clientId.value}/tickets`)
  }
}

// Populate form when ticket loads
watch(() => store.currentTicket, () => {
  populateForm()
})

onMounted(() => {
  store.fetchById(ticketId.value)
  store.fetchDepartments()
})
</script>

<template>
  <UiStack>
    <UiStack v-if="store.loading && !store.currentTicket" data-testid="client-ticket-loading">
      <UiSkeleton :lines="3" />
      <UiText :tone="UiTextTone.Secondary">{{ t('clients.clientTicketDetail.loadingTicket') }}</UiText>
    </UiStack>

    <UiAlert v-else-if="store.error && !store.currentTicket" variant="error" data-testid="client-ticket-error">
      {{ store.error }}
    </UiAlert>

    <template v-else-if="store.currentTicket">
      <UiStack direction="row" wrap :justify="UiJustify.Between" :align="UiAlignItems.Center">
        <UiStack direction="row" :align="UiAlignItems.Center">
          <UiLink
            :to="`/clients/${clientId}/tickets`"
            :variant="UiButtonVariant.Secondary"
            data-testid="client-ticket-back"
          >
            <UiIcon :markup="BACK_ICON" :size="UiSize.Sm" />
            {{ t('shared.back') }}
          </UiLink>
          <UiHeading :level="1" variant="card">
            {{ t('shared.tktIdSubject', { id: store.currentTicket.id, subject: store.currentTicket.subject }) }}
          </UiHeading>
          <UiBadge :tone="ticketStatusTone[store.currentTicket.status]" small>
            {{ ticketStatusLabel(store.currentTicket.status) }}
          </UiBadge>
        </UiStack>
        <UiStack direction="row" gap="sm" :align="UiAlignItems.Center">
          <UiButton
            v-if="store.currentTicket.status !== TicketStatus.Closed"
            type="button"
            :variant="UiButtonVariant.Secondary"
            data-testid="client-ticket-close"
            @click="handleClose"
          >
            {{ t('shared.close') }}
          </UiButton>
          <UiButton
            v-else
            type="button"
            :variant="UiButtonVariant.Secondary"
            data-testid="client-ticket-reopen"
            @click="handleReopen"
          >
            {{ t('clients.clientTicketDetail.reopen') }}
          </UiButton>
          <UiButton
            type="button"
            :variant="UiButtonVariant.Danger"
            data-testid="client-ticket-delete"
            @click="showDeleteModal = true"
          >
            {{ t('shared.delete') }}
          </UiButton>
        </UiStack>
      </UiStack>

      <UiAlert v-if="store.error" variant="error" data-testid="client-ticket-action-error">
        {{ store.error }}
      </UiAlert>

      <UiCard padding="md">
        <UiStack>
          <UiHeading :level="2" variant="card">{{ t('clients.clientTicketDetail.ticketSettings') }}</UiHeading>
          <UiGrid :columns="4">
            <UiField :label="t('shared.department')">
              <UiSelect v-model="editDepartmentId" :options="departmentOptions" :placeholder="t('shared.selectDepartment')" data-testid="client-ticket-department" />
            </UiField>
            <UiField :label="t('shared.priority')">
              <UiSelect v-model="editPriority" :options="priorityOptions" :placeholder="t('shared.selectPriority')" data-testid="client-ticket-priority" />
            </UiField>
            <UiField :label="t('shared.status')">
              <UiBadge :tone="ticketStatusTone[store.currentTicket.status]">
                {{ ticketStatusLabel(store.currentTicket.status) }}
              </UiBadge>
            </UiField>
            <UiField :label="t('shared.lastReply')">
              <UiText :tone="UiTextTone.Secondary">
                {{ lastReplyAt ? formatDate(lastReplyAt) : t('clients.clientTicketDetail.noRepliesYet') }}
              </UiText>
            </UiField>
          </UiGrid>
          <UiStack direction="row">
            <UiButton
              type="button"
              :variant="UiButtonVariant.Primary"
              :disabled="store.loading"
              data-testid="client-ticket-save"
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
            <template #default="{ id }">
              <UiTextarea
                :id="id"
                v-model="replyMessage"
                rows="5"
                :placeholder="t('clients.clientTicketDetail.typeYourReply')"
                data-testid="client-ticket-message"
              />
            </template>
          </UiField>
          <UiStack direction="row">
            <UiButton
              type="button"
              :variant="UiButtonVariant.Primary"
              :disabled="store.loading || replyIncomplete"
              data-testid="client-ticket-reply"
              @click="handleReply"
            >
              {{ store.loading ? t('shared.sending') : t('clients.clientTicketDetail.reply') }}
            </UiButton>
          </UiStack>
        </UiStack>
      </UiCard>

      <UiCard padding="md">
        <UiStack data-testid="client-ticket-thread">
          <UiHeading :level="2" variant="card">{{ t('clients.clientTicketDetail.conversation') }}</UiHeading>

          <UiCard padding="sm">
            <UiStack gap="sm">
              <UiStack direction="row" :justify="UiJustify.Between" :align="UiAlignItems.Center">
                <UiText :tone="UiTextTone.Primary" :size="UiTextSize.Sm">{{ t('shared.client') }}</UiText>
                <UiText :tone="UiTextTone.Muted" :size="UiTextSize.Xs">{{ formatDateTimeCompact(store.currentTicket.createdAt) }}</UiText>
              </UiStack>
              <UiText :tone="UiTextTone.Secondary" class="whitespace-pre-wrap">{{ store.currentTicket.message }}</UiText>
            </UiStack>
          </UiCard>

          <UiCard v-for="reply in store.currentTicket.replies" :key="reply.id" padding="sm">
            <UiStack gap="sm">
              <UiStack direction="row" :justify="UiJustify.Between" :align="UiAlignItems.Center">
                <UiStack direction="row" gap="sm" :align="UiAlignItems.Center">
                  <UiText :tone="UiTextTone.Primary" :size="UiTextSize.Sm">{{ reply.authorName }}</UiText>
                  <UiBadge v-if="reply.isStaffReply" :tone="UiTone.Primary" small>
                    {{ t('clients.clientTicketDetail.staff') }}
                  </UiBadge>
                </UiStack>
                <UiText :tone="UiTextTone.Muted" :size="UiTextSize.Xs">{{ formatDateTimeCompact(reply.createdAt) }}</UiText>
              </UiStack>
              <UiText :tone="UiTextTone.Secondary" class="whitespace-pre-wrap">{{ reply.message }}</UiText>
            </UiStack>
          </UiCard>

          <UiText v-if="store.currentTicket.replies.length === 0" :tone="UiTextTone.Muted">
            {{ t('clients.clientTicketDetail.noRepliesYet') }}
          </UiText>
        </UiStack>
      </UiCard>
    </template>

    <UiDialog
      v-if="showDeleteModal"
      variant="confirm"
      :title="t('clients.clientTicketDetail.deleteTicket')"
      :message="t('clients.clientTicketDetail.confirmDeleteTicket', { id: ticketId })"
      :confirm-label="t('shared.delete')"
      :loading-label="t('shared.deleting')"
      :loading="store.loading"
      tone="danger"
      @confirm="handleDelete"
      @close="showDeleteModal = false"
    />
  </UiStack>
</template>
