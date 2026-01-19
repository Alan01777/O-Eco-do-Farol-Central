using Godot;
using EcoDoFarolCentral;

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
                return;
            }

            _isActivated = true;

            // Toca animação local se configurada
            if (LighthouseAnimationPlayer != null)
            {
                LighthouseAnimationPlayer.Play(ActivateAnimation);
            }

            // Notifica o Level3 para iniciar sequência de final
            var level3 = GetTree().CurrentScene as Level3;
            if (level3 != null)
            {
                level3.ActivateLighthouse();
            }
            else
            {
            }
        }
    }
}

