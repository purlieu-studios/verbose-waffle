# Godot Scene Testing Standards

## Testing Philosophy

**Two-Layer Testing Strategy:**

### Layer 1: Backend Logic (xUnit)
✅ **Already Implemented**
- Pure C# unit tests
- Fast execution (<2 seconds for 152 tests)
- No Godot runtime required
- Run in CI/CD
- Test game rules, calculations, ECS systems

**Location:** `apps/CookingProject.Logic.Tests/`

---

### Layer 2: Frontend/Scene Testing (Godot)
**What we're standardizing now**
- Visual validation
- ECS ↔ Godot integration
- User input handling
- Scene setup verification

**Location:** `apps/game/tests/scenes/`

---

## Scene Test Structure

### Directory Layout
```
apps/game/tests/
├─ scenes/
│  ├─ test_movement.tscn         # Movement system tests
│  ├─ test_movement.gd           # Test script
│  ├─ test_cooking.tscn          # Cooking system tests
│  ├─ test_cooking.gd
│  ├─ test_sharpening.tscn       # Sharpening system tests
│  ├─ test_sharpening.gd
│  └─ _test_template.tscn        # Template for new tests
└─ scripts/
   ├─ TestRunner.gd              # Base test runner class
   └─ TestReporter.gd            # Visual test results display
```

---

## Test Scene Standards

### 1. Scene Naming
**Pattern:** `test_<feature>.tscn`

**Examples:**
- `test_movement.tscn` - Movement system
- `test_cooking.tscn` - Cooking mechanics
- `test_collision.tscn` - Collision handling
- `test_inventory.tscn` - Inventory UI

---

### 2. Scene Structure

**Every test scene should have:**

```
TestMovement (Node2D)
├─ GameController               # ECS backend integration
├─ TestRunner (script)          # Runs automated tests
├─ UI                          # Visual feedback
│  ├─ TestResults (Label)      # Shows pass/fail
│  ├─ Instructions (Label)     # Manual test instructions
│  └─ StatusIndicator (Panel)  # Visual status
└─ TestEntities                # Entities being tested
   ├─ TestEntity1
   ├─ TestEntity2
   └─ ...
```

---

### 3. Test Script Template

**Base structure for all test scripts:**

```gdscript
extends Node2D

# Test configuration
var test_name: String = "Movement System Tests"
var auto_run: bool = true  # Run tests on scene load
var test_duration: float = 5.0  # Max test time

# Test state
var tests_passed: int = 0
var tests_failed: int = 0
var tests_total: int = 0
var current_test: String = ""

# References
@onready var game_controller: GameController = $GameController
@onready var test_results: Label = $UI/TestResults
@onready var instructions: Label = $UI/Instructions

func _ready():
	if auto_run:
		await get_tree().create_timer(0.5).timeout  # Let scene initialize
		run_all_tests()
	else:
		show_manual_instructions()

func run_all_tests():
	print("=== Starting %s ===" % test_name)
	test_results.text = "Running tests..."

	# Run each test
	await test_movement_right()
	await test_movement_left()
	await test_movement_stop()

	# Show results
	display_results()

# Individual test methods
func test_movement_right():
	current_test = "Move Right"
	tests_total += 1

	# Setup
	var entity = game_controller._testMovingEntity
	var start_pos = get_entity_position(entity)

	# Execute
	simulate_input(KEY_RIGHT)
	await get_tree().create_timer(1.0).timeout

	# Assert
	var end_pos = get_entity_position(entity)
	if end_pos.x > start_pos.x:
		test_pass()
	else:
		test_fail("Entity did not move right")

func test_movement_left():
	current_test = "Move Left"
	tests_total += 1

	var entity = game_controller._testMovingEntity
	var start_pos = get_entity_position(entity)

	simulate_input(KEY_LEFT)
	await get_tree().create_timer(1.0).timeout

	var end_pos = get_entity_position(entity)
	if end_pos.x < start_pos.x:
		test_pass()
	else:
		test_fail("Entity did not move left")

func test_movement_stop():
	current_test = "Stop Movement"
	tests_total += 1

	var entity = game_controller._testMovingEntity

	# Start moving
	simulate_input(KEY_RIGHT)
	await get_tree().create_timer(0.5).timeout

	# Stop
	simulate_input(KEY_SPACE)
	await get_tree().create_timer(0.1).timeout

	var pos1 = get_entity_position(entity)
	await get_tree().create_timer(0.5).timeout
	var pos2 = get_entity_position(entity)

	if pos1.distance_to(pos2) < 0.1:
		test_pass()
	else:
		test_fail("Entity did not stop moving")

# Helper methods
func simulate_input(key: Key):
	var event = InputEventKey.new()
	event.keycode = key
	event.pressed = true
	Input.parse_input_event(event)
	await get_tree().create_timer(0.1).timeout
	event.pressed = false
	Input.parse_input_event(event)

func get_entity_position(entity) -> Vector2:
	var world = game_controller._gameFacade.World
	if world.Has[Position](entity):
		var position = world.Get[Position](entity)
		return Vector2(position.Value.X, position.Value.Y)
	return Vector2.ZERO

func test_pass():
	tests_passed += 1
	print("✅ PASS: %s" % current_test)

func test_fail(reason: String = ""):
	tests_failed += 1
	print("❌ FAIL: %s - %s" % [current_test, reason])

func display_results():
	var result_text = """
	=== TEST RESULTS ===
	Total: %d
	Passed: %d ✅
	Failed: %d ❌
	Success Rate: %.1f%%
	""" % [tests_total, tests_passed, tests_failed,
	       (float(tests_passed) / tests_total) * 100]

	test_results.text = result_text
	print(result_text)

	if tests_failed == 0:
		test_results.modulate = Color.GREEN
	else:
		test_results.modulate = Color.RED

func show_manual_instructions():
	instructions.text = """
	MANUAL TEST INSTRUCTIONS:

	1. Press Arrow Keys to move
	2. Press Space to stop
	3. Verify sprite moves correctly
	4. Check console for logs

	Press F5 to re-run automated tests
	"""
	instructions.visible = true

func _input(event):
	if event.is_action_pressed("ui_cancel"):  # ESC key
		get_tree().change_scene_to_file("res://main.tscn")
	elif event.is_action_pressed("ui_select"):  # F5 key
		get_tree().reload_current_scene()
```

