using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Modal que exibe informações do power-up coletado e pausa o jogo.
    /// </summary>
    public partial class PowerUpModal : CanvasLayer
    {
        private TextureRect _iconRect;
        private Label _nameLabel;
        private Label _descriptionLabel;
        private Label _continueLabel;
        private ColorRect _background;
        private PanelContainer _panel;
        private bool _canClose = false;

        public override void _Ready()
        {
            // Define o layer alto para ficar sobre tudo
            Layer = 100;

            // Pega referências dos nodes
            _background = GetNode<ColorRect>("Background");
            _panel = GetNode<PanelContainer>("Background/Panel");
            _iconRect = GetNode<TextureRect>("Background/Panel/VBox/Icon");
            _nameLabel = GetNode<Label>("Background/Panel/VBox/Name");
            _descriptionLabel = GetNode<Label>("Background/Panel/VBox/Description");
            _continueLabel = GetNode<Label>("Background/Panel/VBox/Continue");

            // Configura para processar mesmo pausado
            ProcessMode = ProcessModeEnum.Always;

            // Pausa o jogo
            GetTree().Paused = true;

            // Animação de entrada
            PlayEnterAnimation();
        }

        private void PlayEnterAnimation()
        {
            // Começa invisível
            _background.Modulate = new Color(1, 1, 1, 0);
            _panel.Scale = Vector2.One * 0.8f;

            var tween = CreateTween();
            tween.SetPauseMode(Tween.TweenPauseMode.Process); // Funciona mesmo pausado

            // Fade in do background
            tween.TweenProperty(_background, "modulate:a", 1f, 0.3f);

            // Scale up do painel
            tween.Parallel().TweenProperty(_panel, "scale", Vector2.One, 0.3f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);

            // Permite fechar após a animação
            tween.TweenCallback(Callable.From(() => _canClose = true));
        }

        public override void _Input(InputEvent @event)
        {
            if (!_canClose) return;

            // Fecha com qualquer tecla ou clique
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                Close();
            }
            else if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                Close();
            }
            else if (@event is InputEventJoypadButton joyEvent && joyEvent.Pressed)
            {
                Close();
            }
        }

        private void Close()
        {
            _canClose = false;

            var tween = CreateTween();
            tween.SetPauseMode(Tween.TweenPauseMode.Process);

            // Fade out
            tween.TweenProperty(_background, "modulate:a", 0f, 0.2f);
            tween.Parallel().TweenProperty(_panel, "scale", Vector2.One * 0.8f, 0.2f);

            // Despausa e remove
            tween.TweenCallback(Callable.From(() =>
            {
                GetTree().Paused = false;
                QueueFree();
            }));
        }

        /// <summary>
        /// Configura o modal com as informações do power-up.
        /// </summary>
        public void Setup(Texture2D icon, string itemName, string description)
        {
            // Aguarda os nodes estarem prontos
            if (!IsNodeReady())
            {
                Ready += () => SetupInternal(icon, itemName, description);
            }
            else
            {
                SetupInternal(icon, itemName, description);
            }
        }

        private void SetupInternal(Texture2D icon, string itemName, string description)
        {
            if (icon != null)
            {
                _iconRect.Texture = icon;
                _iconRect.Visible = true;
            }
            else
            {
                _iconRect.Visible = false;
            }

            _nameLabel.Text = itemName;
            _descriptionLabel.Text = description;
        }
    }
}
