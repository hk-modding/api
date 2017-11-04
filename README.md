Preface
=======

Hollow Knight Modding generally requires changing the game's code using an assembly editor such as dnSpy.   While this works, it means that each time the game releases a new version, all mods had to be recoded by hand.  The Hollow Knight API aims to remove this requirement by exposing hooks to perform most needed operations without having to rewrite decompiled code.

Building The API
================
Building the API has 2 manual and 2 automatic steps.

1. Clone this!
2. Go to `%HollowKnightGameInstallPath%/hollow_knight_Data/Managed/` and copy it's contents to the `Vanilla` folder in this repository. 
3. Open the solution in Visual Studio
4. Set the configuration to FirstPass and build the solution. That action should:
    1. Restore a nuget package called ILMerge
    2. Build the first initial required part of the API
    3. Copy All the DLLs from `Vanilla` into `Output1`
    4. Merge the built code into `Assembly-CSharp.dll`
5. Open up the new `Output1/Assembly-CSharp.dll` in dnSpy and make the following initial changes.

    1. Add the following to `SaveGameData`

```csharp
public ModSettingsDictionary modData;
```

    2. Add the following to `PlayerData`

```csharp
public void SetBoolInternal(string boolName, bool value)
{
    FieldInfo field = base.GetType().GetField(boolName);
    if (field != null)
    {
        field.SetValue(PlayerData.instance, value);
        return;
    }
    Debug.Log("PlayerData: Could not find field named " + boolName + ", check variable name exists and FSM variable string is correct.");
}


public bool GetBoolInternal(string boolName)
{
    if (string.IsNullOrEmpty(boolName))
    {
        return false;
    }
    FieldInfo field = base.GetType().GetField(boolName);
    if (field != null)
    {
        return (bool)field.GetValue(PlayerData.instance);
    }
    Debug.Log("PlayerData: Could not find bool named " + boolName + " in PlayerData");
    return false;
}


public void SetIntInternal(string intName, int value)
{
    FieldInfo field = base.GetType().GetField(intName);
    if (field != null)
    {
        field.SetValue(PlayerData.instance, value);
        return;
    }
    Debug.Log("PlayerData: Could not find field named " + intName + ", check variable name exists and FSM variable string is correct.");
}


public int GetIntInternal(string intName)
{
    if (string.IsNullOrEmpty(intName))
    {
        Debug.LogError("PlayerData: Int with an EMPTY name requested.");
        return -9999;
    }
    FieldInfo field = base.GetType().GetField(intName);
    if (field != null)
    {
        return (int)field.GetValue(PlayerData.instance);
    }
    Debug.LogError("PlayerData: Could not find int named " + intName + " in PlayerData");
    return -9999;
}
```

    3. Add the following to Language.Language

```csharp
public static string GetInternal(string key, string sheetTitle)
{
    if (Language.currentEntrySheets == null || !Language.currentEntrySheets.ContainsKey(sheetTitle))
    {
        Debug.LogError("The sheet with title \"" + sheetTitle + "\" does not exist!");
        return string.Empty;
    }
    if (Language.currentEntrySheets[sheetTitle].ContainsKey(key))
    {
        return Language.currentEntrySheets[sheetTitle][key];
    }
    return "#!#" + key + "#!#";
}
```

    4. Save the Module.
    5. Close the module.

6. Back in VS, switch configuration to 2nd pass. This will automatically
    1. Build the rest of the Modding namespace
    2. Copy Output1/* to OutputFinal/*
    3. Merge the rest of this new code into `OutputFinal/Assembly-CSharp.dll`
7. Now finally, the tedious part.  Reopen Assembly-CSharp.dll in dnspy and add all the hooks in.  _Writers Note: I have no Idea where all these are right now.  Perhaps someone *cough* *sean* *cough* will go find them....