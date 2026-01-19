shader_type canvas_item;

// Shader de highlight para objetos interativos
// Cria um brilho pulsante suave ao redor do sprite

uniform vec4 highlight_color : source_color = vec4(1.0, 0.9, 0.5, 0.8);
uniform float outline_width : hint_range(0.0, 10.0) = 2.0;
uniform float pulse_speed : hint_range(0.0, 5.0) = 2.0;
uniform float pulse_intensity : hint_range(0.0, 1.0) = 0.3;
uniform bool enabled = true;

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    
    if (!enabled) {
        COLOR = tex_color;
    } else {
        // Calcula o brilho pulsante
        float pulse = sin(TIME * pulse_speed) * 0.5 + 0.5;
        float glow_strength = pulse * pulse_intensity + (1.0 - pulse_intensity);
        
        // Detecta bordas do sprite para criar outline
        float alpha = tex_color.a;
        vec2 pixel_size = 1.0 / vec2(textureSize(TEXTURE, 0));
        
        // Amostra pixels vizinhos para detectar borda
        float outline = 0.0;
        for (float x = -outline_width; x <= outline_width; x += 1.0) {
            for (float y = -outline_width; y <= outline_width; y += 1.0) {
                vec2 offset = vec2(x, y) * pixel_size;
                float neighbor_alpha = texture(TEXTURE, UV + offset).a;
                outline = max(outline, neighbor_alpha);
            }
        }
        
        // Cria o efeito de glow na borda
        float border = outline - alpha;
        border = clamp(border, 0.0, 1.0);
        
        // Mistura o sprite original com o highlight
        vec4 glow = highlight_color * border * glow_strength;
        
        // Adiciona um leve brilho ao sprite inteiro
        vec4 sprite_glow = vec4(highlight_color.rgb * pulse_intensity * glow_strength, 0.0);
        
        COLOR = tex_color + glow + sprite_glow * alpha;
        COLOR.a = max(tex_color.a, border * highlight_color.a * glow_strength);
    }
}
