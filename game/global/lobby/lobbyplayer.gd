extends Panel

var player: SPJPlayer

func _ready():
	var popup: PopupMenu = $OptionsButton.get_popup()
	popup.id_pressed.connect(self.option_selected)

func flash():
	self.set_username()
	self.player.username_changed.connect(self.set_username)
	var tw = create_tween().set_trans(Tween.TRANS_CIRC).set_ease(Tween.EASE_OUT)
	tw.tween_method(self._lobbyplayer_progress, 1.0, 0.115, 0.3)
	tw.parallel().tween_property($Name, "position", $Name.position, 0.5)
	tw.parallel().tween_property($Name, "size", $Name.size, 0.5)
	self._lobbyplayer_progress(0.0)
	$Name.position.x += $Name.size.x
	$Name.size.x = 0
	SPJ.play_sfx("join")

func set_username():
	$Name.text = self.player.username 

func _lobbyplayer_progress(p: float):
	self.material.set_shader_parameter(&"progress", p)

func option_selected(id: int):
	match id:
		0: 
			player.kick()
			self.queue_free()
