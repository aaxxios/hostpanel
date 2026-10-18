/**
 * Shape gate: every component, layout, view, store, API composable, composable, lib module and
 * route guard has a test that imports it. A module added without one fails here, so "a component
 * without a unit test is unfinished" is enforced instead of remembered.
 */
import { readdirSync, readFileSync } from 'node:fs'
import { join, relative, sep } from 'node:path'
import { describe, expect, it } from 'vitest'

/** The admin root: the folder above `tests/`. */
const ROOT = join(__dirname, '..')

/** Source folders whose modules need a test, with the extension each one uses. */
const GATED: ReadonlyArray<{ folder: string; extension: string }> = [
  { folder: 'components', extension: '.vue' },
  { folder: 'layouts', extension: '.vue' },
  { folder: 'views', extension: '.vue' },
  { folder: 'stores', extension: '.ts' },
  { folder: 'composables', extension: '.ts' },
  { folder: 'lib', extension: '.ts' },
  { folder: 'middleware', extension: '.ts' },
]

/**
 * Lists every file under a folder, recursively.
 *
 * @param folder - Absolute folder path.
 * @returns Absolute file paths.
 */
const filesUnder = (folder: string): string[] =>
  readdirSync(folder, { withFileTypes: true }).flatMap((entry) =>
    entry.isDirectory() ? filesUnder(join(folder, entry.name)) : [join(folder, entry.name)],
  )

/**
 * Turns an absolute path into the module path under `src` with forward slashes.
 *
 * @param file - Absolute file path.
 * @returns For example `stores/auth.ts`.
 */
const modulePath = (file: string): string => relative(join(ROOT, 'src'), file).split(sep).join('/')

/** Text of every test file, joined: a module is covered when some test names its path. */
const testSource = filesUnder(join(ROOT, 'tests'))
  .filter((file) => file.endsWith('.test.ts'))
  .map((file) => readFileSync(file, 'utf8'))
  .join('\n')

/**
 * Whether some test imports the module (with or without its extension).
 *
 * @param module - Module path under `src`, with its extension.
 * @returns True when a test names it.
 */
const isTested = (module: string): boolean => {
  const bare = module.replace(/\.(vue|ts)$/, '')
  return [module, `${bare}'`, `${bare}"`, `${bare}\``].some((needle) => testSource.includes(needle))
}

describe('test coverage shape', () => {
  it.each(GATED)('every module under src/$folder is imported by a test', ({ folder, extension }) => {
    const modules = filesUnder(join(ROOT, 'src', folder))
      .filter((file) => file.endsWith(extension))
      .map(modulePath)

    expect(modules.filter((module) => !isTested(module))).toEqual([])
  })
})
