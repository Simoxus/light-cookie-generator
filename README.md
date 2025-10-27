## Light Cookie Generator

**By**: @Simoxus ([GitHub](https://github.com/Simoxus), [YouTube](https://www.youtube.com/@simoxusofficial))

**Thanks to**: @spazi (who made me think about making this in the first place)

## Usage
**1.** On the main page of the repository, you should see a green "Code" button. Click it and do "Download ZIP":
<img width="220" height="202" alt="tutorial1" src="https://github.com/user-attachments/assets/73879a5e-2b48-4acb-b34c-efb28a3cdf71" />

**2.** Once it's done downloading, extract it, import the folder into your project, and wait for it to compile

**3.** Go to your URP asset and make sure "Light Cookies" are turned on
   
**4.** Navigate to "Tools > Light Cookie Generator"
   
**5.** Select either the "Single Light" or "Batch Generation" modes
   
**6.** In the Save Settings section, input a folder for the cookies to save to and a prefix for the light cookie's name

**7.** Add either a "Camera Transform" and "Reference Light" if you're in "Single Light" mode, or just drag in your lights if you're in "Batch Generation" mode

**8.** Do "Generate Cookie Texture" or "Generate All Cookies"

**9.** Very crunchy

## Features
* ### Single Light Mode:
  * Allows you to save and load cookies (that were generated using the tool)
  * Choose to use all scene geometry as shadow casters
  * You can use a shadow caster blacklist however
  * Set up your single light from whatever you have selected
  * Pick a folder for the cookie texture to save to
  * Provide a prefix for the cookie texture name
  * Provide a custom transform to be used as the camera
  * Visualize the capture region, direction, etc. (using gizmos my beloved)
  * Use a rotation offset
  * Set a "capture size" for the cookie texture (orthographic size)
  * Pick a resolution for the light cookie
  * Use a shadow simulation to fake shadows (albeit not perfectly)
  * Set the shadow opacity of the cookie texture
  * Set the overall brightness of the cookie texture
  
* ### Batch Generation Mode
  * Allows you to save and load cookies (that were generated using the tool)
  * Pick a folder for the cookie texture(s) to save to
  * Provide a prefix for the cookie texture name(s)
  * Pick a resolution for the light cookie(s)
  * Use either the light's range or shadow plane distance
  * Set the shadow opacity of the cookie texture(s)
  * Set the overall brightness of the cookie texture(s)
  * Set the "capture size" for the cookie texture(s) (orthographic size)

## Notes That May Be Useful To You Perchance And I Hope They Are But I'm Not Sure
* Tested with Unity 6.0 (0.58f2) and URP
* You can only load settings from cookies (that were made using the genertor)
* A resolution of 512x512 for the cookie is probably enough
* Only spot lights are supported
* THESE ARE NOT REAL COOKIES (sadly D:)
* Please leave a star so I can get GitHub famous
