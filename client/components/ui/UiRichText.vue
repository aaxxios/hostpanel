<script lang="ts">
/**
 * UiRichText - renders operator-authored HTML (knowledge-base articles, announcements) safely.
 *
 * The string is never injected as markup. `parseRichText` reads it into a tree containing only
 * allow-listed tags and attributes (see `utils/ui/richText.ts`) and this component turns that
 * tree into Vue nodes with `h()`, so text is always escaped by Vue and no `on*`, `style`,
 * `script`, `iframe` or `javascript:` URL can come through. SSR-safe: the same pure functions run
 * on the server and in the browser, so the output hydrates without a mismatch.
 */
import { defineComponent, h } from 'vue'
import type { PropType, VNode } from 'vue'
import { TAG_CLASSES, parseRichText } from '~/utils/ui/richText'
import type { RichNode } from '~/utils/ui/richText'
import { UiRichNodeType } from '~/utils/ui/enums'

/**
 * Converts one sanitised node into a VNode.
 *
 * @param node A node produced by `parseRichText`.
 * @returns The VNode (a text node for text).
 */
const toVNode = (node: RichNode): VNode | string => {
  if (node.type === UiRichNodeType.Text) {
    return node.text
  }
  const props: Record<string, string> = { ...node.attrs }
  const classes = TAG_CLASSES[node.tag]
  if (classes) {
    props.class = classes
  }
  return h(node.tag, props, node.children.map(toVNode))
}

export default defineComponent({
  name: 'UiRichText',
  props: {
    /** The raw HTML body from the API. */
    html: { type: String as PropType<string>, default: '' },
    /** Tag of the wrapper element. */
    as: { type: String as PropType<string>, default: 'div' },
  },
  setup(props) {
    return () => h(
      props.as,
      { 'data-testid': 'ui-rich-text', class: 'text-gray-700 dark:text-gray-300 leading-relaxed break-words' },
      parseRichText(props.html).map(toVNode),
    )
  },
})
</script>
