shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear_mipmap_anisotropic;
uniform float blur_amount;

void fragment() {
	vec3 color = textureLod(screen_texture, SCREEN_UV, blur_amount).rgb;
	COLOR.rgb = color.rgb;
}