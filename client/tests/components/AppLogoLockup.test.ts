/**
 * Unit tests for AppLogoLockup: the cube mark beside the site name when nothing is uploaded,
 * and the operator's wordmark winning once one is.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'
import AppLogoLockup from '~/components/AppLogoLockup.vue'
import { STATIC_LOGO_MARK } from '~/utils/appLogo'

describe('AppLogoLockup', () => {
  it('draws the bundled cube mark beside the operator site name when nothing is uploaded', () => {
    stubTemplateGlobals({ siteName: 'Hostpanel Demo' })
    const wrapper = mountWithKit(AppLogoLockup)

    expect(wrapper.get('[data-testid="app-logo-mark"]').attributes('src')).toContain(STATIC_LOGO_MARK)
    expect(wrapper.get('[data-testid="app-logo-name"]').text()).toBe('Hostpanel Demo')
    expect(wrapper.find('[data-testid="app-logo-light"]').exists()).toBe(false)
  })

  it('hides the mark from assistive technology, since the name beside it says the same', () => {
    stubTemplateGlobals()
    const mark = mountWithKit(AppLogoLockup).get('[data-testid="app-logo-mark"]')

    expect(mark.attributes('alt')).toBe('')
  })

  it('renders the uploaded wordmark instead of the mark and the name', () => {
    stubTemplateGlobals({ logo: '/api/uploads/logo.png' })
    const wrapper = mountWithKit(AppLogoLockup, { props: { imgClass: 'h-8' } })

    expect(wrapper.get('[data-testid="app-logo-light"]').attributes('src')).toBe('/api/uploads/logo.png')
    expect(wrapper.get('[data-testid="app-logo-light"]').classes()).toContain('h-8')
    expect(wrapper.find('[data-testid="app-logo-mark"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="app-logo-name"]').exists()).toBe(false)
  })
})
