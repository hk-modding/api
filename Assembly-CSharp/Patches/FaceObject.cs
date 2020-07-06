using HutongGames.PlayMaker;
using MonoMod;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0414, CS0626

namespace Modding.Patches
{
    [MonoModPatch("HutongGames.PlayMaker.Actions.FaceObject")]
    public class FaceObject : HutongGames.PlayMaker.Actions.FaceObject
    {
        [MonoModIgnore]
        public FsmGameObject objectA;

        [MonoModIgnore]
        public FsmGameObject objectB;

        private extern void orig_DoFace();

        // Added checks for null and an attempt to fix any missing references
        // as well as a try/catch in case something goes wrong to keep the whole FSM from breaking down...
        private void DoFace()
        {
            try
            {
                if (objectA == null || objectA.Value == null)
                {
                    objectA = new FsmGameObject(Fsm.GameObject);
                }

                if ((objectB == null || objectB.Value == null) && HeroController.instance != null)
                {
                    objectB = new FsmGameObject(HeroController.instance?.proxyFSM.Fsm.GameObject);
                }

                if ((objectA == null || objectA.Value == null) || (objectB == null || objectA.Value == null))
                {
                    Finish();
                    return;
                }

                orig_DoFace();
            }
            catch
            {
                Finish();
            }
        }
    }
}