# camera_follow.gd (verbessert)
extends Camera3D

@export var target_node: Node3D # Dein SpringArm oder das SpringPosition-Node
@export var lerp_speed: float = 6.0

func _ready() -> void:
	# Macht die Kamera unabhängig von den Bewegungen ihres Parents
	set_as_top_level(true)

func _physics_process(delta: float) -> void:
	if not target_node:
		return

	global_position = global_position.lerp(target_node.global_position, delta * lerp_speed)
	
	var q_current = global_transform.basis.get_rotation_quaternion()
	var q_target = target_node.global_transform.basis.get_rotation_quaternion()
	
	var q_next = q_current.slerp(q_target, delta * lerp_speed)
	global_transform.basis = Basis(q_next)
