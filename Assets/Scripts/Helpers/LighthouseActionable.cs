using Godot;

namespace Helpers
{
    /// <summary>
    /// Actionable customizado para ativar o farol quando o player interage.
    /// Estende Actionable para funcionar com o sistema de interação existente do Player.
    /// </summary>
    public partial class LighthouseActionable : Actionable
    {
        [ExportGroup("Lighthouse")]
        [Export] public AnimationPlayer LighthouseAnimationPlayer;
        [Export] public string ActivateAnimation = "ligar_farol";

        private bool _isActivated = false;

        /// <summary>
        /// Sobrescreve o método Action() do Actionable.
        /// Chamado automaticamente pelo Player quando ele interage.
        /// </summary>
        public override void Action()
        {
            if (_isActivated)
            {
                GD.Print("[LIGHTHOUSE] Farol já está ativado!");
                return;
            }

            GD.Print("[LIGHTHOUSE] Ativando o farol...");

            if (LighthouseAnimationPlayer != null)
            {
                LighthouseAnimationPlayer.Play(ActivateAnimation);
                _isActivated = true;
                GD.Print("[LIGHTHOUSE] Farol ativado com sucesso!");
            }
            else
            {
                GD.PrintErr("[LIGHTHOUSE] AnimationPlayer não configurado!");
            }
        }
    }
}
