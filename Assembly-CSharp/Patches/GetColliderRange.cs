using HutongGames.PlayMaker;
using MonoMod;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0414, CS0626

namespace Modding.Patches
{
    [MonoModPatch("HutongGames.PlayMaker.Actions.GetColliderRange")]
    public class GetColliderRange : HutongGames.PlayMaker.Actions.GetColliderRange
    {
        [MonoModIgnore]
        public FsmOwnerDefault gameObject;

        private extern void orig_GetRange();

        //Added checks for null and an attempt to fix any missing references
        //as well as a try/catch in case something goes wrong to keep the whole FSM from breaking down...
        private void GetRange()
        {
            try
            {
                if (gameObject == null || gameObject.GameObject == null || gameObject.GameObject.Value == null)
                {
                    if (gameObject == null)
                    {
                        gameObject = new HutongGames.PlayMaker.FsmOwnerDefault();
                        gameObject.OwnerOption = HutongGames.PlayMaker.OwnerDefaultOption.UseOwner;
                    }

                    gameObject.GameObject = new HutongGames.PlayMaker.FsmGameObject(Fsm.GameObject);
                }

                if ((gameObject == null || gameObject.GameObject == null || gameObject.GameObject.Value == null))
                {
                    base.Finish();
                    return;
                }

                orig_GetRange();
            }
            catch (System.Exception ex)
            {
                Logger.APILogger.LogError(ex);
                base.Finish();
            }
        }
    }
}