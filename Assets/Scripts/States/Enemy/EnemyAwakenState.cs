using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Estado de despertar (transição Sleep -> Idle/Chase)
    /// </summary>
    public class EnemyAwakenState : EnemyState
    {
        private bool _isAwakening = false;

        public override void Enter()
        {
            Enemy.CurrentStateEnum = ShadowEnemy.EnemyStates.Awakening;
            Enemy.PlayAnimation("awakening", true);
            _isAwakening = true;
        }

        public override void OnAnimationFinished()
        {
            if (_isAwakening)
            {
                _isAwakening = false;
                StateMachine.ChangeState("Idle");
            }
        }
    }
}
