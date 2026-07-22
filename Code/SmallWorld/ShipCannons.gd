extends Node3D

@export_group("Combat Settings")
@export var ball_scene: PackedScene = preload("res://Prefabs/Cannonball.tscn")
@export var shoot_force: float = 50.0
@export var firing_angle: float = 60.0
@export var shoot_cooldown: float = 2.5

@export var cannon_sounds: Array[AudioStream] = []

var target_node: Node3D = null

var cannon_left_ready = true
var cannon_right_ready = true

@export_group("Visuals & Physics")
@export var aim_smoothing: float = 15.0

@onready var planet = get_node("/root/World/Planet")
@onready var cannon_left = $"PortsideCannon"
@onready var cannon_right = $"StarboardCannon"
@onready var aim_pivot_left = $PortsideCannon/AimPivot
@onready var aim_line_left = $"PortsideCannon/AimPivot/AimLine"
@onready var aim_pivot_right = $"StarboardCannon/AimPivot"
@onready var aim_line_right = $"StarboardCannon/AimPivot/AimLine"

@export var smoke_scene: PackedScene = preload("res://Prefabs/CannonSmoke.tscn")

@export var camera: Camera3D = null
@onready var audio_source := $AudioStreamPlayer3D

@onready var smoothed_aim_dir_left: Vector3 = -global_transform.basis.x 
@onready var smoothed_aim_dir_right: Vector3 = global_transform.basis.x

func _physics_process(delta: float):
	if not planet: return
	
	var planet_up = (global_position - planet.global_position).normalized()
	var input_dir: Vector3 = Vector3.ZERO

	if camera:
		input_dir = camera.global_transform.basis.z
	elif target_node:
		var dir_to_enemy = (target_node.global_position - global_position).normalized()
		input_dir = -dir_to_enemy
	else:
		aim_line_left.visible = false
		aim_line_right.visible = false
		return

	var target_dir = input_dir.slide(planet_up).normalized()

	# Glättung (Lerp)
	smoothed_aim_dir_right = smoothed_aim_dir_right.lerp(target_dir, delta * aim_smoothing).normalized()
	smoothed_aim_dir_left = smoothed_aim_dir_left.lerp(target_dir, delta * aim_smoothing).normalized()
	
	update_aim_visuals(planet_up)

func update_aim_visuals(up_vector: Vector3):
	var ship_right = global_transform.basis.x
	
	var angle_right = smoothed_aim_dir_right.angle_to(ship_right)
	var angle_left = smoothed_aim_dir_left.angle_to(-ship_right)

	aim_line_right.visible = cannon_right_ready and angle_right < deg_to_rad(firing_angle / 2)
	aim_line_left.visible = cannon_left_ready and angle_left < deg_to_rad(firing_angle / 2)

	if aim_line_right.visible:
		aim_pivot_right.look_at(aim_pivot_right.global_position + smoothed_aim_dir_right, up_vector)
	if aim_line_left.visible:
		aim_pivot_left.look_at(aim_pivot_left.global_position + smoothed_aim_dir_left, up_vector)

func attempt_fire():
	if aim_line_right.visible:
		_execute_fire_sequence("right")
	elif aim_line_left.visible:
		_execute_fire_sequence("left")

# Hilfsfunktion um Redundanz beim Cooldown zu vermeiden
func _execute_fire_sequence(side: String):
	if side == "right":
		cannon_right_ready = false
		fire(cannon_right, smoothed_aim_dir_right)
		await get_tree().create_timer(shoot_cooldown).timeout
		cannon_right_ready = true
	else:
		cannon_left_ready = false
		fire(cannon_left, smoothed_aim_dir_left)
		await get_tree().create_timer(shoot_cooldown).timeout
		cannon_left_ready = true

func fire(cannon_node: Node3D, shoot_dir: Vector3):
	# Play sound effect
	var random_index = randi() % cannon_sounds.size()
	audio_source.stream = cannon_sounds[random_index]
	audio_source.pitch_scale = randf_range(0.9, 1.1)
	audio_source.play()
	
	# Play particle effect
	var smoke = smoke_scene.instantiate()
	cannon_node.get_node("AimPivot").add_child(smoke)
	var smoke_particles = smoke.get_child(0)
	var total_life = smoke_particles.lifetime - 0.1
	get_tree().create_timer(total_life / 2).timeout.connect(smoke.queue_free)
	
	# Spawn and launch cannonball
	var ball = ball_scene.instantiate()
	ball.attacker = get_parent().get_parent()
	get_tree().root.add_child(ball)
	ball.global_position = cannon_node.global_position
	
	ball.apply_central_impulse(-shoot_dir * shoot_force)
	
	if "linear_velocity" in get_parent():
		ball.apply_central_impulse(get_parent().linear_velocity * ball.mass)
