shader_type canvas_item;

uniform vec2 size;
uniform float circle_size;
uniform float timing;
uniform float rotation;

vec2 rotate(vec2 v, float a) {
	float s = sin(a);
	float c = cos(a);
	mat2 m = mat2(vec2(c, s), vec2(-s, c));
	return m * v;
}

void fragment() {
	vec2 pos = rotate(UV * size - mod(size, circle_size)/2.0, rotation);
	float posX = floor(pos.x / circle_size);
	float posY = floor(pos.y / circle_size);
	
	pos = vec2(mod(pos.x, circle_size), mod(pos.y, circle_size)) - vec2(0.5 * circle_size, 0.5 * circle_size);
	pos = pos / vec2(circle_size * 0.5, circle_size * 0.5);
	
	float cutoff = pow(sin(TIME + (posX + 2.0*posY) * (timing/100.0)*PI) * 0.9 , 2.0);
	
	if(pos.x * pos.x + pos.y * pos.y < cutoff) {
		COLOR.r -= 0.1;
		COLOR.g -= 0.1;
		COLOR.b -= 0.1;
	}
}
