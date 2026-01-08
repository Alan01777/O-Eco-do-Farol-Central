using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Estrutura que armazena os dados de um power-up coletado.
    /// Usado para exibir informações na UI.
    /// </summary>
    public class PowerUpData
    {
        public string AbilityName { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public Texture2D Icon { get; set; }
        public PowerUp.PowerUpType Type { get; set; }

        public PowerUpData(PowerUp.PowerUpType type, string itemName, string description, Texture2D icon)
        {
            Type = type;
            AbilityName = type.ToString();
            ItemName = itemName;
            Description = description;
            Icon = icon;
        }
    }
}
