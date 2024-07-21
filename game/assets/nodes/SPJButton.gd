extends Button
class_name SPJButton

const SQUEEZE_PX = 20

var tw

func _ready():
	self.mouse_entered.connect(self.grab_focus)
	self.focus_entered.connect(SPJ.play_sfx.bind("cursor"))
	if self.is_in_group(&"cancel"):
		self.pressed.connect(SPJ.play_sfx.bind("cancel"))
	else:
		self.pressed.connect(SPJ.play_sfx.bind("select"))
	self.focus_entered.connect(self.stretch)
	self.focus_exited.connect(self.chill)
	self.button_down.connect(self.squeeze)
	self.button_up.connect(self.unsqueeze)

func wait_anim_exit():
	if tw is Tween and tw.is_running():
		await tw.finished

func stretch():
	await self.wait_anim_exit()
	self.pivot_offset = self.size/2
	tw = get_tree().create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_CIRC)
	tw.parallel().tween_property(self, "size:x", self.size.x + SQUEEZE_PX * 2, 0.2)
	tw.parallel().tween_property(self, "global_position:x", self.global_position.x - SQUEEZE_PX, 0.2)
	tw.play()

func chill():
	await self.wait_anim_exit()
	self.pivot_offset = self.size/2
	tw = get_tree().create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_CIRC)
	tw.parallel().tween_property(self, "size:x", self.size.x - SQUEEZE_PX * 2, 0.2)
	tw.parallel().tween_property(self, "global_position:x", self.global_position.x + SQUEEZE_PX, 0.2)
	tw.play()

func squeeze():
	await self.wait_anim_exit()
	self.pivot_offset = self.size/2
	tw = get_tree().create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_CIRC)
	tw.parallel().tween_property(self, "size:x", self.size.x - SQUEEZE_PX * 4, 0.2)
	tw.parallel().tween_property(self, "global_position:x", self.global_position.x + SQUEEZE_PX * 2, 0.2)
	tw.play()
	
func unsqueeze():
	await self.wait_anim_exit()
	self.pivot_offset = self.size/2
	tw = get_tree().create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_CIRC)
	tw.parallel().tween_property(self, "size:x", self.size.x + SQUEEZE_PX * 4, 0.2)
	tw.parallel().tween_property(self, "global_position:x", self.global_position.x - SQUEEZE_PX * 2, 0.2)
	tw.play()
