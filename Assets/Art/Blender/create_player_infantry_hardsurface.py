import math
import bmesh
import bpy
from mathutils import Vector


COLLECTION_NAME = "PrototypeInfantryHardSurface"
ROOT_NAME = "PlayerInfantryHardSurfaceRoot"


def ensure_collection(name):
    collection = bpy.data.collections.get(name)
    if collection is None:
        collection = bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(collection)
    return collection


def clear_collection(collection):
    for obj in list(collection.objects):
        bpy.data.objects.remove(obj, do_unlink=True)


def add_object_to_collection(obj, collection):
    for linked in list(obj.users_collection):
        linked.objects.unlink(obj)
    collection.objects.link(obj)


def set_parent_keep_transform(obj, parent):
    world_matrix = obj.matrix_world.copy()
    obj.parent = parent
    obj.matrix_world = world_matrix


def apply_transform(obj, apply_rotation=True, apply_scale=True):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=apply_rotation, scale=apply_scale)
    obj.select_set(False)


def smooth_object(obj):
    if obj.type != "MESH":
        return
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.shade_smooth()
    obj.select_set(False)


def add_bevel(obj, width=0.008, segments=2):
    modifier = obj.modifiers.new(name="Bevel", type="BEVEL")
    modifier.width = width
    modifier.segments = segments
    modifier.limit_method = "ANGLE"


def add_subsurf(obj, levels=1):
    modifier = obj.modifiers.new(name="Subsurf", type="SUBSURF")
    modifier.levels = levels
    modifier.render_levels = levels


def finalize_mesh(obj, bevel=None, subsurf=None, smooth=False):
    if bevel is not None:
        add_bevel(obj, width=bevel, segments=2)
    if subsurf is not None:
        add_subsurf(obj, levels=subsurf)
    if smooth:
        smooth_object(obj)
    return obj


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
    if output is None:
        output = nodes.new(type="ShaderNodeOutputMaterial")

    if not any(link.from_node == principled and link.to_node == output for link in links):
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


def create_cube(name, location, scale, collection, parent=None, rotation=None, material=None):
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    if rotation is not None:
        obj.rotation_euler = rotation
    apply_transform(obj)
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
    apply_transform(obj)
    if parent is not None:
        set_parent_keep_transform(obj, parent)
    if material is not None:
        assign_material(obj, material)
    add_object_to_collection(obj, collection)
    return obj


def create_uv_sphere(name, location, scale, collection, parent=None, material=None, segments=18, rings=10):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    apply_transform(obj, apply_rotation=False, apply_scale=True)
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
    apply_transform(obj)
    if parent is not None:
        set_parent_keep_transform(obj, parent)
    if material is not None:
        assign_material(obj, material)
    add_object_to_collection(obj, collection)
    return obj


def axis_vector(axis, sign):
    if axis == "X":
        return Vector((sign, 0.0, 0.0))
    if axis == "Y":
        return Vector((0.0, sign, 0.0))
    return Vector((0.0, 0.0, sign))


def panelize_face(obj, axis="Y", sign=1.0, inset=0.08, depth=0.02):
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    bm.faces.ensure_lookup_table()

    direction = axis_vector(axis, sign).normalized()
    faces = [face for face in bm.faces if face.normal.normalized().dot(direction) > 0.98]
    if not faces:
        bm.free()
        return

    result = bmesh.ops.inset_region(
        bm,
        faces=faces,
        thickness=inset,
        depth=0.0,
        use_even_offset=True,
    )

    inset_faces = [
        face
        for face in result["faces"]
        if face.normal.normalized().dot(direction) > 0.98
    ]
    if not inset_faces:
        bm.to_mesh(obj.data)
        obj.data.update()
        bm.free()
        return

    target_face = max(inset_faces, key=lambda face: face.calc_center_median().dot(direction))
    extruded = bmesh.ops.extrude_face_region(bm, geom=[target_face])
    verts = [item for item in extruded["geom"] if isinstance(item, bmesh.types.BMVert)]
    bmesh.ops.translate(bm, verts=verts, vec=direction * depth)

    bm.to_mesh(obj.data)
    obj.data.update()
    bm.free()


def bevel_support_loops(obj, cuts=1):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.subdivide(number_cuts=cuts, smoothness=0.0)
    bpy.ops.object.mode_set(mode="OBJECT")
    obj.select_set(False)


