extends Resource
class_name SPJModuleMetadata

var name: String
var display_name: String
var description: String
var min_players: int
var max_players: int
var thumbnail: Texture

func describe_player_count() -> String:
	if self.min_players == -1 and self.max_players == -1:
		return "Any player count"
	elif self.min_players == -1:
		return "<%s players" % [self.max_players]
	elif self.max_players == -1:
		return "%s+ players" % [self.min_players]
	else:
		return "%s-%s players" % [self.min_players, self.max_players]

func player_count_satisfied(players):
	if self.min_players == -1 and self.max_players == -1:
		return players >= 1
	elif self.min_players == -1:
		return players <= self.max_players
	elif self.max_players == -1:
		return players >= self.min_players
	else:
		return players <= self.max_players and players >= self.min_players
