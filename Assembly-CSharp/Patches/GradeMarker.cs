using MonoMod;
using System.Collections;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 114, 0414,0162, CS0626, IDE1005, IDE1006

namespace Modding.Patches
{
    // These changes fix NREs that happen in this class when pre-processing scenes without a hero in them
    [MonoModPatch("global::GradeMarker")]
    public class GradeMarker : global::GradeMarker
    {
        [MonoModIgnore]
        private HeroController hero;

        [MonoModIgnore]
        private SceneColorManager scm;

        IEnumerator startup = null;

        private extern void orig_Start();

        private void Start()
        {
            if (startup != null)
                StopCoroutine(startup);

            startup = OnStart();

            StartCoroutine(startup);
        }

        IEnumerator OnStart()
        {
            while (HeroController.instance == null)
                yield return null;

            orig_Start();
        }

        private extern void orig_Update();

        private void Update()
        {
            if (hero == null)
                return;

            orig_Update();
        }

        private extern void orig_UpdateLow();

        private void UpdateLow()
        {
            if (hero == null)
                return;

            orig_UpdateLow();
        }

        private extern void orig_Deactivate();

        public void Deactivate()
        {
            if (startup != null)
                StopCoroutine(startup);
            startup = null;
            if (scm == null)
            {
                this.enableGrade = false;
            }
            else
            {
                orig_Deactivate();
            }
        }
    }
}