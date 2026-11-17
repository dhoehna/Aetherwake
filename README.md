# Aetherwake

Aetherwake is based off of [Eden ULTIMA](https://www.nexusmods.com/finalfantasy14/mods/185).

Eden ULTIMA has not updated in two years and is broken because Marty McFly's shaders changed branding.  The brand change rendered Eden ULTIMA incompatible with the newer RTGI files because they changed names.

I re-made it.

Aetherwake is a tool to preview FF XIV with different image options.  Aetherwake sets up a Dx11 chain and uses the ReShade in your FF XIV folder to apply a preset to an image.  Because this runs the image through ReShade rendering can take up to 20 seconds.  The generated image is saved and Aetherwake will use any image it finds before trying to render it.

A consequence of this is the application will look like it is hanging for a bit on first start if you have no generated images.  This is because Aetherwake generates one image before moving to the next screen.

The options: Color Filter, Foundation, Brightness, Contrast, Saturation, and Ray Tracing all come from Eden ULTIMA.

The Permutations and Techniques files are from Eden ULTIMA.  Eden ULTIMA ships with an XML for each combination  of options.  Aetherwake generates them on the fly.

Some of the techniques did not have a clean 1:1 translation to the new iMMERSE names.  AI did its best to figure that out.

# Required components
1. ReShade installed and sitting next to ffxiv_dx11.exe

# How to build
Open up the solution with VS 2026.  Rebuild.

# How to use
1. Run Aetherwake.exe
2. Tell Aetherwake where ffxiv_dx11.exe is.
3. Wait for the image to load
4. Change options and see the results!

## Saving the preset
1. Click "Save Preset" in the lower right corner.
2. Save your preset where you want.
3. Move it over to your ReSHade-presets folder.
4. Open the game and use the generated preset.

# Replace the example image
You can change the image Aetherwake uses.  You can use an image from your game!

The screenshot has to be 1920x1440.

Do this only if you know what you are doing.

## Get the image and depth buffer
Aetherwake needs the image with NO presets applied and the Depth Buffer.  Because the depth buffer and image are taken at different times the image Aetherwake generates might look a bit off.  It will not show up in the preset though.

### Get the image
1. Open up FF XIV
2. Open ReShade and turn off all presets.  We need the vanilla image
3. In ReShade got o "Settings" and change the "Screenshot format" to "Portable Network Graphics (*.png)
4. Close ReShade
5. Set the camera up where you want.
6. Disable UI.
7. screenshot!
8. Do not move the camera.  Time to capture the depth buffer.

### Depth buffer
1. Open ReShade.
2. Go to "Home"
3. Find DisplayDepth [DisplayDepth.fx] and enable it
4. Still in "Home" find the options for DisplayDepth and change the "Present Type" to "Depth Map"
5. Close ReShade
6. Take a screenshot

## Put them into Aetherwake
1. Rename the non-DepthBuffer image to Before.png
2. Place Before.png into RunReShade/BaseImage.  Over write the existing image
3. Rename the DepthBuffer image to DepthAfter.png.
4. Place DepthAfter.png into RunReShade/BaseImage.  Over write the existing image
5. Delete all images in GeneratesImages

Aetherwake wil use your image for RReShade!

# A.I. Involvment

I do not like AI and I did not ler it write the whole project.  I prefer to code a project myself.  I did use AI only after trying to solve a problem myself.