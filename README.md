# Channel Packer

Unity Editor tool for packing grayscale textures into RGBA channels.

## Features

- Pick which channel to take from each source texture (not just R→R)
- Auto-handles read/write settings (turns on, packs, turns back off)
- Checks texture sizes before packing (no crashes from mismatched dimensions)
- Clean UI with texture previews and color-coded slots

## How to use

1. Open `Tools > Channel Packer` in Unity
2. Drop textures into slots (R, G, B, A)
3. Pick which channel to grab from each texture (dropdown under each slot)
4. Hit Pack
5. Choose where to save

<img width="868" height="439" alt="image" src="https://github.com/user-attachments/assets/4410674a-4486-41ad-9e6e-3a4851fce049" />

If texture sizes don't match, the Pack button turns gray and you get an error message.

## Example use case

You have these textures from Substance:
- `metal_roughness.png` (roughness in green channel)
- `metal_metallic.png` (metallic in red channel)  
- `metal_ao.png` (AO in red channel)

Pack them:
- R slot: `metal_metallic.png`, channel R
- G slot: `metal_roughness.png`, channel G  
- B slot: `metal_ao.png`, channel R
- A slot: empty (or height map)

Result: one texture with all PBR data packed efficiently.

## Technical notes

- Uses `GetPixels()` so textures need read/write enabled (tool does this automatically)
- Validates sizes before packing to avoid index errors
- Restores original read/write settings after packing
- Outputs PNG format
