/**
 * Runs before every test file.
 *
 * Every component a test mounts is unmounted when the test ends, so its timers and in-flight
 * requests cannot outlive the test and fail after the environment is torn down.
 */
import { afterEach } from 'vitest'
import { enableAutoUnmount } from '@vue/test-utils'

enableAutoUnmount(afterEach)
