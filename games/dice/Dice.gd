extends Node2D

@onready var players = ModuleManager.get_players()

func _ready():
	var profileScene = preload("./DiceProfile.tscn")
	var idx = 1
	for p in players:
		var profile = profileScene.instantiate()
		profile.player = p
		self.add_child(profile)
		profile.position = Vector2(20 * idx, 40)
		idx += 1
