/**
 * Mount helper for the client-area view smoke tests.
 *
 * Every view is mounted with the real kit, a real Pinia and the English catalogue; the API is
 * answered at network level with a refusal, so the view settles in its error/empty state without
 * any store being faked.
 */
import { flushPromises, mount } from '@vue/test-utils'
import type { Component } from 'vue'
import { createPinia } from 'pinia'
import { createMemoryHistory, createRouter } from 'vue-router'
import { createTestI18n } from './i18n'
import { problem, stubFetch } from './fetch'

/** Path every client view test starts at; the ids fill `:id`, `:serviceId`, `:domainId`, `:ticketId`. */
export const CLIENT_VIEW_PATH = '/clients/5/section/9'

/**
 * Mounts a client-area view and waits for its first requests to settle.
 *
 * @param view - The view component.
 * @param refusal - Sentence the stubbed API refuses with.
 * @returns The wrapper after the pending promises resolved.
 */
export const mountClientView = async (view: Component, refusal = 'Server said no') => {
  stubFetch(problem(refusal, 500))
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [{ path: '/clients/:id/:section/:subId', component: { render: () => null } }, { path: '/:pathMatch(.*)*', component: { render: () => null } }],
  })
  await router.push(CLIENT_VIEW_PATH)
  await router.isReady()
  const wrapper = mount(view, {
    global: { plugins: [createTestI18n(), createPinia(), router], stubs: { teleport: true } },
  })
  await flushPromises()
  return wrapper
}
