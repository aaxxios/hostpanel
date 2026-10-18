/**
 * Client-side CSV download for reports whose rows are already on screen.
 */

/**
 * Quotes one CSV field, doubling any quote inside it.
 *
 * @param value - The field.
 * @returns The quoted field.
 */
const quote = (value: string | number): string => `"${String(value).split('"').join('""')}"`

/**
 * Builds CSV text from a header and rows.
 *
 * @param header - Column captions.
 * @param rows - One array of values per line.
 * @returns The CSV text, lines joined by newlines.
 */
export const buildCsv = (header: string[], rows: (string | number)[][]): string =>
  [header, ...rows].map(line => line.map(quote).join(',')).join('\n')

/**
 * Offers rows as a file download.
 *
 * @param filename - Name of the downloaded file.
 * @param header - Column captions.
 * @param rows - One array of values per line.
 */
export const downloadCsv = (filename: string, header: string[], rows: (string | number)[][]): void => {
  const blob = new Blob([buildCsv(header, rows)], { type: 'text/csv' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  link.click()
  URL.revokeObjectURL(url)
}
