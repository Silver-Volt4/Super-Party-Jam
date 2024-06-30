class_name Helpers

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
	child.unhandled_event.connect(SPJClient.event_handler.bind(this))
