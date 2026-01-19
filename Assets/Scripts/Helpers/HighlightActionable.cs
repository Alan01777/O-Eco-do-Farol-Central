using Godot;
using Helpers;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Actionable com sistema de highlight automático quando o player se aproxima.
    /// Pode ser usado para placas, baús, e outros objetos interativos.
    /// </summary>
    public partial class HighlightActionable : Actionable
    {
        [ExportGroup("Highlight")]
        [Export] public Sprite2D SpriteNode; // Sprite com shader de highlight

        private ShaderMaterial _shaderMaterial;

        public override void _Ready()
        {
            // Obtém referência do shader material do sprite
            if (SpriteNode != null && SpriteNode.Material is ShaderMaterial shader)
            {
                _shaderMaterial = shader;
                SetHighlightEnabled(false); // Começa desativado
            }

            // Conecta sinais de entrada/saída da área para detectar player
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body is Player)
            {
                SetHighlightEnabled(true);
            }
        }

        private void OnBodyExited(Node2D body)
        {
            if (body is Player)
            {
                SetHighlightEnabled(false);
            }
        }

        private void SetHighlightEnabled(bool enabled)
        {
            if (_shaderMaterial != null)
            {
                _shaderMaterial.SetShaderParameter("enabled", enabled);
            }
        }
    }
}
