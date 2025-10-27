extends Node2D

# Test configuration
var test_name: String = "Movement System Tests"
var auto_run: bool = true
var test_duration: float = 5.0

# Test state
var tests_passed: int = 0
var tests_failed: int = 0
var tests_total: int = 0
var current_test: String = ""

# References
var game_controller
var test_sprite: Sprite2D
var results_label: Label

func _ready():
	setup_ui()
	game_controller = get_node_or_null("GameController")

	if game_controller == null:
		push_error("GameController not found! Add it to the scene.")
		return

	test_sprite = get_node_or_null("TestSprite")

	if auto_run:
		await get_tree().create_timer(0.5).timeout
		run_all_tests()
	else:
		show_manual_instructions()

func setup_ui():
	# Create results label if it doesn't exist
	results_label = get_node_or_null("UI/TestResults")
	if results_label == null:
		var ui = Node2D.new()
		ui.name = "UI"
		add_child(ui)

		results_label = Label.new()
		results_label.name = "TestResults"
		results_label.position = Vector2(20, 20)
		results_label.add_theme_font_size_override("font_size", 24)
		ui.add_child(results_label)

func run_all_tests():
	print("\n=== Starting %s ===" % test_name)
	results_label.text = "Running tests..."
	results_label.modulate = Color.YELLOW

	# Run each test
	await test_entity_creation()
	await test_movement_right()
	await test_movement_left()
	await test_movement_up()
	await test_movement_down()
	await test_movement_stop()

	# Show results
	display_results()

func test_entity_creation():
	current_test = "Entity Creation"
	tests_total += 1

	if game_controller.TestEntityExists():
		test_pass()
	else:
		test_fail("Test entity was not created")

func test_movement_right():
	current_test = "Move Right"
	tests_total += 1

	var start_pos = game_controller.GetTestEntityPosition()

	# Simulate right arrow key
	simulate_key(KEY_RIGHT)
	await get_tree().create_timer(1.0).timeout

	var end_pos = game_controller.GetTestEntityPosition()

	if end_pos.x > start_pos.x + 50:  # Should move ~100 pixels in 1 second
		test_pass()
	else:
		test_fail("Entity did not move right (start: %.1f, end: %.1f)" % [start_pos.x, end_pos.x])

	# Stop movement
	simulate_key(KEY_SPACE)
	await get_tree().create_timer(0.1).timeout

func test_movement_left():
	current_test = "Move Left"
	tests_total += 1

	var start_pos = game_controller.GetTestEntityPosition()

	simulate_key(KEY_LEFT)
	await get_tree().create_timer(1.0).timeout

	var end_pos = game_controller.GetTestEntityPosition()

	if end_pos.x < start_pos.x - 50:
		test_pass()
	else:
		test_fail("Entity did not move left")

	simulate_key(KEY_SPACE)
	await get_tree().create_timer(0.1).timeout

func test_movement_up():
	current_test = "Move Up"
	tests_total += 1

	var start_pos = game_controller.GetTestEntityPosition()

	simulate_key(KEY_UP)
	await get_tree().create_timer(1.0).timeout

	var end_pos = game_controller.GetTestEntityPosition()

	if end_pos.y < start_pos.y - 50:
		test_pass()
	else:
		test_fail("Entity did not move up")

	simulate_key(KEY_SPACE)
	await get_tree().create_timer(0.1).timeout

func test_movement_down():
	current_test = "Move Down"
	tests_total += 1

	var start_pos = game_controller.GetTestEntityPosition()

	simulate_key(KEY_DOWN)
	await get_tree().create_timer(1.0).timeout

	var end_pos = game_controller.GetTestEntityPosition()

	if end_pos.y > start_pos.y + 50:
		test_pass()
	else:
		test_fail("Entity did not move down")

	simulate_key(KEY_SPACE)
	await get_tree().create_timer(0.1).timeout

func test_movement_stop():
	current_test = "Stop Movement"
	tests_total += 1

	# Start moving right
	simulate_key(KEY_RIGHT)
	await get_tree().create_timer(0.5).timeout

	# Stop
	simulate_key(KEY_SPACE)
	await get_tree().create_timer(0.1).timeout

	var pos1 = game_controller.GetTestEntityPosition()
	await get_tree().create_timer(0.5).timeout
	var pos2 = game_controller.GetTestEntityPosition()

	if pos1.distance_to(pos2) < 1.0:
		test_pass()
	else:
		test_fail("Entity did not stop (moved %.1f pixels)" % pos1.distance_to(pos2))

# Helper methods
func simulate_key(keycode: Key):
	var event = InputEventKey.new()
	event.keycode = keycode
	event.pressed = true
	Input.parse_input_event(event)

	await get_tree().create_timer(0.05).timeout

	event.pressed = false
	Input.parse_input_event(event)

func get_entity_position(entity, world) -> Vector2:
	if world.Has(entity, typeof(entity)):  # Check if entity is alive
		# Access Position component (C# interop)
		# Note: This requires the entity to have Position component
		# Godot can't directly access C# ref structs, so we read from sprite
		if test_sprite:
			return test_sprite.position
	return Vector2.ZERO

func test_pass():
	tests_passed += 1
	print("  ✅ PASS: %s" % current_test)

func test_fail(reason: String = ""):
	tests_failed += 1
	print("  ❌ FAIL: %s%s" % [current_test, (" - " + reason) if reason else ""])

func display_results():
	var success_rate = (float(tests_passed) / tests_total) * 100 if tests_total > 0 else 0.0

	var result_text = """TEST RESULTS

Total: %d
Passed: %d ✅
Failed: %d ❌
Success: %.0f%%""" % [tests_total, tests_passed, tests_failed, success_rate]

	results_label.text = result_text

	var summary = "\n=== TEST SUMMARY ==="
	summary += "\nTotal: %d | Passed: %d | Failed: %d | Success: %.1f%%" % [
		tests_total, tests_passed, tests_failed, success_rate
	]
	print(summary)
	print("===================\n")

	if tests_failed == 0:
		results_label.modulate = Color.GREEN
		print("🎉 All tests passed!")
	else:
		results_label.modulate = Color.RED
		print("⚠️ Some tests failed!")

func show_manual_instructions():
	var instructions = """MOVEMENT TEST (Manual Mode)

Press Arrow Keys to move
Press Space to stop
Watch the sprite move

Press F5 to run automated tests
Press ESC to exit"""

	results_label.text = instructions
	results_label.modulate = Color.WHITE

func _input(event):
	if event.is_action_pressed("ui_cancel"):  # ESC
		get_tree().change_scene_to_file("res://main.tscn")
	elif event is InputEventKey and event.pressed and event.keycode == KEY_F5:
		get_tree().reload_current_scene()
