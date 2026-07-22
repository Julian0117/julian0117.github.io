extends HBoxContainer

@onready var attacker_label = $AttackerLabel
@onready var victim_label = $VictimLabel

@export var display_time: float = 5.0

func setup(attacker_name: String, victim_name: String):
	attacker_label.text = attacker_name
	victim_label.text = victim_name

func _ready():
	modulate.a = 0.0
	var tween_in = create_tween()
	tween_in.tween_property(self, "modulate:a", 1.0, 0.2)

	await get_tree().create_timer(display_time - 0.5).timeout

	var tween_out = create_tween()
	tween_out.tween_property(self, "modulate:a", 0.0, 0.5)

	await tween_out.finished
	queue_free()
