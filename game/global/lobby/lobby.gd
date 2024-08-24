extends Control

@onready var GAMES = DirAccess.get_directories_at("res://games/")

func _ready():
	var buttonBase = preload("res://global/lobby/modulebutton.tscn").instantiate()
	for game in self.GAMES:
		var button = buttonBase.duplicate()
		button.metadata = load("res://games/" + game + "/meta.gd").new()
		$Outer/Inner/VBoxContainer/Games/GamesGrid.add_child(button)
		button.focus_entered.connect(self.touch_game.bind(button))
		button.pressed.connect(self.play_game.bind(button))
	for player in GameServer.get_players():
		self.add_player(player, true)
	GameServer.new_player.connect(self.add_player)

func setup():
	var self_ip = Helpers.get_self_ip()
	if self_ip == "":
		SPJ.alert("Cannot display QR code", "Automatic detection of your IP address has failed.\nThe QR code which allows you to quickly join the game cannot be shown.\nPlease connect manually.")
	var self_addr = "http://" + self_ip + ":" + str(HttpServer.server.get_local_port())
	$Instruction/QRCode.texture = Helpers.create_qr_code(self_addr)

func touch_game(button):
	var metadata: SPJModuleMetadata = button.metadata
	$Outer/Inner/VBoxContainer/GameInfo/Name.text = metadata.display_name
	$Outer/Inner/VBoxContainer/GameInfo/Description.text = metadata.description
	$Outer/Inner/VBoxContainer/GameInfo/Players.text = metadata.describe_player_count()

func play_game(button):
	var metadata: SPJModuleMetadata = button.metadata
	if metadata.player_count_satisfied(GameServer.get_player_count()):
		self.launch_game(button)
	else:
		SPJ.alert("Insufficient player count",
			 "You do not have enough players to play this game."
			)

func launch_game(button):
	SPJ.play_sfx("select")
	var players = []
	for p in GameServer.get_players():
		if not (p as SPJPlayer).is_spectator():
			players.append(p)
	SPJ.switch_module("dice", players)

func add_player(player: SPJPlayer, initial: bool = false):
	var p = preload("res://global/lobby/lobbyplayer.tscn").instantiate()
	p.player = player
	$Players.add_child(p)
	if not initial:
		p.flash()
