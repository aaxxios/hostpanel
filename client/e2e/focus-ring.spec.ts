/**
 * Flow: focus ring.
 *
 * A mouse click on a link must leave no outline behind; the ring belongs to keyboard focus only.
 */
import { expect, test } from './support/test'
import { waitForHydration } from './support/helpers'

test('clicking a link leaves no focus outline or ring', async ({ page }) => {
  await page.goto('/faq')
  await waitForHydration(page)

  const link = page.getByTestId('faq-category-general')
  await link.click()
  await expect(page).toHaveURL(/\/faq\/general$/)

  // Press the mouse without releasing it: the press is what focuses the link, and only the
  // release would navigate. A full click() leaves /faq/general, so the link is gone from the
  // page before its style can be read and the evaluate below waits until the test times out.
  const back = page.getByTestId('faq-back-to-all')
  // The link sits below the accordion, past the fold on the CI viewport: a press at its
  // off-screen coordinates lands on nothing, so bring it into view before measuring it.
  await back.scrollIntoViewIfNeeded()
  const box = await back.boundingBox()
  expect(box).not.toBeNull()
  if (!box) {
    return
  }
  await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2)
  await page.mouse.down()
  await expect(back).toBeFocused()
  // Tailwind's `outline-none` is `2px solid transparent` (kept for forced-colours mode), so
  // "no outline" means none or an invisible one, not the style keyword alone.
  const style = await back.evaluate(el => {
    const css = getComputedStyle(el)
    // Any serialisation of a zero-alpha colour: rgba(…, 0), color(srgb … / 0), transparent.
    const clear = /transparent|,\s*0\)$|\/\s*0\)$/.test(css.outlineColor)
    const invisible = css.outlineStyle === 'none' || css.outlineWidth === '0px' || clear
    // The colour rides along in the value so a failure says what was actually drawn.
    return { outline: invisible ? 'none' : `${css.outlineStyle} ${css.outlineWidth} ${css.outlineColor}`, shadow: css.boxShadow }
  })
  await page.mouse.up()
  expect(style.outline).toBe('none')
  expect(style.shadow).toBe('none')
})
