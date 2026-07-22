@tool # Unverzichtbar für Editor-Vorschau
extends MeshInstance3D

@export var settings: WaveParameters:
	set(value):
		if settings:
			settings.changed.disconnect(_on_settings_changed)
		settings = value
		if settings:
			settings.changed.connect(_on_settings_changed)
			_on_settings_changed()

func _ready() -> void:
	_on_settings_changed()
	
	# Für das fertige Spiel melden wir uns trotzdem beim Manager an
	if not Engine.is_editor_hint():
		var mat = get_active_material(0)
		if mat is ShaderMaterial:
			WaveManager.register_water_material(mat)

func _on_settings_changed():
	var mat = get_active_material(0)
	if mat is ShaderMaterial and settings:
		mat.set_shader_parameter("WaveCount", settings.wave_count)
		mat.set_shader_parameter("WaveSteepnesses", settings.wave_steepnesses)
		mat.set_shader_parameter("WaveAmplitudes", settings.wave_amplitudes)
		mat.set_shader_parameter("WaveDirectionsDegrees", settings.wave_directions)
		mat.set_shader_parameter("WaveFrequencies", settings.wave_frequencies)
		mat.set_shader_parameter("WaveSpeeds", settings.wave_speeds)
