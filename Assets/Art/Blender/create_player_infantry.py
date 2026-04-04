import math
import bpy


COLLECTION_NAME = "PrototypeInfantry"
ROOT_NAME = "PlayerInfantryRoot"


def ensure_collection(name):
    collection = bpy.data.collections.get(name)
    if collection is None:
        collection = bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(collection)
    return collection


def clear_collection(collection):
    for obj in list(collection.objects):
        bpy.data.objects.remove(obj, do_unlink=True)


def make_material(name, color, metallic=0.0, roughness=0.5):
    material = bpy.data.materials.get(name)
    if material is None:
        material = bpy.data.materials.new(name=name)

    material.use_nodes = True
    nodes = material.node_tree.nodes
    links = material.node_tree.links

    principled = None
    output = None
    for node in nodes:
        if node.type == "BSDF_PRINCIPLED":
            principled = node
        elif node.type == "OUTPUT_MATERIAL":
            output = node

    if principled is None:
        principled = nodes.new(type="ShaderNodeBsdfPrincipled")
        principled.location = (0, 0)

    if output is None:
        output = nodes.new(type="ShaderNodeOutputMaterial")
        output.location = (300, 0)

    linked = False
    for link in links:
        if link.from_node == principled and link.to_node == output:
            linked = True
            break

    if not linked:
        links.new(principled.outputs["BSDF"], output.inputs["Surface"])

    principled.inputs["Base Color"].default_value = color
    principled.inputs["Metallic"].default_value = metallic
    principled.inputs["Roughness"].default_value = roughness
    return material


def assign_material(obj, material):
    if not obj.data.materials:
        obj.data.materials.append(material)
    else:
        obj.data.materials[0] = material


def add_object_to_collection(obj, collection):
    for linked in list(obj.users_collection):
        linked.objects.unlink(obj)
    collection.objects.link(obj)


def set_parent_keep_transform(obj, parent):
    world_matrix = obj.matrix_world.copy()
    obj.parent = parent
    obj.matrix_world = world_matrix


def set_parent_local_transform(obj, parent, location=(0.0, 0.0, 0.0), rotation=None):
    obj.parent = parent
    obj.matrix_parent_inverse = parent.matrix_world.inverted()
    obj.location = location
    if rotation is not None:
        obj.rotation_euler = rotation


def add_bevel(obj, width=0.015, segments=2):
    modifier = obj.modifiers.new(name="Bevel", type="BEVEL")
    modifier.width = width
    modifier.segments = segments


def add_subsurf(obj, levels=1):
    modifier = obj.modifiers.new(name="Subsurf", type="SUBSURF")
    modifier.levels = levels
    modifier.render_levels = levels


def smooth_object(obj):
    if obj.type != "MESH":
        return
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.shade_smooth()
    obj.select_set(False)


def finalize_mesh(obj, bevel=None, subsurf=None, smooth=False):
    if bevel is not None:
        add_bevel(obj, width=bevel, segments=2)
    if subsurf is not None:
        add_subsurf(obj, levels=subsurf)
    if smooth:
        smooth_object(obj)
    return obj


def create_cube(name, location, scale, collection, parent=None, rotation=None, material=None):
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    if rotation is not None:
        obj.rotation_euler = rotation
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    if parent is not None:
        set_parent_keep_transform(obj, parent)
    if material is not None:
        assign_material(obj, material)
    add_object_to_collection(obj, collection)
    return obj


def create_cylinder(
    name,
    location,
    scale,
    collection,
    parent=None,
    rotation=None,
    material=None,
    vertices=16,
):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    if rotation is not None:
        obj.rotation_euler = rotation
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    if parent is not None:
        set_parent_keep_transform(obj, parent)
    if material is not None:
        assign_material(obj, material)
    add_object_to_collection(obj, collection)
    return obj


def create_uv_sphere(name, location, scale, collection, parent=None, material=None, segments=24, rings=12):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if parent is not None:
        set_parent_keep_transform(obj, parent)
    if material is not None:
        assign_material(obj, material)
    add_object_to_collection(obj, collection)
    return obj


def create_cone(name, location, scale, collection, parent=None, rotation=None, material=None, vertices=16):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    if rotation is not None:
        obj.rotation_euler = rotation
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    if parent is not None:
        set_parent_keep_transform(obj, parent)
    if material is not None:
        assign_material(obj, material)
    add_object_to_collection(obj, collection)
    return obj


def create_plane(name, location, scale, collection, parent=None, rotation=None, material=None):
    bpy.ops.mesh.primitive_plane_add(location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    if rotation is not None:
        obj.rotation_euler = rotation
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    if parent is not None:
        set_parent_keep_transform(obj, parent)
    if material is not None:
        assign_material(obj, material)
    add_object_to_collection(obj, collection)
    return obj


def build_leg(side, root, pelvis, collection, armor_mat, trim_mat, undersuit_mat):
    sign = -1.0 if side == "L" else 1.0
    x = 0.085 * sign

    hip = finalize_mesh(create_cube(
        f"HipGuard_{side}",
        location=(x, 0.018, 0.775),
        scale=(0.038, 0.024, 0.078),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(3.0), 0.0, math.radians(7.0 * sign)),
        material=armor_mat,
    ), bevel=0.008)

    thigh = finalize_mesh(create_cylinder(
        f"Thigh_{side}",
        location=(x, -0.004, 0.57),
        scale=(0.043, 0.043, 0.148),
        collection=collection,
        parent=root,
        rotation=(math.radians(3.0), math.radians(2.0 * sign), math.radians(3.0 * sign)),
        material=undersuit_mat,
        vertices=18,
    ), smooth=True)

    thigh_plate = finalize_mesh(create_cube(
        f"ThighPlate_{side}",
        location=(x, 0.028, 0.57),
        scale=(0.03, 0.016, 0.10),
        collection=collection,
        parent=root,
        rotation=(math.radians(4.0), 0.0, math.radians(5.0 * sign)),
        material=armor_mat,
    ), bevel=0.004)

    thigh_side = finalize_mesh(create_cube(
        f"ThighSide_{side}",
        location=(x + 0.032 * sign, 0.0, 0.57),
        scale=(0.01, 0.032, 0.09),
        collection=collection,
        parent=root,
        rotation=(0.0, math.radians(6.0 * -sign), math.radians(4.0 * sign)),
        material=trim_mat,
    ), bevel=0.003)

    knee = finalize_mesh(create_uv_sphere(
        f"Knee_{side}",
        location=(x, 0.016, 0.405),
        scale=(0.036, 0.03, 0.036),
        collection=collection,
        parent=root,
        material=trim_mat,
        segments=16,
        rings=10,
    ), smooth=True)

    shin = finalize_mesh(create_cylinder(
        f"Shin_{side}",
        location=(x, -0.014, 0.245),
        scale=(0.036, 0.036, 0.15),
        collection=collection,
        parent=root,
        rotation=(math.radians(-6.0), math.radians(1.0 * sign), 0.0),
        material=armor_mat,
        vertices=18,
    ), smooth=True)

    shin_plate = finalize_mesh(create_cube(
        f"ShinPlate_{side}",
        location=(x, 0.016, 0.248),
        scale=(0.026, 0.014, 0.11),
        collection=collection,
        parent=root,
        rotation=(math.radians(-8.0), 0.0, math.radians(4.0 * sign)),
        material=trim_mat,
    ), bevel=0.003)

    knee_guard = finalize_mesh(create_cube(
        f"KneeGuard_{side}",
        location=(x, 0.03, 0.41),
        scale=(0.028, 0.014, 0.022),
        collection=collection,
        parent=root,
        rotation=(math.radians(-16.0), 0.0, math.radians(4.0 * sign)),
        material=armor_mat,
    ), bevel=0.003)

    boot_main = finalize_mesh(create_cube(
        f"BootMain_{side}",
        location=(x, 0.045, 0.064),
        scale=(0.034, 0.056, 0.036),
        collection=collection,
        parent=root,
        rotation=(math.radians(6.0), 0.0, math.radians(4.0 * sign)),
        material=armor_mat,
    ), bevel=0.008)

    boot_toe = finalize_mesh(create_cube(
        f"BootToe_{side}",
        location=(x, 0.098, 0.048),
        scale=(0.032, 0.036, 0.022),
        collection=collection,
        parent=root,
        rotation=(math.radians(10.0), 0.0, math.radians(4.0 * sign)),
        material=armor_mat,
    ), bevel=0.008)

    boot_upper = finalize_mesh(create_cube(
        f"BootUpper_{side}",
        location=(x, 0.03, 0.15),
        scale=(0.031, 0.034, 0.038),
        collection=collection,
        parent=root,
        rotation=(math.radians(-4.0), 0.0, math.radians(4.0 * sign)),
        material=armor_mat,
    ), bevel=0.006)

    boot_side_l = finalize_mesh(create_cube(
        f"BootSideInner_{side}",
        location=(x - 0.018 * sign, 0.046, 0.082),
        scale=(0.006, 0.036, 0.024),
        collection=collection,
        parent=root,
        rotation=(math.radians(6.0), 0.0, math.radians(4.0 * sign)),
        material=trim_mat,
    ), bevel=0.003)

    boot_side_r = finalize_mesh(create_cube(
        f"BootSideOuter_{side}",
        location=(x + 0.018 * sign, 0.046, 0.082),
        scale=(0.006, 0.036, 0.024),
        collection=collection,
        parent=root,
        rotation=(math.radians(6.0), 0.0, math.radians(4.0 * sign)),
        material=trim_mat,
    ), bevel=0.003)

    boot_heel = finalize_mesh(create_cube(
        f"BootHeel_{side}",
        location=(x, 0.0, 0.05),
        scale=(0.028, 0.022, 0.024),
        collection=collection,
        parent=root,
        rotation=(math.radians(4.0), 0.0, math.radians(4.0 * sign)),
        material=armor_mat,
    ), bevel=0.006)

    boot_guard = finalize_mesh(create_cube(
        f"BootGuard_{side}",
        location=(x, 0.05, 0.106),
        scale=(0.03, 0.04, 0.018),
        collection=collection,
        parent=root,
        rotation=(math.radians(-12.0), 0.0, math.radians(4.0 * sign)),
        material=trim_mat,
    ), bevel=0.004)

    boot_ankle = finalize_mesh(create_uv_sphere(
        f"BootAnkle_{side}",
        location=(x, 0.01, 0.12),
        scale=(0.032, 0.03, 0.028),
        collection=collection,
        parent=root,
        material=armor_mat,
        segments=16,
        rings=10,
    ), smooth=True, subsurf=1)

    foot = boot_main

    set_parent_keep_transform(thigh, hip)
    set_parent_keep_transform(thigh_plate, thigh)
    set_parent_keep_transform(thigh_side, thigh)
    set_parent_keep_transform(knee, thigh)
    set_parent_keep_transform(knee_guard, thigh)
    set_parent_keep_transform(shin, thigh)
    set_parent_keep_transform(shin_plate, shin)
    set_parent_keep_transform(boot_ankle, shin)
    set_parent_keep_transform(boot_main, shin)
    set_parent_keep_transform(boot_toe, boot_main)
    set_parent_keep_transform(boot_heel, boot_main)
    set_parent_keep_transform(boot_guard, boot_main)
    set_parent_keep_transform(boot_upper, shin)
    set_parent_keep_transform(boot_side_l, boot_main)
    set_parent_keep_transform(boot_side_r, boot_main)

    return {"hip": hip, "thigh": thigh, "shin": shin, "foot": foot}


