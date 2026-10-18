/**
 * Turns an icon's inner SVG markup (a string such as `<path d="M1 1"/><circle cx="2" cy="2" r="1"/>`)
 * into Vue nodes, so an icon set stored as strings renders without `v-html`.
 *
 * Only a fixed list of shape tags and presentation attributes is ever built; anything else in the
 * string is dropped, so a string that was not written by us can never inject a script, an event
 * handler or a `style`.
 */
import { h } from 'vue'
import type { VNode } from 'vue'

/** SVG shape tags an icon may use. */
const ALLOWED_TAGS = new Set(['path', 'circle', 'rect', 'line', 'polyline', 'polygon', 'ellipse'])

/** SVG attributes an icon shape may carry. */
const ALLOWED_ATTRS = new Set([
  'd', 'cx', 'cy', 'r', 'rx', 'ry', 'x', 'y', 'x1', 'y1', 'x2', 'y2', 'width', 'height', 'points',
  'fill', 'stroke', 'stroke-width', 'stroke-linecap', 'stroke-linejoin', 'transform', 'opacity',
])

/** Matches one self-closing or empty shape tag; group 1 is the tag name, group 2 its attributes. */
const TAG_PATTERN = new RegExp('<([a-z]+)([^>]*?)/?>', 'g')

/** Matches one `name="value"` attribute; group 1 is the name, group 2 the value. */
const ATTR_PATTERN = new RegExp('([a-z0-9-]+)="([^"]*)"', 'g')

/**
 * Parses the attribute text of one tag into a props object, keeping only allowed attributes.
 *
 * @param source - Text between the tag name and the closing bracket.
 * @returns The allowed attributes by name.
 */
const parseAttrs = (source: string): Record<string, string> => {
  const attrs: Record<string, string> = {}
  for (const match of source.matchAll(ATTR_PATTERN)) {
    const name = match[1]
    const value = match[2]
    if (name !== undefined && value !== undefined && ALLOWED_ATTRS.has(name)) {
      attrs[name] = value
    }
  }
  return attrs
}

/**
 * Builds the child nodes of an `<svg>` from inner markup.
 *
 * @param markup - Inner SVG markup made of shape tags only.
 * @returns One virtual node per recognised shape, in source order.
 */
export const svgNodes = (markup: string): VNode[] => {
  const nodes: VNode[] = []
  for (const match of markup.matchAll(TAG_PATTERN)) {
    const tag = match[1]
    if (tag !== undefined && ALLOWED_TAGS.has(tag)) {
      nodes.push(h(tag, parseAttrs(match[2] ?? '')))
    }
  }
  return nodes
}
