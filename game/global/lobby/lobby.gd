extends Control

@onready var GAMES = DirAccess.get_directories_at("res://games/")

func _ready():
	for game in self.GAMES:
		var button = preload("res://global/lobby/modulebutton.tscn").instantiate()
		button.metadata = load("res://games/" + game + "/meta.gd")
		$Outer/Inner/VBoxContainer/Games/GamesGrid.add_child(button)
		button.focus_entered.connect(self.touch_game.bind(button))

func setup():
	var self_ip = Helpers.get_self_ip()
	if self_ip == "":
		SPJ.alert("Cannot display QR code", "Automatic detection of your IP address has failed.\nThe QR code which allows you to quickly join the game cannot be shown.\nPlease connect manually.")
	var self_addr = "http://" + self_ip + ":" + str(HttpServer.server.get_local_port())
	$Instruction/QRCode.texture = Helpers.create_qr_code(self_addr)

func touch_game(button):
	$Outer/Inner/VBoxContainer/GameInfo/Name.text = button.metadata.display_name
	$Outer/Inner/VBoxContainer/GameInfo/Description.text = button.metadata.description
