<script setup lang="ts">
/**
 * Predefined replies management: tabs for categories, reply templates and a search over the
 * templates.
 */
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import SupportRowActions from '@/components/support/SupportRowActions.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiField from '@/components/ui/UiField.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTabs from '@/components/ui/UiTabs.vue'
import UiText from '@/components/ui/UiText.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import { enumValueOf } from '@/lib/enumValue'
import { usePredefinedRepliesStore } from '@/stores/predefinedReplies'
import type { PredefinedReply, PredefinedReplyCategory } from '@/types/predefinedreply'
import { PredefinedRepliesTab } from '@/types/supporttab'
import { UiButtonVariant } from '@/utils/ui/button'
import { UiSize } from '@/utils/ui/size'
import type { UiTabItem } from '@/utils/ui/tabs'

const { t } = useI18n()

const store = usePredefinedRepliesStore()

/** Delay between the last keystroke in the search box and the search, in milliseconds. */
const SEARCH_DEBOUNCE_MS = 300

/** Currently active tab. */
const activeTab = ref<PredefinedRepliesTab>(PredefinedRepliesTab.Category)

/** Tab items for the kit's tab strip. */
const tabItems = computed<UiTabItem[]>(() => [
  { value: PredefinedRepliesTab.Category, label: t('shared.addCategory'), testid: 'predefined-replies-tab-category' },
  { value: PredefinedRepliesTab.Reply, label: t('support.predefinedReplies.addPredefinedReply'), testid: 'predefined-replies-tab-reply' },
  { value: PredefinedRepliesTab.Search, label: t('shared.searchFilter'), testid: 'predefined-replies-tab-search' },
])

/** New category name input. */
const newCategoryName = ref('')

/** Whether a category is being saved. */
const categorySaving = ref(false)

/** ID of the category being edited in place; null when none. */
const editingCategoryId = ref<number | null>(null)

/** Name being typed for the category under edit. */
const editingCategoryName = ref('')

/** ID of the category waiting for delete confirmation; null when no dialog is open. */
const deleteCategoryTarget = ref<number | null>(null)

/** Whether a category deletion is in progress. */
const deletingCategory = ref(false)

/** New reply name input. */
const newReplyName = ref('')

/** New reply category selection (0 = none chosen). */
const newReplyCategoryId = ref<number>(0)

/** New reply content. */
const newReplyContent = ref('')

/** Whether a reply is being saved. */
const replySaving = ref(false)

/** ID of the reply waiting for delete confirmation; null when no dialog is open. */
const deleteReplyTarget = ref<number | null>(null)

/** Whether a reply deletion is in progress. */
const deletingReply = ref(false)

/** ID of the reply being edited in place; null when none. */
const editingReplyId = ref<number | null>(null)

/** Fields of the reply under edit. */
const editingReply = ref({ name: '', categoryId: 0, content: '' })

/** Search query string. */
const searchQuery = ref('')

/** Search results. */
const searchResults = ref<PredefinedReply[]>([])

/** Whether a search is in progress. */
const searching = ref(false)

/** Debounce timer of the search box. */
let searchTimer: ReturnType<typeof setTimeout> | undefined

/** Category options for select dropdowns. */
const categoryOptions = computed(() => store.categories.map((category) => ({ value: category.id, label: category.name })))

/** Whether the add-reply form is complete. */
const canAddReply = computed<boolean>(
  () => newReplyName.value.trim() !== '' && newReplyCategoryId.value !== 0 && newReplyContent.value.trim() !== '',
)

/**
 * Switches tab.
 *
 * @param value - The tab key the tab strip emitted.
 */
const selectTab = (value: string): void => {
  const tab = enumValueOf(PredefinedRepliesTab, value)
  if (tab !== null) {
    activeTab.value = tab
  }
}

/**
 * Creates a category from the add form and clears it on success.
 */
const addCategory = async (): Promise<void> => {
  if (!newCategoryName.value.trim()) {
    return
  }
  categorySaving.value = true
  const id = await store.createCategory(newCategoryName.value.trim())
  categorySaving.value = false
  if (id !== null) {
    newCategoryName.value = ''
  }
}

/**
 * Starts editing a category's name in place.
 *
 * @param category - The category to edit.
 */
const startEditCategory = (category: PredefinedReplyCategory): void => {
  editingCategoryId.value = category.id
  editingCategoryName.value = category.name
}

