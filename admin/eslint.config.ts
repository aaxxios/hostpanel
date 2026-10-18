/**
 * ESLint flat config for the admin app.
 *
 * Holds the base Vue/TypeScript config, the local `require-placeholder` rule and the strict house
 * rules (`strictRules`, exported for the lint tests). Plugins `vue` and `@typescript-eslint` are
 * registered by the base config; this file registers `jsdoc` and `local`.
 */
import { globalIgnores } from 'eslint/config'
import { defineConfigWithVueTs, vueTsConfigs } from '@vue/eslint-config-typescript'
import pluginVue from 'eslint-plugin-vue'
import pluginOxlint from 'eslint-plugin-oxlint'
import jsdoc from 'eslint-plugin-jsdoc'
import type { Rule } from 'eslint'

/** Tags that always need a placeholder. */
const INPUT_TAGS: ReadonlySet<string> = new Set([
  'textarea',
  'select',
  'UiInput',
  'UiTextarea',
  'UiSelect',
  'UiNumberInput',
  'UiNativeSelect',
  'UiTextField',
])

/** Native `input` types that have no placeholder to show. */
const EXEMPT_INPUT_TYPES: ReadonlySet<string> = new Set([
  'checkbox',
  'radio',
  'file',
  'hidden',
  'button',
  'submit',
  'reset',
  'image',
  'range',
  'color',
])

/** Minimal shape of a template attribute node (static attribute or directive). */
interface TemplateAttribute {
  /** True for a directive (`v-bind`, `:x`, `@x`). */
  directive: boolean
  /** Static attribute: `name` is a string; directive: `name` is `{ name }` and `argument` is the bound name. */
  key: { name: string | { name: string }; argument?: { name?: string } | null }
  /** Static attribute value, when present. */
  value?: { value?: string } | null
}

/** Minimal shape of an element node of the template AST. */
interface TemplateElement {
  /** Tag name as written. */
  rawName: string
  /** The start tag with its attributes. */
  startTag: { attributes: TemplateAttribute[] }
}

/**
 * Reads the effective attribute name: `placeholder` for both `placeholder="x"` and `:placeholder="x"`.
 *
 * @param attribute - The template attribute.
 * @returns The attribute name, `'*'` for a bare `v-bind="obj"`, or `''` for other directives.
 */
const attributeName = (attribute: TemplateAttribute): string => {
  if (!attribute.directive) {
    return typeof attribute.key.name === 'string' ? attribute.key.name : ''
  }
  if (typeof attribute.key.name === 'object' && attribute.key.name.name === 'bind') {
    return attribute.key.argument?.name ?? '*'
  }
  return ''
}

/**
 * Finds the value of the static `type` attribute of an element.
 *
 * @param element - The element.
 * @returns The static type value, or `undefined` when absent or dynamic.
 */
const staticType = (element: TemplateElement): string | undefined => {
  const found = element.startTag.attributes.find((a) => !a.directive && a.key.name === 'type')
  return found?.value?.value
}

/** The `local/require-placeholder` rule definition. */
const requirePlaceholder: Rule.RuleModule = {
  meta: {
    type: 'problem',
    docs: { description: 'Every input-like element has a placeholder' },
    schema: [],
    messages: { missing: '<{{tag}}> needs a placeholder (an i18n key).' },
  },
  create(context) {
    /**
     * Reports an element that is input-like and lacks a placeholder.
     *
     * @param node - The template element.
     */
    const check = (node: TemplateElement): void => {
      const tag = node.rawName
      const isNativeInput = tag === 'input' && !EXEMPT_INPUT_TYPES.has(staticType(node) ?? 'text')
      if (!isNativeInput && !INPUT_TAGS.has(tag)) {
        return
      }
      const names = node.startTag.attributes.map(attributeName)
      if (names.includes('placeholder') || names.includes('*')) {
        return
      }
      context.report({ node: node as unknown as Rule.Node, messageId: 'missing', data: { tag } })
    }

    const services = context.sourceCode.parserServices as {
      defineTemplateBodyVisitor?: (visitor: Record<string, unknown>) => Rule.RuleListener
    }
    if (!services.defineTemplateBodyVisitor) {
      return {}
    }
    return services.defineTemplateBodyVisitor({ VElement: check })
  },
}

