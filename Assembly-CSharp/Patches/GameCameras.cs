using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using MonoMod;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable all
#pragma warning disable 1591, 649, 414, 169, CS0108, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::GameCameras")]
    public class GameCameras : global::GameCameras
    {
        [MonoModIgnore]
        private static GameCameras _instance;

        public static GameCameras instance
        {
            get
            {
                if (GameCameras._instance == null)
                {
                    GameCameras._instance = UnityEngine.Object.FindObjectOfType<GameCameras>();
                    if (GameCameras._instance == null)
                    {
                        Debug.LogError("Couldn't find GameCameras, make sure one exists in the scene.");
                    }
                    else
                    {
                        UnityEngine.Object.DontDestroyOnLoad(GameCameras._instance.gameObject);
                    }
                }
                return GameCameras._instance;
            }
        }
    }
}