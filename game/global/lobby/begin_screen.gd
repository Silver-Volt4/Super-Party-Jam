extends Control

var begin = false
signal local
signal remote

func _input(event):
	if event.is_pressed() and not begin:
		SPJ.play_sfx("start")
		self.begin = true
		var tw = get_tree().create_tween()
		tw.tween_property($WelcomeText, "modulate:a", 0, 0.1)
		tw.tween_property($ModeSelect, "modulate:a", 0, 0)
		tw.tween_callback($ModeSelect.show)
		tw.tween_property($ModeSelect, "modulate:a", 1, 0.1)

func _on_local_focus_entered():
	$ModeSelect/Explanation.text = "For parties where all players reside on the same network"

func _on_remote_focus_entered():
	$ModeSelect/Explanation.text = "For remote parties"

func _on_local_pressed():
	self.local.emit()

func _on_remote_pressed():
	self.remote.emit()
