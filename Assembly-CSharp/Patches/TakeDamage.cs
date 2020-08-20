using MonoMod;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0414

namespace Modding.Patches
{
    [MonoModPatch("HutongGames.PlayMaker.Actions.TakeDamage")]
    public class TakeDamage : HutongGames.PlayMaker.Actions.TakeDamage
    {
        [MonoModReplace]
        public override void OnEnter()
        {
            HitInstance hit = new HitInstance
            {
                Source = base.Owner,
                AttackType = (AttackTypes) this.AttackType.Value,
                CircleDirection = this.CircleDirection.Value,
                DamageDealt = this.DamageDealt.Value,
                Direction = this.Direction.Value,
                IgnoreInvulnerable = this.IgnoreInvulnerable.Value,
                MagnitudeMultiplier = this.MagnitudeMultiplier.Value,
                MoveAngle = this.MoveAngle.Value,
                MoveDirection = this.MoveDirection.Value,
                Multiplier = ((!this.Multiplier.IsNone) ? this.Multiplier.Value : 1f),
                SpecialType = (SpecialTypes) this.SpecialType.Value,
                IsExtraDamage = false
            };
            hit = ModHooks.Instance.OnHitInstanceBeforeHit(this.Fsm, hit);
            HitTaker.Hit(this.Target.Value, hit, 3);
            base.Finish();
        }
    }
}