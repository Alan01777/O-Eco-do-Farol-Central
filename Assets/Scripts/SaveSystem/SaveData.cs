using Godot;
using System.Collections.Generic;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Estrutura de dados para informações de salvamento do jogo
    /// TODO: Salvar habilidades desbloqueadas, se coletáveis já foram coletados, etc.
    /// </summary>
    public partial class SaveData : GodotObject
    {
        // Dados do Jogador
        public Vector2 PlayerPosition { get; set; } = Vector2.Zero;
        public float CurrentHealth { get; set; } = 100f;
        public float MaxHealth { get; set; } = 100f;

        // Progresso do Jogo
        public string CurrentScene { get; set; } = "res://Scenes/game.tscn";

        // Metadados
        public string SaveDate { get; set; } = "";
        public float PlayTime { get; set; } = 0f;

        // TODO: salvar habilidades desbloqueadas
        public Dictionary<string, bool> UnlockedAbilities { get; set; }

        public SaveData()
        {
            SaveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Converte SaveData para Dictionary para serialização JSON
        /// </summary>
        public Dictionary<string, Variant> ToDictionary()
        {
            var dict = new Dictionary<string, Variant>
            {
                { "player_position_x", PlayerPosition.X },
                { "player_position_y", PlayerPosition.Y },
                { "current_health", CurrentHealth },
                { "max_health", MaxHealth },
                { "current_scene", CurrentScene },
                { "save_date", SaveDate },
                { "play_time", PlayTime }
            };

            return dict;
        }

        /// <summary>
        /// Cria SaveData a partir de Dictionary após deserialização JSON
        /// </summary>
        public static SaveData FromDictionary(Dictionary<string, Variant> dict)
        {
            var data = new SaveData
            {
                PlayerPosition = new Vector2(
                    dict.GetValueOrDefault("player_position_x", 0f).AsSingle(),
                    dict.GetValueOrDefault("player_position_y", 0f).AsSingle()
                ),
                CurrentHealth = dict.GetValueOrDefault("current_health", 100f).AsSingle(),
                MaxHealth = dict.GetValueOrDefault("max_health", 100f).AsSingle(),
                CurrentScene = dict.GetValueOrDefault("current_scene", "res://Scenes/game.tscn").AsString(),
                SaveDate = dict.GetValueOrDefault("save_date", "").AsString(),
                PlayTime = dict.GetValueOrDefault("play_time", 0f).AsSingle()
            };

            return data;
        }
    }
}
