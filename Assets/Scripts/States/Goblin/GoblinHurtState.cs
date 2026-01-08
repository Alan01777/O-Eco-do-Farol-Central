using Godot;

namespace EcoDoFarolCentral.States.Goblin
{
    public class GoblinHurtState : GoblinShadowState
    {
        private float _hurtTimer;

        public override void Enter()
        {
            Goblin.CurrentStateEnum = GoblinShadow.GoblinStates.Hurt;
            Goblin.AnimControllerInstance.Play("hurt", true);
            _hurtTimer = Goblin.HurtDuration;
        }

        public override void PhysicsUpdate(double delta)
        {
            Goblin.ApplyGravity(delta);

            // Apply friction or knockback decay if desired
            Vector2 v = Goblin.Velocity;
            v.X = Mathf.MoveToward(v.X, 0, Goblin.Speed * (float)delta); // Friction
            Goblin.Velocity = v;

            _hurtTimer -= (float)delta;
            if (_hurtTimer <= 0)
            {
                StateMachine.ChangeState("Flee"); // Usually flee after being hit
            }
        }
    }
}
