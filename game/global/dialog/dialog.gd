extends Control

signal resolve(accept)

func _ready():
	$Dialog/Panel/Dismiss.grab_focus.call_deferred()
	self.resolve.connect(self.dispose)
	$Dialog.size.y = $Dialog/Header.size.y
	$Dialog.position.y = (self.size.y - $Dialog/Header.size.y)/2
	
	$FrostedGlass.self_modulate.a = 0.0
	$Dimming.self_modulate.a = 0.0
	
	$Dialog/Header.flash()
	var tw = create_tween().set_ease(Tween.EASE_IN_OUT).set_trans(Tween.TRANS_CIRC)
	tw.tween_property($FrostedGlass, "self_modulate:a", 1, 0.3)
	tw.parallel().tween_property($Dimming, "self_modulate:a", 1, 0.3)
	tw.tween_callback(SPJ.play_sfx.bind("dialog"))
	tw.tween_property($Dialog, "size:y", 650, 0.3)
	tw.parallel().tween_property($Dialog, "position:y", (self.size.y - 650)/2, 0.3)

func title(title_text: String):
	$Dialog/Header/Label.text = title_text
	return self

func text(dialog_text: String):
	$Dialog/Panel/DialogText.text = dialog_text
	return self

func alert(dismiss_text: String):
	$Dialog/Panel/Dismiss.show()
	$Dialog/Panel/OK.hide()
	$Dialog/Panel/Cancel.hide()
	$Dialog/Panel/Dismiss.text = dismiss_text
	return self

func decide(ok_text: String, cancel_text: String):
	$Dialog/Panel/Dismiss.hide()
	$Dialog/Panel/OK.show()
	$Dialog/Panel/Cancel.show()
	$Dialog/Panel/OK.text = ok_text
	$Dialog/Panel/Cancel.text = cancel_text
	return self

func dispose(_resolve):
	var tw = create_tween().set_ease(Tween.EASE_IN_OUT).set_trans(Tween.TRANS_CIRC)
	tw.tween_property($Dialog, "size:y", $Dialog/Header.size.y, 0.1)
	tw.parallel().tween_property($Dialog, "position:y", (self.size.y - $Dialog/Header.size.y)/2, 0.2)
	$Dialog/Header.flash(false)
	tw.tween_property($FrostedGlass, "self_modulate:a", 0, 0.3)
	tw.parallel().tween_property($Dimming, "self_modulate:a", 0, 0.3)
	tw.finished.connect(self.queue_free)
