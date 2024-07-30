extends Control

func _on_begin_screen_local():
	start()

func _on_begin_screen_remote():
	SPJ.alert("Not implemented", "This feature is not available yet.")

func start():
	Websocket.start()
	HttpServer.start()
	var tw = create_tween()
	tw.tween_property($BeginScreen, "modulate:a", 0, 0.1)
	tw.tween_callback($BeginScreen.queue_free)
	$Lobby.modulate.a = 0.0
	$Lobby.show()
	$Lobby.setup()
	tw.tween_property($Lobby, "modulate:a", 1.0, 0.1)