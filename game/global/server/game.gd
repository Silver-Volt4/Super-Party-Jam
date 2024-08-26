class_name SPJGameServer
extends Node

# 41xx - Client reasons. These always invalidate the token
enum CloseClient {
	INVALID_TOKEN = 4100,
	REMOVED_BY_HOST = 4101,
}
# 42xx - Player reasons
enum ClosePlayer {
	REPLACED = 4200,
}
# 45xx - Module reasons
enum CloseModule {
	SWITCH_TO_MODULE = 4500,
	EXIT_MODULE = 4501,
}

signal new_player(player)

@onready var server = TCPServer.new()

var clients: Array = []

func start():
	Helpers.find_port(server, 12004)

func get_port() -> int:
	return self.server.get_local_port()

func _process(delta):
	while server.is_connection_available():
		var peer = WebSocketPeer.new()
		peer.inbound_buffer_size = 134215680
		peer.accept_stream(server.take_connection())
		var client = load("res://global/server/SPJClient.cs").new(peer)
		print("Take client ", client)
		self.clients.append(client)
		#client.closed.connect(self.client_closed.bind(client), CONNECT_ONE_SHOT)
		#client.activate.connect(self.client_activated.bind(client), CONNECT_ONE_SHOT)

	for client in self.clients:
		client.Poll()

func client_activated(data: Dictionary, client):
	if !data.has(&"token"):
		var player = load("res://global/server/SPJPlayer.cs").new(client)
		player.username = data.username
		self.add_child(player)
		self.clients.erase(client)
		player.accept(data.module)
		self.new_player.emit(player)
	else:
		var player = self.get_node_or_null(data.token)
		if player:
			self.clients.erase(client)
			player.reassign(client)
			player.accept(data.module)
		else:
			client.close(SPJGameServer.CloseClient.INVALID_TOKEN)

func client_closed(client):
	self.clients.erase(client)

func get_players() -> Array:
	return self.get_children()

func get_player_count() -> int:
	return len(self.get_players())
