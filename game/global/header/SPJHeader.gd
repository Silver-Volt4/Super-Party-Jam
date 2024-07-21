extends Control

func _flasher_progress(p: float):
	$Flasher.material.set_shader_parameter(&"progress", p)

func flash(show = true):
	$Header.visible = not show
	$Label.visible = not show
	var tw = create_tween().set_ease(Tween.EASE_IN_OUT).set_trans(Tween.TRANS_CIRC)
	tw.tween_method(self._flasher_progress, 0.0, 1.0, 0.15)
	tw.tween_callback($Header.set_visible.bind(show))
	tw.tween_callback($Label.set_visible.bind(show))
	tw.tween_method(self._flasher_progress, 1.0, 2.0, 0.15)
	await tw.finished
