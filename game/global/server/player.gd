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
var force_spectator: bool = false :
	set(new_force_spectator):
		force_spectator = new_force_spectator
		sync_spectator_mode()

var spectator: bool = false : 
	set(new_spectator):
		spectator = new_spectator
		sync_spectator_mode()


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
		self.sync_spectator_mode()
	else:
		self.handoff_client()

func kick():
	self.client.close(SPJGameServer.CloseClient.REMOVED_BY_HOST)
	self.queue_free()

func handoff_client():
	if self.module:
		self.client.close(SPJGameServer.CloseModule.SWITCH_TO_MODULE, self.module.name)
	else:
		self.client.close(SPJGameServer.CloseModule.EXIT_MODULE)

func is_spectator():
	return self.force_spectator or self.spectator

func sync_spectator_mode():
	self.client.send("spectator", 
		{
			"spectator": self.spectator, 
			"force_spectator": self.force_spectator
		}
	)

func __event_setusername(data: Dictionary):
	self.username = data.username
	self.username_changed.emit()
	
func __event_setspectator(data: Dictionary):
	self.spectator = data.spectator or self.force_spectator

func _process(delta):
	self.client.poll()
