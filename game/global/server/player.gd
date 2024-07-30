extends Node
class_name SPJPlayer

signal joined
signal left
signal username_changed
signal unhandled_event(event, data)

var username: String
var client: SPJClient :
	set(new_client):
		client = new_client
		client.active = true
		connected = true
		client.closed.connect(func(): self.connected = false)
		Helpers.create_event_handler(self, new_client)
var module: SPJModule = null :
	set(new_module):
		module = new_module
		if module:
			module.player = self
		handoff_client()
var connected: bool = false

func _init(
	client: SPJClient
):
	self.name = str(randi())
	self.client = client

func reassign(client: SPJClient):
	self.client.disconnect_peer(4002, "Replaced by another client")
	self.client = client

func accept(module: String):
	if (not self.module and module == "") or (self.module and self.module.name == module):
		self.client.send("accepted", {"username": self.username, "token": self.name})
	else:
		self.handoff_client()

func handoff_client():
	self.client.disconnect_peer(4500, "" if not self.module else self.module.name)

func __event_setusername(data: Dictionary):
	self.username = data.username
	self.username_changed.emit()

func _process(delta):
	self.client.poll()
