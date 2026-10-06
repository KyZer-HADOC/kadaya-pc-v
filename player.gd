extends CharacterBody3D

const SPEED = 5.0
const JUMP = 6.0
var gravity = 16.0

func _physics_process(delta):
    var input_vec = Input.get_vector("move_left","move_right","move_forward","move_back")
    var dir = Vector3(input_vec.x,0,input_vec.y)
    if dir.length() > 0:
        velocity.x = dir.x * SPEED
        velocity.z = dir.z * SPEED
        rotation.y = lerp_angle(rotation.y, atan2(-dir.x,-dir.z), 0.18)
    else:
        velocity.x = move_toward(velocity.x,0,SPEED*8*delta)
        velocity.z = move_toward(velocity.z,0,SPEED*8*delta)
    if not is_on_floor():
        velocity.y -= gravity * delta
    elif Input.is_key_pressed(KEY_SPACE):
        velocity.y = JUMP
    move_and_slide()
