extends Node3D
#class_name ShipMovement

@export_group("Movement Settings")
@export var engine_force : float = 60.0
@export var brake_force : float = 0.0
@export var turn_torque : float = 150.0

@onready var planet = get_node("/root/World/Planet")
@onready var ship = get_parent() as RigidBody3D

var throttle_input: float = 0.0 # 0.0 bis 1.0
var brake_input: float = 0.0    # 0.0 bis 1.0
var steering_input: float = 0.0 # -1.0 bis 1.0

func _physics_process(_delta: float) -> void:
	if not ship or not planet:
		return

	var forward_dir = ship.global_transform.basis.z
	var up_direction = (ship.global_position - planet.global_position).normalized()

	# --- VORWÄRTS ---
	if throttle_input > 0.0:
		ship.apply_central_force(forward_dir * engine_force * throttle_input)
		
	# --- BREMSEN ---
	if brake_input > 0.0:
		if ship.linear_velocity.length() > 0.2:
			ship.apply_central_force(-ship.linear_velocity.normalized() * brake_force * brake_input)

	# --- LENKEN ---
	if steering_input != 0:
		ship.apply_torque(up_direction * steering_input * turn_torque)