/**
 * Stops editing the category without saving.
 */
const cancelEditCategory = (): void => {
  editingCategoryId.value = null
  editingCategoryName.value = ''
}

/**
 * Saves the category name under edit.
 */
const saveEditCategory = async (): Promise<void> => {
  if (editingCategoryId.value === null || !editingCategoryName.value.trim()) {
    return
  }
  if (await store.updateCategory(editingCategoryId.value, editingCategoryName.value.trim())) {
    cancelEditCategory()
  }
}

/**
 * Deletes the category the dialog asked about.
 */
const handleDeleteCategory = async (): Promise<void> => {
  if (deleteCategoryTarget.value === null) {
    return
  }
  deletingCategory.value = true
  if (await store.deleteCategory(deleteCategoryTarget.value)) {
    deleteCategoryTarget.value = null
  }
  deletingCategory.value = false
}

/**
 * Creates a reply from the add form, clears it and reloads the replies.
 */
const addReply = async (): Promise<void> => {
  if (!canAddReply.value) {
    return
  }
  replySaving.value = true
  const id = await store.createReply({
    name: newReplyName.value.trim(),
    categoryId: newReplyCategoryId.value,
    content: newReplyContent.value.trim(),
  })
  replySaving.value = false
  if (id !== null) {
    newReplyName.value = ''
    newReplyCategoryId.value = 0
    newReplyContent.value = ''
    await store.fetchReplies()
  }
}

/**
 * Starts editing a reply in place.
 *
 * @param reply - The reply to edit.
 */
const startEditReply = (reply: PredefinedReply): void => {
  editingReplyId.value = reply.id
  editingReply.value = { name: reply.name, categoryId: reply.categoryId, content: reply.content }
}

/**
 * Saves the reply under edit and reloads the replies.
 */
const saveEditReply = async (): Promise<void> => {
  if (editingReplyId.value === null) {
    return
  }
  if (await store.updateReply(editingReplyId.value, { ...editingReply.value })) {
    editingReplyId.value = null
    await store.fetchReplies()
  }
}

/**
 * Deletes the reply the dialog asked about, then reloads the replies.
 */
const handleDeleteReply = async (): Promise<void> => {
  if (deleteReplyTarget.value === null) {
    return
  }
  deletingReply.value = true
  if (await store.deleteReply(deleteReplyTarget.value)) {
    deleteReplyTarget.value = null
    await store.fetchReplies()
  }
  deletingReply.value = false
}

// The search runs once typing pauses; an empty box clears the results.
watch(searchQuery, (query) => {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(async () => {
    if (!query.trim()) {
      searchResults.value = []
      return
    }
    searching.value = true
    const hits = await store.searchReplies(query.trim())
    if (hits) {
      searchResults.value = hits
    }
    searching.value = false
  }, SEARCH_DEBOUNCE_MS)
})

onBeforeUnmount(() => {
  clearTimeout(searchTimer)
})

onMounted(async () => {
  await Promise.all([store.fetchCategories(), store.fetchReplies()])
})
</script>

