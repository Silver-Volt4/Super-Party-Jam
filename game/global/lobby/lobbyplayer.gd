extends Panel

var player: SPJPlayer

func _ready():
	var popup: PopupMenu = $OptionsButton.get_popup()
	popup.id_pressed.connect(self.option_selected)
	self.update_buttons()
	self.player.username_changed.connect(self.redraw)
	self.player.spectator_changed.connect(self.redraw)

func flash():
	self.redraw()
	var tw = create_tween().set_trans(Tween.TRANS_CIRC).set_ease(Tween.EASE_OUT)
	tw.tween_method(self._lobbyplayer_progress, 1.0, 0.115, 0.3)
	tw.parallel().tween_property($Name, "position", $Name.position, 0.5)
	tw.parallel().tween_property($Name, "size", $Name.size, 0.5)
	self._lobbyplayer_progress(0.0)
	$Name.position.x += $Name.size.x
	$Name.size.x = 0
	SPJ.play_sfx("join")

func update_buttons():
	$OptionsButton.get_popup().set_item_text(1,
		"Unenforce spectator" if self.player.force_spectator else "Force spectator"
	)

func redraw():
	$Name.text = self.player.username 
	self.self_modulate.a = 0.5 if self.player.is_spectator() else 1.0
	$Backdrop.self_modulate.a = 0.5 if self.player.is_spectator() else 1.0

func _lobbyplayer_progress(p: float):
	self.material.set_shader_parameter(&"progress", p)

func option_selected(id: int):
	match id:
		0: # Remove
			self.player.kick()
			self.queue_free()
		1: # Make spectator 
			self.player.force_spectator = not self.player.force_spectator
			self.update_buttons()
