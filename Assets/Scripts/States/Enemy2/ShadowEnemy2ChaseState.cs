using Godot;

namespace EcoDoFarolCentral
{
    public class ShadowEnemy2ChaseState : ShadowEnemy2State
    {
        public override void Enter()
        {
            Enemy.CurrentStateEnum = ShadowEnemy2.EnemyStates.Chasing;
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
            float direction = Mathf.Sign(Enemy.TargetPlayer.GlobalPosition.X - Enemy.GlobalPosition.X);

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