/** Plugin object exposing the local rules as `local/<name>`. */
const localPlugin = { rules: { 'require-placeholder': requirePlaceholder } }

/** Raw elements a screen must compose from the kit instead. */
const RESTRICTED_ELEMENTS = ['button', 'input', 'select', 'textarea', 'table', 'form', 'dialog', 'a']

/** Every source file the rules govern. */
const SRC = 'src/**/*.{vue,ts}'

/** Generated copy of the storefront's palette spec; its guard test requires it byte for byte. */
const SHARED_COPY = 'src/lib/brandPalette.ts'

/** The strict-rule blocks, in the order they must be applied. */
export const strictRules: Record<string, unknown>[] = [
  {
    name: 'strict/all',
    files: [SRC],
    ignores: [SHARED_COPY],
    plugins: { jsdoc, local: localPlugin },
    rules: {
      curly: ['error', 'all'],
      'no-restricted-syntax': [
        'error',
        { selector: 'FunctionDeclaration', message: 'Use a const arrow function, never `function name()`.' },
      ],
      '@typescript-eslint/no-explicit-any': 'error',
      '@typescript-eslint/consistent-type-imports': 'error',
      'jsdoc/require-jsdoc': [
        'error',
        {
          require: { FunctionDeclaration: true, ArrowFunctionExpression: true, FunctionExpression: false },
          contexts: ['TSInterfaceDeclaration', 'TSTypeAliasDeclaration', 'TSEnumDeclaration'],
        },
      ],
      'jsdoc/require-description': 'error',
      'jsdoc/require-param': 'error',
      'jsdoc/require-returns': 'error',
      // Types come from TypeScript; JSDoc carries prose only.
      'jsdoc/require-param-type': 'off',
      'jsdoc/require-returns-type': 'off',
    },
  },
  {
    name: 'strict/templates',
    files: [[SRC, '**/*.vue']],
    plugins: { local: localPlugin },
    rules: {
      'vue/no-restricted-html-elements': ['error', ...RESTRICTED_ELEMENTS],
      'vue/no-static-inline-styles': 'error',
      'vue/no-v-html': 'error',
      'local/require-placeholder': 'error',
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              group: ['**/composables/useApi', '**/composables/apis/*'],
              message: 'A view calls its store, never the transport.',
            },
            { group: ['**/lib/errorHandler'], message: 'Errors are read from the store.' },
          ],
        },
      ],
    },
  },
  {
    name: 'strict/stores',
    files: [[SRC, '**/stores/**']],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            { group: ['**/composables/useApi'], message: 'A store calls its composables/apis/use<Area>Api.' },
          ],
        },
      ],
    },
  },
  {
    name: 'strict/transport',
    files: [[SRC, '**/composables/useApi.ts']],
    rules: {
      'no-restricted-imports': [
        'error',
        { patterns: [{ group: ['**/stores/*'], message: 'The transport never imports a store.' }] },
      ],
    },
  },
  {
    // The kit is where raw elements live.
    name: 'strict/kit',
    files: [[SRC, '**/components/ui/**']],
    rules: { 'vue/no-restricted-html-elements': 'off' },
  },
]

// To allow more languages other than `ts` in `.vue` files, uncomment the following lines:
// import { configureVueProject } from '@vue/eslint-config-typescript'
// configureVueProject({ scriptLangs: ['ts', 'tsx'] })
// More info at https://github.com/vuejs/eslint-config-typescript/#advanced-setup

export default defineConfigWithVueTs(
  {
    name: 'app/files-to-lint',
    files: ['**/*.{vue,ts,mts,tsx}'],
  },

  globalIgnores(['**/dist/**', '**/dist-ssr/**', '**/coverage/**']),

  ...pluginVue.configs['flat/essential'],
  vueTsConfigs.recommended,

  // The strict house rules (JSDoc, braces, const arrows, kit-only templates, placeholders,
  // layering, type imports): all `error`, for every file under `src`.
  ...strictRules,

  ...pluginOxlint.buildFromOxlintConfigFile('.oxlintrc.json'),
)
