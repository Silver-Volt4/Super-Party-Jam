extends Control

@onready var GAMES = DirAccess.get_directories_at("res://games/")

func _ready():
	for game in self.GAMES:
		var button = preload("res://global/lobby/modulebutton.tscn").instantiate()
		button.metadata = load("res://games/" + game + "/meta.gd").new()
		$Outer/Inner/VBoxContainer/Games/GamesGrid.add_child(button)
		button.focus_entered.connect(self.touch_game.bind(button.metadata))
		button.pressed.connect(self.play_game.bind(button.metadata))
	for player in Websocket.get_players():
		self.add_player(player, true)
	Websocket.new_player.connect(self.add_player)

func setup():
	var self_ip = Helpers.get_self_ip()
	if self_ip == "":
		SPJ.alert("Cannot display QR code", "Automatic detection of your IP address has failed.\nThe QR code which allows you to quickly join the game cannot be shown.\nPlease connect manually.")
	var self_addr = "http://" + self_ip + ":" + str(HttpServer.server.get_local_port())
	$Instruction/QRCode.texture = Helpers.create_qr_code(self_addr)

func touch_game(metadata: SPJModuleMetadata):
	$Outer/Inner/VBoxContainer/GameInfo/Name.text = metadata.display_name
	$Outer/Inner/VBoxContainer/GameInfo/Description.text = metadata.description
	$Outer/Inner/VBoxContainer/GameInfo/Players.text = metadata.describe_player_count()

func play_game(metadata: SPJModuleMetadata):
	if metadata.player_count_satisfied(Websocket.get_player_count()):
		print("satisfied!")
	else:
		SPJ.alert("Insufficient player count", "You do not have enough players to play this game.")

func add_player(player: SPJPlayer, initial: bool = false):
	var p = preload("res://global/lobby/lobbyplayer.tscn").instantiate()
	p.player = player
	$Players.add_child(p)
	if not initial:
		p.flash()