# Logging

Logging can be used for debugging and can be done through the built-in logger
of the API.

## Logging within a mod

Mods, inheriting from [`Mod`](xref:Modding.Mod), implement
[`ILogger`](xref:Modding.ILogger), giving you access to a variety of logging
methods, ranging in level from fine to error.

```cs
public class Example : Mod {

    public override void Initialize() {
        Log("Initializing!");
    
        try 
        {
            SetupSomething();
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }
    
}
```

Output
```
[Example] - Initializing!
```

Outside of the Mod class, you can use the static class
[`Logger`](xref:Modding.Logger), but it's recommended you create a
[`SimpleLogger`](xref:Modding.SimpleLogger), which prepends the name you give
it to your logs, similar to how your mod does.

## Log output

Logs go into `ModLog.txt` under your saves folder. This is operating system
dependent but you can find yours at

|   OS    |                             Path                                 |
|:-------:|:----------------------------------------------------------------:|
| Windows |        `%APPDATA%\..\LocalLow\Team Cherry\Hollow Knight\`        |
|  Linux  |          `~/.config/unity3d/Team Cherry/Hollow Knight/`          |
|  macOS  | `~/Library/Application Support/unity.Team Cherry.Hollow Knight/` |

Previous logs are saved under the `Old ModLogs` directory , also in your saves
folder.

## In-game console

The in-game console can be enabled in `ModdingApi.GlobalSettings.json`, located
in your saves folder. 

```json
{
    ...
    "ShowDebugLogInGame": true
    ...
}
```

The in-game console can then be toggled using `F10`.
