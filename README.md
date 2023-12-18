# REngine
![Engine Logo](https://github.com/rbnpontes/REngine/blob/main/doc/EngineLogo_sm.jpg)

Welcome to my game engine, full made by a Brazilian
I'm excited to share my open-source game engine project with you. This is a personal initiative where I've leveraged my knowledge in game engine development.

# Build
Clone this repository by the command line above

    https://github.com/rbnpontes/REngine.git

Run `update_dependencies.bat` or `update_dependencies.sh` if you are under linux.
    
Then, restore nuget packages(you can do this easily on Visual Studio)
Build and Run.

## Windows Build
If you are under windows, you just need to build and run `REngine.Sandbox` project. (Again, you can do easily with Visual Studio.)

## Linux Build
Before Build, you must have the follow libraries on your system to execute engine:
`libFLAC.so.8, libdl.so, vulkan(LIB or SDK)`
Do the same steps before and execute.

## Android Build
Make sure you have Android SDK, NDK Tools and Xamarin installed in your machine
After that, Do the sames steps before and execute `REngine.Android.Sandbox`

## Steam Deck Build
Perform the same steps on Linux, copy the published or built project, and transfer it to your Steam Deck. The Steam Deck OS is Linux-based, so you should encounter no issues in this regard.

## Troubleshooting

1. Does the engine work with MacOS? 
**Answer:** NO, but its could be easy to port, feel free to open PR for that.
2. Can i collaborate ? Answer: Yeah, feel free to open PR
3. I can't build the project, or something is wrong. 
**Answer:** Feel free to send me a e-mail to [rbnpontes@gmail.com](mailto:rbnpontes@gmail.com)
4. I want a feature; how can I request it? 
**Answer:** Open your request on the issues tab. If I know how to do it, I can prioritize it.
5. How to enable Tracy Profiler ? 
**Answer:** If you're using Visual Studio, just select `Debug-Profiler`, `Debug-Profiler-Android` or `Release-Profiler` configuration
6. `update_dependencies.bat` or `update_dependencies.sh` is not working. Answer: 
You can run manually this command `.\Utils\winREngine.DependencyTool.exe -o dependencies` or `./Utils/linuxREngine.DependencyTool -o dependencies` if your are under linux
7. External Downloaded Dependencies is not work. Answer: if third party dependencies is not work, you can build all dependencies manually.
Basically you must generate correct libraries( Windows=.dll, Linux=.so, and so on) and place under `dependencies` folder. (On tracy lib, you must build from this repo: https://github.com/rbnpontes/tracy)
8. I want to build REngine Diligent Driver. 
**Answer:** Great, clone this repo https://github.com/rbnpontes/REngine-DiligentNativeDriver, build and place libs under `dependencies`

# Samples
![REngine Samples](https://github.com/rbnpontes/REngine/blob/main/doc/sample.gif)

![REngine Sandbox](https://github.com/rbnpontes/REngine/blob/main/doc/doge_sandbox.gif)

### Sprite Effect
![Doge Rounded](https://github.com/rbnpontes/REngine/blob/main/doc/sprite_effect_sample.gif)

### Sprite Instanced
![Doge Kaleidoscope](https://github.com/rbnpontes/REngine/blob/main/doc/doge_kaleidoscope.gif)

### Steam Deck
![Steam Deck](https://github.com/rbnpontes/REngine/blob/main/doc/steam-deck-doge.jpeg)

### Text Rendering
![Text Rendering](https://github.com/rbnpontes/REngine/blob/main/doc/text-rendering-sample.jpg)

### Render Graph
![Render Graph](https://github.com/rbnpontes/REngine/blob/main/doc/render_graph_sample.gif)

### Sound Sample
![Sound Sample](https://github.com/rbnpontes/REngine/blob/main/doc/sound_sample.gif)

### Simple Pong Game
![Pong Game](https://github.com/rbnpontes/REngine/blob/main/doc/pong_game.gif)

### Tray Support
![Tracy Profiler](https://github.com/rbnpontes/REngine/blob/main/doc/tracy_profiler.gif)

### Android Support
![Android](https://github.com/rbnpontes/REngine/blob/main/doc/android_pong.png)

## Third Party Credits
- Tracy (https://github.com/clibequilibrium/Tracy-CSharp | MIT) (https://github.com/wolfpld/tracy | BSD)
- SFML (https://www.sfml-dev.org/ | ZLIB/Libpng)  (https://www.sfml-dev.org/download/sfml.net/)
- FreeType (https://github.com/ryancheung/FreeTypeSharp | MIT) (https://freetype.org/ | FTL or GPL)
- ImGui (https://github.com/ImGuiNET/ImGui.NET | MIT) (https://github.com/ocornut/imgui | MIT) 
- Diligent Engine (https://github.com/DiligentGraphics/DiligentEngine | Apache 2.0)
- GLFW (https://github.com/ForeverZer0/glfw-net | MIT) (https://www.glfw.org/ | Zlib/Libpng)
- JSON.Net (https://www.newtonsoft.com/json | MIT)
## Huge Thanks to Support JetBrains

I want to express my heartfelt gratitude to JetBrains for providing a full license of their products and making my hobby become a reality.
Purchase JetBrains; it's an incredible product, and as a developer, you won't regret it.
https://jb.gg/OpenSourceSupport.
![JetBrains Logo (Main) logo](https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png)
