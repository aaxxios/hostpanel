/**
 * `npm run lint:i18n` — fails the build when the catalogues drift.
 *
 * Loads `src/locales/{en,ru,hy}/<area>.json` (every key prefixed with the file name, the area),
 * then reports keys missing in a language, extra keys, unused keys and `t('literal')` calls whose key
 * is absent from `en`. The comparison itself lives in `scripts/lib/i18nCheck.ts`.
 */
import { existsSync, readdirSync, readFileSync, statSync } from 'node:fs'
import { basename, join } from 'node:path'
import { fileURLToPath } from 'node:url'
import { checkI18n, flattenKeys, LANGUAGES, type Catalogues, type Language } from './lib/i18nCheck'

/** Admin root: the folder above `scripts/`. */
const root = fileURLToPath(new URL('..', import.meta.url))

/** The locale folder. */
const localesDir = join(root, 'src', 'locales')

/**
 * Reads the flat keys of one language from its per-area folder.
 *
 * @param language - The language to load.
 * @returns The language's dotted keys.
 */
const loadLanguage = (language: Language): Set<string> => {
  const keys = new Set<string>()
  const folder = join(localesDir, language)
  if (existsSync(folder)) {
    for (const file of readdirSync(folder).filter((f) => f.endsWith('.json'))) {
      const area = basename(file, '.json')
      flattenKeys(JSON.parse(readFileSync(join(folder, file), 'utf8')), area).forEach((k) => keys.add(k))
    }
  }
  return keys
}

/**
 * Concatenates every `.vue` and `.ts` file under a directory.
 *
 * @param dir - Directory to walk.
 * @returns The joined source text.
 */
const readSource = (dir: string): string =>
  readdirSync(dir)
    .map((name) => {
      const path = join(dir, name)
      if (statSync(path).isDirectory()) {
        return readSource(path)
      }
      return /\.(vue|ts)$/.test(name) ? readFileSync(path, 'utf8') : ''
    })
    .join('\n')

const catalogues = Object.fromEntries(LANGUAGES.map((l) => [l, loadLanguage(l)])) as Catalogues
const problems = checkI18n({ catalogues, source: readSource(join(root, 'src')) })

if (problems.length > 0) {
  console.error(problems.join('\n'))
  console.error(`\nlint:i18n failed: ${problems.length} problem(s).`)
  process.exit(1)
}
console.log('lint:i18n passed.')
