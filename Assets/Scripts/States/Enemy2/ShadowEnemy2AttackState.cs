using Godot;

namespace EcoDoFarolCentral
{
    public class ShadowEnemy2AttackState : ShadowEnemy2State
    {
        public override void Enter()
        {
            if (Enemy.CurrentStateEnum == ShadowEnemy2.EnemyStates.Dead) return;
            Enemy.CurrentStateEnum = ShadowEnemy2.EnemyStates.Attacking;
            Enemy.Velocity = Vector2.Zero;
            Enemy.ExecuteAttack();
        }

        public override void OnAnimationFinished()
        {
            if (Enemy.CurrentStateEnum == ShadowEnemy2.EnemyStates.Dead) return;
            Enemy.FinishAttack();
            StateMachine.ChangeState("Idle"); // Volta pra idle pra ter chance de reavaliar (Chase ou Sleep)
        }
    }
}
