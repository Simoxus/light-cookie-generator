Unity editor tool that lets you generate light cookies and simulate shadows... fresh out of the oven (i'm sorry)

## Light Cookie Generator

**By**: @Simoxus ([GitHub](https://github.com/Simoxus), [YouTube](https://www.youtube.com/@simoxusofficial))

**Thanks to**: @spazi (who made me think about making this in the first place)

## Usage
**1.** In Unity, go to "Window > Package Manager"

**2.** You should see an "add" icon at the top left of the package manager; do "Install package from Git URL"

**3.** Paste this URL in: ```https://github.com/Simoxus/light-cookie-generator.git```
   
**4.** Once installed, navigate to "Tools > Light Cookie Generator"
   
**5.** In the Save Settings section, input a folder for the cookies to save to and a prefix for the light cookie's name

**6.** Just drag your light object(s) into the "Add Light" field or select all your light(s) and do "Add Selected Lights"

**7.** Do "Generate Cookies" (you can also do "Generate Previews")

**8.** Very crunchy

## Features
* ### Saving and loading
  * When a setting is changed in the tool, it will save to EditorPrefs
  * When a cookie is generated, the .meta that goes along with that file will have the data the tool needs to load that texture's settings
  * Pain in my ass to make

* ### Customizable settings
  to be rewritten (eventually)
  ~~* Drag in lights or add all selected~~
  ~~* Button to clear all lights~~
  ~~* Option to use spotlight range~~
    ~~* If you don't use this, you can manually set the shadow plane's distance instead~~
  ~~* Ability to set the opacity of the actual shadows in the cookie texture~~
  ~~* Ability to set the overall brightness of the cookie texture~~
  ~~* Set amount of samples anti-aliasing takes for each pixel (1x, 2x, 4x, 8x)~~
  ~~* Use numerous methods for blurring (Penumbra blur is my favorite)~~
  ~~* Set a blur radius~~
  ~~* Set the amount of iterations blur goes through~~
  ~~* Use Gizmos~~
  ~~* Set a rotation offset for capture~~
  ~~* Set a "capture size" for the texture (orthographic size)~~
  ~~* Use a custom folder for output~~
  ~~* Set a prefix for the texture's name~~
  ~~* Set the resolution of the outputted texture~~
  ~~* Make the cookie(s) also assign to lights~~

## Notes That May Be Useful To You Perchance And I Hope They Are But I'm Not Sure
* Tested with Unity 6.0 (0.58f2) and URP
* The GUI originally had two different modes, but I decided to just merge them (there may be some leftover variables and stuff in the code)
* You can only load settings from cookies (that were made using the generator)
* A resolution of 512x512 for the cookie is probably enough
* Only spot lights are supported
* THESE ARE NOT REAL COOKIES (sadly D:)
* Please leave a star so I can get GitHub famous
