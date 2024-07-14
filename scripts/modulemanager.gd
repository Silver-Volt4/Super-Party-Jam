extends Node

var __players: Array

func load_module_class(name: String):
	return load("res://games/" + name + "/module.gd")

func switch_module(name: String, players: Array):
	var module = load_module_class(name)
	for p in players:
		p.module = module.new()
	self.__players = players
	get_tree().change_scene_to_file("res://games/" + name + "/root.tscn")

func get_players():
	return __players
