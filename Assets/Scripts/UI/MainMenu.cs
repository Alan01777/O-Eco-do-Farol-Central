using Godot;

namespace EcoDoFarolCentral
{
    public partial class MainMenu : Control
    {
        private Button _newGameButton;
        private Button _continueButton;
        private Button _creditsButton;
        private Button _quitButton;

        public override void _Ready()
        {
            // Obtém botões
            _newGameButton = GetNode<Button>("CanvasLayer/CenterContainer/VBoxContainer/NewGameButton");
            _continueButton = GetNode<Button>("CanvasLayer/CenterContainer/VBoxContainer/ContinueButton");
            _creditsButton = GetNode<Button>("CanvasLayer/CenterContainer/VBoxContainer/CreditsButton");
            _quitButton = GetNode<Button>("CanvasLayer/CenterContainer/VBoxContainer/QuitButton");

            // Conecta sinais
            _newGameButton.Pressed += OnNewGamePressed;
            _continueButton.Pressed += OnContinuePressed;
            _creditsButton.Pressed += OnCreditsPressed;
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
            }
        }

        private void OnNewGamePressed()
        {

            // Inicializa novo jogo via GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NewGame();
            }

            GetTree().ChangeSceneToFile("res://Scenes/UI/Intro.tscn");
        }

        private void OnContinuePressed()
        {

            // Carrega dados do save
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadGame();
            }

            GetTree().ChangeSceneToFile("res://Scenes/Levels/level_1.tscn");
        }

        private void OnCreditsPressed()
        {
            GetTree().ChangeSceneToFile("res://Scenes/UI/Credits.tscn");
        }

        private void OnQuitPressed()
        {
            GetTree().Quit();
        }
    }
}

