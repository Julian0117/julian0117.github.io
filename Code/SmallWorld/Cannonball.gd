extends RigidBody3D

@export_group("Combat")
@export var damage: float = 20.0
@export var explosion_force: float = 10.0
@export var life_time: float = 10.0
@export var grav_force: float = 9.8

@export_group("Visuals")
@export var impact_effect_scene: PackedScene = preload("res://Prefabs/CannonImpact.tscn")

@export_group("Audio")
@export var impact_sounds: Array[AudioStream] = []
@export var impact_sound_scene: PackedScene = preload("res://Prefabs/ImpactSound.tscn")

var attacker: Node3D = null:
	set(value):
		attacker = value
		for child in attacker.get_children():
			if child is RigidBody3D:
				attacker_body = child
				break

var attacker_body: RigidBody3D
var planet: Node3D

func _ready():
	body_entered.connect(_on_body_entered)
	planet = get_tree().get_first_node_in_group("Planet")
	await get_tree().create_timer(life_time).timeout
	queue_free()

#func _physics_process(delta):
	#if not planet: return
	#
	## Planetare Gravitation für die Kugel
	#var dir_to_planet = (planet.global_position - global_position).normalized()
	#apply_central_force(dir_to_planet * grav_force * mass)

func _on_body_entered(body):
	if body.has_meta("health_system") and body != attacker_body:
		#print(body, " getroffen!")
		spawn_impact_visuals(global_position)
		play_impact_sound(global_position)
		body.get_meta("health_system").take_damage(damage, attacker)
		queue_free()
		
func spawn_impact_visuals(impact_position):
	var impact_visuals = impact_effect_scene.instantiate()

	get_tree().root.add_child(impact_visuals)
	impact_visuals.global_position = impact_position
	get_tree().create_timer(0.45).timeout.connect(impact_visuals.queue_free)
	
func play_impact_sound(impact_position):
	var audio_player = impact_sound_scene.instantiate()
	var random_index = randi() % impact_sounds.size()
	audio_player.stream = impact_sounds[random_index]
	audio_player.pitch_scale = randf_range(0.9, 1.1)

	get_tree().root.add_child(audio_player)
	audio_player.global_position = impact_position
	
	audio_player.finished.connect(audio_player.queue_free)

	audio_player.play()
