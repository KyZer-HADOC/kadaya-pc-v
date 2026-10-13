extends Node3D

var panel: Panel
var camera: Camera3D
var player: CharacterBody3D

func _ready():
    panel = $HUD/Panel
    camera = $Camera
    player = $Player
    $HUD/Top/MapButton.pressed.connect(_show_map)
    $HUD/Top/SkillsButton.pressed.connect(_show_skills)
    $HUD/Top/PlayButton.pressed.connect(_close_panels)
    $HUD/Panel/Close.pressed.connect(_close_panels)
    _build_world()

func _process(delta):
    if player:
        var target = player.global_position + Vector3(0, 1.0, 0)
        var desired = target + Vector3(0, 6.0, 9.0)
        camera.global_position = camera.global_position.lerp(desired, min(delta * 5.0, 1.0))
        camera.look_at(target, Vector3.UP)

func _input(event):
    if event is InputEventKey and event.pressed and not event.echo:
        if event.keycode == KEY_K: _show_skills()
        elif event.keycode == KEY_M: _show_map()
        elif event.keycode == KEY_ESCAPE: _close_panels()

func _show_skills():
    panel.visible = true
    panel.get_node("Header").text = "SHADOW ARTS"
    panel.get_node("Body").text = "01  SHADOW DASH       UNLOCKED\n    Hold SHIFT — burst movement\n\n02  WIND STEP         LOCKED\n    Unlock after Bamboo Pass\n\n03  RASEN STRIKE      LOCKED\n    Unlock after Forgotten Shrine\n\n04  SHADOW CLONE      LOCKED\n    Unlock after Ashen Fortress\n\nSKILL POINTS   0"

func _show_map():
    panel.visible = true
    panel.get_node("Header").text = "WORLD MAP  •  ACT I"
    panel.get_node("Body").text = "                 NORTH\n\n        [03] FORGOTTEN SHRINE\n                  ○\n                  │\n[02] BAMBOO PASS ── ● ── [04] ASHEN FORTRESS\n                  │\n            ★ SHADOW VILLAGE\n                  │\n               YOU ARE HERE\n\nAreas discovered: 1 / 4"

func _close_panels():
    panel.visible = false

func _mat(color: Color, metallic := 0.0, roughness := 0.8) -> StandardMaterial3D:
    var m = StandardMaterial3D.new()
    m.albedo_color = color
    m.metallic = metallic
    m.roughness = roughness
    return m

func _box(pos: Vector3, size: Vector3, mat: Material):
    var n = MeshInstance3D.new()
    var mesh = BoxMesh.new()
    mesh.size = size
    n.mesh = mesh
    n.material_override = mat
    n.position = pos
    add_child(n)
    return n

func _cylinder(pos: Vector3, radius: float, height: float, mat: Material):
    var n = MeshInstance3D.new()
    var mesh = CylinderMesh.new()
    mesh.top_radius = radius
    mesh.bottom_radius = radius * 1.05
    mesh.height = height
    mesh.radial_segments = 8
    n.mesh = mesh
    n.material_override = mat
    n.position = pos
    add_child(n)
    return n

func _tree(pos: Vector3, scale := 1.0):
    _cylinder(pos + Vector3(0,1.1*scale,0), 0.22*scale, 2.2*scale, _mat(Color(0.18,0.10,0.055)))
    var leaf = _cylinder(pos + Vector3(0,2.45*scale,0), 1.0*scale, 2.0*scale, _mat(Color(0.055,0.20,0.10)))
    leaf.rotation_degrees.x = 10
    _cylinder(pos + Vector3(0,3.15*scale,0), 0.7*scale, 1.4*scale, _mat(Color(0.08,0.28,0.13)))

func _build_world():
    var grass = _mat(Color(0.075,0.16,0.10))
    var path = _mat(Color(0.28,0.22,0.16))
    var stone = _mat(Color(0.20,0.22,0.24), 0.05)
    var wood = _mat(Color(0.25,0.12,0.055))
    var roof = _mat(Color(0.10,0.055,0.07))
    var red = _mat(Color(0.45,0.035,0.045))
    
    _box(Vector3(0,-0.25,0), Vector3(30,0.5,30), grass)
    _box(Vector3(0,0.02,0), Vector3(4,0.06,30), path)
    _box(Vector3(0,0.04,0), Vector3(30,0.08,4), path)
    
    for x in range(-13,14,3):
        _tree(Vector3(x,0,-11), 0.9)
        _tree(Vector3(x,0,11), 0.75)
    for z in range(-8,9,4):
        _tree(Vector3(-12,0,z), 0.8)
        _tree(Vector3(12,0,z), 1.0)
    
    for p in [Vector3(-6,0,-4),Vector3(7,0,-5),Vector3(-7,0,6),Vector3(8,0,6)]:
        var rock = _cylinder(p + Vector3(0,0.55,0), 0.8, 1.1, stone)
        rock.scale = Vector3(1.3,1.0,0.9)
    
    # Simple village gate landmark
    _cylinder(Vector3(-2.4,1.8,-6.5), 0.25, 3.6, red)
    _cylinder(Vector3(2.4,1.8,-6.5), 0.25, 3.6, red)
    _box(Vector3(0,3.45,-6.5), Vector3(5.3,0.35,0.35), red)
    _box(Vector3(0,2.8,-6.5), Vector3(4.2,0.25,0.25), wood)
    
    # Small shrine at the north edge
    _box(Vector3(0,0.6,-11.5), Vector3(5,1.2,3), stone)
    _box(Vector3(0,2.0,-11.5), Vector3(5.6,0.25,3.5), roof)
    _box(Vector3(0,2.35,-11.5), Vector3(4.8,0.2,3.0), roof)
    
    var body = StaticBody3D.new()
    var collision = CollisionShape3D.new()
    var shape = BoxShape3D.new()
    shape.size = Vector3(30,0.5,30)
    collision.shape = shape
    body.position = Vector3(0,-0.25,0)
    body.add_child(collision)
    add_child(body)
