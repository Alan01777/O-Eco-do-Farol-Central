using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Tela de Game Over
    /// </summary>
    public partial class GameOverScreen : CanvasLayer
    {
        private Button _retryButton;
        private Button _mainMenuButton;
        private Label _titleLabel;
        private ColorRect _background;

        public override void _Ready()
        {
            Layer = 100;
            ProcessMode = ProcessModeEnum.Always;

            _background = GetNode<ColorRect>("Background");
            _titleLabel = GetNode<Label>("Background/CenterContainer/VBox/Title");
            _retryButton = GetNode<Button>("Background/CenterContainer/VBox/RetryButton");
            _mainMenuButton = GetNode<Button>("Background/CenterContainer/VBox/MainMenuButton");

            _retryButton.Pressed += OnRetryPressed;
            _mainMenuButton.Pressed += OnMainMenuPressed;

            // Pausa o jogo
            GetTree().Paused = true;

            // Animação de entrada
            PlayEnterAnimation();
        }

        private void PlayEnterAnimation()
        {
            _background.Modulate = new Color(1, 1, 1, 0);

            var tween = CreateTween();
            tween.SetPauseMode(Tween.TweenPauseMode.Process);
            tween.TweenProperty(_background, "modulate:a", 1f, 0.5f);

            // Shake no título
            tween.TweenCallback(Callable.From(() => _retryButton.GrabFocus()));
        }

        private void OnRetryPressed()
        {
            GD.Print("[GAME OVER] Retry pressed");
            GetTree().Paused = false;

            // Remove o modal antes de recarregar
            QueueFree();
            
            GameManager.Instance.LoadGame();
        }

        private void OnMainMenuPressed()
        {
            GD.Print("[GAME OVER] Returning to main menu");
            GetTree().Paused = false;

            // Remove o modal antes de trocar de cena
            QueueFree();

            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        }

        /// <summary>
        /// Método estático para exibir a tela de game over.
        /// </summary>
        public static void ShowGameOver()
        {
            var scene = GD.Load<PackedScene>("res://Scenes/UI/GameOverScreen.tscn");
            if (scene == null)
            {
                GD.PrintErr("[GAME OVER] GameOverScreen.tscn not found!");
                return;
            }

            var gameOver = scene.Instantiate<GameOverScreen>();

            // Adiciona à árvore de cena
            var tree = Engine.GetMainLoop() as SceneTree;
            tree?.Root.AddChild(gameOver);
        }
    }
}
