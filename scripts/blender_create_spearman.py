"""
창병 (Sci-Fi Crusader Spearman) 모델 생성 스크립트
Blender 5.1 bpy — 헤드리스 실행용

실행법:
  blender --background --python blender_create_spearman.py
"""

import bpy
import math
import os

# ── 출력 경로 ──────────────────────────────────────────────────────────────
OUTPUT_PATH = r"C:\Users\gurwl\project\3D-C--Unity\Assets\Models\spearman.fbx"

# ── 초기화 ─────────────────────────────────────────────────────────────────
def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete()
    for col in list(bpy.data.collections):
        bpy.data.collections.remove(col)

# ── 유틸 ───────────────────────────────────────────────────────────────────
def add_box(name, loc, scale, rot=(0,0,0)):
    bpy.ops.mesh.primitive_cube_add(location=loc)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    obj.rotation_euler = rot
    bpy.ops.object.transform_apply(scale=True, rotation=True)
    return obj

def add_cylinder(name, loc, r, depth, rot=(0,0,0), verts=12):
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=verts, radius=r, depth=depth, location=loc)
    obj = bpy.context.active_object
    obj.name = name
    obj.rotation_euler = rot
    bpy.ops.object.transform_apply(rotation=True)
    return obj

def add_sphere(name, loc, r, segs=12):
    bpy.ops.mesh.primitive_uv_sphere_add(
        segments=segs, ring_count=segs//2, radius=r, location=loc)
    obj = bpy.context.active_object
    obj.name = name
    return obj

def add_cone(name, loc, r1, r2, depth, rot=(0,0,0)):
    bpy.ops.mesh.primitive_cone_add(
        vertices=8, radius1=r1, radius2=r2, depth=depth, location=loc)
    obj = bpy.context.active_object
    obj.name = name
    obj.rotation_euler = rot
    bpy.ops.object.transform_apply(rotation=True)
    return obj

def make_mat(name, color, metallic=0.0, roughness=0.5, emission=None):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes["Principled BSDF"]
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    if emission:
        bsdf.inputs["Emission Color"].default_value = (*emission, 1.0)
        bsdf.inputs["Emission Strength"].default_value = 2.0
    return mat

def assign_mat(obj, mat):
    if obj.data.materials:
        obj.data.materials[0] = mat
    else:
        obj.data.materials.append(mat)

def bevel(obj, width=0.02, segs=2):
    mod = obj.modifiers.new("Bevel", "BEVEL")
    mod.width = width
    mod.segments = segs

# ── 머티리얼 팔레트 ────────────────────────────────────────────────────────
MAT_IVORY    = make_mat("M_Ivory",   (0.92, 0.90, 0.82), metallic=0.1, roughness=0.35)
MAT_BRASS    = make_mat("M_Brass",   (0.80, 0.60, 0.20), metallic=0.9, roughness=0.25)
MAT_CRIMSON  = make_mat("M_Crimson", (0.55, 0.05, 0.05), metallic=0.0, roughness=0.8)
MAT_DARK     = make_mat("M_Dark",    (0.08, 0.08, 0.10), metallic=0.4, roughness=0.5)
MAT_GLOW     = make_mat("M_Glow",    (0.1, 0.3, 1.0),    metallic=0.0, roughness=1.0,
                         emission=(0.1, 0.4, 1.0))
MAT_STEEL    = make_mat("M_Steel",   (0.55, 0.57, 0.60), metallic=0.95, roughness=0.2)

# ── 파츠 생성 ──────────────────────────────────────────────────────────────
clear_scene()
parts = []

# 발 (그리브)
fl = add_box("Foot_L", (-0.09, 0.0,  0.06), (0.07, 0.09, 0.06))
fr = add_box("Foot_R", ( 0.09, 0.0,  0.06), (0.07, 0.09, 0.06))
assign_mat(fl, MAT_IVORY); assign_mat(fr, MAT_IVORY)
bevel(fl); bevel(fr)
parts += [fl, fr]

# 정강이
sl = add_cylinder("Shin_L", (-0.09, 0.0, 0.24), 0.055, 0.32)
sr = add_cylinder("Shin_R", ( 0.09, 0.0, 0.24), 0.055, 0.32)
assign_mat(sl, MAT_IVORY); assign_mat(sr, MAT_IVORY)
parts += [sl, sr]

# 무릎 패드
kl = add_box("Knee_L", (-0.09, 0.03, 0.40), (0.07, 0.04, 0.06))
kr = add_box("Knee_R", ( 0.09, 0.03, 0.40), (0.07, 0.04, 0.06))
assign_mat(kl, MAT_BRASS); assign_mat(kr, MAT_BRASS)
bevel(kl); bevel(kr)
parts += [kl, kr]

# 허벅지
tl = add_cylinder("Thigh_L", (-0.09, 0.0, 0.57), 0.068, 0.28)
tr = add_cylinder("Thigh_R", ( 0.09, 0.0, 0.57), 0.068, 0.28)
assign_mat(tl, MAT_IVORY); assign_mat(tr, MAT_IVORY)
parts += [tl, tr]

# 허리 (pelvis)
pelvis = add_box("Pelvis", (0.0, 0.0, 0.73), (0.17, 0.10, 0.07))
assign_mat(pelvis, MAT_DARK)
bevel(pelvis, 0.03)
parts.append(pelvis)

# 허리 천(타바드 앞자락)
tabard = add_box("Tabard_Front", (0.0, 0.04, 0.63), (0.10, 0.01, 0.10))
assign_mat(tabard, MAT_CRIMSON)
parts.append(tabard)

# 흉갑
chest = add_box("Chest", (0.0, 0.0, 0.97), (0.20, 0.12, 0.20))
assign_mat(chest, MAT_IVORY)
bevel(chest, 0.03, 3)
parts.append(chest)

# 흉갑 가운데 십자 문양
cross_v = add_box("Cross_V", (0.0, 0.13, 0.97), (0.025, 0.005, 0.14))
cross_h = add_box("Cross_H", (0.0, 0.13, 1.00), (0.10,  0.005, 0.025))
assign_mat(cross_v, MAT_BRASS); assign_mat(cross_h, MAT_BRASS)
parts += [cross_v, cross_h]

# 허리 벨트
belt = add_box("Belt", (0.0, 0.0, 0.79), (0.21, 0.13, 0.04))
assign_mat(belt, MAT_BRASS)
bevel(belt, 0.01)
parts.append(belt)

# 어깨 패드
spl = add_box("Shoulder_L", (-0.27, 0.0, 1.10), (0.10, 0.10, 0.08))
spr = add_box("Shoulder_R", ( 0.27, 0.0, 1.10), (0.10, 0.10, 0.08))
assign_mat(spl, MAT_IVORY); assign_mat(spr, MAT_IVORY)
bevel(spl, 0.025, 3); bevel(spr, 0.025, 3)
parts += [spl, spr]

# 어깨 패드 테두리
spl_r = add_box("Shoulder_Rim_L", (-0.27, 0.0, 1.10), (0.11, 0.11, 0.02))
spr_r = add_box("Shoulder_Rim_R", ( 0.27, 0.0, 1.10), (0.11, 0.11, 0.02))
assign_mat(spl_r, MAT_BRASS); assign_mat(spr_r, MAT_BRASS)
parts += [spl_r, spr_r]

# 위팔
ual = add_cylinder("UpperArm_L", (-0.27, 0.0, 0.95), 0.052, 0.24)
uar = add_cylinder("UpperArm_R", ( 0.27, 0.0, 0.95), 0.052, 0.24)
assign_mat(ual, MAT_IVORY); assign_mat(uar, MAT_IVORY)
parts += [ual, uar]

# 팔꿈치
ell = add_sphere("Elbow_L", (-0.27, 0.0, 0.82), 0.055)
elr = add_sphere("Elbow_R", ( 0.27, 0.0, 0.82), 0.055)
assign_mat(ell, MAT_BRASS); assign_mat(elr, MAT_BRASS)
parts += [ell, elr]

# 아래팔
fal = add_cylinder("Forearm_L", (-0.27, 0.0, 0.68), 0.045, 0.24)
far = add_cylinder("Forearm_R", ( 0.27, 0.0, 0.68), 0.045, 0.24)
assign_mat(fal, MAT_IVORY); assign_mat(far, MAT_IVORY)
parts += [fal, far]

# 장갑 (왼손 — 창 쥐는 손)
gl = add_box("Glove_L", (-0.27, 0.0, 0.54), (0.055, 0.055, 0.07))
gr = add_box("Glove_R", ( 0.27, 0.0, 0.54), (0.055, 0.055, 0.07))
assign_mat(gl, MAT_DARK); assign_mat(gr, MAT_DARK)
bevel(gl); bevel(gr)
parts += [gl, gr]

# 목
neck = add_cylinder("Neck", (0.0, 0.0, 1.19), 0.055, 0.10)
assign_mat(neck, MAT_DARK)
parts.append(neck)

# 헬멧 (기본 형태)
helmet = add_box("Helmet", (0.0, 0.0, 1.37), (0.14, 0.13, 0.16))
assign_mat(helmet, MAT_IVORY)
bevel(helmet, 0.03, 3)
parts.append(helmet)

# 헬멧 정수리 능선
crest = add_box("Helmet_Crest", (0.0, 0.0, 1.47), (0.025, 0.10, 0.05))
assign_mat(crest, MAT_CRIMSON)
bevel(crest, 0.01)
parts.append(crest)

# 바이저 슬릿 (발광)
visor = add_box("Visor", (0.0, 0.12, 1.36), (0.08, 0.005, 0.025))
assign_mat(visor, MAT_GLOW)
parts.append(visor)

# 헬멧 볼 가드
cgl = add_box("CheekGuard_L", (-0.10, 0.07, 1.30), (0.04, 0.07, 0.07))
cgr = add_box("CheekGuard_R", ( 0.10, 0.07, 1.30), (0.04, 0.07, 0.07))
assign_mat(cgl, MAT_IVORY); assign_mat(cgr, MAT_IVORY)
bevel(cgl); bevel(cgr)
parts += [cgl, cgr]

# ─── 창 ────────────────────────────────────────────────────────────────────
# 창 위치: 오른손 옆, 위로 뻗음 (T-포즈 기준 오른팔 바깥)
SPEAR_X = 0.45

# 창 자루
shaft = add_cylinder("Spear_Shaft", (SPEAR_X, 0.0, 1.05), 0.018, 1.80)
assign_mat(shaft, MAT_DARK)
parts.append(shaft)

# 자루 중간 그립 밴드 2개
for z, name in [(0.80, "Grip_Lo"), (1.20, "Grip_Hi")]:
    band = add_cylinder(name, (SPEAR_X, 0.0, z), 0.024, 0.05)
    assign_mat(band, MAT_BRASS)
    parts.append(band)

# 창끝 (날)
blade_body = add_cone("Blade_Body", (SPEAR_X, 0.0, 2.08), 0.030, 0.005, 0.28)
assign_mat(blade_body, MAT_STEEL)
parts.append(blade_body)

# 창끝 에너지 발광 홈
blade_glow = add_box("Blade_Glow", (SPEAR_X, 0.0, 1.97), (0.008, 0.008, 0.10))
assign_mat(blade_glow, MAT_GLOW)
parts.append(blade_glow)

# 창 아랫 석장식 (butt spike)
butt = add_cone("Spear_Butt", (SPEAR_X, 0.0, 0.12), 0.020, 0.005, 0.12,
                rot=(math.pi, 0, 0))
assign_mat(butt, MAT_STEEL)
parts.append(butt)

# ─── 등 망토 ───────────────────────────────────────────────────────────────
cape_up   = add_box("Cape_Upper", (0.0, -0.08, 1.00), (0.16, 0.01, 0.16))
cape_mid  = add_box("Cape_Mid",   (0.0, -0.10, 0.78), (0.14, 0.01, 0.20))
cape_low  = add_box("Cape_Lower", (0.0, -0.11, 0.56), (0.12, 0.01, 0.16))
assign_mat(cape_up,  MAT_CRIMSON)
assign_mat(cape_mid, MAT_CRIMSON)
assign_mat(cape_low, MAT_CRIMSON)
parts += [cape_up, cape_mid, cape_low]

# ── 전체 선택 후 조인 ──────────────────────────────────────────────────────
bpy.ops.object.select_all(action='DESELECT')
for p in parts:
    p.select_set(True)
bpy.context.view_layer.objects.active = parts[0]
bpy.ops.object.join()
combined = bpy.context.active_object
combined.name = "Spearman"

# 원점을 기하 중심으로
bpy.ops.object.origin_set(type='ORIGIN_GEOMETRY', center='BOUNDS')
combined.location = (0, 0, 0)
bpy.ops.object.transform_apply(location=True)

# 스무딩
bpy.ops.object.shade_smooth()

# ── FBX 내보내기 ───────────────────────────────────────────────────────────
os.makedirs(os.path.dirname(OUTPUT_PATH), exist_ok=True)
bpy.ops.export_scene.fbx(
    filepath=OUTPUT_PATH,
    use_selection=True,
    global_scale=1.0,
    axis_forward='-Z',
    axis_up='Y',
    apply_unit_scale=True,
    apply_scale_options='FBX_SCALE_NONE',
    mesh_smooth_type='FACE',
    use_mesh_modifiers=True,
    add_leaf_bones=False,
    bake_anim=False,
)

print(f"\n✔ 창병 FBX 생성 완료: {OUTPUT_PATH}\n")
