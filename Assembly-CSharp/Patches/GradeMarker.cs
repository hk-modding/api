using MonoMod;
using UnityEngine;
using System.Collections;
//We disable a bunch of warnings here because they don't mean anything.  They all relate to not finding proper stuff for methods/properties/fields that are stubs to make the new methods work.
//We don't care about XML docs for these as they are being patched into the original code
// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 114, 0414,0162, IDE1005, IDE1006
namespace Modding.Patches
{
    //these changes fix NREs that happen in this class when pre-processing scenes without a hero in them

    [MonoModPatch( "global::GradeMarker" )]
    public class GradeMarker : global::GradeMarker
    {
        [MonoModIgnore] private HeroController hero;
        [MonoModIgnore] private SceneColorManager scm;

        IEnumerator startup = null;

        private void orig_Start() { }
        private void Start()
        {
            if( startup != null )
                StopCoroutine( startup );
            startup = OnStart();
            StartCoroutine( startup );
        }

        IEnumerator OnStart()
        {
            while( HeroController.instance == null )
                yield return null;
            orig_Start();
        }

        private void orig_Update() { }
        private void Update()
        {
            if( hero == null )
                return;
            orig_Update();
        }

        private void orig_UpdateLow() { }
        private void UpdateLow()
        {
            if( hero == null )
                return;
            orig_UpdateLow();
        }

        private void orig_Deactivate() { }
        public void Deactivate()
        {
            if( startup != null )
                StopCoroutine( startup );
            startup = null;
            if( scm == null )
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