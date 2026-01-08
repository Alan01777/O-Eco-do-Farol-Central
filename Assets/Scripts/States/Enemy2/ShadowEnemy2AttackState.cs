using Godot;

namespace EcoDoFarolCentral
{
    public class ShadowEnemy2AttackState : ShadowEnemy2State
    {
        public override void Enter()
        {
            Enemy.CurrentStateEnum = ShadowEnemy2.EnemyStates.Attacking;
            Enemy.Velocity = Vector2.Zero;
            Enemy.ExecuteAttack();
        }

        public override void OnAnimationFinished()
        {
            Enemy.FinishAttack();
            StateMachine.ChangeState("Idle"); // Volta pra idle pra ter chance de reavaliar (Chase ou Sleep)
        }
    }
}
