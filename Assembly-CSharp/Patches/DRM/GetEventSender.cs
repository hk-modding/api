using HutongGames.PlayMaker;
using MonoMod;

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
                ModHooks.Instance.OnGetEventSender(sentByGameObject.Value);

            base.Finish();
        }
    }
}
