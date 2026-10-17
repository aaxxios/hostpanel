/**
 * Pinia store for published editorial content: announcements and the knowledgebase.
 *
 * A server-rendered page wraps a load action in `useAsyncData(key, () => content.loadX(), ...)`
 * and renders store state, which `@pinia/nuxt` carries from the server render to the browser.
 * The store owns the loading flags and the failure sentence, so a page holds no `catch` and
 * never builds a URL -- transport belongs to {@link useContentApi}.
 *
 * @module stores/content
 */

import { defineStore } from 'pinia'
import { useContentApi } from '~/composables/apis/useContentApi'
import { apiErrorMessage } from '~/lib/errorHandler'
import type { Announcement } from '~/types/announcement'
import type { AnnouncementSummary } from '~/types/announcementsummary'
import type { KbArticle } from '~/types/kbarticle'
import type { KbArticleQuery } from '~/types/kbarticlequery'
import type { KbCategory } from '~/types/kbcategory'

/** How many articles the knowledgebase landing page lists as popular. */
const POPULAR_ARTICLE_COUNT = 10

/** How many articles a category page and a search result list at most. */
const CATEGORY_ARTICLE_LIMIT = 100

/** How many articles a search returns at most. */
const SEARCH_ARTICLE_LIMIT = 50

/**
 * Central store for announcements and the knowledgebase.
 *
 * @returns The content state and the actions that change it.
 */
