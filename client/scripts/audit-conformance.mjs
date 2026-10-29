/**
 * `yarn audit:conformance [paths] [--files] [--json] [--strict]`: measures how far the client is
 * from the frontend rules.
 *
 * Every metric is a regex heuristic over the source, kept deliberately close to what the guard hook
 * and the ESLint rules check, so a path that reports 0 here is a path the rules will accept:
 *
 *  - hookHits       lines of a `pages/` file the hook flags (`<div|span|button|input|table|ul|ol|p|h1-6|a`);
 *  - rawInteractive raw `button input select textarea table form dialog a img label` outside `components/ui/`;
 *  - functionDecl   `function name()` declarations (const arrows only);
 *  - braceless      `if/else/for/while` controlling a bare statement;
 *  - noJsdoc        top-level `const` arrows / `ref` / `computed` / `defineStore` with no `* /` comment above;
 *  - noFileHeader   `.vue`/`.ts` files whose first comment is not a JSDoc block;
 *  - noPlaceholder  input-like elements (checkbox, radio, file, hidden exempt) without a placeholder;
 *  - noTestid       interactive elements without `data-testid`;
 *  - textNodes      bare visible text between tags; textAttrs  hard-coded `label|title|alt|aria-label`;
 *  - style          `style=` / `:style=` / `<style>` / arbitrary hex classes / `v-html`;
 *  - vueCatch / vueApi  `catch` blocks and transport/api-composable calls inside a `.vue`;
 *  - anyType        `: any` / `as any`;
 *  - literalSentence  `statusMessage:` with a literal sentence in `server/`.
 *
 * The i18n figures (missing / extra / unused / undefined keys) come from `scripts/lint-i18n.mjs`.
 *
 * Arguments are path prefixes relative to `client/` (default: the whole tree). `--files` prints the
 * per-file figures, `--json` prints machine-readable output, `--strict` exits 1 when any figure is non-zero.
 *
 * @module scripts/audit-conformance
 */

import { spawnSync } from 'node:child_process'
import { readdirSync, readFileSync, statSync } from 'node:fs'
import { dirname, join, relative } from 'node:path'
import { fileURLToPath } from 'node:url'

/** The client root. */
const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..')

/** Source roots that are audited; generated and test trees are not. */
const SOURCE_ROOTS = ['app.vue', 'error.vue', 'components', 'pages', 'layouts', 'templates', 'composables', 'stores', 'plugins', 'middleware', 'utils', 'lib', 'server', 'types', 'constants', 'i18n.config.ts', 'tailwind.config.ts']

/** Raw elements that are interactive or typographic and belong to the kit, not to screens. */
const RAW_INTERACTIVE = /<(button|input|select|textarea|table|form|dialog|a|img|label)(?=[\s>/])/g

/** Metric names, in the order they are printed. */
const METRICS = ['hookHits', 'rawInteractive', 'functionDecl', 'braceless', 'noJsdoc', 'noFileHeader', 'noPlaceholder', 'noTestid', 'textNodes', 'textAttrs', 'style', 'vueCatch', 'vueApi', 'anyType', 'literalSentence']

/**
 * Lists the audited files under a path.
 *
 * @param {string} path File or directory (absolute).
 * @returns {string[]} Absolute paths of `.vue`/`.ts` files.
 */
const walk = (path) => {
  let stat
  try {
    stat = statSync(path)
  } catch {
    return []
  }
  if (stat.isFile()) {
    return /\.(vue|ts)$/.test(path) && !path.endsWith('.d.ts') ? [path] : []
  }
  return readdirSync(path).flatMap((name) => (name === 'node_modules' || name.startsWith('.') ? [] : walk(join(path, name))))
}

/**
 * Splits a `.vue` file into its template and script text.
 *
 * @param {string} source The whole file.
 * @returns {{ template: string, script: string }} The two regions ('' when absent).
 */
const regions = (source) => {
  const templateStart = source.indexOf('<template')
  const templateEnd = source.lastIndexOf('</template>')
  const template = templateStart >= 0 && templateEnd > templateStart ? source.slice(source.indexOf('>', templateStart) + 1, templateEnd) : ''
  const script = [...source.matchAll(/<script[^>]*>([\s\S]*?)<\/script>/g)].map((m) => m[1]).join('\n')
  return { template, script }
}

/**
 * Counts regex matches.
 *
 * @param {string} text Text to scan.
 * @param {RegExp} pattern A global pattern.
 * @returns {number} Number of matches.
 */
const count = (text, pattern) => [...text.matchAll(pattern)].length

/**
 * Counts control statements that govern a bare (brace-less) statement on one line.
 *
 * @param {string} script Script text.
 * @returns {number} Number of brace-less statements.
 */
