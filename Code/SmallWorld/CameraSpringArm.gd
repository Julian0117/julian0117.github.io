#camera_spring_arm.gd
extends Node3D

@export var look_sensitivity: float = 0.005
@export var zoom_sensitivity: float = 2.0
@export var min_cam_distance: float = 10.0
@export var max_cam_distance: float = 100.0

# Pitch-Limits (in Grad)
@export_range(-90.0, 90.0) var min_pitch_deg: float = -80.0
@export_range(-90.0, 90.0) var max_pitch_deg: float = 20.0

@onready var spring_arm := $SpringArm3D
@onready var planet := %Planet
@onready var ship := $"../PlayerShip/Rigidbody3D"

var camera_yaw: float = 0.0
var camera_pitch: float = 0.0
var surface_basis: Basis = Basis.IDENTITY

var local_offset: Vector3 = Vector3.ZERO

func _ready() -> void:
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

	if ship:
		local_offset = ship.global_transform.basis.inverse() * (global_position - ship.global_position)

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		camera_yaw -= event.relative.x * look_sensitivity
		camera_pitch -= event.relative.y * look_sensitivity
		# Pitch limitieren
		camera_pitch = clamp(camera_pitch, deg_to_rad(min_pitch_deg), deg_to_rad(max_pitch_deg))

	if event.is_action_pressed("mouse_wheel_up"):
		spring_arm.spring_length = max(min_cam_distance, spring_arm.spring_length - zoom_sensitivity)
	if event.is_action_pressed("mouse_wheel_down"):
		spring_arm.spring_length = min(max_cam_distance, spring_arm.spring_length + zoom_sensitivity)

	if event.is_action_pressed("toggle_mouse_capture"):
		if Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _physics_process(_delta: float) -> void:
	if not ship or not planet: return

	var current_offset_world = ship.global_transform.basis * local_offset
	global_position = ship.global_position + current_offset_world

	var gravity_up = (ship.global_position - planet.global_position).normalized()

	var align_quat = Quaternion(surface_basis.y, gravity_up)
	if align_quat.is_finite():
		surface_basis = Basis(align_quat) * surface_basis
		surface_basis = surface_basis.orthonormalized()

	var final_basis = surface_basis.rotated(surface_basis.y, camera_yaw)
	final_basis = final_basis.rotated(final_basis.x, camera_pitch)

	global_transform.basis = final_basis.orthonormalized()
