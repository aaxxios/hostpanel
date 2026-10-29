/**
 * Local ESLint plugin for the hostpanel client: the two template rules no published plugin
 * covers (`placeholder`, `no-card-div`).
 *
 * Both visit the template through vue-eslint-parser's `defineTemplateBodyVisitor`, so they only
 * ever run on `.vue` files. They are switched on for every file by the conformance block of `eslint.config.mjs`.
 *
 * @module eslint/hostpanel
 */

/**
 * The part of a rule's context these rules read: the parser services, which carry
 * `defineTemplateBodyVisitor` only under vue-eslint-parser. Declared structurally because the
 * plugin type that the flat-config helper expects and the one `eslint` exports disagree on the
 * context's full shape.
 *
 * @typedef {{ sourceCode: object }} TemplateRuleContext
 *
 * @typedef {Record<string, ((...args: any[]) => void) | undefined>} RuleVisitor
 *
 * @typedef {{ defineTemplateBodyVisitor?: (visitor: object) => RuleVisitor }} TemplateServices
 */

/** Components and tags that take typed text and therefore must show a placeholder. */
const FIELD_TAGS = new Set(['input', 'textarea', 'select', 'UiInput', 'UiTextarea', 'UiSelect', 'ui-input', 'ui-textarea', 'ui-select'])

/** `<input type>` values that have no text area, so a placeholder is meaningless on them. */
const NON_TEXT_INPUT_TYPES = new Set(['hidden', 'checkbox', 'radio', 'file', 'submit', 'button', 'range', 'color'])

/**
 * Finds an attribute on a template element, plain or bound.
 *
 * @param {import('vue-eslint-parser').AST.VElement} element The element to search.
 * @param {string} name Attribute name, e.g. `placeholder`.
 * @returns {import('vue-eslint-parser').AST.VAttribute | import('vue-eslint-parser').AST.VDirective | undefined} The attribute when present.
 */
const findAttribute = (element, name) =>
  element.startTag.attributes.find((attr) => {
    if (!attr.directive) {
      return attr.key.name === name
    }
    return attr.key.name.name === 'bind' && attr.key.argument?.name === name
  })

/**
 * Rule: every text field carries a `placeholder`.
 *
 * Covers raw `input`/`textarea`/`select` (outside the kit) and `UiInput`/`UiTextarea`/`UiSelect`;
 * a hidden/checkbox/radio/file `input` is exempt because it shows no text.
 */
const placeholder = {
  meta: {
    type: /** @type {const} */ ('problem'),
    docs: { description: 'Require a placeholder on every text field.' },
    schema: [],
    messages: { missing: '<{{tag}}> needs a placeholder (an i18n key).' },
  },
  /**
   * Builds the visitor.
   *
   * @param {TemplateRuleContext} context Rule context.
   * @returns {RuleVisitor} Template-body visitor.
   */
  create(context) {
    const services = /** @type {TemplateServices | undefined} */ (/** @type {{ parserServices?: unknown }} */ (context.sourceCode).parserServices)
    // A plain `.ts` file has no template; the vue parser services are only there for `.vue`.
    if (typeof services?.defineTemplateBodyVisitor !== 'function') {
      return {}
    }
    return services.defineTemplateBodyVisitor({
      /**
       * Checks one element.
       *
       * @param {import('vue-eslint-parser').AST.VElement} node The element.
       */
      VElement(node) {
        if (!FIELD_TAGS.has(node.rawName)) {
          return
        }
        if (node.rawName === 'input') {
          const type = findAttribute(node, 'type')
          if (type && !type.directive && NON_TEXT_INPUT_TYPES.has(type.value?.value ?? '')) {
            return
          }
        }
        if (!findAttribute(node, 'placeholder')) {
          context.report({ node: node.startTag, messageId: 'missing', data: { tag: node.rawName } })
        }
      },
    })
  },
}

/**
 * Whether a class string draws a card: a radius plus a border or a shadow.
 *
 * @param {string} classText Static class value plus the source of any bound class expression.
 * @returns {boolean} True when both a radius and a border/shadow utility are present.
 */
const drawsCard = (classText) => {
  // Quotes, brackets and colons of a :class expression are separators, not part of a token.
  const tokens = classText.replace(/['"`,[\]{}:?]/g, ' ').split(/\s+/)
  const hasRadius = tokens.some((token) => token === 'rounded' || token.startsWith('rounded-'))
  const hasFrame = tokens.some((token) => token === 'border' || token.startsWith('border-') || token === 'shadow' || token.startsWith('shadow-'))
  return hasRadius && hasFrame
}

/**
 * Rule: no card drawn in place from a `div`.
 *
 * A `div` whose classes combine a radius with a border or shadow is a card; it belongs in
 * `UiCard` (or another kit component). Checks the static `class` and the literal text of `:class`.
 */
const noCardDiv = {
  meta: {
    type: /** @type {const} */ ('problem'),
    docs: { description: 'Forbid a card-styled div; use UiCard.' },
    schema: [],
    messages: { card: 'A card-styled <div> is drawn in place; compose UiCard instead.' },
  },
  /**
   * Builds the visitor.
   *
   * @param {TemplateRuleContext} context Rule context.
   * @returns {RuleVisitor} Template-body visitor.
   */
  create(context) {
    const services = /** @type {TemplateServices | undefined} */ (/** @type {{ parserServices?: unknown }} */ (context.sourceCode).parserServices)
    // A plain `.ts` file has no template; the vue parser services are only there for `.vue`.
    if (typeof services?.defineTemplateBodyVisitor !== 'function') {
      return {}
    }
    return services.defineTemplateBodyVisitor({
      /**
       * Checks one element.
       *
       * @param {import('vue-eslint-parser').AST.VElement} node The element.
       */
      VElement(node) {
        if (node.rawName !== 'div') {
          return
        }
        // A div may carry both `class` and `:class`; both count towards the card.
        const classText = node.startTag.attributes
          .filter((attr) => (attr.directive ? attr.key.name.name === 'bind' && attr.key.argument?.name === 'class' : attr.key.name === 'class'))
          .map((attr) => (attr.directive ? context.sourceCode.getText(attr.value ?? node) : attr.value?.value ?? ''))
          .join(' ')
        if (drawsCard(classText)) {
          context.report({ node: node.startTag, messageId: 'card' })
        }
      },
    })
  },
}

/** The plugin object registered as `hostpanel` in `eslint.config.mjs`. */
export default {
  meta: { name: 'hostpanel' },
  rules: { placeholder, 'no-card-div': noCardDiv },
}
