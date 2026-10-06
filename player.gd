extends CharacterBody3D
const SPEED = 5.0
const DASH_SPEED = 15.0
const JUMP = 6.0
var gravity = 16.0
var dash_time = 0.0

func _physics_process(delta):
    var x = int(Input.is_key_pressed(KEY_D)) - int(Input.is_key_pressed(KEY_A))
    var z = int(Input.is_key_pressed(KEY_S)) - int(Input.is_key_pressed(KEY_W))
    var dir = Vector3(x,0,z).normalized()
    var speed = SPEED
    if Input.is_key_pressed(KEY_SHIFT):
        speed = DASH_SPEED
    if dir.length() > 0:
        velocity.x = dir.x * speed
        velocity.z = dir.z * speed
        rotation.y = lerp_angle(rotation.y, atan2(-dir.x,-dir.z), 0.18)
    else:
        velocity.x = move_toward(velocity.x,0,SPEED*8*delta)
        velocity.z = move_toward(velocity.z,0,SPEED*8*delta)
    if not is_on_floor():
        velocity.y -= gravity * delta
    elif Input.is_key_pressed(KEY_SPACE):
        velocity.y = JUMP
    move_and_slide()
