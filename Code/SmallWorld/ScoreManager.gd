# ScoreManager.gd (Autoload)
extends Node

signal score_updated(ship_node, new_score)
signal ship_sunk_feed(victim_node, attacker_node)
signal win_condition_reached(winner_node)

@export var score_to_win: int = 500
@export var points_per_kill: int = 100

# Dictionary { Schiff_Node : Punktzahl }
var scores: Dictionary = {}

func register_kill(victim: Node3D, attacker: Node3D):
	if not attacker or victim == attacker:
		return

	#scores[attacker] += points_per_kill
	print("--", attacker.name, "-- versenkt --", victim.name, "--!")
	add_score(attacker, points_per_kill)

	# Signale für das UI aussenden
	ship_sunk_feed.emit(victim, attacker)

func add_score(ship: Node3D, amount: int):
	# Schiff im Dictionary anlegen, falls es noch keine Punkte hat
	if not scores.has(ship):
		scores[ship] = 0
	scores[ship] += amount
	print("--", ship.name, "-- erhält ", amount, " Punkte! Gesamt: ", scores[ship])
	score_updated.emit(ship, scores[ship])

	# Win-Condition prüfen
	if scores[ship] >= score_to_win:
		print(">-----", ship, "-----< gewinnt die Partie!!!")
		win_condition_reached.emit(ship)
