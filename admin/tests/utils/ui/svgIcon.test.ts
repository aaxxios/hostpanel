/**
 * Unit tests for svgNodes: shapes become nodes, everything else is dropped.
 */
import { describe, expect, it } from 'vitest'
import { svgNodes } from '@/utils/ui/svgIcon'

describe('svgNodes', () => {
  it('builds one node per allowed shape, in order', () => {
    const nodes = svgNodes('<rect x="1" y="2" width="3" height="4"/><circle cx="5" cy="6" r="7"/>')

    expect(nodes.map(n => n.type)).toEqual(['rect', 'circle'])
    expect(nodes[0]?.props).toEqual({ x: '1', y: '2', width: '3', height: '4' })
  })

  it('drops tags that are not shapes', () => {
    const nodes = svgNodes('<script>alert(1)</script><path d="M1 1"/>')

    expect(nodes.map(n => n.type)).toEqual(['path'])
  })

  it('drops attributes that are not presentation attributes', () => {
    const nodes = svgNodes('<path d="M1 1" onclick="steal()" style="x:y"/>')

    expect(nodes[0]?.props).toEqual({ d: 'M1 1' })
  })

  it('returns an empty list for an empty string', () => {
    expect(svgNodes('')).toEqual([])
  })
})
