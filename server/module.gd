extends Resource
class_name SPJModule

signal unhandled_event(event, data)
signal effect

var player: SPJPlayer : 
	set(new_player):
		player = new_player
		Helpers.create_event_handler(self, new_player)

func __event_sync(data):
	var name = data.name
	var value = data.value
	self.set(name, value)
	self.effect.emit()
