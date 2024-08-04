extends Button
class_name SPJButton

var _t_size
var _t_global_position

var squish_px = 0 :
	set(new_squish):
		var diff = new_squish - squish_px
		squish_px = new_squish
		tw = get_tree().create_tween().set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_CIRC)
		self._t_size = self.size.x + diff * 2
		tw.parallel().tween_property(self, "size:x", self._t_size , 0.2)
		self._t_global_position = self.global_position.x - diff
		tw.parallel().tween_property(self, "global_position:x", self._t_global_position, 0.2)
		tw.play()

var tw

func _ready():
	self.mouse_entered.connect(self.grab_focus)
	self.focus_entered.connect(SPJ.play_sfx.bind("cursor"))
	if self.is_in_group(&"cancel"):
		self.pressed.connect(SPJ.play_sfx.bind("cancel"))
	else:
		self.pressed.connect(SPJ.play_sfx.bind("select"))
	if not self.is_in_group(&"static"):
		self.focus_entered.connect(self.stretch)
		self.focus_exited.connect(self.chill)
		self.button_down.connect(self.squeeze)
		self.button_up.connect(self.chill)

func ensure():
	self.pivot_offset = self.size/2
	if tw is Tween and tw.is_running():
		tw.stop()
		self.size.x = self._t_size
		self.global_position.x = self._t_global_position

func stretch():
	self.ensure()
	squish_px = 20

func chill():
	self.ensure()
	squish_px = 0

func squeeze():
	self.ensure()
	squish_px = -20
