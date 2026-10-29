/**
 * Tests that every SSH key agent failure reason has a label in every shipped language, and that
 * the labels carry no key the enum does not have, so a failure can never render as a raw key.
 */
import { describe, expect, it } from 'vitest'
import { MessageLanguage, messages } from '@/locales/messages'
import type { MessageTree } from '@/locales/messages'
import { SshProviderFailure } from '@/types/sshenums'

/**
 * The failure labels of one language, read from `servers.ssh.failures`.
 *
 * @param language - The language whose labels are read.
 * @returns The label tree keyed by failure name.
 */
const failureLabelsOf = (language: MessageLanguage): MessageTree => {
  const ssh = messages[language].servers?.ssh as MessageTree
  return ssh.failures as MessageTree
}

describe('SshProviderFailure labels', () => {
  for (const language of Object.values(MessageLanguage)) {
    it(`has exactly one label per failure in ${language}`, () => {
      const labels = failureLabelsOf(language)

      expect(Object.keys(labels).sort()).toEqual(Object.values(SshProviderFailure).sort())
      for (const failure of Object.values(SshProviderFailure)) {
        expect(labels[failure]).toEqual(expect.any(String))
        expect(labels[failure]).not.toBe('')
      }
    })
  }
})
