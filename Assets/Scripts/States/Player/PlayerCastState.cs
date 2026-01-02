using Godot;
using System;

namespace EcoDoFarolCentral
{
    public class PlayerCastState : PlayerState
    {
        private bool _castTriggered = false;

        public override void Enter()
        {
            Player.CurrentStateEnum = Player.PlayerStates.Cast;
            Player.HandleMovement(0, 0.1f);
            _castTriggered = false;

            // Animação de conjuração
            Player.AnimControllerInstance.Play("special_1", true);
        }

        public override void PhysicsUpdate(double delta)
        {
            Player.ApplyGravity(delta);
            Player.HandleMovement(0, 0.1f);

            // Dispara no frame 7, evita problema de sincronização com a fireball
            if (!_castTriggered && Player.SpriteInstance.Frame >= 7)
            {
                _castTriggered = true;
                Player.CastFireball();
            }
        }

        public override void OnAnimationFinished()
        {
            // Fail-safe: se o check falhar, dispara mesmo assim
            if (!_castTriggered)
            {
                Player.CastFireball();
            }
            StateMachine.ChangeState("Idle");
        }
    }
}
