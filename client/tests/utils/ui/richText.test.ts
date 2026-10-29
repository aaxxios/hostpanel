/**
 * Unit tests for the rich-text tokenizer: entity decoding, URL safety, the allow-lists, implicit
 * closing, malformed input and an XSS corpus. The tree it returns is the only thing UiRichText
 * renders, so these tests are the sanitiser's contract.
 */
import { describe, expect, it } from 'vitest'
import { ALLOWED_TAGS, decodeEntities, EXTERNAL_LINK_REL, parseRichText } from '~/utils/ui/richText'
import type { RichNode } from '~/utils/ui/richText'
import { UiRichNodeType } from '~/utils/ui/enums'

/**
 * Serialises a tree to a compact string for readable assertions.
 *
 * @param nodes Parsed nodes.
 * @returns For example `<p>hi<b>x</b></p>` with attributes as `name=value`.
 */
const dump = (nodes: RichNode[]): string =>
  nodes
    .map((node) => {
      if (node.type === UiRichNodeType.Text) {
        return node.text
      }
      const attrs = Object.entries(node.attrs).map(([name, value]) => ` ${name}=${value}`).join('')
      return `<${node.tag}${attrs}>${dump(node.children)}</${node.tag}>`
    })
    .join('')

/**
 * Parses and serialises in one step.
 *
 * @param html Operator HTML.
 * @returns The serialised tree.
 */
const parse = (html: string): string => dump(parseRichText(html))

/**
 * Collects every element of a tree, depth first.
 *
 * @param nodes Parsed nodes.
 * @returns The elements.
 */
const elements = (nodes: RichNode[]): Array<Extract<RichNode, { type: UiRichNodeType.Element }>> =>
  nodes.flatMap((node) => (node.type === UiRichNodeType.Element ? [node, ...elements(node.children)] : []))

describe('decodeEntities', () => {
  it('decodes named, decimal and hex references', () => {
    expect(decodeEntities('&amp; &lt; &gt; &quot; &apos; &mdash; &#65; &#x42;')).toBe('& < > " \' — A B')
  })

  it('leaves unknown and invalid references as written', () => {
    expect(decodeEntities('&bogus; &#0; &#xD800; &#1114112;')).toBe('&bogus; &#0; &#xD800; &#1114112;')
  })
})

describe('parseRichText', () => {
  it('returns an empty tree for empty input', () => {
    expect(parseRichText('')).toEqual([])
  })

  it('keeps plain text as a text node', () => {
    expect(parseRichText('hello')).toEqual([{ type: UiRichNodeType.Text, text: 'hello' }])
  })

  it('keeps allow-listed tags and nests them', () => {
    expect(parse('<p>a <strong>b</strong> <em>c</em></p>')).toBe('<p>a <strong>b</strong> <em>c</em></p>')
  })

  it('lower-cases tag names', () => {
    expect(parse('<P>x</P>')).toBe('<p>x</p>')
  })

  it('unwraps unknown tags but keeps their text', () => {
    expect(parse('<p><font color="red">warm</font> <custom-thing>x</custom-thing></p>')).toBe('<p>warm x</p>')
  })

  it('keeps void elements without children', () => {
    expect(parse('a<br>b<hr>c')).toBe('a<br></br>b<hr></hr>c')
  })

  it('merges adjacent text', () => {
    expect(parseRichText('a<unknown>b</unknown>c')).toEqual([{ type: UiRichNodeType.Text, text: 'abc' }])
  })

  it('decodes entities into text, never into markup', () => {
    expect(parseRichText('&lt;script&gt;alert(1)&lt;/script&gt;')).toEqual([{ type: UiRichNodeType.Text, text: '<script>alert(1)</script>' }])
  })

  it('drops comments, doctype and processing instructions', () => {
    expect(parse('a<!-- hidden -->b<!DOCTYPE html>c<?php echo 1 ?>d')).toBe('abcd')
  })

  it('treats a lone angle bracket as text', () => {
    expect(parse('1 < 2 and 3 > 2')).toBe('1 < 2 and 3 > 2')
  })

  it('closes tags left open at the end', () => {
    expect(parse('<p>one<strong>two')).toBe('<p>one<strong>two</strong></p>')
  })

  it('ignores a stray closing tag', () => {
    expect(parse('a</p>b')).toBe('ab')
  })

  it('closes inner tags when an outer one closes', () => {
    expect(parse('<p><strong>bold</p>after')).toBe('<p><strong>bold</strong></p>after')
  })

  it('closes a paragraph when a block element opens inside it', () => {
    expect(parse('<p>one<ul><li>x</li></ul>')).toBe('<p>one</p><ul><li>x</li></ul>')
  })

  it('closes a list item when the next one opens', () => {
    expect(parse('<ul><li>a<li>b</ul>')).toBe('<ul><li>a</li><li>b</li></ul>')
  })

  it('closes table cells and rows implicitly', () => {
    expect(parse('<table><tr><td>a<td>b<tr><td>c</table>')).toBe(
      '<table><tr><td>a</td><td>b</td></tr><tr><td>c</td></tr></table>',
    )
  })

  it('stops nesting past the depth limit instead of recursing forever', () => {
    const deep = '<div>'.repeat(200) + 'x' + '</div>'.repeat(200)

    expect(elements(parseRichText(deep)).length).toBeLessThanOrEqual(33)
  })
})

