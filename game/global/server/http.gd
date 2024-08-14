class_name SPJHttpServer
extends Node

@onready var server: TCPServer = TCPServer.new()

var clients = []

func start():
	Helpers.find_port(server, 12003)

func _process(delta):
	while server.is_connection_available():
		var connection = server.take_connection()
		clients.append(connection)
	
	for client in clients:
		tcp_process(client)

func tcp_process(peer: StreamPeerTCP):
	if peer.get_status() == StreamPeerTCP.STATUS_CONNECTED:
		var bytes = peer.get_available_bytes()
		if bytes > 0:
			var request = peer.get_utf8_string(bytes)
			handle_http(request, peer)
	elif peer.get_status() == StreamPeerTCP.STATUS_NONE:
		clients.erase(peer)

func get_file(path: String):
	if path.ends_with("/"):
		path += "index.html"
	var mimetype: String
	
	var ext = path.split("/")[-1].split(".")[-1]
	if ext == "html":
		mimetype = "text/html"
	elif ext == "js":
		mimetype = "text/javascript"
	elif ext == "css":
		mimetype = "text/css"
	elif ext == "png":
		mimetype = "text/png"
	elif ext == "jpg":
		mimetype = "text/jpeg"
	elif ext == "svg":
		mimetype = "image/svg+xml"
		
	return [FileAccess.get_file_as_bytes("res://assets/controller" + path), mimetype]

func handle_http(string: String, peer: StreamPeerTCP):
	var split = string.split("\r\n")
	var main = split.slice(0, 1)[0].split(" ")
	
	if main[0] != "GET":
		peer.put_data("HTTP/1.1 405 Method Not Allowed\r\n".to_utf8_buffer())
		clients.erase(peer)
		return
	
	if main[1] == "/ws":
		var port = str(GameServer.get_port())
		peer.put_data("HTTP/1.1 200 OK\r\n".to_utf8_buffer())
		peer.put_data("Access-Control-Allow-Origin: *\r\n".to_utf8_buffer())
		peer.put_data(("Content-Length: %s\r\n" % len(port)).to_utf8_buffer())
		peer.put_data(("Content-Type: text/plain\r\n").to_utf8_buffer())
		peer.put_data("\r\n".to_utf8_buffer())
		peer.put_data(port.to_utf8_buffer())
		return
	
	var headers = split.slice(1)

	var read = get_file(main[1])
	var content = read[0]
	var mime = read[1]
	var length = len(content)
	if length > 0:
		peer.put_data("HTTP/1.1 200 OK\r\n".to_utf8_buffer())
		peer.put_data("Access-Control-Allow-Origin: *\r\n".to_utf8_buffer())
		peer.put_data(("Content-Length: %s\r\n" % length).to_utf8_buffer())
		peer.put_data(("Content-Type: %s\r\n" % mime).to_utf8_buffer())
		peer.put_data("\r\n".to_utf8_buffer())
		peer.put_data(content)
	else:
		peer.put_data("HTTP/1.1 404 Not Found\r\n".to_utf8_buffer())
		clients.erase(peer)
		return
