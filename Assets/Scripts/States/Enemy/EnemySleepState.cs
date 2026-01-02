using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Estado dormindo (inativo até player chegar perto)
    /// </summary>
    public class EnemySleepState : EnemyState
    {
        public override void Enter()
        {
            Enemy.CurrentStateEnum = ShadowEnemy.EnemyStates.Sleeping;
            Enemy.ResetToSleep();
        }

        public override void PhysicsUpdate(double delta)
        {
            if (Enemy.IsPlayerInRange())
            {
                StateMachine.ChangeState("Awaken");
            }
        }
    }
}
