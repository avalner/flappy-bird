shader_type canvas_item;

uniform vec2 tiling = vec2(1.0, 1.0); // Number of tiles in X and Y

void fragment() {
    vec2 uv = UV * tiling; // Scale UV coordinates by tiling factor
    COLOR = texture(TEXTURE, fract(uv)); // Repeat texture using fract()
}