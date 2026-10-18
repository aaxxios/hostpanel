<script setup lang="ts">
/**
 * Knowledgebase management: tabs for adding categories (with the category browser) and articles.
 */
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import SupportCheckboxField from '@/components/support/SupportCheckboxField.vue'
import SupportRowActions from '@/components/support/SupportRowActions.vue'
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
import UiInput from '@/components/ui/UiInput.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTabs from '@/components/ui/UiTabs.vue'
import UiText from '@/components/ui/UiText.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import { enumValueOf } from '@/lib/enumValue'
import { useKnowledgebaseStore } from '@/stores/knowledgebase'
import type { KbCategory } from '@/types/kbarticle'
import { KnowledgebaseTab } from '@/types/supporttab'
import { Icon } from '@/lib/icons'
import { UiButtonVariant } from '@/utils/ui/button'
import { UiSize } from '@/utils/ui/size'
import { UiTone } from '@/utils/ui/tone'
import type { UiTabItem } from '@/utils/ui/tabs'

const { t } = useI18n()

const store = useKnowledgebaseStore()

/** Currently active tab. */
const activeTab = ref<KnowledgebaseTab>(KnowledgebaseTab.Category)

/** Tab items for the kit's tab strip. */
const tabItems = computed<UiTabItem[]>(() => [
  { value: KnowledgebaseTab.Category, label: t('shared.addCategory'), testid: 'knowledgebase-tab-category' },
  { value: KnowledgebaseTab.Article, label: t('support.knowledgebase.addArticle'), testid: 'knowledgebase-tab-article' },
])

/** New category form fields. */
const newCategory = ref({ name: '', description: '', isHidden: false })

/** Whether a category is being saved. */
const categorySaving = ref(false)

/** ID of the category waiting for delete confirmation; null when no dialog is open. */
const deleteCategoryTarget = ref<number | null>(null)

/** Whether a category deletion is in progress. */
const deletingCategory = ref(false)

/** ID of the category being edited in place; null when none. */
const editingCategoryId = ref<number | null>(null)

/** Edit form data of the category being edited. */
const editingCategory = ref({ name: '', description: '', isHidden: false })

/** New article form fields. */
const newArticle = ref({ title: '', categoryId: 0, content: '', published: false })

/** Whether an article is being saved. */
const articleSaving = ref(false)

/** Category options for select dropdowns. */
const categoryOptions = computed(() => store.categories.map((category) => ({ value: category.id, label: category.name })))

/** Whether the article form is complete. */
const canSaveArticle = computed<boolean>(
  () => newArticle.value.title.trim() !== '' && newArticle.value.categoryId !== 0 && newArticle.value.content.trim() !== '',
)

/**
 * Switches tab.
 *
 * @param value - The tab key the tab strip emitted.
 */
const selectTab = (value: string): void => {
  const tab = enumValueOf(KnowledgebaseTab, value)
  if (tab !== null) {
    activeTab.value = tab
  }
}

/**
 * Creates a category from the add form and clears the form on success.
 */
const addCategory = async (): Promise<void> => {
  if (!newCategory.value.name.trim()) {
    return
  }
  categorySaving.value = true
  const id = await store.createCategory({
    name: newCategory.value.name.trim(),
    description: newCategory.value.description.trim(),
    isHidden: newCategory.value.isHidden,
  })
  categorySaving.value = false
  if (id !== null) {
    newCategory.value = { name: '', description: '', isHidden: false }
  }
}

/**
 * Starts editing a category in place.
 *
 * @param category - The category to edit.
 */
const startEditCategory = (category: KbCategory): void => {
  editingCategoryId.value = category.id
  editingCategory.value = {
    name: category.name,
    description: category.description,
    isHidden: category.isHidden,
  }
}

/**
 * Saves the category being edited.
 */
