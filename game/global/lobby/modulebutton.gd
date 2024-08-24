extends Button

var metadata: SPJModuleMetadata
var tw: Tween

func _ready():
	$Thumbnail.texture = self.metadata.thumbnail
	self.mouse_entered.connect(self.grab_focus)
	self.focus_entered.connect(self.focus)
	self.focus_exited.connect(self.unfocus)
	self.button_down.connect(self.press)
	self.button_up.connect(self.unpress)

func anim_hover(anim):
	$Thumbnail.pivot_offset = self.size/2
	$Thumbnail.scale = lerp(Vector2.ONE, Vector2.ONE * 1.15, anim)
	$Thumbnail.rotation_degrees = lerp(0.0, -4.0, anim)
	self.pivot_offset = self.size/2
	self.scale = lerp(Vector2.ONE, Vector2.ONE * 1.05, anim)

func anim_press(anim):
	self.pivot_offset = self.size/2
	self.scale = lerp(Vector2.ONE * 1.05, Vector2.ONE * 0.85, anim)

func mktween():
	if self.tw: self.tw.stop()
	self.tw = get_tree().create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_CIRC)

func focus():
	SPJ.play_sfx("cursor")
	self.mktween()
	self.tw.tween_method(self.anim_hover, 0.0, 1.0, 0.1)

func unfocus():
	self.mktween()
	self.tw.tween_method(self.anim_hover, 1.0, 0.0, 0.1)

func press():
	self.mktween()
	self.tw.tween_method(self.anim_press, 0.0, 1.0, 0.1)

func unpress():
	self.mktween()
	self.tw.tween_method(self.anim_press, 1.0, 0.0, 0.1)
