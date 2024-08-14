extends Node2D

var __players: Array

func play_sfx(sound: String):
	get_node(sound).play()

func load_module_class(name: String):
	return load("res://games/" + name + "/module.gd")

func switch_module(name: String, players: Array):
	var module = load_module_class(name)
	for p in players:
		p.module = module.new()
	self.__players = players
	get_tree().change_scene_to_file.call_deferred("res://games/" + name + "/root.tscn")

func get_players():
	return __players

func alert(title: String, text: String):
	var dialog = preload("res://global/dialog/dialog.tscn").instantiate()
	$Overlays.add_child(
		dialog
			.title(title)
			.text(text)
			.alert("OK")
	)
