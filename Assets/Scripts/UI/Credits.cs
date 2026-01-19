using Godot;

namespace EcoDoFarolCentral
{
    public partial class Credits : Control
    {
        [Export] public float ScrollSpeed = 50f;
        [Export] public string MainMenuPath = "res://Scenes/UI/MainMenu.tscn";

        private VBoxContainer _creditsContainer;
        private float _startY;
        private bool _finished = false;

        public override void _Ready()
        {
            _creditsContainer = GetNode<VBoxContainer>("CreditsContainer");
            _startY = _creditsContainer.Position.Y;

            // Conecta botão de voltar
            var backButton = GetNodeOrNull<Button>("BackButton");
            if (backButton != null)
            {
                backButton.Pressed += OnBackPressed;
            }
        }

        public override void _Process(double delta)
        {
            if (_finished) return;

            // Rola os créditos para cima
            _creditsContainer.Position -= new Vector2(0, ScrollSpeed * (float)delta);

            // Verifica se terminou
            if (_creditsContainer.Position.Y < -_creditsContainer.Size.Y)
            {
                _finished = true;
                // Volta ao menu automaticamente após um delay
                GetTree().CreateTimer(2.0).Timeout += () => GoToMainMenu();
            }
        }

        public override void _Input(InputEvent @event)
        {
            // Permite pular créditos com qualquer tecla
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                GoToMainMenu();
            }
            else if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                GoToMainMenu();
            }
        }

        private void OnBackPressed()
        {
            GoToMainMenu();
        }

        private void GoToMainMenu()
        {
            GetTree().ChangeSceneToFile(MainMenuPath);
        }
    }
}
