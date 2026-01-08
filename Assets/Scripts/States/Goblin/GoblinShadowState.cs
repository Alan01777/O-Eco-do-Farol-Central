using Godot;

namespace EcoDoFarolCentral.States.Goblin
{
    public abstract class GoblinShadowState : BaseState
    {
        protected GoblinShadow Goblin;

        public void Initialize(GoblinShadow goblin, StateMachine stateMachine)
        {
            Goblin = goblin;
            base.Initialize(goblin, stateMachine);
        }
    }
}
