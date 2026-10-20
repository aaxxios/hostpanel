/**
 * Class strings the aurora template sets its headings and small caps with.
 *
 * `UiHeading` (`size="bare"`) and `UiText` (`tone="inherit"`) carry no palette or size, so the
 * template supplies both from these literals. Each is a complete class list that Tailwind reads
 * from this file, kept in one place so every section headline is set the same way.
 *
 * @module templates/aurora/typography
 */

/** Small uppercase label above a section headline; the colour utility is added at the call site. */
export const EYEBROW_CLASS = 'text-[13px] uppercase tracking-[0.12em]'

/** Headline of a homepage section. */
export const SECTION_TITLE_CLASS = 'font-display text-[clamp(26px,3.2vw,36px)] font-bold -tracking-[0.025em] leading-[1.15] text-tx'

/** Headline of a block inside a page (comparison, price table, FAQ). */
export const BLOCK_TITLE_CLASS = 'font-display text-[clamp(24px,2.8vw,32px)] font-bold -tracking-[0.025em] leading-[1.15] text-tx'

/** Headline of a standalone page (hosting, domains, checkout). */
export const PAGE_TITLE_CLASS = 'font-display text-[clamp(30px,4.2vw,46px)] font-bold -tracking-[0.025em] leading-[1.15] text-tx'

/** Padding that separates a bordered section from its neighbours. */
export const SECTION_SPACING_CLASS = 'relative border-t border-line'
