extends CanvasLayer

@export var feed_item_scene: PackedScene = preload("res://Prefabs/KillFeedItem.tscn")

@onready var feed_container = $MarginContainer/VBoxContainer

@onready var win_screen = $WinScreen
@onready var message_label = $WinScreen/CenterContainer/MessageLabel

func _ready():
	ScoreManager.ship_sunk_feed.connect(_on_ship_sunk)
	ScoreManager.win_condition_reached.connect(_on_win_condition)
	
	win_screen.visible = false
	win_screen.modulate.a = 0.0

func _on_ship_sunk(victim: Node3D, attacker: Node3D):
	var feed_item = feed_item_scene.instantiate()
	
	feed_container.add_child(feed_item)
	
	var attacker_name = attacker.name if attacker else "Unknown"
	var victim_name = victim.name if victim else "Unknown"
	
	feed_item.setup(attacker_name, victim_name)
	
func _on_win_condition(winner: Node3D):
	if win_screen.visible:
		return
		
	var winner_name = winner.name if winner else "Unknown"
	message_label.text = winner_name + " Wins!"
	
	win_screen.visible = true
	
	var tween = create_tween()
	tween.tween_property(win_screen, "modulate:a", 1.0, 1.5)
	
	tween.finished.connect(_restart_game)
	
func _restart_game():
	return
	#await get_tree().create_timer(15.0).timeout
	#get_tree().reload_current_scene()
