using Godot;

namespace EcoDoFarolCentral
{
    public class ShadowEnemy2IdleState : ShadowEnemy2State
    {
        private float _idleTimer;
        private float _idleDuration = 1.0f;

        public override void Enter()
        {
            Enemy.CurrentStateEnum = ShadowEnemy2.EnemyStates.Idle;
            Enemy.PlayAnimation("idle");
            Enemy.Velocity = Vector2.Zero;
            _idleTimer = 0;
            _idleDuration = (float)GD.RandRange(0.5, 1.5);
        }

        public override void PhysicsUpdate(double delta)
        {
            Enemy.ApplyGravity(delta);

            _idleTimer += (float)delta;
            if (_idleTimer >= _idleDuration)
            {
                if (Enemy.IsPlayerInRange())
                {
                    StateMachine.ChangeState("Chase");
                }
                else
                {
                    // Se o player sumiu, reseta o timer mas continua em Idle
                    _idleTimer = 0;
                    _idleDuration = (float)GD.RandRange(0.5, 1.5);
                }
            }
        }
    }
}