def build_arm(side, root, torso, collection, armor_mat, trim_mat, undersuit_mat):
    sign = -1.0 if side == "L" else 1.0
    shoulder_x = 0.17 * sign
    is_left = side == "L"

    upper_arm_location = (-0.196, 0.012, 0.922) if is_left else (0.214, 0.024, 0.916)
    upper_arm_rotation = (
        (math.radians(34.0), math.radians(16.0), math.radians(-32.0))
        if is_left
        else (math.radians(42.0), math.radians(-18.0), math.radians(34.0))
    )
    elbow_location = (-0.238, 0.072, 0.824) if is_left else (0.258, 0.112, 0.812)
    forearm_location = (-0.256, 0.126, 0.694) if is_left else (0.304, 0.168, 0.704)
    forearm_rotation = (
        (math.radians(-44.0), math.radians(14.0), math.radians(-26.0))
        if is_left
        else (math.radians(-56.0), math.radians(-20.0), math.radians(34.0))
    )
    wrist_location = (-0.262, 0.164, 0.602) if is_left else (0.332, 0.238, 0.63)
    wrist_rotation = (
        (math.radians(-34.0), math.radians(12.0), math.radians(-22.0))
        if is_left
        else (math.radians(-42.0), math.radians(-22.0), math.radians(28.0))
    )
    hand_location = (-0.258, 0.196, 0.57) if is_left else (0.35, 0.286, 0.596)
    hand_guard_location = (-0.258, 0.214, 0.578) if is_left else (0.352, 0.308, 0.606)
    hand_guard_rotation = (
        (math.radians(-22.0), 0.0, math.radians(-28.0))
        if is_left
        else (math.radians(-26.0), 0.0, math.radians(34.0))
    )
    knuckle_guard_location = (-0.252, 0.236, 0.556) if is_left else (0.348, 0.33, 0.582)
    knuckle_guard_rotation = (
        (math.radians(18.0), 0.0, math.radians(-28.0))
        if is_left
        else (math.radians(20.0), 0.0, math.radians(34.0))
    )
    thumb_location = (-0.286, 0.194, 0.562) if is_left else (0.378, 0.286, 0.594)

    shoulder = finalize_mesh(create_uv_sphere(
        f"Shoulder_{side}",
        location=(shoulder_x, -0.004, 1.03),
        scale=(0.048, 0.062, 0.045),
        collection=collection,
        parent=root,
        material=armor_mat,
        segments=20,
        rings=10,
    ), smooth=True, subsurf=1)

    rim = finalize_mesh(create_cube(
        f"ShoulderRim_{side}",
        location=(shoulder_x, 0.005, 1.035),
        scale=(0.046, 0.014, 0.034),
        collection=collection,
        parent=root,
        rotation=(math.radians(6.0), 0.0, math.radians(6.0 * -sign)),
        material=trim_mat,
    ), bevel=0.004)

    grand_pauldron = finalize_mesh(create_uv_sphere(
        f"GrandPauldron_{side}",
        location=(-0.198, 0.01, 1.0) if is_left else (0.2, 0.006, 1.01),
        scale=(0.072, 0.092, 0.06) if is_left else (0.058, 0.074, 0.05),
        collection=collection,
        parent=root,
        material=armor_mat,
        segments=18,
        rings=10,
    ), smooth=True, subsurf=1)

    pauldron_trim = finalize_mesh(create_cube(
        f"GrandPauldronTrim_{side}",
        location=(-0.206, 0.048, 1.0) if is_left else (0.204, 0.034, 1.006),
        scale=(0.05, 0.014, 0.04) if is_left else (0.04, 0.012, 0.032),
        collection=collection,
        parent=root,
        rotation=(
            (math.radians(-10.0), math.radians(6.0), math.radians(-28.0))
            if is_left
            else (math.radians(-8.0), math.radians(-6.0), math.radians(22.0))
        ),
        material=trim_mat,
    ), bevel=0.003)

    pauldron_flap = finalize_mesh(create_cube(
        f"GrandPauldronFlap_{side}",
        location=(-0.23, 0.022, 0.92) if is_left else (0.222, 0.02, 0.944),
        scale=(0.026, 0.014, 0.062) if is_left else (0.02, 0.012, 0.046),
        collection=collection,
        parent=root,
        rotation=(
            (math.radians(10.0), math.radians(8.0), math.radians(-24.0))
            if is_left
            else (math.radians(8.0), math.radians(-8.0), math.radians(20.0))
        ),
        material=armor_mat,
    ), bevel=0.003)

    upper_arm = finalize_mesh(create_cylinder(
        f"UpperArm_{side}",
        location=upper_arm_location,
        scale=(0.029, 0.029, 0.112),
        collection=collection,
        parent=root,
        rotation=upper_arm_rotation,
        material=undersuit_mat,
        vertices=18,
    ), smooth=True)

    upper_arm_plate = finalize_mesh(create_cube(
        f"UpperArmPlate_{side}",
        location=(-0.204, 0.032, 0.926) if is_left else (0.222, 0.046, 0.918),
        scale=(0.022, 0.014, 0.076),
        collection=collection,
        parent=root,
        rotation=upper_arm_rotation,
        material=armor_mat,
    ), bevel=0.003)

    elbow = finalize_mesh(create_uv_sphere(
        f"Elbow_{side}",
        location=elbow_location,
        scale=(0.028, 0.028, 0.028),
        collection=collection,
        parent=root,
        material=trim_mat,
    ), smooth=True)

    elbow_guard = finalize_mesh(create_cube(
        f"ElbowGuard_{side}",
        location=(-0.244, 0.094, 0.81) if is_left else (0.268, 0.132, 0.796),
        scale=(0.02, 0.014, 0.018),
        collection=collection,
        parent=root,
        rotation=(
            (math.radians(-28.0), math.radians(10.0), math.radians(-22.0))
            if is_left
            else (math.radians(-34.0), math.radians(-14.0), math.radians(26.0))
        ),
        material=armor_mat,
    ), bevel=0.003)

    forearm = finalize_mesh(create_cylinder(
        f"Forearm_{side}",
        location=forearm_location,
        scale=(0.026, 0.026, 0.108),
        collection=collection,
        parent=root,
        rotation=forearm_rotation,
        material=armor_mat,
        vertices=18,
    ), smooth=True)

    forearm_plate = finalize_mesh(create_cube(
        f"ForearmPlate_{side}",
        location=(-0.258, 0.142, 0.708) if is_left else (0.31, 0.19, 0.716),
        scale=(0.02, 0.012, 0.078),
        collection=collection,
        parent=root,
        rotation=forearm_rotation,
        material=trim_mat,
    ), bevel=0.003)

    forearm_side = finalize_mesh(create_cube(
        f"ForearmSide_{side}",
        location=(-0.274, 0.126, 0.694) if is_left else (0.322, 0.166, 0.704),
        scale=(0.008, 0.026, 0.068),
        collection=collection,
        parent=root,
        rotation=forearm_rotation,
        material=armor_mat,
    ), bevel=0.002)

    wrist = finalize_mesh(create_cylinder(
        f"Wrist_{side}",
        location=wrist_location,
        scale=(0.017, 0.017, 0.026),
        collection=collection,
        parent=root,
        rotation=wrist_rotation,
        material=trim_mat,
        vertices=14,
    ), smooth=True, subsurf=1)

    hand = finalize_mesh(create_uv_sphere(
        f"Hand_{side}",
        location=hand_location,
        scale=(0.022, 0.03, 0.022),
        collection=collection,
        parent=root,
        material=undersuit_mat,
        segments=16,
        rings=8,
    ), smooth=True, subsurf=1)

    hand_guard = finalize_mesh(create_cube(
        f"HandGuard_{side}",
        location=hand_guard_location,
        scale=(0.022, 0.014, 0.016),
        collection=collection,
        parent=root,
        rotation=hand_guard_rotation,
        material=armor_mat,
    ), bevel=0.003)

    knuckle_guard = finalize_mesh(create_cube(
        f"KnuckleGuard_{side}",
        location=knuckle_guard_location,
        scale=(0.018, 0.012, 0.01),
        collection=collection,
        parent=root,
        rotation=knuckle_guard_rotation,
        material=trim_mat,
    ), bevel=0.002)

    thumb = finalize_mesh(create_uv_sphere(
        f"Thumb_{side}",
        location=thumb_location,
        scale=(0.01, 0.014, 0.012),
        collection=collection,
        parent=root,
        material=undersuit_mat,
        segments=12,
        rings=8,
    ), smooth=True, subsurf=1)

    palm_plate = finalize_mesh(create_cube(
        f"PalmPlate_{side}",
        location=(-0.246, 0.214, 0.548) if is_left else (0.34, 0.314, 0.574),
        scale=(0.015, 0.012, 0.012),
        collection=collection,
        parent=root,
        rotation=(
            (math.radians(-12.0), 0.0, math.radians(-28.0))
            if is_left
            else (math.radians(-16.0), 0.0, math.radians(34.0))
        ),
        material=trim_mat,
    ), bevel=0.002)

    wrist_cuff = finalize_mesh(create_cube(
        f"WristCuff_{side}",
        location=(-0.262, 0.174, 0.596) if is_left else (0.334, 0.252, 0.626),
        scale=(0.022, 0.01, 0.016),
        collection=collection,
        parent=root,
        rotation=wrist_rotation,
        material=armor_mat,
    ), bevel=0.002)

    set_parent_keep_transform(shoulder, torso)
    set_parent_keep_transform(rim, shoulder)
    set_parent_keep_transform(grand_pauldron, shoulder)
    set_parent_keep_transform(pauldron_trim, grand_pauldron)
    set_parent_keep_transform(pauldron_flap, grand_pauldron)
    set_parent_keep_transform(upper_arm, shoulder)
    set_parent_keep_transform(upper_arm_plate, upper_arm)
    set_parent_keep_transform(elbow, upper_arm)
    set_parent_keep_transform(elbow_guard, upper_arm)
    set_parent_keep_transform(forearm, upper_arm)
    set_parent_keep_transform(forearm_plate, forearm)
    set_parent_keep_transform(forearm_side, forearm)
    set_parent_keep_transform(wrist, forearm)
    set_parent_keep_transform(hand, wrist)
    set_parent_keep_transform(hand_guard, hand)
    set_parent_keep_transform(knuckle_guard, hand)
    set_parent_keep_transform(thumb, hand)
    set_parent_keep_transform(palm_plate, hand)
    set_parent_keep_transform(wrist_cuff, wrist)

    return {"shoulder": shoulder, "forearm": forearm, "hand": hand}


