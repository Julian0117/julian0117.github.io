extends Node
class_name HealthSystem

signal health_changed(current_health, max_health)
signal died(victim_node, attacker_node)
signal respawned

@export var max_health: float = 100.0
var current_health: float
var ship_root: Node3D

@export var trail: MeshInstance3D
@export var min_trail_lifetime: float = 0.0
var original_trail_lifetime: float

func _ready():
	get_parent().set_meta("health_system", self)
	current_health = max_health
	original_trail_lifetime = trail.lifetime
	ship_root = get_parent().get_parent()
	died.connect(ScoreManager.register_kill)
	respawned.connect(func(): if trail: trail.lifetime = original_trail_lifetime)

func take_damage(amount: float, attacker: Node3D):
	if current_health <= 0: return

	current_health -= amount
	health_changed.emit(current_health, max_health)

	# Trail-Länge proportional zur Gesundheit anpassen
	_update_trail_visuals()

	if current_health <= 0:
		current_health = 0
		died.emit(ship_root, attacker)

func _update_trail_visuals():
	if not trail: return

	var health_pct = current_health / max_health

	trail.lifetime = lerp(min_trail_lifetime, original_trail_lifetime, health_pct)

func heal_to_full():
	current_health = max_health
	health_changed.emit(current_health, max_health)
	respawned.emit()
