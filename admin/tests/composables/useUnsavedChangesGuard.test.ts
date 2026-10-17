/**
 * Tests for the unsaved-changes guard: a clean form leaves at once, a dirty one parks the
 * navigation until the reader answers, "leave" lets it through and "stay" cancels it.
 */
import { describe, expect, it } from 'vitest'
import { defineComponent, h } from 'vue'
import { mount } from '@vue/test-utils'
import { RouterView, createMemoryHistory, createRouter } from 'vue-router'
import { useUnsavedChangesGuard } from '@/composables/useUnsavedChangesGuard'
import type { UnsavedChangesGuard } from '@/composables/useUnsavedChangesGuard'

/**
 * Mounts a form screen under a router with a second page to navigate to.
 *
 * @param dirty - Reads whether the form has unsaved edits.
 * @returns The router, and the guard handed to the screen.
 */
const setup = async (dirty: () => boolean) => {
  let guard: UnsavedChangesGuard | null = null
  const Form = defineComponent({
    setup() {
      guard = useUnsavedChangesGuard(dirty)
      return () => h('div')
    },
  })
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/form', component: Form },
      { path: '/other', component: { render: () => h('div') } },
    ],
  })
  await router.push('/form')
  await router.isReady()
  mount({ render: () => h(RouterView) }, { global: { plugins: [router] } })
  return { router, guard: () => guard as unknown as UnsavedChangesGuard }
}

describe('useUnsavedChangesGuard', () => {
  it('lets a clean form navigate away without asking', async () => {
    const { router, guard } = await setup(() => false)

    await router.push('/other')

    expect(router.currentRoute.value.path).toBe('/other')
    expect(guard().pending.value).toBe(false)
  })

  it('parks a dirty form until the reader leaves', async () => {
    const { router, guard } = await setup(() => true)

    const navigation = router.push('/other')
    await Promise.resolve()
    expect(guard().pending.value).toBe(true)

    guard().confirmLeave()
    await navigation

    expect(router.currentRoute.value.path).toBe('/other')
    expect(guard().pending.value).toBe(false)
  })

  it('cancels the navigation when the reader stays', async () => {
    const { router, guard } = await setup(() => true)

    const navigation = router.push('/other')
    await Promise.resolve()
    guard().cancelLeave()
    await navigation

    expect(router.currentRoute.value.path).toBe('/form')
  })

  it('ignores an answer when nothing is parked', async () => {
    const { guard } = await setup(() => false)

    expect(() => guard().confirmLeave()).not.toThrow()
    expect(guard().pending.value).toBe(false)
  })
})
