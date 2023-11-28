

# REngine
![Engine Logo](https://github.com/rbnpontes/REngine/blob/main/doc/EngineLogo_sm.jpg)

Welcome to my game engine, full made by a Brazilian
I'm excited to share my open-source game engine project with you. This is a personal initiative where I've leveraged my knowledge in game engine development.

# Build
Clone this repository by the command line above

    https://github.com/rbnpontes/REngine.git
    
Then, restore nuget packages(you can do this easily on Visual Studio)
Build and Run.

## Windows Build
If you are under windows, you just need to build and run `REngine.Sandbox` project. (Again, you can do easily with Visual Studio.)

## Linux Build
Before Build, you must have the follow libraries on your system:
`libFLAC.so.8, libdl.so, vulkan(LIB or SDK)`

If you are using Linux, you will need to build the graphics engine driver first. To accomplish this, clone the driver repository and build it using CMake. 

> Ensure that you have CMake and C++ tools installed on your Linux
> system.

If you don't have access to a Linux machine, you can also build the Linux library using Windows Subsystem for Linux (WSL) on a Windows system.

**Open terminal and run the above command line**

    git clone https://github.com/rbnpontes/REngine-DiligentNativeDriver

**Before build driver, publish or build `REngine.Sandbox` project with DotNet command**    

Inside `REngine-DiligentNativeDriver` run the above command line through terminal. (You must have [Vulkan SDK](https://vulkan.lunarg.com/doc/view/latest/linux/getting_started_ubuntu.html) installed on your environment).

    cmake -S . -B ./build-linux -G "Unix Makefiles"

And

    cmake --build ./build-linux
    
After completing the build process, copy the `libREngine-DiligentNativeDriver.so` file to the runtime path of the published or built engine directory.

## Steam Deck Build
Perform the same steps on Linux, copy the published or built project, and transfer it to your Steam Deck. The Steam Deck OS is Linux-based, so you should encounter no issues in this regard.

## Troubleshooting

1. Does engine works with MacOS ? Answer: NO, but its could be easy to port, feel free to open PR for that.
2. Can i collaborate ? Answer: Yeah, feel free to open PR
3. I can´t build project, or something is wrong. Answer: Feel free to send me a e-mail to rbnpontes@gmail.com
4. I want a feature, how can i request? Answer: Open your request on issues tab, if i know how to-do i can prioritize
5. How to enable Tracy Profiler ? Answer: If you're under Visual Studio, just select `Debug-Profiler` or `Release-Profiler` configuration
6. I need to use Tracy on Linux. Answer: To use on linux you must build manualy tracy with CMAKE parameters: 
`-DTRACY_FIBERS=ON -DTRACY_DELAYED_INIT=ON -DTRACY_MANUAL_LIFETIME=ON -DBUILD_SHARED_LIBS=ON`

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

### Huge Thanks to Support JetBrains

I want to express my heartfelt gratitude to JetBrains for providing a full license of their products and making my hobby become a reality.
Purchase JetBrains; it's an incredible product, and as a developer, you won't regret it.
https://jb.gg/OpenSourceSupport.
![JetBrains Logo (Main) logo](https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png)