const bracelessCount = (script) => {
  let total = 0
  for (const line of script.split('\n')) {
    const trimmed = line.trim()
    if (/^else\s+(?!if\b)[^{\s]/.test(trimmed) || /^\}?\s*else\s+(?!if\b|\{)\S/.test(trimmed)) {
      total++
      continue
    }
    const head = /^(?:\}\s*else\s+)?(if|for|while)\s*\(/.exec(trimmed)
    if (!head) {
      continue
    }
    let depth = 0
    let end = -1
    for (let i = head[0].length - 1; i < trimmed.length; i++) {
      if (trimmed[i] === '(') {
        depth++
      } else if (trimmed[i] === ')') {
        depth--
        if (depth === 0) {
          end = i
          break
        }
      }
    }
    if (end < 0) {
      continue
    }
    const rest = trimmed.slice(end + 1).trim()
    if (rest !== '' && !rest.startsWith('{') && !rest.startsWith('//') && !rest.startsWith('/*')) {
      total++
    }
  }
  return total
}

/**
 * Counts top-level members (const arrows, refs, computeds, stores) with no JSDoc block directly above.
 *
 * @param {string} script Script text.
 * @returns {number} Number of undocumented members.
 */
const noJsdocCount = (script) => {
  const lines = script.split('\n')
  let total = 0
  lines.forEach((line, index) => {
    if (!/^(?:export\s+)?(?:const|let)\s+\w+(?:\s*:\s*[^=]+)?\s*=\s*(?:async\s*)?(?:\(|<|[\w$]+\s*=>|ref\b|shallowRef\b|computed\b|reactive\b|defineStore\b|useState\b|defineProps\b|defineEmits\b|definePageMeta\b)/.test(line)) {
      return
    }
    if (/=\s*(?:defineProps|defineEmits|definePageMeta)\b/.test(line)) {
      return
    }
    let prev = index - 1
    while (prev >= 0 && lines[prev].trim() === '') {
      prev--
    }
    if (prev < 0 || !lines[prev].trim().endsWith('*/')) {
      total++
    }
  })
  return total
}

/**
 * Finds multi-line opening tags of the given element names.
 *
 * @param {string} template Template text.
 * @param {string} names Alternation of tag names.
 * @returns {string[]} The raw opening tags.
 */
const openingTags = (template, names) => [...template.matchAll(new RegExp(`<(?:${names})(?=[\\s>/])[^>]*?(?:"[^"]*"|'[^']*'|[^>"'])*?>`, 'g'))].map((m) => m[0])

/**
 * Measures one file.
 *
 * @param {string} file Absolute path.
 * @param {string} rel Path relative to the client root, forward slashes.
 * @returns {Record<string, number>} Figures per metric.
 */
const measure = (file, rel) => {
  const source = readFileSync(file, 'utf8')
  const isVue = rel.endsWith('.vue')
  const { template, script } = isVue ? regions(source) : { template: '', script: source }
  const inKit = rel.startsWith('components/ui/')
  const figures = Object.fromEntries(METRICS.map((metric) => [metric, 0]))

  if (rel.includes('/pages/') || rel.startsWith('pages/')) {
    figures.hookHits = count(source, /^\s*<(div|span|button|input|table|ul|ol|p|h[1-6]|a)(?=[\s>/])/gm)
  }
  if (isVue && !inKit) {
    figures.rawInteractive = count(template, RAW_INTERACTIVE)
  }
  figures.functionDecl = count(script, /^\s*(?:export\s+(?:default\s+)?)?(?:async\s+)?function\s*\*?\s*\w+/gm)
  figures.braceless = bracelessCount(script)
  figures.noJsdoc = noJsdocCount(script)
  figures.noFileHeader = /^\s*(?:<script[^>]*>\s*)?\/\*\*/.test(source) || (isVue && /<script[^>]*>\s*\/\*\*/.test(source)) ? 0 : 1
  figures.anyType = count(script, /(?::\s*any\b|\bas\s+any\b|<any>)/g)

  if (isVue) {
    const inputs = openingTags(template, 'input|textarea|select|UiInput|UiTextarea|UiSelect')
    figures.noPlaceholder = inputs.filter((tag) => !/\bplaceholder\b|:placeholder\b|v-bind=/.test(tag) && !/type=["'](?:checkbox|radio|file|hidden)["']/.test(tag)).length
    const interactive = openingTags(template, 'button|input|select|textarea|a|form|UiButton|UiInput|UiSelect|UiTextarea|UiLink|UiIconButton|UiCheckbox|UiForm')
    figures.noTestid = interactive.filter((tag) => !/data-testid|:testid|\btestid=/.test(tag)).length
    figures.textNodes = count(template.replace(/<!--[\s\S]*?-->/g, ''), />\s*[A-Za-z][^<>{}]*[A-Za-z.!?]\s*</g)
    figures.textAttrs = count(template, /\s(?:label|title|alt|aria-label)="[^"]*[A-Za-z][^"]*"/g)
    figures.style = count(source, /(?:^|\s):?style=|<style\b|\[#[0-9a-fA-F]{3,8}\]|v-html=/gm)
    figures.vueCatch = count(script, /\bcatch\b\s*(?:\(|\{)/g)
    figures.vueApi = count(script, /\b(?:useApi|apiFetch|useFetch|\$fetch)\s*\(|\buse[A-Z]\w*Api\s*\(/g)
  } else {
    figures.style = 0
  }
  if (rel.startsWith('server/')) {
    figures.literalSentence = count(script, /statusMessage:\s*['"`][^'"`]*[a-zA-Z ]{4,}/g)
  }
  return figures
}

/**
 * Groups a file under the area the plan's tasks use.
 *
 * @param {string} rel Relative path.
 * @returns {string} The group label.
 */
const areaOf = (rel) => {
  const parts = rel.split('/')
  if (parts.length === 1) {
    return 'root'
  }
  if (parts[0] === 'templates') {
    return `templates/${parts[1]}/${parts[2] ?? ''}`.replace(/\/$/, '')
  }
  if (parts[0] === 'components' || parts[0] === 'server') {
    return `${parts[0]}/${parts[1]}`
  }
  return parts[0]
}

/**
 * Runs the i18n gate and tallies its report by problem kind.
 *
 * @returns {Record<string, number>} Counts for missing, extra, unused, undefined.
 */
const i18nFigures = () => {
  const run = spawnSync(process.execPath, [join(ROOT, 'scripts', 'lint-i18n.mjs')], { encoding: 'utf8' })
  const lines = `${run.stdout}\n${run.stderr}`.split('\n')
  return Object.fromEntries(['missing', 'extra', 'unused', 'undefined'].map((kind) => [kind, lines.filter((line) => line.startsWith(`${kind} `)).length]))
}

const args = process.argv.slice(2)
const flags = new Set(args.filter((arg) => arg.startsWith('--')))
const prefixes = args.filter((arg) => !arg.startsWith('--')).map((arg) => arg.replaceAll('\\', '/').replace(/^\.\//, '').replace(/\/$/, ''))

const files = SOURCE_ROOTS.flatMap((entry) => walk(join(ROOT, entry)))
  .map((file) => ({ file, rel: relative(ROOT, file).replaceAll('\\', '/') }))
  .filter(({ rel }) => prefixes.length === 0 || prefixes.some((prefix) => rel === prefix || rel.startsWith(`${prefix}/`)))

/** Figures per file. */
const perFile = new Map(files.map(({ file, rel }) => [rel, measure(file, rel)]))

/** Figures per area, with file count. */
const perArea = new Map()
for (const [rel, figures] of perFile) {
  const area = areaOf(rel)
  const entry = perArea.get(area) ?? { files: 0, ...Object.fromEntries(METRICS.map((metric) => [metric, 0])) }
  entry.files++
  for (const metric of METRICS) {
    entry[metric] += figures[metric]
  }
  perArea.set(area, entry)
}

/** Totals over all audited files. */
const totals = Object.fromEntries(METRICS.map((metric) => [metric, [...perFile.values()].reduce((sum, figures) => sum + figures[metric], 0)]))
const i18n = prefixes.length === 0 ? i18nFigures() : null

if (flags.has('--json')) {
  console.log(JSON.stringify({ files: perFile.size, totals, areas: Object.fromEntries(perArea), i18n }, null, 2))
} else {
  console.log(`conformance audit: ${perFile.size} file(s)${prefixes.length > 0 ? ` under ${prefixes.join(', ')}` : ''}\n`)
  if (flags.has('--files')) {
    for (const [rel, figures] of perFile) {
      const nonZero = METRICS.filter((metric) => figures[metric] > 0).map((metric) => `${metric}=${figures[metric]}`)
      if (nonZero.length > 0) {
        console.log(`${rel}  ${nonZero.join(' ')}`)
      }
    }
    console.log('')
  }
  console.log(['area'.padEnd(34), 'files', ...METRICS].join(' '))
  for (const [area, entry] of [...perArea].sort(([a], [b]) => a.localeCompare(b))) {
    console.log([area.padEnd(34), String(entry.files).padStart(5), ...METRICS.map((metric) => String(entry[metric]).padStart(metric.length))].join(' '))
  }
  console.log(['TOTAL'.padEnd(34), String(perFile.size).padStart(5), ...METRICS.map((metric) => String(totals[metric]).padStart(metric.length))].join(' '))
  if (i18n) {
    console.log(`\ni18n: missing ${i18n.missing}, extra ${i18n.extra}, unused ${i18n.unused}, undefined ${i18n.undefined}`)
  }
}

if (flags.has('--strict') && METRICS.some((metric) => totals[metric] > 0)) {
  process.exit(1)
}
