using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Estado ocioso do inimigo (pode dormir ou esperar)
    /// </summary>
    public class EnemyIdleState : EnemyState
    {
        public override void Enter()
        {
            Enemy.CurrentStateEnum = ShadowEnemy.EnemyStates.Idle;
            Enemy.PlayAnimation("idle");
            Enemy.StartSleepTimer();
        }

        public override void PhysicsUpdate(double delta)
        {
            if (Enemy.IsPlayerInRange())
            {
                StateMachine.ChangeState("Chase");
                return;
            }

            Enemy.ApplyGravity(delta);
            Enemy.Velocity = new Vector2(0, Enemy.Velocity.Y);
        }

        public override void Exit()
        {
            Enemy.StopSleepTimer();
        }
    }
}
