# client.gd
# A helper class to make working with WebSockets less painful
extends Resource
class_name SPJClient

signal connected
signal closed
signal activate
signal unhandled_event(event, data)

var peer: WebSocketPeer
var __peer_state: WebSocketPeer.State = WebSocketPeer.STATE_CONNECTING

var active: bool = false

func _init(
	peer: WebSocketPeer
):
	self.peer = peer

func send(event: String, data: Dictionary):
	data.event = event
	self.peer.send_text(JSON.stringify(data))

func disconnect_peer(code: int, reason: String):
	self.peer.close(code, reason)

func poll():
	self.peer.poll()
	
	var state = self.peer.get_ready_state()
	if state != self.__peer_state:
		match state:
			WebSocketPeer.STATE_OPEN:
				self.connected.emit()
			WebSocketPeer.STATE_CLOSED:
				self.closed.emit()
			_:
				pass
		self.__peer_state = state
	
	print("poll")
	
	while self.peer.get_available_packet_count():
		var packet = self.peer.get_packet()
		if not self.peer.was_string_packet():
			continue
			
		var data = JSON.parse_string(packet.get_string_from_utf8())
		var event = "__event_" + data.event
		SPJClient.event_handler(event, data, self)

static func event_handler(event: String, data: Dictionary, object: Object):
	if object.has_method(event):
		object.call(event, data)
	else:
		print_debug("Event ", event, " bubbling upwards")
		object.emit_signal(&"unhandled_event", event, data)

func __event_register(data: Dictionary):
	if self.active:
		return
	self.activate.emit(data)
