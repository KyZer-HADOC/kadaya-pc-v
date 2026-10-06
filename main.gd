extends Node3D
var panel: Panel

func _ready():
    panel = $HUD/Panel
    $HUD/Top/MapButton.pressed.connect(_show_map)
    $HUD/Top/SkillsButton.pressed.connect(_show_skills)
    $HUD/Top/PlayButton.pressed.connect(_close_panels)
    $HUD/Panel/Close.pressed.connect(_close_panels)
    _build_world()

func _input(event):
    if event is InputEventKey and event.pressed and not event.echo:
        if event.keycode == KEY_K: _show_skills()
        elif event.keycode == KEY_M: _show_map()
        elif event.keycode == KEY_ESCAPE: _close_panels()

func _show_skills():
    panel.visible = true
    panel.get_node("Header").text = "SKILLS"
    panel.get_node("Body").text = "SHADOW DASH     [UNLOCKED]\nHold SHIFT to move at high speed.\n\nWIND STEP        [LOCKED]\nAir movement technique.\n\nRASEN STRIKE     [LOCKED]\nA powerful chakra-style attack.\n\nSHADOW CLONE    [LOCKED]\nCreate a temporary decoy.\n\nSkill points: 0"

func _show_map():
    panel.visible = true
    panel.get_node("Header").text = "WORLD MAP"
    panel.get_node("Body").text = "KADAYA — ACT I\n\n[01] SHADOW VILLAGE       ★ CURRENT\n[02] BAMBOO PASS          ○ LOCKED\n[03] FORGOTTEN SHRINE     ○ LOCKED\n[04] ASHEN FORTRESS       ○ LOCKED\n\nProgress: 1 / 4 areas discovered"

func _close_panels():
    panel.visible = false

func _build_world():
    var ground_mat = StandardMaterial3D.new()
    ground_mat.albedo_color = Color(0.10,0.13,0.10)
    for x in range(-8,9):
        for z in range(-8,9):
            var box = MeshInstance3D.new()
            var mesh = BoxMesh.new()
            mesh.size = Vector3(1,0.4,1)
            box.mesh = mesh
            box.material_override = ground_mat
            box.position = Vector3(x,-0.25,z)
            add_child(box)
    var body = StaticBody3D.new()
    var collision = CollisionShape3D.new()
    var shape = BoxShape3D.new()
    shape.size = Vector3(17,0.4,17)
    collision.shape = shape
    body.position = Vector3(0,-0.25,0)
    body.add_child(collision)
    add_child(body)
    for p in [Vector3(-4,0,-3),Vector3(4,0,-5),Vector3(-5,0,5),Vector3(5,0,4)]:
        var rock = MeshInstance3D.new()
        var sphere = SphereMesh.new()
        sphere.radius = 1.0
        sphere.height = 1.7
        rock.mesh = sphere
        var rm = StandardMaterial3D.new()
        rm.albedo_color = Color(0.16,0.18,0.20)
        rock.material_override = rm
        rock.position = p
        add_child(rock)
