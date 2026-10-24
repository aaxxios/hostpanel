/**
 * Tests for the storefront setting shape checks: an empty value always passes, a malformed one
 * never does.
 */
import { describe, expect, it } from 'vitest'
import { isValidSettingValue, SettingFieldRule } from '@/lib/settingValidation'

describe('isValidSettingValue', () => {
  it.each([
    ['support@example.com', true],
    ['  a.b+c@mail.example.org  ', true],
    ['', true],
    ['   ', true],
    ['support', false],
    ['support@example', false],
    ['two words@example.com', false],
    ['a@b@example.com', false],
  ])('checks the email %j', (value, valid) => {
    expect(isValidSettingValue(SettingFieldRule.Email, value)).toBe(valid)
  })

  it.each([
    ['https://maps.example.com/embed?x=1', true],
    ['https://maps.example.com', true],
    ['', true],
    ['http://maps.example.com/embed', false],
    ['javascript:alert(1)', false],
    ['maps.example.com', false],
  ])('checks the https address %j', (value, valid) => {
    expect(isValidSettingValue(SettingFieldRule.HttpsUrl, value)).toBe(valid)
  })
})
