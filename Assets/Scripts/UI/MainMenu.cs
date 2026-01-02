using Godot;

namespace EcoDoFarolCentral
{
    public partial class MainMenu : Control
    {
        private Button _newGameButton;
        private Button _continueButton;
        private Button _quitButton;

        public override void _Ready()
        {
            // Obtém botões
            _newGameButton = GetNode<Button>("CenterContainer/VBoxContainer/NewGameButton");
            _continueButton = GetNode<Button>("CenterContainer/VBoxContainer/ContinueButton");
            _quitButton = GetNode<Button>("CenterContainer/VBoxContainer/QuitButton");

            // Conecta sinais
            _newGameButton.Pressed += OnNewGamePressed;
            _continueButton.Pressed += OnContinuePressed;
            _quitButton.Pressed += OnQuitPressed;

            // Verifica se existe save
            CheckSaveFile();

            // Auto-foco no primeiro botão
            _newGameButton.GrabFocus();
        }

        private void CheckSaveFile()
        {
            bool hasSaveFile = SaveSystem.SaveFileExists();
            _continueButton.Disabled = !hasSaveFile;

            if (hasSaveFile)
            {
                GD.Print("[MAIN MENU] Save file detected");
            }
        }

        private void OnNewGamePressed()
        {
            GD.Print("[MAIN MENU] Starting new game...");

            // Inicializa novo jogo via GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NewGame();
            }

            GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
        }

        private void OnContinuePressed()
        {
            GD.Print("[MAIN MENU] Loading saved game...");

            // Carrega dados do save
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadGame();
            }

            GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
        }

        private void OnQuitPressed()
        {
            GD.Print("[MAIN MENU] Quitting game...");
            GetTree().Quit();
        }
    }
}
