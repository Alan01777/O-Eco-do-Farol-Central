using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Gerencia o estado do jogo e coordena save/load
    /// </summary>
    public partial class GameManager : Node
    {
        // Instância Singleton
        public static GameManager Instance { get; private set; }

        // Dados do save atual
        public SaveData CurrentSave { get; private set; }

        // Referências para nodes importantes (setados quando a cena carrega)
        public Player CurrentPlayer { get; set; }

        // Rastreia se precisamos aplicar dados carregados
        private bool _saveLoadedPending = false;

        public override void _Ready()
        {
            Instance = this;
            CurrentSave = new SaveData();
        }

        /// <summary>
        /// Salva o jogo atual
        /// </summary>
        public bool SaveGame()
        {
            if (CurrentPlayer == null)
            {
                GD.PrintErr("[GAME MANAGER] Cannot save: Player reference is null");
                return false;
            }

            // Atualiza dados do save a partir do estado atual
            CurrentSave.PlayerPosition = CurrentPlayer.GlobalPosition;
            CurrentSave.CurrentHealth = CurrentPlayer.CurrentHealth;
            CurrentSave.MaxHealth = CurrentPlayer.MaxHealth;
            CurrentSave.CurrentScene = GetTree().CurrentScene.SceneFilePath;

            // Salva no disco
            return SaveSystem.SaveGame(CurrentSave);
        }

        /// <summary>
        /// Carrega o jogo e aplica à cena atual
        /// Chame isso após a cena ser carregada
        /// </summary>
        public void LoadGame()
        {
            CurrentSave = SaveSystem.LoadGame();
            _saveLoadedPending = true;
        }

        /// <summary>
        /// Aplica dados salvos se foram carregados (chamado depois que o player é registrado)
        /// </summary>
        public void ApplySaveDataIfLoaded()
        {
            if (_saveLoadedPending)
            {
                ApplySaveData();
                _saveLoadedPending = false;
            }
        }

        /// <summary>
        /// Aplica dados carregados para a cena atual
        /// </summary>
        private void ApplySaveData()
        {
            if (CurrentPlayer == null)
            {
                GD.PrintErr("[GAME MANAGER] Cannot apply save: Player reference is null");
                return;
            }

            // Aplica dados salvos ao player
            CurrentPlayer.GlobalPosition = CurrentSave.PlayerPosition;
            CurrentPlayer.CurrentHealth = CurrentSave.CurrentHealth;
            CurrentPlayer.MaxHealth = CurrentSave.MaxHealth;

            GD.Print($"[GAME MANAGER] Save data applied to player");
        }

        /// <summary>
        /// Inicia um novo jogo com dados limpos
        /// </summary>
        public void NewGame()
        {
            CurrentSave = new SaveData();
            GD.Print("[GAME MANAGER] New game started");
        }
    }
}
