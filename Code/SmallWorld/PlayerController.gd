extends Node

@onready var movement = $"../ShipMovement"
@onready var cannons = $"../ShipCannons"

func _physics_process(_delta):
	if not movement: return

	movement.throttle_input = Input.get_action_strength("move_forward")
	movement.brake_input = Input.get_action_strength("move_backward")

	movement.steering_input = Input.get_axis("move_right", "move_left")

func _input(event):
	if event.is_action_pressed("shoot"):
		cannons.attempt_fire()
