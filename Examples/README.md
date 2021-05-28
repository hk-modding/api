# Examples
This directory contains many example mods to show the common uses of the modding api.
These mods are intended to give beginners a look at how to start to develop mods using this api,
although proficient knowlege of C# is fairly required.  

## Setup
These projects can be built without any additional configuration.
The modding api project defines a build setting to copy the files into a `HollowKnightManaged` folder
to set up the references.
See the [README](../README.md) for setup of the general modding API.
* To build the project with an IDE, set the `SetupExamples` build property to `true`.  
* To build the project with the `dotnet` cli, add the `-p:SetupExamples=true` flag to the build command.  

Alternatively, if you have the latest version of the modding API installed already, you can just create
the `HollowKnightManaged` folder in this directory and then copy the contents of the `Managed` folder in your
Hollow Knight install into the `HollowKnightManaged` folder.

## Example Mods
The following table of contents is listed in order of complexity, with each example usually building
on concepts from the previous.

1. [Simple Hooks](./SimpleHooks/SimpleHooks.cs) - A mod that demostrates the basics of creating a mod.  
    The [`.csproj`](./SimpleHooks/SimpleHooks.csproj) file is a good baseline for the general config file you will
    find in almost every mod. It defines the reference to the games assemblies in the `HollowKnightManaged` folder
    (which gets populated using the [Setup](#Setup) step), or a user defined folder by changing the `HollowKnightRefs` tag.
2. [Custom Save Data](./CustomSaveData/CustomSaveData.cs) - This mod shows how the mod loader can save global
    and savegame specific data.  
    This is the main way mods will save persistent data. The relevant save files will be `ModName.GlobalSettings.json` for global data and all save data is stored to
    `user1.modded.json` for save slot 1, `user2.modded.json` for save slot 2, etc.

