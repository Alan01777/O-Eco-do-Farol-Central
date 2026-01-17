using Godot;

namespace EcoDoFarolCentral
{
    public partial class PauseMenu : CanvasLayer
    {
        private Control _menuContainer;
        private Button _resumeButton;
        private Button _restartButton;
        private Button _quitButton;

        private bool _isPaused = false;

        public override void _Ready()
        {
            // Obtém nodes da UI
            _menuContainer = GetNode<Control>("MenuContainer");
            _resumeButton = GetNode<Button>("MenuContainer/VBoxContainer/ResumeButton");
            _restartButton = GetNode<Button>("MenuContainer/VBoxContainer/RestartButton");
            _quitButton = GetNode<Button>("MenuContainer/VBoxContainer/QuitButton");

            // Conecta sinais dos botões
            _resumeButton.Pressed += OnResumePressed;
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



        private void OnRestartPressed()
        {
            GD.Print("[PAUSE MENU] Loading last checkpoint...");
            Resume();
            _isPaused = false;

            // Carrega o último checkpoint salvo
            if (SaveSystem.SaveFileExists())
            {
                GameManager.Instance?.LoadGame();

                // Troca para a cena salva (aplica os dados automaticamente via GameManager)
                var savedScene = GameManager.Instance?.CurrentSave?.CurrentScene;
                if (!string.IsNullOrEmpty(savedScene))
                {
                    GetTree().ChangeSceneToFile(savedScene);
                }
                else
                {
                    // Fallback: recarrega cena atual se não houver cena salva
                    GetTree().ReloadCurrentScene();
                }
            }
            else
            {
                // Sem save disponível, apenas recarrega a cena
                GetTree().ReloadCurrentScene();
            }
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
