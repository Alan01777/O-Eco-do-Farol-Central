using Godot;

namespace EcoDoFarolCentral
{
    public class ShadowEnemy2HurtState : ShadowEnemy2State
    {
        private float _hurtTimer;

        public override void Enter()
        {
            Enemy.CurrentStateEnum = ShadowEnemy2.EnemyStates.Hurt;
            Enemy.FlashDamageEffect();
            _hurtTimer = Enemy.HurtDuration;
            Enemy.DisableAllHitBoxes();
        }

        public override void PhysicsUpdate(double delta)
        {
            Enemy.ApplyGravity(delta);
            // Aplica fricção no knockback
            Enemy.Velocity = Enemy.Velocity.Lerp(new Vector2(0, Enemy.Velocity.Y), (float)delta * 5f);

            _hurtTimer -= (float)delta;
            if (_hurtTimer <= 0)
            {
                StateMachine.ChangeState("Chase"); // Volta furioso
            }
        }
    }
}
