using GlobalEnums;
using UnityEngine;

namespace Modding
{
    public delegate void ApplicationQuitHandler();

    public delegate void BeforeSavegameSaveHandler(SaveGameData data);
	public delegate void SavegameSaveHandler(int id);
    public delegate void SavegameLoadHandler(int id);
    public delegate void AfterSavegameLoadHandler(SaveGameData data);
    public delegate void ClearSaveGameHandler(int save);
    public delegate void NewGameHandler();
    public delegate void NewPlayerDataHandler(PlayerData data);

    public delegate void SceneChangedHandler(string targetScene);
    public delegate string BeforeSceneLoadHandler(string sceneName);
    public delegate string LanguageGetHandler(string key, string sheetTitle);

    public delegate void HeroUpdateHandler();
    public delegate void CharmUpdateHandler(PlayerData data, HeroController controller);
    
    public delegate void AttackHandler(AttackDirection dir);
    public delegate void AfterAttackHandler(AttackDirection dir);

    public delegate Vector2 DashVelocityHandler();
    public delegate void DashPressedHandler();
    public delegate void ColliderCreateHandler(GameObject go);
    
    
}
