using Godot;

namespace EcoDoFarolCentral
{
    public class EnemyAttackState : EnemyState
    {
        private bool _isAttacking = false;

        public override void Enter()
        {
            if (Enemy.CurrentStateEnum == ShadowEnemy.EnemyStates.Dead) return;
            Enemy.CurrentStateEnum = ShadowEnemy.EnemyStates.Attacking;
            Enemy.ExecuteCurrentAttack();
            _isAttacking = true;
        }

        public override void PhysicsUpdate(double delta)
        {
            if (Enemy.CurrentStateEnum == ShadowEnemy.EnemyStates.Dead) return;
            Enemy.ApplyGravity(delta);
            // Opcional: reduz velocidade de movimento durante ataque
            Enemy.Velocity = new Vector2(Enemy.Velocity.X * 0.9f, Enemy.Velocity.Y);
        }

        public override void OnAnimationFinished()
        {
            if (Enemy.CurrentStateEnum == ShadowEnemy.EnemyStates.Dead) return;
            if (_isAttacking)
            {
                _isAttacking = false;
                Enemy.FinishAttack();
                StateMachine.ChangeState("Idle");
            }
        }

        public override void Exit()
        {
            Enemy.FinishAttack();
        }
    }
}
