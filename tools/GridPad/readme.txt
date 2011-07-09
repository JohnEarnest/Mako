GRIDPAD

Gridpad is an extremely simple map editor intended for use with Mako. You can draw and erase, copy and paste and enjoy infinite undo/redo steps. Gridpad is designed to load and save files via the clipboard, so you can easily paste finished maps into your Maker source files.

The Palette:

On the right of the application window is the tile palette. Clicking changes the selected tile. You can also switch tiles using ASWD or the cursor keys. The bracket keys ('[' and ']') cycle through available tilesets. Tilesets are loaded from the 'tilesets' directory and should be 128-pixel (16 tile) wide .PNGs for best results. The two shades of green in the background allow you to easily distinguish the first 8 columns of a tileset from the second 8 columns- the former are often walkable while the latter are often solid, for purposes of sprite collision.

The Editor:

The left half of the application window is the editor. By default all tiles are initialized to -1 (transparent), leaving a series of red guides. The dark rim on the bottom and right edge indicate row 31 and column 41, respectively, which are only drawn if the scroll registers are in use. The light red area near the bottom of the screen indicates the bottom 6 visible rows, which are often reserved for status displays.

Keys:

- Option : (or Alt on a PC keyboard) toggle between 'select' mode and 'draw' mode. When drawing, the right mouse button erases (replacing with transparency) and the left mouse button paints with the selected tile. In select mode, clicking and dragging allows you to outline a rectangular region of that can be copied or cleared.

- Control + Z : Undo. Roll back the last editing action performed.

- Control + R : Redo. If you've used 'undo' several times, editing actions will clear the 'redo' buffer- be careful.

- Control + O : Write-Out. Saves the current map to the system clipboard.

- Control + L : Load. Read in a map from the system clipboard. Newlines are used to delimit rows, while whitespace-delimited signed integers represent tile indices.

- Control + C : Copy. Copies the selected region to the Gridpad clipboard.

- Control + V : Paste. Draws the Gridpad clipboard, starting with the top-left corner at the mouse cursor's current position.

- Control + F : Fill. Fill the selected region with the current tile.

- Delete: Erase the selected region, replacing tiles with transparency.

- ',' : reduce   the horizontal size of the map by 40 tiles

- '.' : increase the horizontal size of the map by 40 tiles

- '<' : reduce   the vertical size of the map by 30 tiles

- '>' : increase the vertical size of the map by 30 tiles

- 'p' : reload all tilesets.