<template>
  <UiStack>
    <UiPageHeader
      :title="t('support.predefinedReplies.predefinedReplies')"
      :subtitle="t('support.predefinedReplies.managePredefinedTicketReplyTemplates')"
    />

    <UiAlert v-if="store.error" variant="error" data-testid="predefined-replies-error">{{ store.error }}</UiAlert>

    <UiTabs :items="tabItems" :model-value="activeTab" @update:model-value="selectTab" />

    <template v-if="activeTab === PredefinedRepliesTab.Category">
      <UiCard padding="md">
        <UiStack direction="row" align="end">
          <UiField :label="t('shared.categoryName')" :error="store.fieldErrors.name" class="flex-1">
            <UiInput
              v-model="newCategoryName"
              :placeholder="t('shared.enterCategoryName')"
              data-testid="predefined-replies-category-name"
              @keyup.enter="addCategory"
            />
          </UiField>
          <UiButton
            :variant="UiButtonVariant.Primary"
            :loading="categorySaving"
            :disabled="!newCategoryName.trim()"
            data-testid="predefined-replies-category-add"
            @click="addCategory"
          >
            {{ t('shared.addCategory') }}
          </UiButton>
        </UiStack>
      </UiCard>

      <UiStack direction="row" gap="xs" align="center">
        <UiText tone="muted" size="md">{{ t('shared.youAreHere') }}</UiText>
        <UiText tone="secondary" size="md" class="font-medium">{{ t('support.predefinedReplies.topLevel') }}</UiText>
      </UiStack>

      <UiSkeleton v-if="store.loading" :lines="3" data-testid="predefined-replies-loading" />

      <UiStack v-else-if="store.categories.length > 0" gap="sm" data-testid="predefined-replies-categories">
        <UiCard
          v-for="category in store.categories"
          :key="category.id"
          padding="sm"
          :data-testid="`predefined-replies-category-${category.id}`"
        >
          <UiStack direction="row" justify="between" align="center" gap="sm">
            <template v-if="editingCategoryId === category.id">
              <UiInput
                v-model="editingCategoryName"
                :placeholder="t('shared.categoryName2')"
                class="flex-1"
                data-testid="predefined-replies-category-edit-name"
                @keyup.enter="saveEditCategory"
                @keyup.escape="cancelEditCategory"
              />
              <UiStack direction="row" gap="sm">
                <UiButton
                  :variant="UiButtonVariant.Primary"
                  :size="UiSize.Sm"
                  data-testid="predefined-replies-category-edit-save"
                  @click="saveEditCategory"
                >
                  {{ t('shared.save') }}
                </UiButton>
                <UiButton
                  :variant="UiButtonVariant.Secondary"
                  :size="UiSize.Sm"
                  data-testid="predefined-replies-category-edit-cancel"
                  @click="cancelEditCategory"
                >
                  {{ t('shared.cancel') }}
                </UiButton>
              </UiStack>
            </template>
            <template v-else>
              <UiText tone="secondary" size="md">{{ category.name }}</UiText>
              <SupportRowActions
                :testid-prefix="`predefined-replies-category-${category.id}`"
                @edit="startEditCategory(category)"
                @delete="deleteCategoryTarget = category.id"
              />
            </template>
          </UiStack>
        </UiCard>
      </UiStack>

      <UiCard v-else>
        <UiEmptyState
          :title="t('support.predefinedReplies.noCategoriesFound')"
          :description="t('support.predefinedReplies.createYourFirstCategoryAbove')"
          data-testid="predefined-replies-categories-empty"
        />
      </UiCard>
    </template>

    <template v-if="activeTab === PredefinedRepliesTab.Reply">
      <UiCard padding="md">
        <UiStack>
          <UiField :label="t('shared.name')" :error="store.fieldErrors.name">
            <UiInput
              v-model="newReplyName"
              :placeholder="t('support.predefinedReplies.replyName')"
              data-testid="predefined-replies-reply-name"
            />
          </UiField>
          <UiField :label="t('shared.category')" :error="store.fieldErrors.categoryId">
            <UiSelect
              v-model="newReplyCategoryId"
              :options="categoryOptions"
              :placeholder="t('shared.selectCategory')"
              data-testid="predefined-replies-reply-category"
            />
          </UiField>
          <UiField :label="t('support.predefinedReplies.content')" :error="store.fieldErrors.content">
            <UiTextarea
              v-model="newReplyContent"
              rows="8"
              :placeholder="t('support.predefinedReplies.replyContent')"
              data-testid="predefined-replies-reply-content"
            />
          </UiField>
          <UiStack direction="row">
            <UiButton
              :variant="UiButtonVariant.Primary"
              :loading="replySaving"
              :disabled="!canAddReply"
              data-testid="predefined-replies-reply-add"
              @click="addReply"
            >
              {{ t('shared.addReply') }}
            </UiButton>
          </UiStack>
        </UiStack>
      </UiCard>

      <UiSkeleton v-if="store.loading" :lines="3" data-testid="predefined-replies-replies-loading" />

      <UiStack v-else-if="store.replies.length > 0" gap="sm" data-testid="predefined-replies-replies">
        <UiCard
          v-for="reply in store.replies"
          :key="reply.id"
          padding="sm"
          :data-testid="`predefined-replies-reply-${reply.id}`"
        >
          <UiStack v-if="editingReplyId === reply.id" gap="sm">
            <UiInput
              v-model="editingReply.name"
              :placeholder="t('support.predefinedReplies.replyName')"
              data-testid="predefined-replies-reply-edit-name"
            />
            <UiSelect
              v-model="editingReply.categoryId"
              :options="categoryOptions"
              :placeholder="t('shared.selectCategory')"
              data-testid="predefined-replies-reply-edit-category"
            />
            <UiTextarea
              v-model="editingReply.content"
              rows="5"
              :placeholder="t('support.predefinedReplies.replyContent2')"
              data-testid="predefined-replies-reply-edit-content"
            />
            <UiStack direction="row" gap="sm">
              <UiButton
                :variant="UiButtonVariant.Primary"
                :size="UiSize.Sm"
                data-testid="predefined-replies-reply-edit-save"
                @click="saveEditReply"
              >
                {{ t('shared.save') }}
              </UiButton>
              <UiButton
                :variant="UiButtonVariant.Secondary"
                :size="UiSize.Sm"
                data-testid="predefined-replies-reply-edit-cancel"
                @click="editingReplyId = null"
              >
                {{ t('shared.cancel') }}
              </UiButton>
            </UiStack>
          </UiStack>

          <UiStack v-else direction="row" justify="between" align="center" gap="sm">
            <UiStack gap="xs">
              <UiText tone="secondary" size="md">{{ reply.name }}</UiText>
              <UiText tone="muted" size="sm">{{ reply.categoryName }}</UiText>
            </UiStack>
            <SupportRowActions
              :testid-prefix="`predefined-replies-reply-${reply.id}`"
              @edit="startEditReply(reply)"
              @delete="deleteReplyTarget = reply.id"
            />
          </UiStack>
        </UiCard>
      </UiStack>

      <UiCard v-else>
        <UiEmptyState
          :title="t('support.predefinedReplies.noPredefinedRepliesFound')"
          :description="t('support.predefinedReplies.createYourFirstReplyUsing')"
          data-testid="predefined-replies-replies-empty"
        />
      </UiCard>
    </template>

    <template v-if="activeTab === PredefinedRepliesTab.Search">
      <UiInput
        v-model="searchQuery"
        :placeholder="t('support.predefinedReplies.searchPredefinedRepliesByName')"
        data-testid="predefined-replies-search"
      />

      <UiSkeleton v-if="searching" :lines="2" data-testid="predefined-replies-searching" />

      <UiStack v-else-if="searchResults.length > 0" gap="sm" data-testid="predefined-replies-results">
        <UiCard
          v-for="result in searchResults"
          :key="result.id"
          padding="sm"
          :data-testid="`predefined-replies-result-${result.id}`"
        >
          <UiStack gap="xs">
            <UiText tone="primary" size="md">{{ result.name }}</UiText>
            <UiText tone="muted" size="sm">{{ result.categoryName }}</UiText>
            <UiText tone="secondary" size="md">{{ result.content }}</UiText>
          </UiStack>
        </UiCard>
      </UiStack>

      <UiCard v-else-if="searchQuery.trim()">
        <UiEmptyState
          :title="t('support.predefinedReplies.noResultsFound')"
          :description="t('support.predefinedReplies.tryADifferentSearchTerm')"
          data-testid="predefined-replies-no-results"
        />
      </UiCard>

      <UiCard v-else>
        <UiEmptyState
          :title="t('support.predefinedReplies.searchPredefinedReplies')"
          :description="t('support.predefinedReplies.typeAboveToSearchBy')"
          data-testid="predefined-replies-search-hint"
        />
      </UiCard>
    </template>

    <UiDialog
      variant="confirm"
      :open="deleteCategoryTarget !== null"
      :title="t('support.predefinedReplies.deleteCategory')"
      :message="t('support.predefinedReplies.areYouSureYouWant')"
      :confirm-label="t('shared.delete')"
      :loading-label="t('shared.deleting')"
      :loading="deletingCategory"
      tone="danger"
      @confirm="handleDeleteCategory"
      @close="deleteCategoryTarget = null"
    />

    <UiDialog
      variant="confirm"
      :open="deleteReplyTarget !== null"
      :title="t('support.predefinedReplies.deletePredefinedReply')"
      :message="t('support.predefinedReplies.areYouSureYouWant2')"
      :confirm-label="t('shared.delete')"
      :loading-label="t('shared.deleting')"
      :loading="deletingReply"
      tone="danger"
      @confirm="handleDeleteReply"
      @close="deleteReplyTarget = null"
    />
  </UiStack>
</template>
