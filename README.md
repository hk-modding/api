Preface
=======

Hollow Knight Modding generally requires changing the game's code using an assembly editor such as dnSpy.   While this works, it means that each time the game releases a new version, all mods had to be recoded by hand.  The Hollow Knight API aims to remove this requirement by exposing hooks to perform most needed operations without having to rewrite decompiled code.

We use the MonoMod patcher to greatly reduce the effort in patching the assembly.  Go check it out! https://github.com/0x0ade/MonoMod

Building The API for Windows
============================
Building the API is fairly straightforward.

1. Clone this!
2. Go to `%HollowKnightGameInstallPath%/hollow_knight_Data/Managed/` and copy it's contents to the `Vanilla` folder in this repository. (Create the Vanilla folder if it does not exist.)
3. Open the solution in Visual Studio 2017 (May work in other versions, only tested on VS2017 Community Edition)
4. Set the build configuration to Debug.
5. The patched assembly should be in `RepoPath/OutputFinal/hollow_knight_Data/Managed/` (There is also a zip file in `RepoPath/ModdingAPI.zip` ready to upload to Google Drive)
6. Copy Assembly-CSharp.* to `%HollowKnightGameInstallPath%/hollow_knight_Data/Managed/`

Building The API for Mac!
============================
Building the API is fairly straightforward. (Note that we're still building this in Windows, just using the Mac files for the game.)

1. Clone this!
2. Go to `%HollowKnightGameInstallPath%/Contents/Resources/Data/Managed/` and copy it's contents to the `VanillaMac` folder in this repository. (Create the VanillaMac folder if it does not exist.)
3. Open the solution in Visual Studio 2017 (May work in other versions, only tested on VS2017 Community Edition)
4. Set the build configuration to DebugMac.
4. Build.  The patched assembly should be in `RepoPath/OutputFinalMac/hollow_knight.app/Contents/Resources/Data/Managed/` (There is also a zip file in `RepoPath/ModdingAPIMac.zip` ready to upload to Google Drive)
5. Copy Assembly-CSharp.* to `%HollowKnightGameInstallPath%/Contents/Resources/Data/Managed/`

Contributors
=======
Original Authors:
MyEyes / Firzen
Seanpr

Contributors:
iamwyza
esipode
Kerr1291
5FiftySix6
natis1
Ayugradow
KayDeeTee
