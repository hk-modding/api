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
            HitInstance hit = new()
			{
                Source = base.Owner,
                AttackType = (AttackTypes) AttackType.Value,
                CircleDirection = CircleDirection.Value,
                DamageDealt = DamageDealt.Value,
                Direction = Direction.Value,
                IgnoreInvulnerable = IgnoreInvulnerable.Value,
                MagnitudeMultiplier = MagnitudeMultiplier.Value,
                MoveAngle = MoveAngle.Value,
                MoveDirection = MoveDirection.Value,
                Multiplier = ((!Multiplier.IsNone) ? Multiplier.Value : 1f),
                SpecialType = (SpecialTypes) SpecialType.Value,
                IsExtraDamage = false
            };
            hit = ModHooks.OnHitInstanceBeforeHit(Fsm, hit);
            HitTaker.Hit(Target.Value, hit, 3);
            base.Finish();
        }
    }
}