extends Label

var player: SPJPlayer : 
	set(new_player):
		player = new_player
		module = player.module
var module: DiceModule :
	set(new_module):
		module = new_module
		player.module.effect.connect(self.effect)
		effect()

func effect():
	text = str(module.dice)
