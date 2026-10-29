/**
 * Test helper: signs a person with a given permission value into the active Pinia's client store.
 *
 * The permission-aware components read `useClientStore().mayDo(flag)`, which answers from the loaded
 * person. A component test mounts first (the mount creates the Pinia the component reads), then
 * calls this, then asserts on what the component now shows.
 *
 * @module tests/helpers/grantPermissions
 */

import { nextTick } from 'vue'
import { useClientStore } from '~/stores/client'
import { makeClientUser } from '~/tests/helpers/clientUser'

/**
 * Loads a person holding exactly the given permissions into the client store and lets the DOM settle.
 *
 * @param permissions - The `Permission` flags the person holds, combined with bitwise OR; `0` holds nothing.
 * @returns Resolves once the components have re-rendered.
 */
export const grantPermissions = async (permissions: number): Promise<void> => {
  useClientStore().user = makeClientUser({ permissions })
  await nextTick()
}
