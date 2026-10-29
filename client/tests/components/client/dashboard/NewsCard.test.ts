/**
 * Unit tests for ClientDashboardNewsCard: the latest announcements from the content store and
 * the optional link to the billing portal.
 *
 * @module tests/components/client/dashboard/NewsCard.test
 */

import { describe, expect, it, vi } from 'vitest'
import { nextTick } from 'vue'
import { mountClient } from '~/tests/helpers/mountClient'
import NewsCard from '~/components/client/dashboard/NewsCard.vue'
import { useContentStore } from '~/stores/content'
import type { PublishedAnnouncement } from '~/types/publishedannouncement'

/**
 * Builds a published announcement.
 *
 * @param id - Announcement id.
 * @returns An announcement with a title and a publication time.
 */
const news = (id: number): PublishedAnnouncement => ({ id, title: `News ${id}`, content: '', publishedAt: '2026-10-01T09:00:00Z' })

describe('ClientDashboardNewsCard', () => {
  it('says there are no announcements when the list is empty', () => {
    const wrapper = mountClient(NewsCard)

    expect(wrapper.find('[data-testid="dashboard-news-empty"]').text()).toBe('client.dashboard.noAnnouncements')
  })

  it('lists at most three announcements', async () => {
    const wrapper = mountClient(NewsCard)
    useContentStore().recentNews = [1, 2, 3, 4].map(news)
    await nextTick()

    expect(wrapper.find('[data-testid="dashboard-news-1"]').text()).toContain('News 1')
    expect(wrapper.find('[data-testid="dashboard-news-3"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="dashboard-news-4"]').exists()).toBe(false)
  })

  it('hides the view-all link when no billing portal is configured', () => {
    const wrapper = mountClient(NewsCard)

    expect(wrapper.find('[data-testid="dashboard-news-all"]').exists()).toBe(false)
  })

  it('links to the billing portal announcements when one is configured', () => {
    vi.stubGlobal('useRuntimeConfig', () => ({ public: { billingPortalUrl: 'https://billing.example.test' } }))

    const wrapper = mountClient(NewsCard)

    const link = wrapper.find('[data-testid="dashboard-news-all"]')
    expect(link.attributes('href')).toBe('https://billing.example.test/announcements.php')
    expect(link.attributes('target')).toBe('_blank')
  })
})
