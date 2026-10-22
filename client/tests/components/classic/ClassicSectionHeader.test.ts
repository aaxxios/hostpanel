/**
 * Unit tests for ClassicSectionHeader: eyebrow, title and lead line, each optional except the title.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ClassicSectionHeader from '~/components/classic/ClassicSectionHeader.vue'

describe('ClassicSectionHeader', () => {
  it('shows the eyebrow, the title as a level-two heading and the subtitle', () => {
    const wrapper = mountWithKit(ClassicSectionHeader, { props: { eyebrow: 'Eyebrow', title: 'Title', subtitle: 'Lead' } })

    expect(wrapper.find('[data-testid="classic-section-header-eyebrow"]').text()).toBe('Eyebrow')
    expect(wrapper.find('h2').text()).toBe('Title')
    expect(wrapper.find('[data-testid="classic-section-header-subtitle"]').text()).toBe('Lead')
  })

  it('leaves out the eyebrow and the subtitle when not given', () => {
    const wrapper = mountWithKit(ClassicSectionHeader, { props: { title: 'Only a title' } })

    expect(wrapper.find('[data-testid="classic-section-header-eyebrow"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="classic-section-header-subtitle"]').exists()).toBe(false)
  })

  it('is plain: no pill badge, no gradient text', () => {
    const html = mountWithKit(ClassicSectionHeader, { props: { eyebrow: 'E', title: 'T' } }).html()

    expect(html).not.toMatch(/rounded-full|gradient|bg-clip-text/)
  })
})
