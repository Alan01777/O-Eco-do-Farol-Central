using Godot;
using Helpers;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Lamparina interativa que pode ser ativada pelo jogador.
    /// Quando ativada, acende a luz (PointLight2D) e opcionalmente toca uma animação.
    /// </summary>
    public partial class LightBulbActionable : Actionable
    {
        [Signal]
        public delegate void LightActivatedEventHandler();

        [ExportGroup("Light Configuration")]
        [Export] public PointLight2D LightNode;
        [Export] public float LightEnergy = 1.0f;
        [Export] public float LightScale = 1.0f;
        [Export] public Color LightColor = new Color(1.0f, 0.9f, 0.7f); // Cor amarelada de lamparina

        [ExportGroup("Animation")]
        [Export] public AnimationPlayer AnimPlayer;
        [Export] public string ActivateAnimation = "activate";

        [ExportGroup("Visual")]
        [Export] public Sprite2D SpriteNode; // Sprite com shader de highlight

        [ExportGroup("Persistence")]
        [Export] public string ID = "";

        private bool _isActivated = false;
        private ShaderMaterial _shaderMaterial;

        public override void _Ready()
        {
            // Verifica se já foi ativada (persistência)
            if (!string.IsNullOrEmpty(ID) && GameManager.Instance != null)
            {
                if (GameManager.Instance.IsItemCollected(ID))
                {
                    ActivateLight(playAnimation: false);
                }
            }

            // Desliga a luz inicialmente se não está ativada
            if (!_isActivated && LightNode != null)
            {
                LightNode.Enabled = false;
            }

            // Obtém referência do shader material do sprite
            if (SpriteNode != null && SpriteNode.Material is ShaderMaterial shader)
            {
                _shaderMaterial = shader;
                SetHighlightEnabled(false); // Começa desativado
            }

            // Conecta sinais de entrada/saída da área para detectar player
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
        }

        private void OnBodyEntered(Node2D body)
        {
            if (_isActivated) return;

            if (body is Player)
            {
                SetHighlightEnabled(true);
            }
        }

        private void OnBodyExited(Node2D body)
        {
            if (body is Player)
            {
                SetHighlightEnabled(false);
            }
        }

        private void SetHighlightEnabled(bool enabled)
        {
            if (_shaderMaterial != null)
            {
                _shaderMaterial.SetShaderParameter("enabled", enabled);
            }
        }

        /// <summary>
        /// Chamado pelo Player quando interage com a lamparina.
        /// </summary>
        public override void Action()
        {
            if (_isActivated)
            {
                return;
            }

            ActivateLight(playAnimation: true);
            SetHighlightEnabled(false); // Desliga highlight após ativar

            // Registra para persistência
            if (!string.IsNullOrEmpty(ID) && GameManager.Instance != null)
            {
                GameManager.Instance.RegisterCollectedItem(ID);
            }
        }

        private void ActivateLight(bool playAnimation = true)
        {
            _isActivated = true;

            // Ativa a luz
            if (LightNode != null)
            {
                LightNode.Enabled = true;
                LightNode.Energy = LightEnergy;
                LightNode.TextureScale = LightScale;
                LightNode.Color = LightColor;
            }
            else
            {
            }

            // Toca animação (áudio é controlado pelo AnimationPlayer)
            if (playAnimation && AnimPlayer != null && !string.IsNullOrEmpty(ActivateAnimation))
            {
                AnimPlayer.Play(ActivateAnimation);
            }

            // Emite sinal para notificar que a luz foi ativada
            EmitSignal(SignalName.LightActivated);
        }

        /// <summary>
        /// Desativa a lamparina (útil para debug ou reset)
        /// </summary>
        public void Deactivate()
        {
            _isActivated = false;
            if (LightNode != null)
            {
                LightNode.Enabled = false;
            }
        }

        /// <summary>
        /// Retorna se a lamparina está ativada
        /// </summary>
        public bool IsActivated => _isActivated;
    }
}
