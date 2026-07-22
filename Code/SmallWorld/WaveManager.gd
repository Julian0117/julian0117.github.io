#WaveManager.gd
@tool
extends Node

@export var settings: WaveParameters = preload("res://Resources/WaveSettings.tres")
var active_material: ShaderMaterial = null
var wave_time: float = 0.0

func _process(delta: float) -> void:
	wave_time += delta
	
	if active_material:
		active_material.set_shader_parameter("wave_time", wave_time)

func _update_shader(mat: ShaderMaterial):
	mat.set_shader_parameter("WaveCount", settings.wave_count)
	mat.set_shader_parameter("WaveSteepnesses", settings.wave_steepnesses)
	mat.set_shader_parameter("WaveAmplitudes", settings.wave_amplitudes)
	mat.set_shader_parameter("WaveDirectionsDegrees", settings.wave_directions)
	mat.set_shader_parameter("WaveFrequencies", settings.wave_frequencies)
	mat.set_shader_parameter("WaveSpeeds", settings.wave_speeds)

# Hilfsfunktion, damit das Wasser-Mesh sich beim Manager anmelden kann
func register_water_material(mat: ShaderMaterial):
	active_material = mat
	_update_shader(mat)
