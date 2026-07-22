extends Node3D

enum State { PATROL, CHASE, ATTACK }

@export_group("AI Settings")
@export var attack_range: float = 40.0
@export var stop_range: float = 20.0
@export var patrol_speed: float = 0.5

@export_group("Obstacle Avoidance")
@export var ray_length: float = 40.0
@export var avoid_turn_strength: float = 1.0
## Zeigt die Raycasts im Spiel als Linien an (Grün = Frei, Rot = Treffer)
@export var show_debug_rays: bool = true

@onready var detection_area = $"../TargetDetection (Area3D)"
@onready var movement = $"../ShipMovement"
@onready var cannons = $"../ShipCannons"
@onready var planet = get_node("/root/World/Planet")

var current_target: Node3D = null
var current_state = State.PATROL
var patrol_timer: float = 0.0
var random_steer: float = 0.0

# --- DEBUG VARIABLEN ---
var debug_mesh: ImmediateMesh
var debug_mesh_instance: MeshInstance3D

func _ready():
	detection_area.body_entered.connect(_on_detection_area_body_entered)
	detection_area.body_exited.connect(_on_detection_area_body_exited)

	# Initialisierung des Debug-Zeichners
	if show_debug_rays:
		debug_mesh = ImmediateMesh.new()
		debug_mesh_instance = MeshInstance3D.new()
		debug_mesh_instance.mesh = debug_mesh

		# Material erstellen, das nicht auf Licht reagiert (Unshaded) und Vertex-Farben nutzt
		var mat = StandardMaterial3D.new()
		mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
		mat.vertex_color_use_as_albedo = true
		debug_mesh_instance.material_override = mat

		get_tree().root.call_deferred("add_child", debug_mesh_instance)

func _physics_process(delta: float):
	if not movement or not planet: return

	if current_target:
		cannons.target_node = current_target
		var dist = global_position.distance_to(current_target.global_position)
		if dist < attack_range:
			current_state = State.ATTACK
		else:
			current_state = State.CHASE
	else:
		cannons.target_node = null
		current_state = State.PATROL

	match current_state:
		State.PATROL:
			_process_patrol(delta)
		State.CHASE:
			_process_chase(delta)
		State.ATTACK:
			_process_attack(delta)

	_apply_obstacle_avoidance()

# --- ZUSTANDS LOGIK ---

func _process_patrol(delta: float):
	patrol_timer -= delta
	if patrol_timer <= 0:
		patrol_timer = randf_range(3.0, 7.0)
		random_steer = randf_range(-0.3, 0.3)

	movement.set("throttle_input", patrol_speed)
	movement.set("steering_input", random_steer)

func _process_chase(_delta: float):
	var target_dir = _get_flat_direction_to(current_target.global_position)
	_steer_towards(target_dir)
	movement.set("throttle_input", 1.0)

func _process_attack(_delta: float):
	var planet_up = (global_position - planet.global_position).normalized()
	var target_dir = _get_flat_direction_to(current_target.global_position)
	var orbit_dir = planet_up.cross(target_dir).normalized()

	_steer_towards(orbit_dir)
	var dist = global_position.distance_to(current_target.global_position)
	movement.set("throttle_input", 0.7 if dist > stop_range else 0.0)

	if cannons:
		cannons.attempt_fire()


# --- AUSWEICH LOGIK ---

func _apply_obstacle_avoidance():
	var space_state = get_world_3d().direct_space_state
	var ship = get_parent_node_3d()

	var start_pos = ship.global_position# + (ship_up * 3.0)

	var forward = ship.global_transform.basis.z
	var right = ship.global_transform.basis.x

	var dir_fwd = forward
	var dir_left = (forward - right * 0.6).normalized()
	var dir_right = (forward + right * 0.6).normalized()

	var hit_left = _check_whisker(space_state, ship, start_pos, dir_left * ray_length)
	var hit_right = _check_whisker(space_state, ship, start_pos, dir_right * ray_length)
	var hit_fwd = _check_whisker(space_state, ship, start_pos, dir_fwd * ray_length)

	# --- DEBUG ZEICHNEN ---
	if show_debug_rays and debug_mesh:
		debug_mesh.clear_surfaces()
		debug_mesh.surface_begin(Mesh.PRIMITIVE_LINES)
		_draw_debug_line(start_pos, start_pos + dir_left * ray_length, hit_left)
		_draw_debug_line(start_pos, start_pos + dir_fwd * ray_length, hit_fwd)
		_draw_debug_line(start_pos, start_pos + dir_right * ray_length, hit_right)
		debug_mesh.surface_end()

	var avoiding = false
	var steer_override = 0.0

	if hit_fwd:
		steer_override = -avoid_turn_strength if hit_right else avoid_turn_strength
		avoiding = true
	elif hit_left:
		steer_override = avoid_turn_strength
		avoiding = true
	elif hit_right:
		steer_override = -avoid_turn_strength
		avoiding = true

	if avoiding:
		movement.set("steering_input", steer_override)
		movement.set("throttle_input", 0.4)

func _check_whisker(space_state: PhysicsDirectSpaceState3D, ship: Node3D, start_pos: Vector3, offset: Vector3) -> bool:
	var end_pos = start_pos + offset
	var query = PhysicsRayQueryParameters3D.create(start_pos, end_pos)
	if ship is CollisionObject3D:
		query.exclude = [ship.get_rid()]

	var result = space_state.intersect_ray(query)
	if result and result.collider.is_in_group("Obstacles"):
		#print(self.name, " hat Obstacle '", result.collider.	name, "' erkannt")
		return true
	return false

# Hilfsfunktion für das Debug-Zeichnen
func _draw_debug_line(start: Vector3, end: Vector3, has_hit: bool):
	if has_hit:
		debug_mesh.surface_set_color(Color.RED) # Hindernis erkannt
	else:
		debug_mesh.surface_set_color(Color.GREEN) # Weg ist frei

	debug_mesh.surface_add_vertex(start)
	debug_mesh.surface_add_vertex(end)

# --- HILFSFUNKTIONEN FÜR KUGEL-NAVIGATION ---

func _get_flat_direction_to(target_pos: Vector3) -> Vector3:
	var planet_up = (global_position - planet.global_position).normalized()
	var dir = (target_pos - global_position).normalized()
	return dir.slide(planet_up).normalized()

func _steer_towards(target_dir: Vector3):
	var ship_forward = get_parent_node_3d().global_transform.basis.z
	var ship_up = (global_position - planet.global_position).normalized()
	var angle_diff = ship_forward.signed_angle_to(target_dir, ship_up)
	movement.set("steering_input", clamp(angle_diff * 2.0, -1.0, 1.0))

# --- DETECTION SIGNALE ---

func _on_detection_area_body_entered(body: Node3D):
	if body == get_parent(): return
	if body.is_in_group("Targets"):
		current_target = body
		#print(self.get_parent_node_3d().name, ": Ziel erkannt -> ", body.name)

func _on_detection_area_body_exited(body: Node3D):
	if body == current_target:
		current_target = null
		#print(self.get_parent_node_3d().name, ": Ziel verloren -> ", body.name)

func _exit_tree():
	if is_instance_valid(debug_mesh_instance):
		debug_mesh_instance.queue_free()
