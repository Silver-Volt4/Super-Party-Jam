class_name SPJWebsocketServer
extends Node

signal new_player(player)

@onready var server = TCPServer.new()

var clients: Array[SPJClient] = []

func start():
	Helpers.find_port(server, 12004)

func get_port() -> int:
	return self.server.get_local_port()

func _process(delta):
	while server.is_connection_available():
		var peer = WebSocketPeer.new()
		peer.inbound_buffer_size = 134215680
		peer.accept_stream(server.take_connection())
		var client = SPJClient.new(peer)
		self.clients.append(client)
		client.closed.connect(self.client_closed.bind(client), CONNECT_ONE_SHOT)
		client.activate.connect(self.client_activated.bind(client), CONNECT_ONE_SHOT)

	for client in self.clients:
		client.poll()

func client_activated(data: Dictionary, client: SPJClient):
	if !data.has(&"token"):
		var player = SPJPlayer.new(client)
		player.username = data.username
		self.add_child(player)
		self.clients.erase(client)
		player.accept(data.module)
		self.new_player.emit(player)
	else:
		var player: SPJPlayer = self.get_node_or_null(data.token)
		if player:
			self.clients.erase(client)
			player.reassign(client)
			player.accept(data.module)
		else:
			client.disconnect_peer(4001, "Incorrect token")

func client_closed(client: SPJClient):
	self.clients.erase(client)

func get_players() -> Array:
	return self.get_children()

func get_player_count() -> int:
	return len(self.get_players())