def build_head(root, collection, armor_mat, trim_mat, dark_mat):
    head_root = bpy.data.objects.new("HeadRoot", None)
    head_root.empty_display_type = "PLAIN_AXES"
    head_root.location = (0.0, 0.0, 1.26)
    collection.objects.link(head_root)
    set_parent_keep_transform(head_root, root)

    helmet = finalize_mesh(create_cube(
        "HelmetCore",
        location=(0.0, 0.0, 1.3),
        scale=(0.105, 0.095, 0.12),
        collection=collection,
        parent=head_root,
        material=armor_mat,
    ), bevel=0.012, subsurf=1, smooth=True)
    panelize_face(helmet, axis="Y", sign=1.0, inset=0.16, depth=0.016)

    visor = finalize_mesh(create_cube(
        "HelmetVisor",
        location=(0.0, 0.09, 1.305),
        scale=(0.06, 0.016, 0.05),
        collection=collection,
        parent=head_root,
        rotation=(math.radians(-6.0), 0.0, 0.0),
        material=dark_mat,
    ), bevel=0.004)

    brow = finalize_mesh(create_cube(
        "HelmetBrow",
        location=(0.0, 0.07, 1.365),
        scale=(0.085, 0.014, 0.02),
        collection=collection,
        parent=head_root,
        rotation=(math.radians(-12.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.004)

    jaw = finalize_mesh(create_cube(
        "HelmetJaw",
        location=(0.0, 0.075, 1.225),
        scale=(0.05, 0.024, 0.03),
        collection=collection,
        parent=head_root,
        rotation=(math.radians(10.0), 0.0, 0.0),
        material=armor_mat,
    ), bevel=0.005)

    side_l = finalize_mesh(create_uv_sphere(
        "HelmetSide_L",
        location=(-0.082, 0.0, 1.3),
        scale=(0.03, 0.06, 0.082),
        collection=collection,
        parent=head_root,
        material=armor_mat,
        segments=14,
        rings=10,
    ), subsurf=1, smooth=True)

    side_r = finalize_mesh(create_uv_sphere(
        "HelmetSide_R",
        location=(0.082, 0.0, 1.3),
        scale=(0.03, 0.06, 0.082),
        collection=collection,
        parent=head_root,
        material=armor_mat,
        segments=14,
        rings=10,
    ), subsurf=1, smooth=True)

    neck = finalize_mesh(create_cylinder(
        "Neck",
        location=(0.0, 0.0, 1.16),
        scale=(0.034, 0.034, 0.04),
        collection=collection,
        parent=root,
        material=dark_mat,
        vertices=14,
    ), smooth=True)

    return {"head_root": head_root, "helmet": helmet, "neck": neck}


def build_torso(root, collection, armor_mat, trim_mat, dark_mat, cloth_mat):
    pelvis = finalize_mesh(create_uv_sphere(
        "Pelvis",
        location=(0.0, 0.0, 0.74),
        scale=(0.105, 0.078, 0.068),
        collection=collection,
        parent=root,
        material=dark_mat,
        segments=18,
        rings=10,
    ), subsurf=1, smooth=True)

    torso = finalize_mesh(create_cube(
        "TorsoShell",
        location=(0.0, 0.0, 0.97),
        scale=(0.12, 0.085, 0.145),
        collection=collection,
        parent=root,
        material=armor_mat,
    ), bevel=0.014, subsurf=1, smooth=True)
    panelize_face(torso, axis="Y", sign=1.0, inset=0.13, depth=0.018)

    chest = finalize_mesh(create_cube(
        "ChestPlate",
        location=(0.0, 0.092, 0.98),
        scale=(0.088, 0.016, 0.09),
        collection=collection,
        parent=torso,
        rotation=(math.radians(-8.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.006)
    panelize_face(chest, axis="Y", sign=1.0, inset=0.14, depth=0.012)

    abdomen = finalize_mesh(create_cube(
        "AbdomenPlate",
        location=(0.0, 0.082, 0.84),
        scale=(0.076, 0.012, 0.06),
        collection=collection,
        parent=torso,
        rotation=(math.radians(10.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.005)

    backpack = finalize_mesh(create_cube(
        "BackPack",
        location=(0.0, -0.11, 0.96),
        scale=(0.084, 0.03, 0.12),
        collection=collection,
        parent=torso,
        material=dark_mat,
    ), bevel=0.008)
    panelize_face(backpack, axis="Y", sign=-1.0, inset=0.14, depth=0.014)

    vent_l = finalize_mesh(create_cylinder(
        "BackVent_L",
        location=(-0.06, -0.14, 1.02),
        scale=(0.018, 0.018, 0.04),
        collection=collection,
        parent=torso,
        rotation=(math.radians(90.0), 0.0, 0.0),
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    vent_r = finalize_mesh(create_cylinder(
        "BackVent_R",
        location=(0.06, -0.14, 1.02),
        scale=(0.018, 0.018, 0.04),
        collection=collection,
        parent=torso,
        rotation=(math.radians(90.0), 0.0, 0.0),
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    tabard = finalize_mesh(create_cube(
        "TabardFront",
        location=(0.0, 0.088, 0.58),
        scale=(0.048, 0.008, 0.15),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(8.0), 0.0, 0.0),
        material=cloth_mat,
    ), bevel=0.003)

    rear_drape = finalize_mesh(create_cube(
        "TabardRear",
        location=(0.0, -0.088, 0.62),
        scale=(0.056, 0.008, 0.15),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(4.0), 0.0, 0.0),
        material=cloth_mat,
    ), bevel=0.003)

    return {"pelvis": pelvis, "torso": torso, "tabard": tabard, "rear_drape": rear_drape}


def build_arm(side, root, torso, collection, armor_mat, trim_mat, dark_mat):
    sign = -1.0 if side == "L" else 1.0
    is_left = side == "L"

    shoulder = finalize_mesh(create_uv_sphere(
        f"Shoulder_{side}",
        location=(0.17 * sign, 0.0, 1.03),
        scale=(0.052, 0.06, 0.046),
        collection=collection,
        parent=torso,
        material=armor_mat,
        segments=18,
        rings=10,
    ), subsurf=1, smooth=True)

    upper_arm = finalize_mesh(create_cylinder(
        f"UpperArm_{side}",
        location=(-0.205, 0.025, 0.91) if is_left else (0.22, 0.03, 0.91),
        scale=(0.03, 0.03, 0.11),
        collection=collection,
        parent=root,
        rotation=(
            (math.radians(30.0), math.radians(14.0), math.radians(-30.0))
            if is_left
            else (math.radians(38.0), math.radians(-16.0), math.radians(34.0))
        ),
        material=dark_mat,
        vertices=16,
    ), smooth=True)

    elbow = finalize_mesh(create_uv_sphere(
        f"Elbow_{side}",
        location=(-0.24, 0.08, 0.81) if is_left else (0.27, 0.12, 0.81),
        scale=(0.028, 0.028, 0.028),
        collection=collection,
        parent=root,
        material=trim_mat,
        segments=16,
        rings=10,
    ), smooth=True)

    forearm = finalize_mesh(create_cylinder(
        f"Forearm_{side}",
        location=(-0.26, 0.14, 0.69) if is_left else (0.315, 0.18, 0.7),
        scale=(0.028, 0.028, 0.115),
        collection=collection,
        parent=root,
        rotation=(
            (math.radians(-40.0), math.radians(12.0), math.radians(-24.0))
            if is_left
            else (math.radians(-54.0), math.radians(-20.0), math.radians(34.0))
        ),
        material=armor_mat,
        vertices=16,
    ), smooth=True)

    wrist = finalize_mesh(create_cylinder(
        f"Wrist_{side}",
        location=(-0.265, 0.18, 0.6) if is_left else (0.34, 0.25, 0.63),
        scale=(0.018, 0.018, 0.026),
        collection=collection,
        parent=root,
        rotation=(
            (math.radians(-30.0), math.radians(12.0), math.radians(-24.0))
            if is_left
            else (math.radians(-40.0), math.radians(-22.0), math.radians(30.0))
        ),
        material=trim_mat,
        vertices=14,
    ), smooth=True)

    hand = finalize_mesh(create_cube(
        f"Hand_{side}",
        location=(-0.258, 0.21, 0.57) if is_left else (0.355, 0.3, 0.6),
        scale=(0.025, 0.032, 0.022),
        collection=collection,
        parent=root,
        rotation=(
            (math.radians(-12.0), 0.0, math.radians(-24.0))
            if is_left
            else (math.radians(-18.0), 0.0, math.radians(30.0))
        ),
        material=dark_mat,
    ), bevel=0.004)
    panelize_face(hand, axis="Y", sign=1.0, inset=0.18, depth=0.008)

    set_parent_keep_transform(upper_arm, shoulder)
    set_parent_keep_transform(elbow, upper_arm)
    set_parent_keep_transform(forearm, upper_arm)
    set_parent_keep_transform(wrist, forearm)
    set_parent_keep_transform(hand, wrist)

    return {"shoulder": shoulder, "hand": hand}


def build_leg(side, root, pelvis, collection, armor_mat, trim_mat, dark_mat):
    sign = -1.0 if side == "L" else 1.0
    x = 0.085 * sign

    hip = finalize_mesh(create_cube(
        f"Hip_{side}",
        location=(x, 0.012, 0.77),
        scale=(0.038, 0.026, 0.074),
        collection=collection,
        parent=pelvis,
        rotation=(math.radians(2.0), 0.0, math.radians(6.0 * sign)),
        material=armor_mat,
    ), bevel=0.006)

    thigh = finalize_mesh(create_cylinder(
        f"Thigh_{side}",
        location=(x, 0.0, 0.57),
        scale=(0.044, 0.044, 0.148),
        collection=collection,
        parent=root,
        rotation=(math.radians(2.0), math.radians(2.0 * sign), math.radians(3.0 * sign)),
        material=dark_mat,
        vertices=16,
    ), smooth=True)

    knee = finalize_mesh(create_uv_sphere(
        f"Knee_{side}",
        location=(x, 0.016, 0.405),
        scale=(0.035, 0.03, 0.035),
        collection=collection,
        parent=root,
        material=trim_mat,
        segments=14,
        rings=10,
    ), smooth=True)

    shin = finalize_mesh(create_cylinder(
        f"Shin_{side}",
        location=(x, -0.01, 0.245),
        scale=(0.038, 0.038, 0.15),
        collection=collection,
        parent=root,
        rotation=(math.radians(-5.0), math.radians(1.0 * sign), 0.0),
        material=armor_mat,
        vertices=16,
    ), smooth=True)

    boot = finalize_mesh(create_cube(
        f"Boot_{side}",
        location=(x, 0.05, 0.066),
        scale=(0.036, 0.06, 0.038),
        collection=collection,
        parent=root,
        rotation=(math.radians(6.0), 0.0, math.radians(4.0 * sign)),
        material=armor_mat,
    ), bevel=0.008)
    panelize_face(boot, axis="Z", sign=1.0, inset=0.2, depth=0.006)

    boot_toe = finalize_mesh(create_cube(
        f"BootToe_{side}",
        location=(x, 0.102, 0.048),
        scale=(0.032, 0.034, 0.022),
        collection=collection,
        parent=root,
        rotation=(math.radians(10.0), 0.0, math.radians(4.0 * sign)),
        material=trim_mat,
    ), bevel=0.006)

    set_parent_keep_transform(thigh, hip)
    set_parent_keep_transform(knee, thigh)
    set_parent_keep_transform(shin, thigh)
    set_parent_keep_transform(boot, shin)
    set_parent_keep_transform(boot_toe, boot)

    return {"hip": hip, "boot": boot}


def build_spear(root, hand, collection, weapon_mat, trim_mat):
    spear_parent = hand if hand is not None else root

    shaft = finalize_mesh(create_cylinder(
        "SpearShaft",
        location=(0.2, 0.34, 0.9),
        scale=(0.015, 0.015, 0.65),
        collection=collection,
        parent=spear_parent,
        rotation=(math.radians(52.0), math.radians(-8.0), math.radians(16.0)),
        material=weapon_mat,
        vertices=12,
    ), smooth=True)

    grip = finalize_mesh(create_cylinder(
        "SpearGrip",
        location=(0.0, 0.0, 0.0),
        scale=(0.026, 0.026, 0.04),
        collection=collection,
        parent=None,
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    tip = finalize_mesh(create_cone(
        "SpearTip",
        location=(0.0, 0.0, 0.0),
        scale=(0.042, 0.042, 0.18),
        collection=collection,
        parent=None,
        material=trim_mat,
        vertices=12,
    ), smooth=True)

    blade = finalize_mesh(create_cube(
        "SpearBlade",
        location=(0.0, 0.0, 0.0),
        scale=(0.024, 0.012, 0.12),
        collection=collection,
        parent=None,
        rotation=(math.radians(45.0), 0.0, 0.0),
        material=trim_mat,
    ), bevel=0.003)

    set_parent_keep_transform(grip, shaft)
    grip.location = (0.0, 0.0, -0.12)
    set_parent_keep_transform(tip, shaft)
    tip.location = (0.0, 0.0, 1.18)
    tip.rotation_euler = (0.0, 0.0, 0.0)
    set_parent_keep_transform(blade, shaft)
    blade.location = (0.0, 0.0, 0.98)
    blade.rotation_euler = (math.radians(45.0), 0.0, 0.0)


def build_shield(root, hand, collection, armor_mat, trim_mat, weapon_mat):
    shield_parent = hand if hand is not None else root

    shield = finalize_mesh(create_cube(
        "Shield",
        location=(-0.24, 0.24, 0.66),
        scale=(0.074, 0.02, 0.175),
        collection=collection,
        parent=shield_parent,
        rotation=(math.radians(44.0), math.radians(8.0), math.radians(-34.0)),
        material=armor_mat,
    ), bevel=0.01)
    panelize_face(shield, axis="Y", sign=1.0, inset=0.12, depth=0.01)

    boss = finalize_mesh(create_uv_sphere(
        "ShieldBoss",
        location=(-0.24, 0.256, 0.66),
        scale=(0.024, 0.012, 0.032),
        collection=collection,
        parent=shield,
        material=weapon_mat,
        segments=14,
        rings=10,
    ), smooth=True)

    frame = finalize_mesh(create_cube(
        "ShieldFrame",
        location=(-0.24, 0.248, 0.66),
        scale=(0.086, 0.007, 0.19),
        collection=collection,
        parent=shield,
        material=trim_mat,
    ), bevel=0.003)

    return {"shield": shield, "boss": boss, "frame": frame}


def build_infantry():
    collection = ensure_collection(COLLECTION_NAME)
    clear_collection(collection)

    armor_mat = make_material("InfantryHS_Armor", (0.78, 0.79, 0.82, 1.0), metallic=0.2, roughness=0.5)
    trim_mat = make_material("InfantryHS_Trim", (0.68, 0.56, 0.24, 1.0), metallic=0.75, roughness=0.32)
    dark_mat = make_material("InfantryHS_Dark", (0.1, 0.11, 0.14, 1.0), metallic=0.05, roughness=0.82)
    cloth_mat = make_material("InfantryHS_Cloth", (0.28, 0.08, 0.11, 1.0), metallic=0.0, roughness=0.84)

    root = bpy.data.objects.new(ROOT_NAME, None)
    root.empty_display_type = "PLAIN_AXES"
    root.location = (0.0, 0.0, 0.0)
    collection.objects.link(root)

    # Step 1: overall proportion and primary silhouette.
    body = build_torso(root, collection, armor_mat, trim_mat, dark_mat, cloth_mat)

    # Step 2: separate part construction for head, arms, and legs.
    build_head(root, collection, armor_mat, trim_mat, dark_mat)
    left_arm = build_arm("L", root, body["torso"], collection, armor_mat, trim_mat, dark_mat)
    right_arm = build_arm("R", root, body["torso"], collection, armor_mat, trim_mat, dark_mat)
    build_leg("L", root, body["pelvis"], collection, armor_mat, trim_mat, dark_mat)
    build_leg("R", root, body["pelvis"], collection, armor_mat, trim_mat, dark_mat)

    # Step 3: inset and extrude style hard-surface details.
    build_shield(root, left_arm["hand"], collection, armor_mat, trim_mat, dark_mat)
    build_spear(root, right_arm["hand"], collection, dark_mat, trim_mat)

    # Step 4: shading and modifiers are already distributed per part
    # through finalize_mesh so the model stays low-poly but readable.
    bpy.ops.object.select_all(action="DESELECT")
    for obj in collection.objects:
        if obj.type == "MESH":
            obj.select_set(True)
    root.select_set(True)
    bpy.context.view_layer.objects.active = root


build_infantry()
