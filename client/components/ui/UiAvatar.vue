<template>
  <span
    :class="sizeClasses"
    class="grid flex-shrink-0 place-items-center rounded-full bg-primary-500/20 font-bold uppercase leading-none text-primary-700 dark:text-primary-300 select-none"
    :title="label || undefined"
    aria-hidden="true"
  >
    <User v-if="!initials" :size="iconSize" :stroke-width="2" />
    <template v-else>{{ initials }}</template>
  </span>
</template>

<script setup lang="ts">
/**
 * Circular identity avatar showing a person's initials.
 *
 * Purely presentational: it takes an already-resolved name and e-mail rather than reading the
 * session itself, so the same component serves the public header, the client-area shell and
 * anywhere a person has to be shown. Marked `aria-hidden` because it never carries information
 * the surrounding control does not already name; the control it sits inside supplies the
 * accessible label.
 */
import { computed } from 'vue'
import { User } from 'lucide-vue-next'
import { UiAvatarSize } from '~/utils/ui/enums'

/** Props of UiAvatar. */
interface Props {
  /** Given name, used for the first initial. */
  firstName?: string
  /** Family name, used for the second initial when present. */
  lastName?: string
  /** E-mail address, used only when no name is known yet. */
  email?: string
  /** Rendered diameter: `sm` for a dense list, `md` for a header control. */
  size?: `${UiAvatarSize}`
}

/** Props of the component with their defaults applied. */
const props = withDefaults(defineProps<Props>(), {
  firstName: '',
  lastName: '',
  email: '',
  size: UiAvatarSize.Md,
})

/** Full name when one is known, otherwise the e-mail; used as the hover title. */
const label = computed(() =>
  [props.firstName, props.lastName].filter(Boolean).join(' ') || props.email)

/**
 * One or two initials.
 *
 * Falls back through name, then e-mail, then an empty string, which the template draws as a
 * person glyph: the avatar appears the moment the session is known, which is before the profile
 * request that carries the name has come back, and a literal `?` there read as a broken account.
 */
const initials = computed(() => {
  const first = props.firstName.trim().charAt(0)
  const last = props.lastName.trim().charAt(0)

  if (first) {
    return `${first}${last}`
  }
  return props.email.trim().charAt(0)
})

/** Pixel size of the person glyph shown while no name or e-mail is known. */
const iconSize = computed(() => (props.size === UiAvatarSize.Sm ? 14 : 18))

/** Diameter and type size for the chosen `size`. */
const sizeClasses = computed(() =>
  props.size === UiAvatarSize.Sm ? 'h-6 w-6 text-[11px]' : 'h-9 w-9 text-[13px]')
</script>
