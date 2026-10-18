/**
 * Registers the motion plugin so `v-motion` directives work in every component.
 *
 * @module plugins/motion
 */

import { MotionPlugin } from '@vueuse/motion'

export default defineNuxtPlugin((nuxtApp) => {
  nuxtApp.vueApp.use(MotionPlugin)
})
