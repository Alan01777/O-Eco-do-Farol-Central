using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Classe base para estados do inimigo
    /// </summary>
    public abstract class EnemyState : BaseState
    {
        protected ShadowEnemy Enemy;

        public void Initialize(ShadowEnemy enemy, StateMachine stateMachine)
        {
            Enemy = enemy;
            base.Initialize(enemy, stateMachine);
        }
    }
}
