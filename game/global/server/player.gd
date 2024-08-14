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
	self.client.close(SPJGameServer.ClosePlayer.REPLACED)
	self.client = client

func accept(module: String):
	if (not self.module and module == "") or (self.module and self.module.name == module):
		self.client.send("accepted", {"username": self.username, "token": self.name})
	else:
		self.handoff_client()

func kick():
	self.client.close(SPJGameServer.ClosePlayer.REMOVED_BY_HOST)
	self.queue_free()

func handoff_client():
	if self.module:
		self.client.close(SPJGameServer.CloseModule.SWITCH_TO_MODULE, self.module.name)
	else:
		self.client.close(SPJGameServer.CloseModule.EXIT_MODULE)

func __event_setusername(data: Dictionary):
	self.username = data.username
	self.username_changed.emit()

func _process(delta):
	self.client.poll()
