namespace EcoDoFarolCentral
{
    /// <summary>
    /// Classe base para estados do player
    /// </summary>
    public abstract partial class PlayerState : BaseState
    {
        protected Player Player => (Player)Actor;
    }
}
