/**
 * What an operator can set as the default colour mode: a fixed mode, or "follow the device".
 *
 * @module types/colormodesetting
 */

import type { ColorMode } from '~/types/colormode'
import type { ColorModeSystem } from '~/types/colormodesystem'

/** A fixed colour mode, or {@link ColorModeSystem.System}. */
export type ColorModeSetting = ColorMode | ColorModeSystem
