@tool
extends Node3D

@export var align = true

func _process(_delta):
	if Engine.is_editor_hint():
		var planet = _find_planet_in_editor()

		if planet and align:
			var target_pos = planet.global_transform.origin
			look_at(target_pos, Vector3.UP)
			rotate_object_local(Vector3.RIGHT, deg_to_rad(90))

func _find_planet_in_editor() -> Node3D:
	# Sucht das erste Objekt in der Gruppe "planets" innerhalb der aktuellen Szene
	var nodes = get_tree().get_nodes_in_group("Planet")
	if nodes.size() > 0:
		return nodes[0] as Node3D
	return null
