extends Node2D


# Called when the node enters the scene tree for the first time.
func _ready():
	$TextureRect.texture = Helpers.create_qr_code("say gex")

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass

func _on_button_pressed():
	ModuleManager.switch_module("dice", Websocket.get_players())
