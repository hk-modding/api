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
        private bool hasGotRefs;

        [MonoModReplace]
        private void GetRefs()
        {
            if (this.hasGotRefs)
            {
                return;
            }
            this.gcams = Modding.Patches.SuppressPreloadException.GameCameras.instance;
            if (this.gcams == null)
            {
                return;
            }
            this.cameraCtrl = this.gcams.cameraController;
            this.camTarget = this.gcams.cameraTarget;
            this.hasGotRefs = true;
        }

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

        [MonoModReplace]
        private IEnumerator Start()
        {
            this.GetRefs();
            if (!this.hasGotRefs)
                yield break;
            Scene scene = this.gameObject.scene;
            if (this.cameraCtrl == null)
                yield break;
            while (this.cameraCtrl.tilemap == null || this.cameraCtrl.tilemap.gameObject.scene != scene)
            {
                yield return null;
            }
            if (!this.ValidateBounds())
            {
                Debug.LogError("Camera bounds are unspecified for " + this.name + ", please specify lock area bounds for this Camera Lock Area.");
            }
            if (this.box2d != null)
            {
                this.leftSideX = this.box2d.bounds.min.x;
                this.rightSideX = this.box2d.bounds.max.x;
                this.botSideY = this.box2d.bounds.min.y;
                this.topSideY = this.box2d.bounds.max.y;
            }
            yield break;
        }
    }
}