---

## Test Types

### Type 1: Automated Tests
**Characteristics:**
- ✅ Run on scene load
- ✅ No manual input needed
- ✅ Print results to console
- ✅ Show visual pass/fail

**Use for:**
- System integration checks
- Regression testing
- Quick validation

**Example:** Movement system test runs automatically and reports results

---

### Type 2: Manual Validation Tests
**Characteristics:**
- ✅ Show instructions on screen
- ✅ Require player interaction
- ✅ Visual verification
- ✅ Useful for feel/polish

**Use for:**
- Animation quality
- Sound effects
- Visual feedback
- Game feel validation

**Example:** Cooking scene shows "Press 1 to place food, verify it sizzles"

---

### Type 3: Stress Tests
**Characteristics:**
- ✅ Create many entities
- ✅ Measure performance
- ✅ Check for memory leaks
- ✅ Validate at scale

**Use for:**
- Performance testing
- Finding bottlenecks
- Scalability validation

**Example:** Create 1000 moving entities and measure FPS

---

## Running Tests

### Option 1: Direct Scene Launch
1. Open test scene in Godot
2. Press F6 (run current scene)
3. Watch automated tests
4. Review console output

### Option 2: Test Menu Scene
Create a master test scene:
```
tests/test_menu.tscn
- Lists all test scenes
- Click to run specific test
- Shows test status history
```

### Option 3: Command Line (Future)
```bash
godot --headless --script run_tests.gd
```

---

## Visual Standards

### UI Elements in Test Scenes

**Test Results Label:**
- Font size: 24
- Position: Top-left
- Colors: Green (pass), Red (fail), Yellow (running)

**Status Indicators:**
- ✅ Green checkmark = Passed
- ❌ Red X = Failed
- ⏳ Yellow spinner = Running

**Instructions Panel:**
- Font size: 16
- Position: Top-right
- Background: Semi-transparent black
- Always readable

---

## Integration with Backend Tests

### Data Flow
```
Backend Test (xUnit)
    ↓
Tests pure logic
    ↓
✅ Logic verified
    ↓
Frontend Test (Godot)
    ↓
Tests ECS ↔ Godot integration
    ↓
✅ Integration verified
```

### Example: Movement System
**Backend Test:**
```csharp
[Fact]
public void MovementSystem_UpdatesPosition()
{
    var entity = _world.Create(new Position(0, 0), new Velocity(10, 0));
    _system.Update(1.0f);
    ref var pos = ref _world.Get<Position>(entity);
    pos.Value.X.Should().Be(10f);  // ✅ Logic works
}
```

**Frontend Test:**
```gdscript
func test_sprite_follows_ecs_position():
    var entity = game_controller._testMovingEntity
    var sprite = get_sprite_for_entity(entity)

    # Move entity via ECS
    simulate_input(KEY_RIGHT)
    await get_tree().create_timer(1.0).timeout

    # Verify Godot sprite updated
    if sprite.position.x > 100:
        test_pass()  # ✅ Integration works
```

---

## Best Practices

### DO:
✅ Keep tests focused on one feature
✅ Use descriptive test names
✅ Print clear pass/fail messages
✅ Clean up entities after tests
✅ Add visual feedback for results
✅ Document manual test steps
✅ Make tests re-runnable

### DON'T:
❌ Test backend logic in scenes (use xUnit)
❌ Make tests dependent on each other
❌ Hardcode entity IDs/references
❌ Skip cleanup (memory leaks)
❌ Test Godot engine features (collision, physics)
❌ Mix multiple features in one test

---

## Maintenance

### When to Update Tests
- ✅ After adding new features
- ✅ After fixing bugs (add regression test)
- ✅ When refactoring presentation layer
- ✅ When changing ECS ↔ Godot sync logic

### Test Review Checklist
- [ ] Tests run without errors
- [ ] Results are clearly visible
- [ ] Console output is informative
- [ ] Manual instructions are clear
- [ ] Scene cleanup works
- [ ] Tests complete in reasonable time (<5 seconds)

---

## Future Enhancements

### Planned Improvements
1. **GUT Integration** - Godot Unit Testing framework
2. **Screenshot Comparison** - Visual regression testing
3. **Performance Metrics** - FPS tracking in tests
4. **CI/CD Integration** - Automated scene testing
5. **Test Report Generator** - HTML test results

### Experimental
- GDAI MCP automated testing
- Headless scene execution
- Video recording of tests
