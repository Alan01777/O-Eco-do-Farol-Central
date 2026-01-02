using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Estado de dano do inimigo (knockback/stun)
    /// </summary>
    public class EnemyHurtState : EnemyState
    {
        private float _hurtTimer;

        public override void Enter()
        {
            Enemy.CurrentStateEnum = ShadowEnemy.EnemyStates.Hurt;
            Enemy.PlayAnimation("hurt");
            Enemy.DisableAllHitBoxes();
            _hurtTimer = Enemy.HurtDuration;
        }

        public override void PhysicsUpdate(double delta)
        {
            _hurtTimer -= (float)delta;
            Enemy.ApplyGravity(delta);

            if (_hurtTimer <= 0)
            {
                if (Enemy.IsPlayerInRange())
                    StateMachine.ChangeState("Chase");
                else
                    StateMachine.ChangeState("Idle");
            }
        }
    }
}
