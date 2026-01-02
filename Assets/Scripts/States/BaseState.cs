using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Classe base para estados do personagem
    /// </summary>
    public abstract partial class BaseState
    {
        protected Actor Actor;
        protected StateMachine StateMachine;

        public void Initialize(Actor actor, StateMachine stateMachine)
        {
            Actor = actor;
            StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update(double delta) { }
        public virtual void PhysicsUpdate(double delta) { }
        public virtual void HandleInput(InputEvent @event) { }
        public virtual void OnAnimationFinished() { }
    }
}
