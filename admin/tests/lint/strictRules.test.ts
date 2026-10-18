/**
 * Tests for the strict house rules: a function declaration, a brace-less block, a raw element and
 * a bare input fail under `src`, clean code passes, and files outside `src` are not governed.
 */
import { Linter } from 'eslint'
import pluginVue from 'eslint-plugin-vue'
import tseslint from 'typescript-eslint'
import vueParser from 'vue-eslint-parser'
import { describe, expect, it } from 'vitest'
import { strictRules } from '../../eslint.config'

/**
 * Lints a fixture with the strict blocks.
 *
 * @param code - Fixture source.
 * @param filename - Virtual file name (decides the parser and whether the rules apply).
 * @returns The rule ids of the reported messages.
 */
const lint = (code: string, filename: string): (string | null)[] => {
  const linter = new Linter()
  const config = [
    { files: ['**/*.vue'], languageOptions: { parser: vueParser, parserOptions: { parser: tseslint.parser } } },
    { files: ['**/*.ts'], languageOptions: { parser: tseslint.parser } },
    { plugins: { vue: pluginVue, '@typescript-eslint': tseslint.plugin } },
    ...strictRules,
  ] as Linter.Config[]
  return linter.verify(code, config, filename).map((m) => m.ruleId)
}

describe('strict rules', () => {
  it('fail a function declaration and a brace-less if under src', () => {
    const ids = lint('function f(a: boolean) { if (a) return 1 }', 'src/a.ts')
    expect(ids).toContain('no-restricted-syntax')
    expect(ids).toContain('curly')
  })

  it('fail an undocumented const arrow and pass a documented one', () => {
    expect(lint('export const f = (): number => 1', 'src/a.ts')).toContain('jsdoc/require-jsdoc')
    const documented = '/**\n * Returns one.\n *\n * @returns One.\n */\nexport const f = (): number => 1'
    expect(lint(documented, 'src/a.ts')).toEqual([])
  })

  it('fail a raw button and an input without a placeholder in a template', () => {
    const ids = lint('<template><button>x</button><input /></template>', 'src/views/A.vue')
    expect(ids).toContain('vue/no-restricted-html-elements')
    expect(ids).toContain('local/require-placeholder')
  })

  it('accept an input that carries a placeholder', () => {
    const ids = lint('<template><UiInput placeholder="x" /></template>', 'src/views/A.vue')
    expect(ids).not.toContain('local/require-placeholder')
  })

  it('allow raw elements inside the kit', () => {
    const ids = lint('<template><button>x</button></template>', 'src/components/ui/UiThing.vue')
    expect(ids).not.toContain('vue/no-restricted-html-elements')
  })

  it('forbid a view importing the transport and a store importing useApi', () => {
    const view = lint('<script setup lang="ts">import { useApi } from "@/composables/useApi"</script>', 'src/views/A.vue')
    expect(view).toContain('no-restricted-imports')
    expect(lint('import { useApi } from "@/composables/useApi"', 'src/stores/a.ts')).toContain('no-restricted-imports')
  })

  it('do not govern files outside src', () => {
    expect(lint('function f() { return 1 }', 'tests/a.ts')).toEqual([])
  })
})
