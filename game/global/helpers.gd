extends Object
class_name Helpers

static var spjlib = SPJLib.new()
static var self_ip_address = null

static func find_port(server: TCPServer, port: int):
	while true:
		var e = server.listen(port)
		if e == OK:
			break
		assert(e == 22)
		port += 1 % 65535
	return port

static func create_event_handler(this: Object, child: Object):
	Signal(this, &"unhandled_event")
	#child.unhandled_event.connect(SPJClient.event_handler.bind(this))

static func get_self_ip() -> String:
	if self_ip_address == null:
		self_ip_address = spjlib.get_local_ip()
	return self_ip_address

static func create_qr_code(text: String) -> Texture:
	return ImageTexture.create_from_image(spjlib.get_qr_image(text))
