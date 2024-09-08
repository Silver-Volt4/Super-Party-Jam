extends SPJModuleMetadata

func _init():
	self.name = "bingo"
	self.display_name = "Sonic Bingo Party"
	self.description = "Enjoy a fun bingo session with your friends. Unlimited players!"
	self.min_players = 2
	self.max_players = -1
	self.thumbnail = preload("res://games/bingo/gallery/thumbnail.png")
