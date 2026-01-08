using Godot;

namespace EcoDoFarolCentral
{
    public partial class PauseMenu : CanvasLayer
    {
        private Control _menuContainer;
        private Button _resumeButton;
        private Button _saveButton;
        private Button _restartButton;
        private Button _quitButton;

        private bool _isPaused = false;

        public override void _Ready()
        {
            // Obtém nodes da UI
            _menuContainer = GetNode<Control>("MenuContainer");
            _resumeButton = GetNode<Button>("MenuContainer/VBoxContainer/ResumeButton");
            _saveButton = GetNode<Button>("MenuContainer/VBoxContainer/SaveButton");
            _restartButton = GetNode<Button>("MenuContainer/VBoxContainer/RestartButton");
            _quitButton = GetNode<Button>("MenuContainer/VBoxContainer/QuitButton");

            // Conecta sinais dos botões
            _resumeButton.Pressed += OnResumePressed;
            _saveButton.Pressed += OnSavePressed;
            _restartButton.Pressed += OnRestartPressed;
            _quitButton.Pressed += OnQuitPressed;

            // Inicia escondido
            Hide();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel")) // Tecla ESC
            {
                TogglePause();
                GetViewport().SetInputAsHandled();
            }
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;

            if (_isPaused)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }

        private void Pause()
        {
            GetTree().Paused = true;
            Show();
            _resumeButton.GrabFocus(); // Auto-foco no botão de voltar
        }

        private void Resume()
        {
            GetTree().Paused = false;
            Hide();
        }

        private void OnResumePressed()
        {
            Resume();
            _isPaused = false;
        }

        private void OnSavePressed()
        {
            GD.Print("[PAUSE MENU] Saving game...");

            // Save via GameManager
            bool success = GameManager.Instance?.SaveGame() ?? false;

            if (success)
            {
                // Feedback visual
                _saveButton.Text = "✓ Saved!";
                GetTree().CreateTimer(1.5).Timeout += () => _saveButton.Text = "Save Game";
            }
            else
            {
                _saveButton.Text = "✗ Save Failed";
                GetTree().CreateTimer(1.5).Timeout += () => _saveButton.Text = "Save Game";
            }
        }

        private void OnRestartPressed()
        {
            GD.Print("[PAUSE MENU] Restarting game...");
            Resume();
            _isPaused = false;

            // Recarrega cena atual
            GetTree().ReloadCurrentScene();
        }

        private void OnQuitPressed()
        {
            GD.Print("[PAUSE MENU] Returning to main menu...");
            Resume();
            _isPaused = false;

            // Retorna ao menu principal
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        }
    }
}
