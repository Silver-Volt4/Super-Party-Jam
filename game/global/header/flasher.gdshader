shader_type canvas_item;

uniform float progress: hint_range(0.0, 2.0);
uniform float skew;

void fragment() {
	vec2 uv = UV;
	uv.x += uv.y * skew;
	if(progress < 1.0) {
		if(uv.x > (progress + uv.x * skew)) discard;
	} else {
		if((uv.x) < (progress - 1.0 + uv.x * skew)) discard;
	}
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
