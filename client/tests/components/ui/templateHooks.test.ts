/**
 * Guards the template hook classes of the shared kit: each component carries the `ui-*` class
 * that classic.css restyles under `.tpl-classic`, and every hook the components carry has a rule
 * in the stylesheet, so one component renders differently per template without a fork.
 */
import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { fieldStateClasses } from '~/utils/ui/tones'
import UiAccentButton from '~/components/ui/UiAccentButton.vue'
import UiAccentPanel from '~/components/ui/UiAccentPanel.vue'
import UiAccentTile from '~/components/ui/UiAccentTile.vue'
import UiBackdrop from '~/components/ui/UiBackdrop.vue'
import UiBadge from '~/components/ui/UiBadge.vue'
import UiCard from '~/components/ui/UiCard.vue'
import UiDecor from '~/components/ui/UiDecor.vue'
import UiGlassPanel from '~/components/ui/UiGlassPanel.vue'
import UiGradientText from '~/components/ui/UiGradientText.vue'
import UiLink from '~/components/ui/UiLink.vue'
import UiPageHero from '~/components/ui/UiPageHero.vue'
import UiSection from '~/components/ui/UiSection.vue'
import UiTile from '~/components/ui/UiTile.vue'
import UiToggle from '~/components/ui/UiToggle.vue'
import UiCheckbox from '~/components/ui/UiCheckbox.vue'
import UiRadio from '~/components/ui/UiRadio.vue'
import UiAccordion from '~/components/ui/UiAccordion.vue'
import UiTabs from '~/components/ui/UiTabs.vue'
import { UiBackdropVariant, UiSectionTone } from '~/utils/ui/enums'

/** The classic stylesheet source. */
const css = readFileSync(resolve(__dirname, '../../../assets/css/templates/classic.css'), 'utf8')

/**
 * Whether classic.css has a rule for a hook class.
 *
 * @param hook - The hook class without its dot.
 * @returns True when the stylesheet mentions it.
 */
const styled = (hook: string): boolean => css.includes(`.${hook}`)

describe('kit template hooks', () => {
  it('puts a hook on every restyled component', () => {
    const cases: Array<[string, string[]]> = [
      ['card', mountWithKit(UiCard).classes()],
      ['badge', mountWithKit(UiBadge, { props: { variant: 'primary' } }).classes()],
      ['glass', mountWithKit(UiGlassPanel).classes()],
      ['gradient text', mountWithKit(UiGradientText).classes()],
      ['decor', mountWithKit(UiDecor).classes()],
      ['link', mountWithKit(UiLink, { props: { href: '/x' } }).classes()],
      ['tile', mountWithKit(UiTile).classes()],
      ['accent button', mountWithKit(UiAccentButton).classes()],
      ['accent panel', mountWithKit(UiAccentPanel).classes()],
      ['accent tile', mountWithKit(UiAccentTile, { props: { icon: 'mdi:cube' } }).classes()],
      ['section', mountWithKit(UiSection, { props: { tone: UiSectionTone.Muted } }).classes()],
      ['toggle', mountWithKit(UiToggle, { props: { modelValue: true, label: 'Auto-renew' } }).classes()],
    ]

    for (const [, classes] of cases) {
      const hooks = classes.filter(name => name.startsWith('ui-'))

      expect(hooks.length).toBeGreaterThan(0)
      for (const hook of hooks) {
        expect(styled(hook)).toBe(true)
      }
    }
  })

  it('hooks the checkbox, radio, tabs and accordion pieces', () => {
    expect(mountWithKit(UiCheckbox, { props: { modelValue: true } }).find('button').classes()).toEqual(expect.arrayContaining(['ui-check', 'ui-check-on']))
    expect(mountWithKit(UiRadio, { props: { name: 'p', value: 'a', modelValue: 'a' } }).find('button').classes()).toEqual(expect.arrayContaining(['ui-radio', 'ui-radio-on']))
    expect(mountWithKit(UiTabs, { props: { tabs: [{ value: 'a', label: 'A' }], modelValue: 'a' } }).find('[role="tab"]').classes()).toEqual(expect.arrayContaining(['ui-tab', 'ui-tab-active']))
    expect(mountWithKit(UiAccordion, { props: { items: [{ title: 'T', content: 'C' }] } }).html()).toContain('ui-accordion-item')
  })

  it('hooks the hero corner marks and the aurora backdrop pieces', () => {
    expect(mountWithKit(UiPageHero, { props: { title: 'T' } }).findAll('.ui-hero-corner')).toHaveLength(2)
    expect(mountWithKit(UiBackdrop, { props: { variant: UiBackdropVariant.Aurora } }).findAll('.ui-backdrop-glow')).toHaveLength(2)
    expect(mountWithKit(UiBackdrop, { props: { variant: UiBackdropVariant.Dots } }).find('.ui-backdrop-dots').exists()).toBe(true)
  })

  it('marks the field frame, and its error state apart', () => {
    expect(fieldStateClasses({})).toContain('ui-field')
    expect(fieldStateClasses({})).not.toContain('ui-field-error')
    expect(fieldStateClasses({ error: true })).toContain('ui-field-error')
  })
})
