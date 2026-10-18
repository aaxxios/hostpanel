/**
 * The clients the report pickers offer, loaded when the screen mounts.
 */
import { onMounted, ref } from 'vue'
import type { Ref } from 'vue'
import { useReportsStore } from '@/stores/reports'
import type { ReportClientOption } from '@/types/reportclientoption'

/**
 * Loads the client list for a picker.
 *
 * @returns The clients; empty until loaded, and empty when the call failed.
 */
export const useReportClients = () => {
  const store = useReportsStore()

  /** Clients to choose from. */
  const clients = ref([]) as Ref<ReportClientOption[]>

  onMounted(async () => {
    clients.value = await store.fetchClientPicker()
  })

  return { clients }
}
