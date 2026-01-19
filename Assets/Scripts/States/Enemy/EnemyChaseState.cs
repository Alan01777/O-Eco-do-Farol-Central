using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Estado de perseguição do inimigo
    /// </summary>
    public class EnemyChaseState : EnemyState
    {
        public override void Enter()
        {
            Enemy.PlayChaseSound();
        }

        public override void PhysicsUpdate(double delta)
        {
            if (Enemy.CurrentStateEnum == ShadowEnemy.EnemyStates.Dead) return;

            if (Enemy.TargetPlayer == null)
            {
                StateMachine.ChangeState("Idle");
                return;
            }

            // Usa valores cacheados (atualizam 2x por segundo)
            float distance = Enemy.GetCachedDistanceToPlayer();

            // Se o inimigo está muito longe, volta para o estado Idle
            if (distance > Enemy.DetectionRange * 1.5f)
            {
                StateMachine.ChangeState("Idle");
                return;
            }

            // Se o inimigo está perto o suficiente para atacar, muda para o estado de ataque
            if (distance < Enemy.NextAttackRange && Enemy.CanAttack())
            {
                StateMachine.ChangeState("Attack");
                return;
            }

            Enemy.ApplyGravity(delta);
            // Usa direção cacheada (atualiza 2x por segundo)
            float direction = Enemy.GetCachedDirectionToPlayer();
            Enemy.MoveTowardsPlayer(direction);
        }
    }
}