describe('parseRichText attributes', () => {
  it('keeps href and title on links', () => {
    expect(parse('<a href="/docs" title="Docs">d</a>')).toBe('<a href=/docs title=Docs>d</a>')
  })

  it('sends off-site links to a new tab without leaking the opener', () => {
    const [link] = parseRichText('<a href="https://example.com">x</a>')

    expect(link).toMatchObject({ attrs: { href: 'https://example.com', target: '_blank', rel: EXTERNAL_LINK_REL } })
    expect(EXTERNAL_LINK_REL.split(' ')).toEqual(expect.arrayContaining(['noopener', 'noreferrer', 'ugc']))
  })

  it('treats a protocol-relative link as off-site', () => {
    const [link] = parseRichText('<a href="//example.com/x">x</a>')

    expect(link).toMatchObject({ attrs: { target: '_blank' } })
  })

  it('does not add a target to same-site links', () => {
    const [link] = parseRichText('<a href="/docs">x</a>')

    expect(link).toMatchObject({ attrs: { href: '/docs' } })
    expect(elements([link as RichNode])[0]?.attrs.target).toBeUndefined()
  })

  it('drops unsafe hrefs but keeps the link text', () => {
    expect(parse('<a href="javascript:alert(1)">click</a>')).toBe('<a>click</a>')
  })

  it('accepts single-quoted and unquoted attribute values', () => {
    expect(parse("<a href='/a'>x</a><a href=/b>y</a>")).toBe('<a href=/a>x</a><a href=/b>y</a>')
  })

  it('decodes entities inside attribute values', () => {
    expect(parse('<a href="/search?a=1&amp;b=2">x</a>')).toBe('<a href=/search?a=1&b=2>x</a>')
  })

  it('keeps image src and alt, and only http(s) or relative sources', () => {
    expect(parse('<img src="https://e.com/a.png" alt="A">')).toBe('<img src=https://e.com/a.png alt=A></img>')
    expect(parse('<img src="data:image/png;base64,AAAA" alt="A">')).toBe('<img alt=A></img>')
    expect(parse('<img src="javascript:x">')).toBe('<img></img>')
  })

  it('keeps numeric colspan, rowspan and start only', () => {
    expect(parse('<table><tr><td colspan="2" rowspan="x">a</td></tr></table>')).toBe('<table><tr><td colspan=2>a</td></tr></table>')
    expect(parse('<ol start="3"><li>a</li></ol>')).toBe('<ol start=3><li>a</li></ol>')
  })

  it('drops every attribute on a tag that allows none', () => {
    expect(parse('<p class="x" style="color:red" id="y" data-a="1" onclick="alert(1)">t</p>')).toBe('<p>t</p>')
  })
})

describe('parseRichText XSS corpus', () => {
  /** Payloads that must never yield a dangerous element, attribute or URL. */
  const CORPUS = [
    '<script>alert(1)</script>',
    '<SCRIPT SRC=//evil.example/x.js></SCRIPT>',
    '<img src=x onerror=alert(1)>',
    '<svg onload=alert(1)>',
    '<svg><script>alert(1)</script></svg>',
    '<iframe src="javascript:alert(1)"></iframe>',
    '<iframe srcdoc="<script>alert(1)</script>"></iframe>',
    '<a href="javascript:alert(1)">x</a>',
    '<a href="&#106;avascript:alert(1)">x</a>',
    '<a href="jav&#x09;ascript:alert(1)">x</a>',
    '<a href=" \njavascript:alert(1)">x</a>',
    '<a href="data:text/html,<script>alert(1)</script>">x</a>',
    '<a href="vbscript:msgbox(1)">x</a>',
    '<body onload=alert(1)>',
    '<div style="background:url(javascript:alert(1))">x</div>',
    '<p style="behavior:url(x.htc)">x</p>',
    '<math><mi xlink:href="javascript:alert(1)">x</mi></math>',
    '<object data="javascript:alert(1)"></object>',
    '<embed src="javascript:alert(1)">',
    '<form action="javascript:alert(1)"><input type=submit></form>',
    '<style>@import "javascript:alert(1)";</style>',
    '<template><script>alert(1)</script></template>',
    '<noscript><p title="</noscript><img src=x onerror=alert(1)>"></noscript>',
    '<p <script>alert(1)</script>>x</p>',
    '<<script>alert(1)//<</script>',
    '<img """><script>alert(1)</script>">',
  ]

  it.each(CORPUS)('yields only allow-listed tags and no handler or style attributes for %s', (payload) => {
    elements(parseRichText(payload)).forEach((element) => {
      expect(ALLOWED_TAGS.has(element.tag)).toBe(true)
      Object.keys(element.attrs).forEach((name) => {
        expect(name).not.toMatch(/^on/i)
        expect(name).not.toBe('style')
      })
    })
  })

  it.each(CORPUS)('never lets a script URL through for %s', (payload) => {
    elements(parseRichText(payload)).forEach((element) => {
      ;[element.attrs.href, element.attrs.src].forEach((url) => {
        if (url) {
          expect(url.replace(/\s/g, '')).not.toMatch(/^(javascript|vbscript|data):/i)
        }
      })
    })
  })
})
