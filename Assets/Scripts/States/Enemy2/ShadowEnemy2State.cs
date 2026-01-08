using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Classe base para estados do ShadowEnemy2
    /// </summary>
    public abstract class ShadowEnemy2State : BaseState
    {
        protected ShadowEnemy2 Enemy;

        public void Initialize(ShadowEnemy2 enemy, StateMachine stateMachine)
        {
            Enemy = enemy;
            base.Initialize(enemy, stateMachine);
        }
    }
}
