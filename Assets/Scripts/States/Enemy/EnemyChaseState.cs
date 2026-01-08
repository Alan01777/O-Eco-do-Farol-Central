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
            if (Enemy.TargetPlayer == null)
            {
                StateMachine.ChangeState("Idle");
                return;
            }

            float distance = Enemy.GlobalPosition.DistanceTo(Enemy.TargetPlayer.GlobalPosition);

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
            float direction = Mathf.Sign(Enemy.TargetPlayer.GlobalPosition.X - Enemy.GlobalPosition.X);
            Enemy.MoveTowardsPlayer(direction);
        }
    }
}
