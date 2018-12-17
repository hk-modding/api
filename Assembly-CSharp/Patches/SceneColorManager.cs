using MonoMod;
// ReSharper disable All
//Sticking this here because right now, we're not sold on the source thing.  But i want to do this to make my life easier.
#pragma warning disable 1591, 0108, 0169, 0649, 0414
namespace Modding.Patches
{
    [MonoModPatch( "global::SceneColorManager" )]
    public class SceneColorManager : global::SceneColorManager
    {
        [MonoModIgnore]
        private bool gameplayScene;

        [MonoModOriginalName( "UpdateScriptParameters" )]
        private void orig_UpdateScriptParameters() { }
        
        //Added checks for null and an attempt to fix any missing references
        private void UpdateScriptParameters()
        {
            if( this.gameplayScene )
            {
                if( HeroController.instance == null || HeroController.instance.heroLight == null )
                {
                    return;
                }
            }

            orig_UpdateScriptParameters();
        }
    }
}
