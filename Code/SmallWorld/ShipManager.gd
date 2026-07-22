extends Node3D

@onready var health = $"Rigidbody3D/HealthSystem"
@onready var movement = $"Rigidbody3D/ShipMovement"
@onready var spawn_transform: Transform3D = global_transform

@export var respawn_time: float = 10.0

@onready var rb := $"Rigidbody3D"

func _ready():
	if health:
		health.died.connect(_on_death)

func _on_death(_victim, _attacker):
	print(name, " wurde versenkt!")

	if movement:
		movement.set_process(false)
		movement.set_physics_process(false)

	# Warten, während das Schiff sinkt
	await get_tree().create_timer(respawn_time).timeout

	respawn()

func respawn():
	rb.linear_velocity = Vector3.ZERO
	rb.angular_velocity = Vector3.ZERO

	rb.global_transform = spawn_transform

	health.heal_to_full()

	# Skripte wieder aktivieren
	if movement:
		movement.set_process(true)
		movement.set_physics_process(true)

	print(name, " ist respawnt!")
