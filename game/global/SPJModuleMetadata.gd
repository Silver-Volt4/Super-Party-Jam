extends Resource
class_name SPJModuleMetadata

static var name: String
static var display_name: String
static var description: String
static var min_players: int
static var max_players: int

static func describe_player_count() -> String:
	if min_players == -1 and max_players == -1:
		return "Any player count"
	elif min_players == -1:
		return "<%s players" % [max_players]
	elif max_players == -1:
		return "%s+ players" % [min_players]
	else:
		return "%s-%s players" % [min_players, max_players]

static func player_count_satisfied(players):
	if min_players == -1 and max_players == -1:
		return true
	elif min_players == -1:
		return players <= max_players
	elif max_players == -1:
		return players >= min_players
	else:
		return players <= max_players and players >= min_players
