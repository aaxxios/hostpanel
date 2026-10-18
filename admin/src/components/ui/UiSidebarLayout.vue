<script setup lang="ts">
/**
 * Layout of a section that has an inner sidebar.
 *
 * The `sidebar` slot sits against the edge of the shell (the layout cancels the shell gutter so the
 * sidebar meets it) and the default slot is the content, which scrolls on its own beside it.
 */
</script>

<template>
  <div class="flex h-full min-h-0 flex-1 -m-4 sm:-m-6 lg:-m-8">
    <slot name="sidebar" />
    <div class="flex-1 min-h-0 overflow-auto p-4 sm:p-6 lg:p-8">
      <slot />
    </div>
  </div>
</template>
