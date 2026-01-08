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

        // Rastreia se precisar aplicar dados carregados
        private bool _saveLoadedPending = false;

        // Rastreia se tem dados de transição de cena pendentes
        private bool _sceneTransitionPending = false;

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

            // Salva habilidades desbloqueadas
            if (CurrentPlayer.Abilities != null)
            {
                CurrentSave.UnlockedAbilities = CurrentPlayer.Abilities.GetUnlockedAbilities();
                CurrentSave.MaxComboLevel = CurrentPlayer.Abilities.MaxComboLevel;
            }

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

            // Restaura habilidades
            if (CurrentPlayer.Abilities != null)
            {
                CurrentPlayer.Abilities.LoadAbilities(CurrentSave.UnlockedAbilities, CurrentSave.MaxComboLevel);
            }

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

        // --- Helpers para rastrear itens coletados ---

        public bool IsItemCollected(string id)
        {
            if (CurrentSave == null || CurrentSave.CollectedItems == null) return false;
            return CurrentSave.CollectedItems.Contains(id);
        }

        public void RegisterCollectedItem(string id)
        {
            if (CurrentSave == null) return;

            if (CurrentSave.CollectedItems == null)
                CurrentSave.CollectedItems = new System.Collections.Generic.List<string>();

            if (!CurrentSave.CollectedItems.Contains(id))
            {
                CurrentSave.CollectedItems.Add(id);
                // Opcional: Salvar imediatamente ao coletar itens importantes?
                // SaveGame(); 
                GD.Print($"[GAME MANAGER] Item collected registered: {id}");
            }
        }

        // --- Transição de cena ---

        /// <summary>
        /// Salva dados do player em memória antes de trocar de cena (sem salvar no disco)
        /// </summary>
        public void SavePlayerDataForTransition()
        {
            if (CurrentPlayer == null)
            {
                GD.PrintErr("[GAME MANAGER] Cannot save for transition: Player reference is null");
                return;
            }

            // Atualiza dados do save a partir do estado atual
            CurrentSave.CurrentHealth = CurrentPlayer.CurrentHealth;
            CurrentSave.MaxHealth = CurrentPlayer.MaxHealth;

            // Salva habilidades desbloqueadas
            if (CurrentPlayer.Abilities != null)
            {
                CurrentSave.UnlockedAbilities = CurrentPlayer.Abilities.GetUnlockedAbilities();
                CurrentSave.MaxComboLevel = CurrentPlayer.Abilities.MaxComboLevel;
            }

            _sceneTransitionPending = true;
            GD.Print("[GAME MANAGER] Player data saved for scene transition");
        }

        /// <summary>
        /// Aplica dados de transição ao player recém-carregado
        /// </summary>
        public void ApplyTransitionDataIfPending()
        {
            if (!_sceneTransitionPending) return;

            if (CurrentPlayer == null)
            {
                GD.PrintErr("[GAME MANAGER] Cannot apply transition data: Player reference is null");
                return;
            }

            // Aplica vida
            CurrentPlayer.CurrentHealth = CurrentSave.CurrentHealth;
            CurrentPlayer.MaxHealth = CurrentSave.MaxHealth;

            // Restaura habilidades
            if (CurrentPlayer.Abilities != null)
            {
                CurrentPlayer.Abilities.LoadAbilities(CurrentSave.UnlockedAbilities, CurrentSave.MaxComboLevel);
            }

            _sceneTransitionPending = false;
            GD.Print("[GAME MANAGER] Transition data applied to player");
        }

        /// <summary>
        /// Registra o player atual e aplica dados pendentes (chamado pelo Player._Ready)
        /// </summary>
        public void RegisterPlayer(Player player)
        {
            CurrentPlayer = player;
            GD.Print("[GAME MANAGER] Player registered");

            // Aplica dados de transição ou save carregado se houver
            ApplyTransitionDataIfPending();
            ApplySaveDataIfLoaded();
        }
    }
}
