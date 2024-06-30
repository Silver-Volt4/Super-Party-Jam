extends Node
class_name SPJPlayer

signal joined
signal left
signal unhandled_event(data)

var player_name: String
var client: SPJClient

func _init(
	client: SPJClient
):
	self.client = client
	Helpers.create_event_handler(self, self.client)
	self.name = str(randi())

func reassign(client: SPJClient):
	self.client.disconnect_peer(4002, "Replaced by another client")
	self.client = client

func __event_setname(data: Dictionary):
	self.player_name = data.name

func _process(delta):
	self.client.poll()
