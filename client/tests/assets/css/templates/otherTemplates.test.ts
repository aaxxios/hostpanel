/**
 * Guards aurora and nova against the classic-only button and tab work: the custom-property
 * declarations of their stylesheets (their token blocks) must stay exactly as snapshotted, so a
 * change meant for classic cannot move a colour, radius or shadow of the other templates.
 */
import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { describe, expect, it } from 'vitest'

/**
 * Reads the token declarations of a template stylesheet: every `--name: value;` line, in order.
 *
 * @param name - Template file name without extension.
 * @returns The declaration lines, trimmed.
 */
const tokens = (name: string): string[] =>
  readFileSync(resolve(__dirname, `../../../../assets/css/templates/${name}.css`), 'utf8')
    .split(/\r?\n/)
    .map(line => line.trim())
    .filter(line => /^--[\w-]+:/.test(line))

describe('other template tokens', () => {
  it.each(['aurora', 'nova'])('keeps the %s token block unchanged', (name) => {
    const declarations = tokens(name)

    expect(declarations.length).toBeGreaterThan(20)
    expect(declarations.join('\n')).toMatchSnapshot()
  })
})
