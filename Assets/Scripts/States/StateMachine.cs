using Godot;
using System.Collections.Generic;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Classe que gerencia os estados do personagem
    /// </summary>
    public partial class StateMachine
    {
        public BaseState CurrentState { get; private set; }
        private readonly Dictionary<string, BaseState> _states = new();

        public void AddState(string name, BaseState state)
        {
            _states[name] = state;
        }

        public void ChangeState(string newStateName)
        {
            if (!_states.ContainsKey(newStateName))
            {
                GD.PrintErr($"[StateMachine] State '{newStateName}' not found!");
                return;
            }

            CurrentState?.Exit();
            CurrentState = _states[newStateName];
            CurrentState.Enter();
        }

        public void Update(double delta) => CurrentState?.Update(delta);
        public void PhysicsUpdate(double delta) => CurrentState?.PhysicsUpdate(delta);
        public void HandleInput(InputEvent @event) => CurrentState?.HandleInput(@event);
        public void OnAnimationFinished() => CurrentState?.OnAnimationFinished();
    }
}