export const useContentStore = defineStore('content', () => {
  const api = useContentApi()

  /** The announcements, newest first; empty until {@link loadAnnouncements}. */
  const announcements = ref<Announcement[]>([])

  /** The latest published announcements for the dashboard card; empty until {@link loadRecentNews}. */
  const recentNews = ref<AnnouncementSummary[]>([])

  /** The knowledgebase categories; empty until {@link loadKbCategories}. */
  const kbCategories = ref<KbCategory[]>([])

  /** The most viewed articles, most viewed first; empty until {@link loadPopularArticles}. */
  const popularArticles = ref<KbArticle[]>([])

  /** Articles already read per category, keyed by category id. */
  const articlesByCategory = ref<Record<string, KbArticle[]>>({})

  /** Articles already read in full, keyed by article id. */
  const articlesById = ref<Record<string, KbArticle>>({})

  /** The articles the last search found; empty before a search. */
  const searchResults = ref<KbArticle[]>([])

  /** Number of content reads in flight. */
  const inFlight = ref(0)

  /** Whether any content read is in flight. */
  const loading = computed(() => inFlight.value > 0)

  /** Whether a knowledgebase search is in flight. */
  const searching = ref(false)

  /** The sentence for the last failed read; empty when there is none to show. */
  const error = ref('')

  /**
   * Runs one content read with the loading flag and the failure sentence handled once.
   *
   * @param read - The read to run.
   * @param fallback - What to return when the read fails.
   * @returns The read's result, or the fallback after a failure ({@link error} then holds the
   * sentence).
   */
  const guarded = async <T>(read: () => Promise<T>, fallback: T): Promise<T> => {
    inFlight.value += 1
    error.value = ''
    try {
      return await read()
    } catch (e: unknown) {
      error.value = apiErrorMessage(e)
      return fallback
    } finally {
      inFlight.value -= 1
    }
  }

  /**
   * Reads the announcement list.
   *
   * @returns The announcements; empty after a failure.
   */
  const loadAnnouncements = async (): Promise<Announcement[]> => {
    const list = await guarded(() => api.fetchAnnouncements(), { items: [] as Announcement[] })
    announcements.value = list.items ?? []
    return announcements.value
  }

  /**
   * Looks up one announcement the list read already holds.
   *
   * @param id - Announcement id, as it appears in the route.
   * @returns The announcement, or null when it is not in the list.
   */
  const announcementFor = (id: string): Announcement | null =>
    announcements.value.find(a => String(a.id) === id) ?? null

  /**
   * Reads the latest published announcements for the dashboard card.
   *
   * @returns The summaries; empty after a failure.
   */
  const loadRecentNews = async (): Promise<AnnouncementSummary[]> => {
    recentNews.value = await guarded(() => api.fetchRecentAnnouncements(), [] as AnnouncementSummary[])
    return recentNews.value
  }

  /**
   * Reads the knowledgebase categories.
   *
   * @returns The categories; empty after a failure.
   */
  const loadKbCategories = async (): Promise<KbCategory[]> => {
    kbCategories.value = await guarded(() => api.fetchKbCategories(), [] as KbCategory[])
    return kbCategories.value
  }

  /**
   * Reads the most viewed articles.
   *
   * @returns The articles, most viewed first; empty after a failure.
   */
  const loadPopularArticles = async (): Promise<KbArticle[]> => {
    const query: KbArticleQuery = { limitnum: POPULAR_ARTICLE_COUNT }
    const list = await guarded(() => api.fetchKbArticles(query), { items: [] as KbArticle[] })
    popularArticles.value = [...(list.items ?? [])].sort((a, b) => (b.views ?? 0) - (a.views ?? 0))
    return popularArticles.value
  }

  /**
   * Reads the articles of one category and keeps them.
   *
   * @param categoryId - Category id, as it appears in the route.
   * @returns The articles; empty after a failure.
   */
  const loadCategoryArticles = async (categoryId: string): Promise<KbArticle[]> => {
    const query: KbArticleQuery = { categoryid: categoryId, limitnum: CATEGORY_ARTICLE_LIMIT }
    const list = await guarded(() => api.fetchKbArticles(query), { items: [] as KbArticle[] })
    articlesByCategory.value = { ...articlesByCategory.value, [categoryId]: list.items ?? [] }
    return articlesByCategory.value[categoryId] ?? []
  }

  /**
   * Looks up the articles {@link loadCategoryArticles} already read.
   *
   * @param categoryId - Category id.
   * @returns The articles; empty when none were read.
   */
  const articlesFor = (categoryId: string): KbArticle[] => articlesByCategory.value[categoryId] ?? []

  /**
   * Looks up a category's name among the categories already read.
   *
   * @param categoryId - Category id, as it appears in the route.
   * @returns The name, or null when the category is unknown.
   */
  const categoryName = (categoryId: string): string | null =>
    kbCategories.value.find(c => String(c.id) === categoryId)?.name ?? null

  /**
   * Reads one article in full and keeps it.
   *
   * @param id - Article id, as it appears in the route.
   * @returns The article; null after a failure ({@link error} holds the sentence).
   */
  const loadArticle = async (id: string): Promise<KbArticle | null> => {
    const article = await guarded(() => api.fetchKbArticle(id), null as KbArticle | null)
    if (article) {
      articlesById.value = { ...articlesById.value, [id]: article }
    }
    return article
  }

  /**
   * Looks up an article {@link loadArticle} already read.
   *
   * @param id - Article id.
   * @returns The article, or null when it was not read.
   */
  const articleFor = (id: string): KbArticle | null => articlesById.value[id] ?? null

  /**
   * Searches the knowledgebase.
   *
   * @param term - What to search for.
   * @returns The matching articles; empty after a failure.
   */
  const searchArticles = async (term: string): Promise<KbArticle[]> => {
    searching.value = true
    try {
      const query: KbArticleQuery = { search: term, limitnum: SEARCH_ARTICLE_LIMIT }
      const list = await guarded(() => api.fetchKbArticles(query), { items: [] as KbArticle[] })
      searchResults.value = list.items ?? []
      return searchResults.value
    } finally {
      searching.value = false
    }
  }

  /** Forgets the last search, returning the landing page to its default view. */
  const clearSearch = (): void => {
    searchResults.value = []
  }

  return {
    announcements,
    recentNews,
    kbCategories,
    popularArticles,
    articlesByCategory,
    articlesById,
    searchResults,
    loading,
    searching,
    error,
    loadAnnouncements,
    announcementFor,
    loadRecentNews,
    loadKbCategories,
    loadPopularArticles,
    loadCategoryArticles,
    articlesFor,
    categoryName,
    loadArticle,
    articleFor,
    searchArticles,
    clearSearch
  }
})