const saveEditCategory = async (): Promise<void> => {
  if (editingCategoryId.value === null) {
    return
  }
  if (await store.updateCategory(editingCategoryId.value, { ...editingCategory.value })) {
    editingCategoryId.value = null
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
 * Creates an article from the add form, clears the form and refreshes the category counts.
 */
const addArticle = async (): Promise<void> => {
  if (!canSaveArticle.value) {
    return
  }
  // The API files an article under a category by its name, not its id.
  const category = store.categories.find((candidate) => candidate.id === newArticle.value.categoryId)
  if (!category) {
    return
  }
  articleSaving.value = true
  const id = await store.createArticle({
    title: newArticle.value.title.trim(),
    content: newArticle.value.content.trim(),
    category: category.name,
    isPublished: newArticle.value.published,
  })
  articleSaving.value = false
  if (id !== null) {
    newArticle.value = { title: '', categoryId: 0, content: '', published: false }
    await store.fetchCategories()
  }
}

onMounted(() => {
  store.fetchCategories()
})
</script>

<template>
  <UiStack>
    <UiPageHeader
      :title="t('support.knowledgebase.knowledgebase')"
      :subtitle="t('support.knowledgebase.manageKnowledgebaseArticlesAndCategories')"
    />

    <UiAlert v-if="store.error" variant="error" data-testid="knowledgebase-error">{{ store.error }}</UiAlert>

    <UiTabs :items="tabItems" :model-value="activeTab" @update:model-value="selectTab" />

    <template v-if="activeTab === KnowledgebaseTab.Category">
      <UiCard padding="md">
        <UiStack>
          <UiField :label="t('shared.categoryName')" :error="store.fieldErrors.name">
            <UiInput
              v-model="newCategory.name"
              :placeholder="t('shared.enterCategoryName')"
              data-testid="knowledgebase-category-name"
            />
          </UiField>
          <SupportCheckboxField
            v-model="newCategory.isHidden"
            :label="t('support.knowledgebase.checkToHide')"
            testid="knowledgebase-category-hidden"
          />
          <UiField :label="t('shared.description')">
            <UiInput
              v-model="newCategory.description"
              :placeholder="t('support.knowledgebase.categoryDescription')"
              data-testid="knowledgebase-category-description"
            />
          </UiField>
          <UiStack direction="row">
            <UiButton
              :variant="UiButtonVariant.Primary"
              :loading="categorySaving"
              :disabled="!newCategory.name.trim()"
              data-testid="knowledgebase-category-add"
              @click="addCategory"
            >
              {{ t('shared.addCategory') }}
            </UiButton>
          </UiStack>
        </UiStack>
      </UiCard>

      <UiStack direction="row" gap="xs" align="center">
        <UiText tone="muted" size="md">{{ t('shared.youAreHere') }}</UiText>
        <UiText tone="secondary" size="md" class="font-medium">{{ t('support.knowledgebase.knowledgebaseHome') }}</UiText>
      </UiStack>

      <UiSkeleton v-if="store.loading" :lines="3" data-testid="knowledgebase-loading" />

      <template v-else>
        <UiHeading :level="2" variant="card">{{ t('support.knowledgebase.browseByCategory') }}</UiHeading>

        <UiGrid v-if="store.categories.length > 0" :columns="3" data-testid="knowledgebase-categories">
          <UiCard
            v-for="category in store.categories"
            :key="category.id"
            padding="md"
            :data-testid="`knowledgebase-category-${category.id}`"
          >
            <UiStack v-if="editingCategoryId === category.id" gap="sm">
              <UiInput
                v-model="editingCategory.name"
                :placeholder="t('shared.categoryName2')"
                data-testid="knowledgebase-edit-name"
              />
              <UiInput
                v-model="editingCategory.description"
                :placeholder="t('shared.description')"
                data-testid="knowledgebase-edit-description"
              />
              <SupportCheckboxField
                v-model="editingCategory.isHidden"
                :label="t('shared.hidden')"
                testid="knowledgebase-edit-hidden"
              />
              <UiStack direction="row" gap="sm">
                <UiButton
                  :variant="UiButtonVariant.Primary"
                  :size="UiSize.Sm"
                  data-testid="knowledgebase-edit-save"
                  @click="saveEditCategory"
                >
                  {{ t('shared.save') }}
                </UiButton>
                <UiButton
                  :variant="UiButtonVariant.Secondary"
                  :size="UiSize.Sm"
                  data-testid="knowledgebase-edit-cancel"
                  @click="editingCategoryId = null"
                >
                  {{ t('shared.cancel') }}
                </UiButton>
              </UiStack>
            </UiStack>

            <UiStack v-else gap="sm">
              <UiStack direction="row" justify="between" align="start">
                <UiStack direction="row" gap="sm" align="center">
                  <UiIcon :markup="Icon.Folder" />
                  <UiText tone="primary" size="lg">{{ category.name }}</UiText>
                  <UiText tone="muted" size="sm">({{ category.articleCount }})</UiText>
                </UiStack>
                <SupportRowActions
                  :testid-prefix="`knowledgebase-category-${category.id}`"
                  @edit="startEditCategory(category)"
                  @delete="deleteCategoryTarget = category.id"
                />
              </UiStack>
              <UiText v-if="category.description" tone="muted" size="sm">{{ category.description }}</UiText>
              <UiStack direction="row">
                <UiBadge v-if="category.isHidden" :tone="UiTone.Warning" small>{{ t('shared.hidden') }}</UiBadge>
              </UiStack>
            </UiStack>
          </UiCard>
        </UiGrid>

        <UiCard v-else>
          <UiEmptyState
            :title="t('support.knowledgebase.noCategoriesFound')"
            :description="t('support.knowledgebase.createYourFirstCategoryAbove')"
            data-testid="knowledgebase-categories-empty"
          />
        </UiCard>

        <UiHeading :level="2" variant="card">{{ t('support.knowledgebase.browseByTag') }}</UiHeading>
        <UiCard>
          <UiEmptyState
            :title="t('support.knowledgebase.noTagsFound')"
            :description="t('support.knowledgebase.tagsWillAppearHereWhen')"
          />
        </UiCard>
      </template>
    </template>

    <UiCard v-if="activeTab === KnowledgebaseTab.Article" padding="md">
      <UiStack>
        <UiField :label="t('shared.title')" :error="store.fieldErrors.title">
          <UiInput
            v-model="newArticle.title"
            :placeholder="t('support.knowledgebase.articleTitle')"
            data-testid="knowledgebase-article-title"
          />
        </UiField>
        <UiField :label="t('shared.category')" :error="store.fieldErrors.category">
          <UiSelect
            v-model="newArticle.categoryId"
            :options="categoryOptions"
            :placeholder="t('shared.selectCategory')"
            data-testid="knowledgebase-article-category"
          />
        </UiField>
        <UiField :label="t('support.knowledgebase.content')" :error="store.fieldErrors.content">
          <UiTextarea
            v-model="newArticle.content"
            rows="12"
            :placeholder="t('support.knowledgebase.articleContent')"
            data-testid="knowledgebase-article-content"
          />
        </UiField>
        <SupportCheckboxField
          v-model="newArticle.published"
          :label="t('shared.published')"
          testid="knowledgebase-article-published"
        />
        <UiStack direction="row">
          <UiButton
            :variant="UiButtonVariant.Primary"
            :loading="articleSaving"
            :disabled="!canSaveArticle"
            data-testid="knowledgebase-article-save"
            @click="addArticle"
          >
            {{ articleSaving ? t('shared.saving') : t('support.knowledgebase.saveArticle') }}
          </UiButton>
        </UiStack>
      </UiStack>
    </UiCard>

    <UiDialog
      variant="confirm"
      :open="deleteCategoryTarget !== null"
      :title="t('support.knowledgebase.deleteCategory')"
      :message="t('support.knowledgebase.areYouSureYouWant')"
      :confirm-label="t('shared.delete')"
      :loading-label="t('shared.deleting')"
      :loading="deletingCategory"
      tone="danger"
      @confirm="handleDeleteCategory"
      @close="deleteCategoryTarget = null"
    />
  </UiStack>
</template>
