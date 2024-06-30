class_name SPJWebsocketServer
extends Node

@onready var server = TCPServer.new()

var clients: Array[SPJClient] = []

func _ready():
	Helpers.find_port(server, 12004)

func _process(delta):
	while server.is_connection_available():
		var peer = WebSocketPeer.new()
		peer.inbound_buffer_size = 134215680
		peer.accept_stream(server.take_connection())
		var client = SPJClient.new(peer)
		clients.append(client)
		client.closed.connect(self.client_closed.bind(client), CONNECT_ONE_SHOT)
		client.activate.connect(self.client_activated.bind(client), CONNECT_ONE_SHOT)

	for client in clients:
		client.poll()

func client_activated(data: Dictionary, client: SPJClient):
	if !data.has(&"token"):
		var player = SPJPlayer.new(client)
		self.add_child(player)
		clients.erase(client)
	else:
		var player: SPJPlayer = self.get_node_or_null(data.token)
		if player:
			player.reassign(client)
		else:
			client.disconnect_peer(4001, "Incorrect token")

func client_closed(client: SPJClient):
	if not client.active:
		clients.erase(client)
		client.free()
