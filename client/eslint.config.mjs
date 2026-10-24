// @ts-check
import jsdoc from 'eslint-plugin-jsdoc'
import withNuxt from './.nuxt/eslint.config.mjs'
import hostpanel from './eslint/hostpanel.mjs'

/**
 * Imports no `.vue` file may reach: transport, the API composables and the error helper belong
 * to stores. (Nuxt auto-imports hide `useApi()` from import rules, so `scripts/check-layers.mjs`
 * greps for the calls too.)
 */
const VUE_FORBIDDEN_IMPORTS = [
  { group: ['**/composables/useApi', '**/composables/apis/*', '**/lib/errorHandler'], message: 'A .vue file reads a store; only a store reaches transport, an api composable or the error helper.' },
]

/** Imports a store may not use: transport is the api composables' business. */
const STORE_FORBIDDEN_IMPORTS = [
  { group: ['**/composables/useApi'], message: 'A store calls its api composable, never transport directly.' },
]

/** Imports the transport module may not use: it must stay store-free (no import cycle). */
const TRANSPORT_FORBIDDEN_IMPORTS = [
  { group: ['**/stores/*'], message: 'useApi.ts imports no store; emit into a listener registry instead.' },
]

/**
 * The conformance rules (spec 3.6), errors on every source file of the client.
 *
 * @type {import('eslint').Linter.RulesRecord}
 */
const conformanceRules = {
  'jsdoc/require-jsdoc': ['error', {
    publicOnly: false,
    require: { FunctionDeclaration: true, ArrowFunctionExpression: false, FunctionExpression: false, MethodDefinition: false, ClassDeclaration: false },
    // Selectors, not node names: a documented `export const` keeps its comment on the export
    // statement, so the exported form is matched there; destructured handles
    // (`const { t } = useI18n()`) and the members of inline object types need none.
    contexts: [
      'TSInterfaceDeclaration',
      'TSTypeAliasDeclaration',
      'TSEnumDeclaration',
      'Program > VariableDeclaration:has(> VariableDeclarator[id.type="Identifier"])',
      'ExportNamedDeclaration:has(> VariableDeclaration)',
      'VariableDeclaration:has(> VariableDeclarator > ArrowFunctionExpression)',
      'TSInterfaceBody > TSPropertySignature',
      'TSTypeAliasDeclaration > TSTypeLiteral > TSPropertySignature',
    ],
  }],
  'jsdoc/require-description': ['error', { contexts: ['any'] }],
  curly: ['error', 'all'],
  'func-style': ['error', 'expression'],
  'no-restricted-syntax': ['error', { selector: 'FunctionDeclaration', message: 'Functions are const arrows, never `function name()`.' }],
  'vue/no-restricted-html-elements': ['error', 'button', 'input', 'select', 'textarea', 'table', 'form', 'dialog', 'a', 'label', 'p', 'ul', 'ol', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6'],
  'vue/no-static-inline-styles': 'error',
  'vue/no-v-html': 'error',
  'vue/require-default-prop': 'error',
  // A non-null assertion needs a comment saying why it is safe; the rule can only forbid the bare form.
  '@typescript-eslint/no-non-null-assertion': 'error',
  '@typescript-eslint/no-explicit-any': 'error',
  '@typescript-eslint/consistent-type-imports': 'error',
  'hostpanel/placeholder': 'error',
  'hostpanel/no-card-div': 'error',
}

/**
 * Flat-config blocks that switch the conformance rules on for the whole client.
 *
 * @type {import('eslint').Linter.Config[]}
 */
const conformance = [
  {
    name: 'hostpanel/conformance',
    files: ['**/*.{ts,vue}'],
    plugins: { jsdoc, hostpanel },
    rules: conformanceRules,
  },
  // The kit is where raw elements legitimately live.
  {
    name: 'hostpanel/conformance-kit',
    files: ['components/ui/**/*.vue'],
    rules: { 'vue/no-restricted-html-elements': 'off', 'hostpanel/no-card-div': 'off' },
  },
  // A test indexes a fixture it has just built (`rows[0]!`); a missing element fails the very
  // next expectation, so the commented-`!` convention of production code adds nothing here.
  {
    name: 'hostpanel/conformance-tests',
    files: ['tests/**/*.ts', 'e2e/**/*.ts'],
    rules: { '@typescript-eslint/no-non-null-assertion': 'off' },
  },
  {
    name: 'hostpanel/layering-vue',
    files: ['**/*.vue'],
    rules: { 'no-restricted-imports': ['error', { patterns: VUE_FORBIDDEN_IMPORTS }] },
  },
  {
    name: 'hostpanel/layering-stores',
    files: ['stores/**/*.ts'],
    rules: { 'no-restricted-imports': ['error', { patterns: STORE_FORBIDDEN_IMPORTS }] },
  },
  {
    name: 'hostpanel/layering-transport',
    files: ['composables/useApi.ts'],
    rules: { 'no-restricted-imports': ['error', { patterns: TRANSPORT_FORBIDDEN_IMPORTS }] },
  },
]

export default withNuxt(
  {
    // Nuxt route files are named by the URL they serve (`index.vue`, `[id].vue`, `default.vue`),
    // so the single-word-name rule cannot apply to them.
    files: ['pages/**/*.vue', 'layouts/**/*.vue', 'error.vue'],
    rules: {
      'vue/multi-word-component-names': 'off'
    }
  },
  {
    // `Logo` is the brand mark itself and is named for what it is; `LogoLockup` is its sibling.
    files: ['components/Logo.vue'],
    rules: {
      'vue/multi-word-component-names': ['error', { ignores: ['Logo'] }]
    }
  },
  ...conformance
)
