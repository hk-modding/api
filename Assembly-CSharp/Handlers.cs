using GlobalEnums;
using UnityEngine;

namespace Modding
{
    /// <summary>
    /// Called just before Application Quits.
    /// </summary>
    public delegate void ApplicationQuitHandler();

    /// <summary>
    /// Called After SaveGameData is created, but before it is serialized and saved to disk.  Usually not called directly in Mods.
    /// </summary>
    /// <param name="data">SaveGameData before serialization</param>
    public delegate void BeforeSavegameSaveHandler(SaveGameData data);

    /// <summary>
    /// Called just after the game has been saved to disk
    /// </summary>
    /// <param name="id">Save slot location</param>
    public delegate void SavegameSaveHandler(int id);

    /// <summary>
    /// Called after the game was loaded from Disk
    /// </summary>
    /// <param name="id">Save slot location</param>
    public delegate void SavegameLoadHandler(int id);

    /// <summary>
    /// Called after the game was loaded from disk, but before the data is loaded into PlayerData and SceneData.  Populates Mod's Settings
    /// </summary>
    /// <param name="data">SaveGameData right after deserialization</param>
    public delegate void AfterSavegameLoadHandler(SaveGameData data);

    /// <summary>
    /// Called after game save is cleared.
    /// </summary>
    /// <param name="save">Save slot location</param>
    public delegate void ClearSaveGameHandler(int save);

    /// <summary>
    /// Called after a new game save is started.
    /// </summary>
    public delegate void NewGameHandler();

    /// <summary>
    /// Called after setting up a new PlayerData
    /// </summary>
    /// <param name="data">New PlayerData</param>
    public delegate void NewPlayerDataHandler(PlayerData data);

    
    /// <summary>
    /// Called after scene has been loaded.
    /// </summary>
    /// <param name="targetScene">Scene name that was loaded.</param>
    public delegate void SceneChangedHandler(string targetScene);

    //TODO: This says it returns a string, but the event in Modhooks returns void?
    /// <summary>
    /// Called right before a scene gets loaded, can change which scene gets loaded
    /// </summary>
    /// <param name="sceneName">Scene name to load</param>
    /// <returns>Unknown</returns>
    public delegate string BeforeSceneLoadHandler(string sceneName);

    /// <summary>
    /// Called whenever localization specific strings are requested
    /// </summary>
    /// <param name="key"></param>
    /// <param name="sheetTitle"></param>
    /// <returns>Localized Value</returns>
    public delegate string LanguageGetHandler(string key, string sheetTitle);

    /// <summary>
    /// Called whenever the hero updates
    /// </summary>
    public delegate void HeroUpdateHandler();

    /// <summary>
    /// Called after player values for charms have been set
    /// </summary>
    /// <param name="data">Current PlayerData</param>
    /// <param name="controller">Current HeroController</param>
    public delegate void CharmUpdateHandler(PlayerData data, HeroController controller);
    
    /// <summary>
    /// Called when the player attacks
    /// </summary>
    /// <param name="dir">Direction of the attack.</param>
    public delegate void AttackHandler(AttackDirection dir);

    /// <summary>
    /// Called at the end of the attack function
    /// </summary>
    /// <param name="dir">Direction of the attack.</param>
    public delegate void AfterAttackHandler(AttackDirection dir);

    /// <summary>
    /// Called during dash function to change velocity
    /// </summary>
    /// <returns>New vector for velocity</returns>
    public delegate Vector2 DashVelocityHandler();

    /// <summary>
    /// Called whenever the dash key is pressed. Overrides normal dash functionalit
    /// </summary>
    public delegate void DashPressedHandler();

    /// <summary>
    /// Called whenever a new gameobject is created with a collider and playmaker2d
    /// </summary>
    /// <param name="go"></param>
    public delegate void ColliderCreateHandler(GameObject go);
    
    
}
