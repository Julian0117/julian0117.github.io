extends RigidBody3D

@export var float_force : float = 100.0
@export var water_torque : float = 150.0
@export var stability_exponent : float = 2.5
@export var water_damping : float = 0.1
@export var ocean_radius : float = 400.0
@export var wave_settings: WaveParameters = preload("res://Resources/WaveSettings.tres")

var original_float_force: float
var is_sinking: bool = false
#@onready var health := $"HealthSystem (Node3D)"
@onready var health: HealthSystem = get_node_or_null("HealthSystem")

@onready var planet = get_node("/root/World/Planet")
@onready var float_points_container := $"FloatPoints"

var float_points: Array[Node3D] = []

func _ready():
	original_float_force = float_force
	
	for child in float_points_container.get_children():
		if child is Node3D and child.is_in_group("FloatPoints"):
			float_points.append(child)
			
	if health:
		health.died.connect(_on_ship_died)
		health.respawned.connect(_on_ship_respawned)
	

func get_wave_y_offset(world_pos: Vector3, time: float) -> float:
	var total_y = 0.0
	
	for i in range(wave_settings.wave_count):
		var dir_rad = deg_to_rad(wave_settings.wave_directions[i])
		var direction = Vector2(sin(dir_rad), cos(dir_rad))
		
		var frequency_dir = wave_settings.wave_frequencies[i] * direction
		var dot_prod = frequency_dir.dot(Vector2(world_pos.x, world_pos.z))
		
		var wave = wave_settings.wave_steepnesses[i] * sin(TAU * dot_prod + (wave_settings.wave_speeds[i] * time))
		total_y += wave
		
	return total_y / float(wave_settings.wave_count)

func _physics_process(_delta):
	if not planet: return
	
	var time = WaveManager.wave_time
	var force_per_point = float_force / float_points.size()
	for point in float_points:
		var p_pos = point.global_position
		var up_direction = (p_pos - planet.global_position).normalized()
		var current_distance = p_pos.distance_to(planet.global_position)
		
		var wave_offset = get_wave_y_offset(p_pos, time)
		var total_water_radius = ocean_radius + wave_offset
		if current_distance < total_water_radius:
			var immersion_depth = total_water_radius - current_distance
			
			var force = up_direction * immersion_depth * force_per_point
			
			var offset = p_pos - global_position
			
			var point_velocity = linear_velocity + angular_velocity.cross(offset)
			
			var damping = -point_velocity * water_damping
			
			apply_force(force + damping, offset)

func _integrate_forces(state):
	if not planet: return
	
	var up_direction = (global_position - planet.global_position).normalized()
	var ship_up = global_transform.basis.y
	
	var correction_axis = ship_up.cross(up_direction)
	var tilt_magnitude = correction_axis.length() # Wie stark ist die Neigung? (0.0 bis 1.0)
	
	if tilt_magnitude > 0.001:
		# pow(tilt, exponent) sorgt dafür, dass kleine Werte klein bleiben 
		# und große Werte massiv ansteigen.
		var exponential_strength = pow(tilt_magnitude, stability_exponent)
		
		var final_torque = correction_axis.normalized() * exponential_strength * water_torque
		state.angular_velocity += final_torque * state.step
	
	#var damping_factor = clamp(1.0 - water_angular_damping * state.step, 0.0, 1.0)
	#state.angular_velocity *= damping_factor
	
func _on_ship_died(_victim, _attacker):
	is_sinking = true
	var tween = create_tween()
	tween.tween_property(self, "float_force", 3.5, 5.0) 

func _on_ship_respawned():
	is_sinking = false
	float_force = original_float_force
