using HutongGames.PlayMaker;
using MonoMod;

// ReSharper disable All
//Sticking this here because right now, we're not sold on the source thing.  But i want to do this to make my life easier.
#pragma warning disable 1591, 0108, 0169, 0649, 0414
namespace Modding.Patches.DRM
{
    [MonoModPatch("HutongGames.PlayMaker.Actions.GetEventSender")]
    public class GetEventSender : HutongGames.PlayMaker.Actions.GetEventSender
    {
        [MonoModReplace]
        public override void OnEnter()
        {
            if (Fsm.EventData.SentByFsm != null)
            {
                this.sentByGameObject.Value = Fsm.EventData.SentByFsm.GameObject;
            }
            else
            {
                this.sentByGameObject.Value = null;
            }
            if (this.sentByGameObject.Value != null)
                ModHooks.Instance.OnGetEventSender(sentByGameObject.Value, Fsm);

            base.Finish();
        }
    }
}
