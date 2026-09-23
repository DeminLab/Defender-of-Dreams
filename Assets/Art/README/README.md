# Defender of Dreams — Pixel Art Update

This pack is a visual foundation for the game's current direction:
- top-down / 2.5D pixel-art readability;
- dark blue-gray fog and stone;
- restrained amber memory light;
- no anime look, wings, fantasy armor or glossy 3D style;
- Nero, Lia, Shoroh and Poziratel silhouettes;
- 32x32 environment tiles;
- a 32x24 prototype map layout with exploration loop, memory anchors and boss gate.

## Unity import
For character and tiles textures:
- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single for characters; Multiple for DreamTiles_32x32
- Pixels Per Unit: 16 for the 64x64 characters if you want a 4-unit character, or 32 for a 2-unit character
- Filter Mode: Point (no filter)
- Compression: None
- Generate Mip Maps: Off

DreamMap_Prototype_32x24.png is a visual map reference, not a Unity Tilemap asset.
Use DreamTiles_32x32.png to rebuild the playable scene as a real Grid + Tilemap.
