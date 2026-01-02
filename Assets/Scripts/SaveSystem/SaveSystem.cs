using Godot;
using System.Collections.Generic;
using Godot.Collections;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Gerencia a persistência do jogo usando arquivos JSON
    /// </summary>
    public static class SaveSystem
    {
        private const string SAVE_PATH = "user://savegame.save";

        /// <summary>
        /// Salva o estado do jogo no disco
        /// </summary>
        public static bool SaveGame(SaveData data)
        {
            try
            {
                var saveFile = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);

                if (saveFile == null)
                {
                    GD.PrintErr($"[SAVE SYSTEM] Failed to open save file: {FileAccess.GetOpenError()}");
                    return false;
                }

                // Converte para dicionário Godot para serialização JSON
                var godotDict = new Dictionary();
                var dataDict = data.ToDictionary();
                foreach (var kvp in dataDict)
                {
                    godotDict[kvp.Key] = kvp.Value;
                }

                var jsonString = Json.Stringify(godotDict, "\t"); // Pretty print with tabs

                saveFile.StoreString(jsonString);
                saveFile.Close();

                GD.Print($"[SAVE SYSTEM] Game saved successfully to: {SAVE_PATH}");
                GD.Print($"[SAVE SYSTEM] Save data: Position={data.PlayerPosition}, Health={data.CurrentHealth}/{data.MaxHealth}");

                return true;
            }
            catch (System.Exception e)
            {
                GD.PrintErr($"[SAVE SYSTEM] Error saving game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Carrega o estado do jogo do disco
        /// </summary>
        public static SaveData LoadGame()
        {
            if (!SaveFileExists())
            {
                GD.Print("[SAVE SYSTEM] No save file found, returning default data");
                return new SaveData();
            }

            try
            {
                var saveFile = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);

                if (saveFile == null)
                {
                    GD.PrintErr($"[SAVE SYSTEM] Failed to open save file: {FileAccess.GetOpenError()}");
                    return new SaveData();
                }

                var jsonString = saveFile.GetAsText();
                saveFile.Close();

                // Parser de JSON
                var json = new Json();
                var parseResult = json.Parse(jsonString);

                if (parseResult != Error.Ok)
                {
                    GD.PrintErr($"[SAVE SYSTEM] JSON parse error at line {json.GetErrorLine()}: {json.GetErrorMessage()}");
                    return new SaveData();
                }

                var dict = json.Data.AsGodotDictionary();

                // Converte para System.Collections.Generic.Dictionary
                var systemDict = new System.Collections.Generic.Dictionary<string, Variant>();
                foreach (var key in dict.Keys)
                {
                    systemDict[key.AsString()] = dict[key];
                }

                var data = SaveData.FromDictionary(systemDict);

                GD.Print($"[SAVE SYSTEM] Game loaded successfully");
                GD.Print($"[SAVE SYSTEM] Loaded data: Position={data.PlayerPosition}, Health={data.CurrentHealth}/{data.MaxHealth}");
                GD.Print($"[SAVE SYSTEM] Save date: {data.SaveDate}");

                return data;
            }
            catch (System.Exception e)
            {
                GD.PrintErr($"[SAVE SYSTEM] Error loading game: {e.Message}");
                return new SaveData();
            }
        }

        /// <summary>
        /// Verifica se existe um arquivo de save
        /// </summary>
        public static bool SaveFileExists()
        {
            return FileAccess.FileExists(SAVE_PATH);
        }

        /// <summary>
        /// Deleta o arquivo de save
        /// </summary>
        public static void DeleteSave()
        {
            if (SaveFileExists())
            {
                DirAccess.RemoveAbsolute(SAVE_PATH);
                GD.Print("[SAVE SYSTEM] Save file deleted");
            }
        }

        /// <summary>
        /// Obtém o caminho completo para o arquivo de save
        /// </summary>
        public static string GetSavePath()
        {
            return ProjectSettings.GlobalizePath(SAVE_PATH);
        }
    }
}
