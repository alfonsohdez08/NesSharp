# NesSharp

A bare bone Nintendo Entertainment System (NES) emulator on C#. Even though it is built on .NET Core 3.1, the emulator UI is not cross platform at all because it uses Windows Form.

# Overview

The CPU is fully tested and it passes all the tests from the nestest.nes cartridge; however, the timing between the CPU and PPU is not the best, so all the PPU cartrige tests fails at least
on one test. The APU is not implemented yet, so there's not sound being emulated.

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
