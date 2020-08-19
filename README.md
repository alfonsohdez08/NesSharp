# NesSharp

A bare bone Nintendo Entertainment System (NES) emulator on C#. Even though it is built on .NET Core 3.1, the emulator UI is not cross platform at all because it uses Windows Form.

# Overview

The CPU is fully tested and it passes all the tests from the nestest.nes cartridge; however, the APU is not implemented yet, so there's not sound being emulated.

## CPU - PPU timing

The model used for mantain in sync the CPU and PPU was a catch up - through a master clock cycle stamping. Both components count how many master clock cycles has been elapsed across their execution. For instance, when the CPU executes an instruction, it counts how many master clock cycles took that instruction, and keep adding to its master clock cycle counter; the PPU does something similar: counts the master clock cycles across the frame rendering pipeline.The catch up would happen when the CPU needs to talk to the PPU by accessing one of the PPU registers mapped to the CPU memory map ($2000 - $2007), so before the read/write, the PPU runs for the amount of master clock cycles elapsed up to that point. 

Now, the order I orchestrate both components is the CPU runs up to the amount of master clock cycles before the beginning of the vertical blank period in a frame; then I run the PPU for catch up the CPU. After this, I let the CPU run up to the remain of master clock cycles in a frame, and then I switch to the PPU for catch up the CPU again.

# Prerequisite

You only need to install the .NET Core 3.1. runtime in your computer and then you are good to go!

# Mappers supported

Mapper | 
-|
0 | 

# Controls

NES Pad | Keyboard
-|-
Left | Left arrow key
Up | Up arrow key
Right | Right arrow key
Down | Down arror key
A | Z
B | X
SELECT | Space bar
START | Enter

# Screenshots

# Should I use this emulator for play my favorite games?

Please, do not do so. I do **not** recommend that; first of all, all the games are not supported - only games that do not have a mapper are playable. Also, there's some games that are not being emulated correclty - for instance, Ice Climbers. And there's not sound yet... so the gaming experience would not be the best. This emulator was for learning purpose.

# Known issues

- The games where their nametables are mirrored horizontally are buggy. For instance, when playing Ice Climbers, the background is not being rendered properly because one quarter of the screen - from top to bottom - looks really odd - like, it's looking up from the non desired nametable.
