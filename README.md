Preface
=======

Hollow Knight Modding generally requires changing the game's code using an assembly editor such as dnSpy.   While this works, it means that each time the game releases a new version, all mods had to be recoded by hand.  The Hollow Knight API aims to remove this requirement by exposing hooks to perform most needed operations without having to rewrite decompiled code.

We use the MonoMod patcher to greatly reduce the effort in patching the assembly. Go check it out! https://github.com/MonoMod/MonoMod

Building The API
============================
Building the API is fairly straightforward.

Using an IDE:
1. Clone this!
2. Go to one of the directories listed below and copy it's contents to the `Vanilla` folder in this repository. (Create the Vanilla folder if it does not exist.)
    * Windows: `%HollowKnightGameInstallPath%/hollow_knight_Data/Managed/`
    * Linux: `~/.steam/steam/steamapps/common/Hollow Knight/hollow_knight_Data/Managed/`
    * Mac: `~/Library/Application Support/Steam/steamapps/common/Hollow Knight/hollow_knight.app/hollow_knight_Data/Managed/`
3. Open the solution in your IDE of choice.
5. The patched assembly should be in `RepoPath/OutputFinal/` 
6. Copy `Assembly-CSharp.dll` to `%HollowKnightGameInstallPath%/hollow_knight_Data/Managed/`

To use the `dotnet` cli:
1. Follow steps 1-3 of "Using an IDE".
2. Navigate to the root of the project
3. Run `dotnet build -p:Configuration=Debug`

Contributors
=======
Original Authors:  
MyEyes / Firzen  
Seresharp  

Contributors:  
iamwyza  
esipode  
Kerr1291  
fifty-six  
natis1  
Ayugradow  
Katie  
SFGrenade  
Yurihaia
