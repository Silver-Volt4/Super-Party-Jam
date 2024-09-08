extends SPJModuleMetadata

func _init():
	self.name = "dice"
	self.display_name = "Test module"
	self.description = "This is not an actual game, it's intended for testing!"
	self.min_players = 0
	self.max_players = -1
	self.thumbnail = preload("res://games/dice/gallery/thumbnail.png")
