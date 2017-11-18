Preface
=======

Hollow Knight Modding generally requires changing the game's code using an assembly editor such as dnSpy.   While this works, it means that each time the game releases a new version, all mods had to be recoded by hand.  The Hollow Knight API aims to remove this requirement by exposing hooks to perform most needed operations without having to rewrite decompiled code.

We use the MonoMod patcher to greatly reduce the effort in patching the assembly.  Go check it out! https://github.com/0x0ade/MonoMod

Building The API
================
Building the API has 1 main step and 1 minor step.  

1. Clone this!
2. Go to `%HollowKnightGameInstallPath%/hollow_knight_Data/Managed/` and copy it's contents to the `Vanilla` folder in this repository. 
3. Open the solution in Visual Studio
4. Build.  The patched assembly should be in `RepoPath/OutputFinal/`
5. Copy Assembly-CSharp.* to `%HollowKnightGameInstallPath%/hollow_knight_Data/Managed/`
6. Open dnspy and drag Assembly-CSharp.dll in. *Note: I'm hoping we can eliminate this, waiting to see if the MonoMod author can fix this.*
    1. Delete the MonoMod namespace
    2. Save the Module.
    3. Close the module.
7. And you're done.