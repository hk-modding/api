using MonoMod;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 114, 0414,0162, CS0626, IDE1005, IDE1006

namespace Modding.Patches
{
    // These changes fix NREs that happen in this class when pre-processing scenes without a hero in them
    [MonoModPatch("global::CameraLockArea")]
    public class CameraLockArea : global::CameraLockArea
    {
        [MonoModIgnore]
        private SuppressPreloadException.GameCameras gcams;
        [MonoModIgnore]
        private CameraController cameraCtrl;
        [MonoModIgnore]
        private CameraTarget camTarget;
        [MonoModIgnore]
        private Collider2D box2d;
        [MonoModIgnore]
        private float leftSideX;
        [MonoModIgnore]
        private float rightSideX;
        [MonoModIgnore]
        private float topSideY;
        [MonoModIgnore]
        private float botSideY;
        [MonoModIgnore]
        private extern bool ValidateBounds();

        private IEnumerator Start()
        {
            gcams = SuppressPreloadException.GameCameras.instance;
            if (gcams == null)
                yield break;
            cameraCtrl = gcams.cameraController;
            camTarget = gcams.cameraTarget;
            Scene scene = gameObject.scene;
            if (cameraCtrl == null)
                yield break;
            while (cameraCtrl.tilemap == null || cameraCtrl.tilemap.gameObject.scene != scene)
            {
                yield return null;
            }
            if (!ValidateBounds())
            {
                Debug.LogError("Camera bounds are unspecified for " + name + ", please specify lock area bounds for this Camera Lock Area.");
            }
            if (box2d != null)
            {
                leftSideX = box2d.bounds.min.x;
                rightSideX = box2d.bounds.max.x;
                botSideY = box2d.bounds.min.y;
                topSideY = box2d.bounds.max.y;
            }
            yield break;
        }
    }
}