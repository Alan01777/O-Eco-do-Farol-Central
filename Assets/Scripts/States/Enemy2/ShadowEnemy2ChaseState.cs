using Godot;

namespace EcoDoFarolCentral
{
    public class ShadowEnemy2ChaseState : ShadowEnemy2State
    {
        public override void Enter()
        {
            if (Enemy.CurrentStateEnum == ShadowEnemy2.EnemyStates.Dead) return;
            Enemy.CurrentStateEnum = ShadowEnemy2.EnemyStates.Chasing;
            Enemy.PlayChaseSound();
        }

        public override void PhysicsUpdate(double delta)
        {
            if (Enemy.CurrentStateEnum == ShadowEnemy2.EnemyStates.Dead) return;

            if (Enemy.TargetPlayer == null)
            {
                StateMachine.ChangeState("Idle");
                return;
            }

            // Usa valores cacheados (atualizam 2x por segundo)
            float distance = Enemy.GetCachedDistanceToPlayer();
            float direction = Enemy.GetCachedDirectionToPlayer();

            // Checa range do ataque único
            float attackRange = Enemy.AttackRange;

            if (distance <= attackRange && Enemy.CanAttack())
            {
                Enemy.Velocity = Vector2.Zero;
                StateMachine.ChangeState("Attack");
            }
            else if (distance > Enemy.DetectionRange * 1.5f) // Perdeu o player
            {
                StateMachine.ChangeState("Idle");
            }
            else
            {
                Enemy.MoveTowardsPlayer(direction);
                Enemy.ApplyGravity(delta);
            }
        }
    }
}
