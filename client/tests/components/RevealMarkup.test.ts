/**
 * The entrance reveal must not hide content in the markup: a block that carries an inline
 * `opacity: 0` is invisible in server HTML until a script reveals it, and stays so if that
 * script never fires.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ProductsBenefits from '~/components/catalog/ProductsBenefits.vue'
import MarketingSectionHeader from '~/components/marketing/MarketingSectionHeader.vue'
import SectionsCTA from '~/components/sections/SectionsCTA.vue'
import SectionsProcess from '~/components/sections/SectionsProcess.vue'
import SectionsWhyChooseUs from '~/components/sections/SectionsWhyChooseUs.vue'
import UiPageHero from '~/components/ui/UiPageHero.vue'

/** Blocks that reveal on entrance, each mounted the way their own tests mount them. */
const REVEALING = {
  ProductsBenefits: () => mountWithKit(ProductsBenefits, { components: { MarketingSectionHeader }, stubs: { 'i18n-t': true } }),
  MarketingSectionHeader: () => mountWithKit(MarketingSectionHeader),
  SectionsCTA: () => mountWithKit(SectionsCTA),
  SectionsProcess: () => mountWithKit(SectionsProcess),
  SectionsWhyChooseUs: () => mountWithKit(SectionsWhyChooseUs),
  UiPageHero: () => mountWithKit(UiPageHero),
}

describe('entrance reveal markup', () => {
  for (const [name, mount] of Object.entries(REVEALING)) {
    it(`${name} renders no inline opacity:0 and reveals with the CSS animation`, () => {
      const html = mount().html()

      expect(html).not.toMatch(/opacity:\s*0\b/)
      expect(html).toContain('animate-reveal-up')
      expect(html).toContain('motion-reduce:animate-none')
    })
  }
})