def build_body(root, collection, armor_mat, trim_mat, cloth_mat, weapon_mat, undersuit_mat):
    pelvis = finalize_mesh(create_uv_sphere(
        "Pelvis",
        location=(0.0, -0.004, 0.735),
        scale=(0.1, 0.075, 0.07),
        collection=collection,
        parent=root,
        material=undersuit_mat,
        segments=22,
        rings=12,
    ), smooth=True, subsurf=1)

    belt = finalize_mesh(create_cylinder(
        "Belt",
        location=(0.0, 0.0, 0.79),
        scale=(0.105, 0.078, 0.014),
        collection=collection,
        parent=root,
        material=trim_mat,
        rotation=(math.radians(90.0), 0.0, 0.0),
        vertices=24,
    ), smooth=True)

    abdomen = finalize_mesh(create_uv_sphere(
        "Abdomen",
        location=(0.0, 0.008, 0.855),
        scale=(0.102, 0.078, 0.082),
        collection=collection,
        parent=root,
        material=armor_mat,
        segments=22,
        rings=12,
    ), smooth=True, subsurf=1)

    torso = finalize_mesh(create_uv_sphere(
        "Torso",
        location=(0.0, 0.006, 0.975),
        scale=(0.118, 0.088, 0.135),
        collection=collection,
        parent=root,
        material=armor_mat,
        segments=24,
        rings=14,
    ), smooth=True, subsurf=1)

    clavicle_l = finalize_mesh(create_uv_sphere(
        "Clavicle_L",
        location=(-0.082, -0.003, 1.025),
        scale=(0.04, 0.04, 0.036),
        collection=collection,
        parent=torso,
        material=armor_mat,
        segments=18,
        rings=10,
    ), smooth=True)

    clavicle_r = finalize_mesh(create_uv_sphere(
        "Clavicle_R",
        location=(0.082, -0.003, 1.025),
        scale=(0.04, 0.04, 0.036),
        collection=collection,
        parent=torso,
        material=armor_mat,
        segments=18,
        rings=10,
    ), smooth=True)

    shoulder_base_l = finalize_mesh(create_uv_sphere(
        "ShoulderBase_L",
        location=(-0.115, -0.002, 1.0),
        scale=(0.028, 0.04, 0.045),
        collection=collection,
        parent=torso,
        material=undersuit_mat,
        segments=18,
        rings=10,
    ), smooth=True)

    shoulder_base_r = finalize_mesh(create_uv_sphere(
        "ShoulderBase_R",
        location=(0.115, -0.002, 1.0),
        scale=(0.028, 0.04, 0.045),
        collection=collection,
        parent=torso,
        material=undersuit_mat,
        segments=18,
        rings=10,
    ), smooth=True)

    neck_base = finalize_mesh(create_uv_sphere(
        "NeckBase",
        location=(0.0, 0.0, 1.105),
        scale=(0.062, 0.055, 0.04),
        collection=collection,
        parent=torso,
        material=undersuit_mat,
        segments=18,
        rings=10,
    ), smooth=True)

    gorget_front = finalize_mesh(create_cube(
        "GorgetFront",
        location=(0.0, 0.05, 1.105),
        scale=(0.055, 0.014, 0.03),
        collection=collection,
        parent=torso,
        rotation=(math.radians(-18.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.004)

    collar_l = finalize_mesh(create_uv_sphere(
        "Collar_L",
        location=(-0.07, 0.0, 1.085),
        scale=(0.05, 0.048, 0.03),
        collection=collection,
        parent=torso,
        material=cloth_mat,
        segments=16,
        rings=10,
    ), smooth=True)

    collar_r = finalize_mesh(create_uv_sphere(
        "Collar_R",
        location=(0.07, 0.0, 1.085),
        scale=(0.05, 0.048, 0.03),
        collection=collection,
        parent=torso,
        material=cloth_mat,
        segments=16,
        rings=10,
    ), smooth=True)

    for side in ("L", "R"):
        sign = -1.0 if side == "L" else 1.0

        for index, (x_pos, y_pos, z_pos, sx, sy, sz, rx, rz) in enumerate((
            (0.048, 0.022, 1.065, 0.026, 0.010, 0.016, -20.0, 10.0),
            (0.072, 0.018, 1.052, 0.024, 0.010, 0.016, -16.0, 12.0),
            (0.094, 0.014, 1.035, 0.022, 0.010, 0.015, -12.0, 14.0),
            (0.112, 0.010, 1.012, 0.020, 0.010, 0.014, -8.0, 16.0),
            (0.123, 0.006, 0.988, 0.018, 0.010, 0.013, -4.0, 18.0),
            (0.128, 0.002, 0.962, 0.016, 0.010, 0.012, 0.0, 20.0),
        ), start=1):
            finalize_mesh(create_cube(
                f"ShoulderBridge_{side}_{index}",
                location=(x_pos * sign, y_pos, z_pos),
                scale=(sx, sy, sz),
                collection=collection,
                parent=torso,
                rotation=(math.radians(rx), math.radians(3.0 * -sign), math.radians(rz * sign)),
                material=trim_mat,
            ), bevel=0.003)

        for index, (x_pos, y_pos, z_pos, sx, sy, sz, rx, rz) in enumerate((
            (0.038, 0.034, 1.082, 0.020, 0.008, 0.012, -24.0, 8.0),
            (0.060, 0.038, 1.067, 0.020, 0.008, 0.012, -20.0, 10.0),
            (0.080, 0.040, 1.047, 0.018, 0.008, 0.011, -16.0, 12.0),
            (0.096, 0.040, 1.024, 0.017, 0.008, 0.010, -12.0, 14.0),
        ), start=1):
            finalize_mesh(create_cube(
                f"ShoulderCap_{side}_{index}",
                location=(x_pos * sign, y_pos, z_pos),
                scale=(sx, sy, sz),
                collection=collection,
                parent=torso,
                rotation=(math.radians(rx), 0.0, math.radians(rz * sign)),
                material=armor_mat,
            ), bevel=0.003)

        for index, (y_pos, z_pos, sx, sy, sz, rx) in enumerate((
            (0.024, 1.106, 0.022, 0.008, 0.012, -22.0),
            (0.030, 1.088, 0.020, 0.008, 0.011, -18.0),
            (0.034, 1.070, 0.018, 0.008, 0.010, -14.0),
        ), start=1):
            finalize_mesh(create_cube(
                f"ClavicleScale_{side}_{index}",
                location=(0.026 * sign * index, y_pos, z_pos),
                scale=(sx, sy, sz),
                collection=collection,
                parent=torso,
                rotation=(math.radians(rx), 0.0, math.radians(8.0 * sign)),
                material=trim_mat,
            ), bevel=0.002)

    for index, (y_pos, z_pos, sx, sy, sz, rx) in enumerate((
        (0.028, 1.112, 0.024, 0.008, 0.012, -24.0),
        (0.036, 1.092, 0.022, 0.008, 0.011, -18.0),
        (0.042, 1.072, 0.020, 0.008, 0.010, -14.0),
        (0.046, 1.052, 0.018, 0.008, 0.010, -10.0),
    ), start=1):
        finalize_mesh(create_cube(
            f"CenterCollar_{index}",
            location=(0.0, y_pos, z_pos),
            scale=(sx, sy, sz),
            collection=collection,
            parent=torso,
            rotation=(math.radians(rx), 0.0, 0.0),
            material=trim_mat,
        ), bevel=0.002)

    chest_upper = finalize_mesh(create_cube(
        "ChestUpper",
        location=(0.0, 0.088, 1.03),
        scale=(0.094, 0.016, 0.036),
        collection=collection,
        parent=torso,
        rotation=(math.radians(-24.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.006)

    chest_mid = finalize_mesh(create_cube(
        "ChestMid",
        location=(0.0, 0.095, 0.975),
        scale=(0.088, 0.015, 0.034),
        collection=collection,
        parent=torso,
        rotation=(math.radians(-8.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.006)

    chest_center = finalize_mesh(create_cube(
        "ChestCenter",
        location=(0.0, 0.102, 0.95),
        scale=(0.024, 0.012, 0.055),
        collection=collection,
        parent=torso,
        rotation=(math.radians(4.0), 0.0, 0.0),
        material=weapon_mat,
    ), bevel=0.005)

    chest_panel = finalize_mesh(create_cube(
        "ChestPanelInset",
        location=(0.0, 0.108, 0.972),
        scale=(0.05, 0.006, 0.04),
        collection=collection,
        parent=torso,
        rotation=(math.radians(-10.0), 0.0, 0.0),
        material=armor_mat,
    ), bevel=0.003)

    chest_panel_l = finalize_mesh(create_cube(
        "ChestPanelInset_L",
        location=(-0.044, 0.104, 0.968),
        scale=(0.026, 0.006, 0.034),
        collection=collection,
        parent=torso,
        rotation=(math.radians(-8.0), 0.0, math.radians(16.0)),
        material=armor_mat,
    ), bevel=0.003)

    chest_panel_r = finalize_mesh(create_cube(
        "ChestPanelInset_R",
        location=(0.044, 0.104, 0.968),
        scale=(0.026, 0.006, 0.034),
        collection=collection,
        parent=torso,
        rotation=(math.radians(-8.0), 0.0, math.radians(-16.0)),
        material=armor_mat,
    ), bevel=0.003)

    abdomen_core = finalize_mesh(create_cube(
        "AbdomenCore",
        location=(0.0, 0.092, 0.822),
        scale=(0.032, 0.008, 0.07),
        collection=collection,
        parent=torso,
        rotation=(math.radians(12.0), 0.0, 0.0),
        material=weapon_mat,
    ), bevel=0.003)

    abdomen_side_l = finalize_mesh(create_cube(
        "AbdomenSide_L",
        location=(-0.032, 0.088, 0.822),
        scale=(0.018, 0.007, 0.06),
        collection=collection,
        parent=torso,
        rotation=(math.radians(12.0), 0.0, math.radians(8.0)),
        material=trim_mat,
    ), bevel=0.003)

    abdomen_side_r = finalize_mesh(create_cube(
        "AbdomenSide_R",
        location=(0.032, 0.088, 0.822),
        scale=(0.018, 0.007, 0.06),
        collection=collection,
        parent=torso,
        rotation=(math.radians(12.0), 0.0, math.radians(-8.0)),
        material=trim_mat,
    ), bevel=0.003)

    chest_lower_l = finalize_mesh(create_cube(
        "ChestLower_L",
        location=(-0.04, 0.09, 0.915),
        scale=(0.052, 0.014, 0.026),
        collection=collection,
        parent=torso,
        rotation=(math.radians(18.0), 0.0, math.radians(28.0)),
        material=trim_mat,
    ), bevel=0.006)

    chest_lower_r = finalize_mesh(create_cube(
        "ChestLower_R",
        location=(0.04, 0.09, 0.915),
        scale=(0.052, 0.014, 0.026),
        collection=collection,
        parent=torso,
        rotation=(math.radians(18.0), 0.0, math.radians(-28.0)),
        material=trim_mat,
    ), bevel=0.006)

    chest_flare_l = finalize_mesh(create_cube(
        "ChestFlare_L",
        location=(-0.062, 0.085, 0.972),
        scale=(0.04, 0.012, 0.04),
        collection=collection,
        parent=torso,
        rotation=(math.radians(-4.0), 0.0, math.radians(34.0)),
        material=trim_mat,
    ), bevel=0.005)

    chest_flare_r = finalize_mesh(create_cube(
        "ChestFlare_R",
        location=(0.062, 0.085, 0.972),
        scale=(0.04, 0.012, 0.04),
        collection=collection,
        parent=torso,
        rotation=(math.radians(-4.0), 0.0, math.radians(-34.0)),
        material=trim_mat,
    ), bevel=0.005)

    abdomen_band_1 = finalize_mesh(create_cube(
        "AbdomenBand_1",
        location=(0.0, 0.086, 0.86),
        scale=(0.084, 0.011, 0.02),
        collection=collection,
        parent=torso,
        rotation=(math.radians(12.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.005)

    abdomen_band_2 = finalize_mesh(create_cube(
        "AbdomenBand_2",
        location=(0.0, 0.083, 0.82),
        scale=(0.078, 0.011, 0.018),
        collection=collection,
        parent=torso,
        rotation=(math.radians(14.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.005)

    abdomen_band_3 = finalize_mesh(create_cube(
        "AbdomenBand_3",
        location=(0.0, 0.079, 0.782),
        scale=(0.072, 0.01, 0.017),
        collection=collection,
        parent=torso,
        rotation=(math.radians(16.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.005)

    abdomen_band_4 = finalize_mesh(create_cube(
        "AbdomenBand_4",
        location=(0.0, 0.074, 0.742),
        scale=(0.064, 0.01, 0.015),
        collection=collection,
        parent=torso,
        rotation=(math.radians(18.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.005)

    fauld_l = finalize_mesh(create_cube(
        "Fauld_L",
        location=(-0.05, 0.088, 0.705),
        scale=(0.06, 0.012, 0.03),
        collection=collection,
        parent=torso,
        rotation=(math.radians(20.0), 0.0, math.radians(10.0)),
        material=trim_mat,
    ), bevel=0.004)

    fauld_r = finalize_mesh(create_cube(
        "Fauld_R",
        location=(0.05, 0.088, 0.705),
        scale=(0.06, 0.012, 0.03),
        collection=collection,
        parent=torso,
        rotation=(math.radians(20.0), 0.0, math.radians(-10.0)),
        material=trim_mat,
    ), bevel=0.004)

    chest_seal = finalize_mesh(create_cube(
        "ChestSeal",
        location=(0.0, 0.118, 0.9),
        scale=(0.026, 0.006, 0.04),
        collection=collection,
        parent=torso,
        material=cloth_mat,
    ), bevel=0.004)

    side_plate_l = finalize_mesh(create_cube(
        "SidePlate_L",
        location=(-0.145, 0.0, 0.96),
        scale=(0.02, 0.085, 0.11),
        collection=collection,
        parent=torso,
        rotation=(0.0, math.radians(8.0), math.radians(4.0)),
        material=trim_mat,
    ), bevel=0.005)

    side_plate_r = finalize_mesh(create_cube(
        "SidePlate_R",
        location=(0.145, 0.0, 0.96),
        scale=(0.02, 0.085, 0.11),
        collection=collection,
        parent=torso,
        rotation=(0.0, math.radians(-8.0), math.radians(-4.0)),
        material=trim_mat,
    ), bevel=0.005)

    rib_plate_l = finalize_mesh(create_cube(
        "RibPlate_L",
        location=(-0.13, 0.0, 0.86),
        scale=(0.018, 0.075, 0.07),
        collection=collection,
        parent=torso,
        rotation=(0.0, math.radians(10.0), math.radians(2.0)),
        material=trim_mat,
    ), bevel=0.004)

    rib_plate_r = finalize_mesh(create_cube(
        "RibPlate_R",
        location=(0.13, 0.0, 0.86),
        scale=(0.018, 0.075, 0.07),
        collection=collection,
        parent=torso,
        rotation=(0.0, math.radians(-10.0), math.radians(-2.0)),
        material=trim_mat,
    ), bevel=0.004)

    arm_socket_l = finalize_mesh(create_cube(
        "ArmSocket_L",
        location=(-0.138, 0.022, 0.99),
        scale=(0.026, 0.024, 0.05),
        collection=collection,
        parent=torso,
        rotation=(math.radians(6.0), math.radians(10.0), math.radians(18.0)),
        material=armor_mat,
    ), bevel=0.004)

    arm_socket_r = finalize_mesh(create_cube(
        "ArmSocket_R",
        location=(0.138, 0.022, 0.99),
        scale=(0.026, 0.024, 0.05),
        collection=collection,
        parent=torso,
        rotation=(math.radians(6.0), math.radians(-10.0), math.radians(-18.0)),
        material=armor_mat,
    ), bevel=0.004)

    shoulder_seal_l = finalize_mesh(create_cube(
        "ShoulderSeal_L",
        location=(-0.12, 0.012, 0.94),
        scale=(0.018, 0.022, 0.07),
        collection=collection,
        parent=torso,
        rotation=(math.radians(12.0), math.radians(8.0), math.radians(10.0)),
        material=trim_mat,
    ), bevel=0.003)

    shoulder_seal_r = finalize_mesh(create_cube(
        "ShoulderSeal_R",
        location=(0.12, 0.012, 0.94),
        scale=(0.018, 0.022, 0.07),
        collection=collection,
        parent=torso,
        rotation=(math.radians(12.0), math.radians(-8.0), math.radians(-10.0)),
        material=trim_mat,
    ), bevel=0.003)

    waist_side_l = finalize_mesh(create_cube(
        "WaistSide_L",
        location=(-0.135, 0.0, 0.77),
        scale=(0.018, 0.085, 0.045),
        collection=collection,
        parent=pelvis,
        rotation=(0.0, math.radians(8.0), 0.0),
        material=trim_mat,
    ), bevel=0.004)

    waist_side_r = finalize_mesh(create_cube(
        "WaistSide_R",
        location=(0.135, 0.0, 0.77),
        scale=(0.018, 0.085, 0.045),
        collection=collection,
        parent=pelvis,
        rotation=(0.0, math.radians(-8.0), 0.0),
        material=trim_mat,
    ), bevel=0.004)

    set_parent_keep_transform(chest_upper, torso)
    set_parent_keep_transform(chest_mid, torso)
    set_parent_keep_transform(chest_center, torso)
    set_parent_keep_transform(chest_panel, torso)
    set_parent_keep_transform(chest_panel_l, torso)
    set_parent_keep_transform(chest_panel_r, torso)
    set_parent_keep_transform(chest_lower_l, torso)
    set_parent_keep_transform(chest_lower_r, torso)
    set_parent_keep_transform(chest_flare_l, torso)
    set_parent_keep_transform(chest_flare_r, torso)
    set_parent_keep_transform(abdomen_band_1, torso)
    set_parent_keep_transform(abdomen_band_2, torso)
    set_parent_keep_transform(abdomen_band_3, torso)
    set_parent_keep_transform(abdomen_band_4, torso)
    set_parent_keep_transform(abdomen_core, torso)
    set_parent_keep_transform(abdomen_side_l, torso)
    set_parent_keep_transform(abdomen_side_r, torso)
    set_parent_keep_transform(fauld_l, torso)
    set_parent_keep_transform(fauld_r, torso)
    set_parent_keep_transform(chest_seal, torso)
    set_parent_keep_transform(side_plate_l, torso)
    set_parent_keep_transform(side_plate_r, torso)
    set_parent_keep_transform(rib_plate_l, torso)
    set_parent_keep_transform(rib_plate_r, torso)
    set_parent_keep_transform(arm_socket_l, torso)
    set_parent_keep_transform(arm_socket_r, torso)
    set_parent_keep_transform(shoulder_seal_l, torso)
    set_parent_keep_transform(shoulder_seal_r, torso)
    set_parent_keep_transform(waist_side_l, pelvis)
    set_parent_keep_transform(waist_side_r, pelvis)

    backpack = finalize_mesh(create_cube(
        "BackPack",
        location=(0.0, -0.105, 0.965),
        scale=(0.082, 0.028, 0.12),
        collection=collection,
        parent=root,
        material=weapon_mat,
    ), bevel=0.008)

    back_plate = finalize_mesh(create_cube(
        "BackPlate",
        location=(0.0, -0.078, 0.955),
        scale=(0.096, 0.014, 0.118),
        collection=collection,
        parent=root,
        rotation=(math.radians(6.0), 0.0, 0.0),
        material=armor_mat,
    ), bevel=0.006)

    back_panel = finalize_mesh(create_cube(
        "BackPanelInset",
        location=(0.0, -0.092, 0.96),
        scale=(0.056, 0.006, 0.08),
        collection=collection,
        parent=root,
        rotation=(math.radians(6.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.003)

    backpack_spine = finalize_mesh(create_cube(
        "BackPackSpine",
        location=(0.0, -0.126, 0.965),
        scale=(0.016, 0.008, 0.104),
        collection=collection,
        parent=root,
        material=trim_mat,
    ), bevel=0.003)

    backpack_side_l = finalize_mesh(create_cube(
        "BackPackSide_L",
        location=(-0.07, -0.112, 0.965),
        scale=(0.012, 0.014, 0.096),
        collection=collection,
        parent=root,
        rotation=(0.0, math.radians(4.0), 0.0),
        material=armor_mat,
    ), bevel=0.003)

    backpack_side_r = finalize_mesh(create_cube(
        "BackPackSide_R",
        location=(0.07, -0.112, 0.965),
        scale=(0.012, 0.014, 0.096),
        collection=collection,
        parent=root,
        rotation=(0.0, math.radians(-4.0), 0.0),
        material=armor_mat,
    ), bevel=0.003)

    backpack_mid_band = finalize_mesh(create_cube(
        "BackPackMidBand",
        location=(0.0, -0.132, 0.965),
        scale=(0.068, 0.006, 0.02),
        collection=collection,
        parent=root,
        material=trim_mat,
    ), bevel=0.002)

    backpack_top = finalize_mesh(create_cube(
        "BackPackTop",
        location=(0.0, -0.122, 1.07),
        scale=(0.058, 0.02, 0.028),
        collection=collection,
        parent=root,
        rotation=(math.radians(-8.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.004)

    backpack_lower = finalize_mesh(create_cube(
        "BackPackLower",
        location=(0.0, -0.122, 0.845),
        scale=(0.05, 0.018, 0.03),
        collection=collection,
        parent=root,
        rotation=(math.radians(10.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.004)

    vent_l = finalize_mesh(create_cylinder(
        "BackPackVent_L",
        location=(-0.06, -0.132, 1.015),
        scale=(0.018, 0.018, 0.04),
        collection=collection,
        parent=root,
        rotation=(math.radians(90.0), 0.0, 0.0),
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    vent_r = finalize_mesh(create_cylinder(
        "BackPackVent_R",
        location=(0.06, -0.132, 1.015),
        scale=(0.018, 0.018, 0.04),
        collection=collection,
        parent=root,
        rotation=(math.radians(90.0), 0.0, 0.0),
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    vent_frame_l = finalize_mesh(create_cube(
        "BackPackVentFrame_L",
        location=(-0.06, -0.105, 1.015),
        scale=(0.026, 0.01, 0.045),
        collection=collection,
        parent=root,
        material=weapon_mat,
    ), bevel=0.003)

    vent_frame_r = finalize_mesh(create_cube(
        "BackPackVentFrame_R",
        location=(0.06, -0.105, 1.015),
        scale=(0.026, 0.01, 0.045),
        collection=collection,
        parent=root,
        material=weapon_mat,
    ), bevel=0.003)

    set_parent_keep_transform(vent_l, torso)
    set_parent_keep_transform(vent_r, torso)
    set_parent_keep_transform(backpack, torso)
    set_parent_keep_transform(back_plate, torso)
    set_parent_keep_transform(back_panel, torso)
    set_parent_keep_transform(backpack_spine, torso)
    set_parent_keep_transform(backpack_side_l, torso)
    set_parent_keep_transform(backpack_side_r, torso)
    set_parent_keep_transform(backpack_mid_band, torso)
    set_parent_keep_transform(backpack_top, torso)
    set_parent_keep_transform(backpack_lower, torso)
    set_parent_keep_transform(vent_frame_l, torso)
    set_parent_keep_transform(vent_frame_r, torso)

    return {"pelvis": pelvis, "torso": torso}


def build_head(root, collection, armor_mat, trim_mat, cloth_mat, weapon_mat, undersuit_mat):
    finalize_mesh(create_cylinder(
        "Neck",
        location=(0.0, 0.0, 1.165),
        scale=(0.034, 0.034, 0.03),
        collection=collection,
        parent=root,
        material=undersuit_mat,
        vertices=14,
    ), smooth=True)

    helmet_core = finalize_mesh(create_uv_sphere(
        "HelmetCore",
        location=(0.0, -0.004, 1.315),
        scale=(0.112, 0.094, 0.136),
        collection=collection,
        parent=root,
        material=armor_mat,
        segments=26,
        rings=14,
    ), smooth=True, subsurf=1)

    helmet_back = finalize_mesh(create_uv_sphere(
        "HelmetBack",
        location=(0.0, -0.038, 1.315),
        scale=(0.1, 0.08, 0.11),
        collection=collection,
        parent=root,
        material=armor_mat,
        segments=22,
        rings=12,
    ), smooth=True, subsurf=1)

    helmet_front_shell = finalize_mesh(create_uv_sphere(
        "HelmetFrontShell",
        location=(0.0, 0.028, 1.302),
        scale=(0.086, 0.066, 0.108),
        collection=collection,
        parent=root,
        material=armor_mat,
        segments=22,
        rings=12,
    ), smooth=True, subsurf=1)

    helmet_mask = finalize_mesh(create_cube(
        "HelmetMask",
        location=(0.0, 0.067, 1.286),
        scale=(0.06, 0.028, 0.09),
        collection=collection,
        parent=root,
        material=armor_mat,
        rotation=(math.radians(-7.0), 0.0, 0.0),
    ), bevel=0.008)

    visor_slot = finalize_mesh(create_cube(
        "VisorSlot",
        location=(0.0, 0.095, 1.332),
        scale=(0.05, 0.006, 0.012),
        collection=collection,
        parent=root,
        rotation=(math.radians(-12.0), 0.0, 0.0),
        material=weapon_mat,
    ), bevel=0.002)

    visor_frame = finalize_mesh(create_cube(
        "VisorFrame",
        location=(0.0, 0.082, 1.33),
        scale=(0.066, 0.01, 0.024),
        collection=collection,
        parent=root,
        rotation=(math.radians(-10.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.003)

    visor_slit = finalize_mesh(create_cube(
        "VisorSlit",
        location=(0.0, 0.104, 1.324),
        scale=(0.038, 0.003, 0.008),
        collection=collection,
        parent=root,
        rotation=(math.radians(-13.0), 0.0, 0.0),
        material=weapon_mat,
    ), bevel=0.001)

    brow_plate = finalize_mesh(create_cube(
        "HelmetBrowPlate",
        location=(0.0, 0.075, 1.352),
        scale=(0.084, 0.012, 0.026),
        collection=collection,
        parent=root,
        rotation=(math.radians(-18.0), 0.0, 0.0),
        material=armor_mat,
    ), bevel=0.004)

    face_ridge = finalize_mesh(create_cube(
        "HelmetFaceRidge",
        location=(0.0, 0.086, 1.285),
        scale=(0.018, 0.008, 0.082),
        collection=collection,
        parent=root,
        rotation=(math.radians(-6.0), 0.0, 0.0),
        material=weapon_mat,
    ), bevel=0.003)

    jaw_plate = finalize_mesh(create_cube(
        "HelmetJawPlate",
        location=(0.0, 0.07, 1.225),
        scale=(0.044, 0.022, 0.03),
        collection=collection,
        parent=root,
        rotation=(math.radians(14.0), 0.0, 0.0),
        material=armor_mat,
    ), bevel=0.005)

    cheek_l = finalize_mesh(create_cube(
        "HelmetCheek_L",
        location=(-0.05, 0.06, 1.265),
        scale=(0.014, 0.02, 0.05),
        collection=collection,
        parent=root,
        rotation=(math.radians(10.0), 0.0, math.radians(14.0)),
        material=armor_mat,
    ), bevel=0.004)

    cheek_r = finalize_mesh(create_cube(
        "HelmetCheek_R",
        location=(0.05, 0.06, 1.265),
        scale=(0.014, 0.02, 0.05),
        collection=collection,
        parent=root,
        rotation=(math.radians(10.0), 0.0, math.radians(-14.0)),
        material=armor_mat,
    ), bevel=0.004)

    side_shell_l = finalize_mesh(create_uv_sphere(
        "HelmetSideShell_L",
        location=(-0.055, -0.004, 1.306),
        scale=(0.028, 0.068, 0.094),
        collection=collection,
        parent=root,
        material=armor_mat,
        segments=16,
        rings=12,
    ), smooth=True)

    side_shell_r = finalize_mesh(create_uv_sphere(
        "HelmetSideShell_R",
        location=(0.055, -0.004, 1.306),
        scale=(0.028, 0.068, 0.094),
        collection=collection,
        parent=root,
        material=armor_mat,
        segments=16,
        rings=12,
    ), smooth=True)

    ear_cover_l = finalize_mesh(create_cube(
        "HelmetEarCover_L",
        location=(-0.088, 0.01, 1.295),
        scale=(0.016, 0.03, 0.05),
        collection=collection,
        parent=root,
        rotation=(math.radians(4.0), 0.0, math.radians(10.0)),
        material=armor_mat,
    ), bevel=0.003)

    ear_cover_r = finalize_mesh(create_cube(
        "HelmetEarCover_R",
        location=(0.088, 0.01, 1.295),
        scale=(0.016, 0.03, 0.05),
        collection=collection,
        parent=root,
        rotation=(math.radians(4.0), 0.0, math.radians(-10.0)),
        material=armor_mat,
    ), bevel=0.003)

    rear_collar = finalize_mesh(create_cube(
        "HelmetRearCollar",
        location=(0.0, -0.085, 1.255),
        scale=(0.07, 0.018, 0.035),
        collection=collection,
        parent=root,
        rotation=(math.radians(14.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.003)

    rear_ridge = finalize_mesh(create_cube(
        "HelmetRearRidge",
        location=(0.0, -0.085, 1.345),
        scale=(0.026, 0.018, 0.07),
        collection=collection,
        parent=root,
        rotation=(math.radians(-10.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.003)

    neck_guard = finalize_mesh(create_uv_sphere(
        "HelmetNeckGuard",
        location=(0.0, -0.07, 1.252),
        scale=(0.086, 0.05, 0.06),
        collection=collection,
        parent=root,
        material=armor_mat,
        segments=18,
        rings=10,
    ), smooth=True)

    hood_l = finalize_mesh(create_uv_sphere(
        "HelmetHood_L",
        location=(-0.032, 0.012, 1.245),
        scale=(0.052, 0.038, 0.062),
        collection=collection,
        parent=root,
        material=cloth_mat,
        segments=16,
        rings=10,
    ), smooth=True)

    hood_r = finalize_mesh(create_uv_sphere(
        "HelmetHood_R",
        location=(0.032, 0.012, 1.245),
        scale=(0.052, 0.038, 0.062),
        collection=collection,
        parent=root,
        material=cloth_mat,
        segments=16,
        rings=10,
    ), smooth=True)

    hood_shell = finalize_mesh(create_uv_sphere(
        "HoodShell",
        location=(0.0, -0.01, 1.338),
        scale=(0.128, 0.112, 0.155),
        collection=collection,
        parent=root,
        material=cloth_mat,
        segments=20,
        rings=12,
    ), smooth=True, subsurf=1)

    hood_front_l = finalize_mesh(create_cube(
        "HoodFront_L",
        location=(-0.072, 0.02, 1.28),
        scale=(0.022, 0.024, 0.08),
        collection=collection,
        parent=root,
        rotation=(math.radians(6.0), math.radians(4.0), math.radians(16.0)),
        material=cloth_mat,
    ), bevel=0.003)

    hood_front_r = finalize_mesh(create_cube(
        "HoodFront_R",
        location=(0.072, 0.02, 1.28),
        scale=(0.022, 0.024, 0.08),
        collection=collection,
        parent=root,
        rotation=(math.radians(6.0), math.radians(-4.0), math.radians(-16.0)),
        material=cloth_mat,
    ), bevel=0.003)

    hood_tail = finalize_mesh(create_cube(
        "HoodTail",
        location=(0.0, -0.11, 1.255),
        scale=(0.048, 0.026, 0.075),
        collection=collection,
        parent=root,
        rotation=(math.radians(18.0), 0.0, 0.0),
        material=cloth_mat,
    ), bevel=0.003)

    crown = finalize_mesh(create_cube(
        "HelmetCrown",
        location=(0.0, -0.012, 1.438),
        scale=(0.014, 0.02, 0.012),
        collection=collection,
        parent=root,
        material=cloth_mat,
    ), bevel=0.004)

    spire = finalize_mesh(create_cone(
        "HelmetSpire",
        location=(0.0, -0.008, 1.49),
        scale=(0.007, 0.007, 0.014),
        collection=collection,
        parent=root,
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    set_parent_keep_transform(helmet_mask, helmet_core)
    set_parent_keep_transform(visor_slot, helmet_core)
    set_parent_keep_transform(visor_frame, helmet_core)
    set_parent_keep_transform(visor_slit, helmet_core)
    set_parent_keep_transform(helmet_front_shell, helmet_core)
    set_parent_keep_transform(helmet_back, helmet_core)
    set_parent_keep_transform(brow_plate, helmet_core)
    set_parent_keep_transform(face_ridge, helmet_core)
    set_parent_keep_transform(jaw_plate, helmet_core)
    set_parent_keep_transform(cheek_l, helmet_core)
    set_parent_keep_transform(cheek_r, helmet_core)
    set_parent_keep_transform(side_shell_l, helmet_core)
    set_parent_keep_transform(side_shell_r, helmet_core)
    set_parent_keep_transform(ear_cover_l, helmet_core)
    set_parent_keep_transform(ear_cover_r, helmet_core)
    set_parent_keep_transform(neck_guard, helmet_core)
    set_parent_keep_transform(rear_collar, helmet_core)
    set_parent_keep_transform(rear_ridge, helmet_core)
    set_parent_keep_transform(hood_shell, helmet_core)
    set_parent_keep_transform(hood_front_l, helmet_core)
    set_parent_keep_transform(hood_front_r, helmet_core)
    set_parent_keep_transform(hood_tail, helmet_core)
    set_parent_keep_transform(hood_l, helmet_core)
    set_parent_keep_transform(hood_r, helmet_core)
    set_parent_keep_transform(crown, helmet_core)
    set_parent_keep_transform(spire, helmet_core)


def build_cloth(root, torso, pelvis, collection, cloth_mat, trim_mat):
    finalize_mesh(create_cube(
        "CapeClasp",
        location=(0.0, -0.018, 1.05),
        scale=(0.04, 0.014, 0.014),
        collection=collection,
        parent=torso,
        material=trim_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "TabardWaist",
        location=(0.0, 0.074, 0.732),
        scale=(0.086, 0.007, 0.03),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(10.0), 0.0, 0.0),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "TabardWaistTrim",
        location=(0.0, 0.082, 0.735),
        scale=(0.094, 0.003, 0.014),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(8.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.002)

    finalize_mesh(create_cube(
        "TabardPanel_C",
        location=(0.0, 0.088, 0.592),
        scale=(0.028, 0.007, 0.135),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(10.0), 0.0, 0.0),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "TabardPanelTrim_C",
        location=(0.0, 0.095, 0.593),
        scale=(0.018, 0.003, 0.11),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(10.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.002)

    finalize_mesh(create_cube(
        "TabardPanel_L1",
        location=(-0.038, 0.088, 0.61),
        scale=(0.026, 0.007, 0.122),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(12.0), 0.0, math.radians(9.0)),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "TabardPanel_R1",
        location=(0.038, 0.088, 0.61),
        scale=(0.026, 0.007, 0.122),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(12.0), 0.0, math.radians(-9.0)),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "TabardPanel_L2",
        location=(-0.064, 0.08, 0.642),
        scale=(0.02, 0.007, 0.088),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(14.0), 0.0, math.radians(14.0)),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "TabardPanel_R2",
        location=(0.064, 0.08, 0.642),
        scale=(0.02, 0.007, 0.088),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(14.0), 0.0, math.radians(-14.0)),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "CapeUpper",
        location=(0.0, -0.09, 0.97),
        scale=(0.132, 0.012, 0.094),
        collection=collection,
        parent=torso,
        rotation=(math.radians(4.0), 0.0, 0.0),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "CapeDrape_L",
        location=(-0.08, -0.118, 0.68),
        scale=(0.038, 0.012, 0.27),
        collection=collection,
        parent=torso,
        rotation=(math.radians(4.0), 0.0, math.radians(7.0)),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "CapeDrape_R",
        location=(0.08, -0.118, 0.68),
        scale=(0.038, 0.012, 0.27),
        collection=collection,
        parent=torso,
        rotation=(math.radians(4.0), 0.0, math.radians(-7.0)),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "CapeFold_L",
        location=(-0.05, -0.1, 0.81),
        scale=(0.022, 0.006, 0.12),
        collection=collection,
        parent=torso,
        rotation=(math.radians(3.0), 0.0, math.radians(7.0)),
        material=trim_mat,
    ), bevel=0.002)

    finalize_mesh(create_cube(
        "CapeFold_R",
        location=(0.05, -0.1, 0.81),
        scale=(0.022, 0.006, 0.12),
        collection=collection,
        parent=torso,
        rotation=(math.radians(3.0), 0.0, math.radians(-7.0)),
        material=trim_mat,
    ), bevel=0.002)

    finalize_mesh(create_cube(
        "CapeMid",
        location=(0.0, -0.102, 0.77),
        scale=(0.118, 0.01, 0.148),
        collection=collection,
        parent=torso,
        rotation=(math.radians(2.0), 0.0, 0.0),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "FrontSash_L",
        location=(-0.03, 0.102, 0.5),
        scale=(0.016, 0.006, 0.18),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(8.0), 0.0, math.radians(6.0)),
        material=cloth_mat,
    ), bevel=0.002)

    finalize_mesh(create_cube(
        "FrontSash_R",
        location=(0.03, 0.102, 0.48),
        scale=(0.016, 0.006, 0.16),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(10.0), 0.0, math.radians(-5.0)),
        material=cloth_mat,
    ), bevel=0.002)

    finalize_mesh(create_cube(
        "CapeLower",
        location=(0.0, -0.108, 0.57),
        scale=(0.104, 0.01, 0.14),
        collection=collection,
        parent=torso,
        rotation=(math.radians(1.0), 0.0, 0.0),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "RearDrapeCenter",
        location=(0.0, -0.09, 0.63),
        scale=(0.05, 0.008, 0.15),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(4.0), 0.0, 0.0),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "RearDrape_L",
        location=(-0.05, -0.084, 0.655),
        scale=(0.032, 0.008, 0.12),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(6.0), 0.0, math.radians(8.0)),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "RearDrape_R",
        location=(0.05, -0.084, 0.655),
        scale=(0.032, 0.008, 0.12),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(6.0), 0.0, math.radians(-8.0)),
        material=cloth_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "WaistGuard",
        location=(0.0, 0.0, 0.79),
        scale=(0.162, 0.096, 0.024),
        collection=collection,
        parent=pelvis,
        material=trim_mat,
    ), bevel=0.004)


def build_spear(root, hand, collection, weapon_mat, trim_mat):
    spear_parent = hand if hand is not None else root
    shaft = finalize_mesh(create_cylinder(
        "SpearShaftMid",
        location=(0.22, 0.36, 0.9),
        scale=(0.015, 0.015, 0.64),
        collection=collection,
        parent=spear_parent,
        rotation=(math.radians(54.0), math.radians(-10.0), math.radians(16.0)),
        material=weapon_mat,
        vertices=12,
    ), smooth=True)

    shaft_mid_band = finalize_mesh(create_cylinder(
        "SpearMidBand",
        location=(0.0, 0.0, 0.0),
        scale=(0.02, 0.02, 0.028),
        collection=collection,
        parent=None,
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    shaft_upper = finalize_mesh(create_cylinder(
        "SpearShaftUpper",
        location=(0.0, 0.0, 0.0),
        scale=(0.017, 0.017, 0.20),
        collection=collection,
        parent=None,
        material=weapon_mat,
        vertices=12,
    ), smooth=True)

    shaft_upper_band = finalize_mesh(create_cylinder(
        "SpearUpperBand",
        location=(0.0, 0.0, 0.0),
        scale=(0.021, 0.021, 0.02),
        collection=collection,
        parent=None,
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    shaft_lower_band = finalize_mesh(create_cylinder(
        "SpearLowerBand",
        location=(0.0, 0.0, 0.0),
        scale=(0.02, 0.02, 0.024),
        collection=collection,
        parent=None,
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    shaft_lower = finalize_mesh(create_cylinder(
        "SpearShaftLower",
        location=(0.0, 0.0, 0.0),
        scale=(0.016, 0.016, 0.24),
        collection=collection,
        parent=None,
        material=weapon_mat,
        vertices=12,
    ), smooth=True)

    grip_low = finalize_mesh(create_cylinder(
        "GripLow",
        location=(0.0, 0.0, 0.0),
        scale=(0.026, 0.026, 0.035),
        collection=collection,
        parent=None,
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    grip_high = finalize_mesh(create_cylinder(
        "GripHigh",
        location=(0.0, 0.0, 0.0),
        scale=(0.022, 0.022, 0.03),
        collection=collection,
        parent=None,
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    grip_core = finalize_mesh(create_cylinder(
        "GripCore",
        location=(0.0, 0.0, 0.0),
        scale=(0.018, 0.018, 0.13),
        collection=collection,
        parent=None,
        material=weapon_mat,
        vertices=12,
    ), smooth=True)

    spear_tip = finalize_mesh(create_cone(
        "SpearTip",
        location=(0.0, 0.0, 0.0),
        scale=(0.042, 0.042, 0.18),
        collection=collection,
        parent=None,
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    spear_wing_l = finalize_mesh(create_cube(
        "SpearWing_L",
        location=(0.0, 0.0, 0.0),
        scale=(0.02, 0.005, 0.09),
        collection=collection,
        parent=None,
        rotation=(math.radians(12.0), math.radians(22.0), math.radians(34.0)),
        material=trim_mat,
    ), bevel=0.002)

    spear_wing_r = finalize_mesh(create_cube(
        "SpearWing_R",
        location=(0.0, 0.0, 0.0),
        scale=(0.02, 0.005, 0.09),
        collection=collection,
        parent=None,
        rotation=(math.radians(12.0), math.radians(-22.0), math.radians(-34.0)),
        material=trim_mat,
    ), bevel=0.002)

    spear_hook_l = finalize_mesh(create_cone(
        "SpearHook_L",
        location=(0.0, 0.0, 0.0),
        scale=(0.012, 0.012, 0.05),
        collection=collection,
        parent=None,
        rotation=(math.radians(84.0), 0.0, math.radians(22.0)),
        material=trim_mat,
        vertices=10,
    ), smooth=True)

    spear_hook_r = finalize_mesh(create_cone(
        "SpearHook_R",
        location=(0.0, 0.0, 0.0),
        scale=(0.012, 0.012, 0.05),
        collection=collection,
        parent=None,
        rotation=(math.radians(84.0), 0.0, math.radians(-22.0)),
        material=trim_mat,
        vertices=10,
    ), smooth=True)

    spear_counter = finalize_mesh(create_cube(
        "SpearCounterWeight",
        location=(0.0, 0.0, 0.0),
        scale=(0.018, 0.018, 0.028),
        collection=collection,
        parent=None,
        material=trim_mat,
    ), bevel=0.002)

    blade_core = finalize_mesh(create_cube(
        "SpearBladeCore",
        location=(0.0, 0.0, 0.0),
        scale=(0.024, 0.012, 0.12),
        collection=collection,
        parent=None,
        rotation=(math.radians(45.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.003)

    collar = finalize_mesh(create_cube(
        "SpearCollar",
        location=(0.0, 0.0, 0.0),
        scale=(0.032, 0.032, 0.026),
        collection=collection,
        parent=None,
        material=trim_mat,
    ), bevel=0.003)

    spear_lug_l = finalize_mesh(create_cube(
        "SpearLug_L",
        location=(0.0, 0.0, 0.0),
        scale=(0.018, 0.004, 0.05),
        collection=collection,
        parent=None,
        rotation=(math.radians(18.0), 0.0, math.radians(28.0)),
        material=trim_mat,
    ), bevel=0.003)

    spear_lug_r = finalize_mesh(create_cube(
        "SpearLug_R",
        location=(0.0, 0.0, 0.0),
        scale=(0.018, 0.004, 0.05),
        collection=collection,
        parent=None,
        rotation=(math.radians(18.0), 0.0, math.radians(-28.0)),
        material=trim_mat,
    ), bevel=0.003)

    spear_butt = finalize_mesh(create_cone(
        "SpearButt",
        location=(0.0, 0.0, 0.0),
        scale=(0.024, 0.024, 0.08),
        collection=collection,
        parent=None,
        rotation=(math.pi, 0.0, 0.0),
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    tassel_l = finalize_mesh(create_cube(
        "SpearTassel_L",
        location=(0.0, 0.0, 0.0),
        scale=(0.004, 0.018, 0.05),
        collection=collection,
        parent=None,
        rotation=(math.radians(8.0), 0.0, math.radians(18.0)),
        material=trim_mat,
    ), bevel=0.001)

    tassel_r = finalize_mesh(create_cube(
        "SpearTassel_R",
        location=(0.0, 0.0, 0.0),
        scale=(0.004, 0.018, 0.05),
        collection=collection,
        parent=None,
        rotation=(math.radians(8.0), 0.0, math.radians(-18.0)),
        material=trim_mat,
    ), bevel=0.001)

    set_parent_local_transform(shaft_mid_band, shaft, location=(0.0, 0.0, 0.34))
    set_parent_local_transform(shaft_upper, shaft, location=(0.0, 0.0, 0.90))
    set_parent_local_transform(shaft_lower, shaft, location=(0.0, 0.0, -0.96))
    set_parent_local_transform(shaft_upper_band, shaft, location=(0.0, 0.0, 0.72))
    set_parent_local_transform(shaft_lower_band, shaft, location=(0.0, 0.0, -0.68))
    set_parent_local_transform(grip_core, shaft, location=(0.0, 0.0, 0.01))
    set_parent_local_transform(grip_low, shaft, location=(0.0, 0.0, -0.16))
    set_parent_local_transform(grip_high, shaft, location=(0.0, 0.0, 0.18))
    set_parent_local_transform(spear_tip, shaft, location=(0.0, 0.0, 1.24), rotation=(0.0, 0.0, 0.0))
    set_parent_local_transform(spear_wing_l, shaft, location=(-0.046, 0.0, 1.11), rotation=(math.radians(12.0), math.radians(22.0), math.radians(34.0)))
    set_parent_local_transform(spear_wing_r, shaft, location=(0.046, 0.0, 1.11), rotation=(math.radians(12.0), math.radians(-22.0), math.radians(-34.0)))
    set_parent_local_transform(spear_hook_l, shaft, location=(-0.034, 0.0, 1.0), rotation=(math.radians(84.0), 0.0, math.radians(22.0)))
    set_parent_local_transform(spear_hook_r, shaft, location=(0.034, 0.0, 1.0), rotation=(math.radians(84.0), 0.0, math.radians(-22.0)))
    set_parent_local_transform(blade_core, shaft, location=(0.0, 0.0, 1.03), rotation=(math.radians(45.0), 0.0, 0.0))
    set_parent_local_transform(collar, shaft, location=(0.0, 0.0, 0.84))
    set_parent_local_transform(spear_lug_l, shaft, location=(-0.028, 0.0, 0.78), rotation=(math.radians(18.0), 0.0, math.radians(28.0)))
    set_parent_local_transform(spear_lug_r, shaft, location=(0.028, 0.0, 0.78), rotation=(math.radians(18.0), 0.0, math.radians(-28.0)))
    set_parent_local_transform(tassel_l, shaft, location=(-0.018, 0.0, 0.8), rotation=(math.radians(8.0), 0.0, math.radians(18.0)))
    set_parent_local_transform(tassel_r, shaft, location=(0.018, 0.0, 0.8), rotation=(math.radians(8.0), 0.0, math.radians(-18.0)))
    set_parent_local_transform(spear_counter, shaft, location=(0.0, 0.0, -1.02))
    set_parent_local_transform(spear_butt, shaft, location=(0.0, 0.0, -1.16), rotation=(math.pi, 0.0, 0.0))


def build_shield(root, hand, collection, armor_mat, trim_mat, weapon_mat):
    shield_parent = hand if hand is not None else root
    shield = finalize_mesh(create_cube(
        "Shield",
        location=(-0.24, 0.24, 0.66),
        scale=(0.072, 0.018, 0.17),
        collection=collection,
        parent=shield_parent,
        rotation=(math.radians(46.0), math.radians(10.0), math.radians(-34.0)),
        material=armor_mat,
    ), bevel=0.01)

    finalize_mesh(create_cube(
        "ShieldFrameOuter",
        location=(-0.205, 0.094, 0.59),
        scale=(0.082, 0.007, 0.182),
        collection=collection,
        parent=shield,
        material=trim_mat,
    ), bevel=0.004)

    finalize_mesh(create_cube(
        "ShieldFrameInner",
        location=(-0.205, 0.099, 0.59),
        scale=(0.06, 0.005, 0.145),
        collection=collection,
        parent=shield,
        material=trim_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "ShieldInsetPanel",
        location=(-0.205, 0.104, 0.59),
        scale=(0.048, 0.003, 0.12),
        collection=collection,
        parent=shield,
        material=armor_mat,
    ), bevel=0.002)

    finalize_mesh(create_cube(
        "ShieldSpine",
        location=(-0.205, 0.101, 0.59),
        scale=(0.015, 0.007, 0.155),
        collection=collection,
        parent=shield,
        material=trim_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "ShieldBarTop",
        location=(-0.205, 0.096, 0.68),
        scale=(0.05, 0.005, 0.016),
        collection=collection,
        parent=shield,
        material=trim_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "ShieldBarBottom",
        location=(-0.205, 0.096, 0.50),
        scale=(0.05, 0.005, 0.016),
        collection=collection,
        parent=shield,
        material=trim_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "ShieldCrestTop",
        location=(-0.205, 0.11, 0.79),
        scale=(0.03, 0.005, 0.028),
        collection=collection,
        parent=shield,
        rotation=(math.radians(10.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "ShieldCrestBottom",
        location=(-0.205, 0.11, 0.39),
        scale=(0.03, 0.005, 0.028),
        collection=collection,
        parent=shield,
        rotation=(math.radians(-10.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.003)

    finalize_mesh(create_uv_sphere(
        "ShieldBoss",
        location=(-0.205, 0.108, 0.59),
        scale=(0.024, 0.012, 0.03),
        collection=collection,
        parent=shield,
        material=weapon_mat,
        segments=16,
        rings=10,
    ), smooth=True, subsurf=1)

    finalize_mesh(create_cube(
        "ShieldEmblemDiamond",
        location=(-0.205, 0.116, 0.59),
        scale=(0.024, 0.004, 0.024),
        collection=collection,
        parent=shield,
        rotation=(0.0, math.radians(45.0), 0.0),
        material=weapon_mat,
    ), bevel=0.002)

    finalize_mesh(create_cube(
        "ShieldEmblemCross",
        location=(-0.205, 0.118, 0.59),
        scale=(0.008, 0.003, 0.05),
        collection=collection,
        parent=shield,
        material=trim_mat,
    ), bevel=0.002)

    finalize_mesh(create_cube(
        "ShieldEmblemBar",
        location=(-0.205, 0.118, 0.59),
        scale=(0.032, 0.003, 0.008),
        collection=collection,
        parent=shield,
        material=trim_mat,
    ), bevel=0.002)

    finalize_mesh(create_cube(
        "ShieldGrip",
        location=(-0.168, 0.055, 0.59),
        scale=(0.012, 0.012, 0.05),
        collection=collection,
        parent=shield,
        rotation=(math.radians(90.0), 0.0, math.radians(90.0)),
        material=weapon_mat,
    ), bevel=0.003)

    finalize_mesh(create_cube(
        "ShieldStrapTop",
        location=(-0.24, 0.045, 0.665),
        scale=(0.01, 0.004, 0.04),
        collection=collection,
        parent=shield,
        rotation=(math.radians(88.0), 0.0, math.radians(24.0)),
        material=weapon_mat,
    ), bevel=0.002)

    finalize_mesh(create_cube(
        "ShieldStrapBottom",
        location=(-0.24, 0.045, 0.515),
        scale=(0.01, 0.004, 0.04),
        collection=collection,
        parent=shield,
        rotation=(math.radians(88.0), 0.0, math.radians(-24.0)),
        material=weapon_mat,
    ), bevel=0.002)


def build_infantry():
    collection = ensure_collection(COLLECTION_NAME)
    clear_collection(collection)

    armor_material = make_material("InfantryArmor", (0.83, 0.82, 0.75, 1.0), metallic=0.15, roughness=0.55)
    trim_material = make_material("InfantryTrim", (0.66, 0.53, 0.22, 1.0), metallic=0.7, roughness=0.35)
    cloth_material = make_material("InfantryCloth", (0.36, 0.06, 0.09, 1.0), metallic=0.0, roughness=0.8)
    weapon_material = make_material("InfantryWeapon", (0.18, 0.2, 0.24, 1.0), metallic=0.65, roughness=0.3)
    undersuit_material = make_material("InfantryUnderSuit", (0.08, 0.08, 0.1, 1.0), metallic=0.0, roughness=0.9)

    root = bpy.data.objects.new(ROOT_NAME, None)
    root.empty_display_type = "PLAIN_AXES"
    root.location = (0.0, 0.0, 0.0)
    collection.objects.link(root)

    body = build_body(root, collection, armor_material, trim_material, cloth_material, weapon_material, undersuit_material)
    build_head(root, collection, armor_material, trim_material, cloth_material, weapon_material, undersuit_material)
    left_arm = build_arm("L", root, body["torso"], collection, armor_material, trim_material, undersuit_material)
    right_arm = build_arm("R", root, body["torso"], collection, armor_material, trim_material, undersuit_material)
    build_leg("L", root, body["pelvis"], collection, armor_material, trim_material, undersuit_material)
    build_leg("R", root, body["pelvis"], collection, armor_material, trim_material, undersuit_material)
    build_cloth(root, body["torso"], body["pelvis"], collection, cloth_material, trim_material)
    build_spear(root, right_arm["hand"], collection, weapon_material, trim_material)
    build_shield(root, left_arm["hand"], collection, armor_material, trim_material, weapon_material)

    bpy.ops.object.select_all(action="DESELECT")
    for obj in collection.objects:
        if obj.type == "MESH":
            obj.select_set(True)
    root.select_set(True)
    bpy.context.view_layer.objects.active = root


build_infantry()
