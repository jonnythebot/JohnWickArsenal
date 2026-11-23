# Generate improved procedural equipment models with better geometry
import bpy
import math
import os

# Clear scene
def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete()
    for block in bpy.data.meshes:
        if block.users == 0:
            bpy.data.meshes.remove(block)
    for block in bpy.data.materials:
        if block.users == 0:
            bpy.data.materials.remove(block)

def create_material(name, base_color, metallic=0.0, roughness=0.5, emission=None):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    bsdf = nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs['Base Color'].default_value = base_color
        bsdf.inputs['Metallic'].default_value = metallic
        bsdf.inputs['Roughness'].default_value = roughness
        if emission:
            bsdf.inputs['Emission Color'].default_value = emission
            bsdf.inputs['Emission Strength'].default_value = 1.0
    return mat

def setup_uv_unwrap(obj):
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=66, island_margin=0.02)
    bpy.ops.object.mode_set(mode='OBJECT')

# ========================================================================
# EQUIPMENT MODELS
# ========================================================================

equipment_path = "/home/wolfwolfwolf8586/JohnWickArsenal/UnityProject/Assets/JohnWickArsenal/Models/Equipment/"

def get_materials():
    """Create materials fresh for each model"""
    leather = create_material("Leather", (0.08, 0.04, 0.02, 1), metallic=0.0, roughness=0.85)
    metal = create_material("MetalBuckle", (0.3, 0.3, 0.35, 1), metallic=0.9, roughness=0.2)
    nylon = create_material("NylonStrap", (0.05, 0.05, 0.05, 1), metallic=0.0, roughness=0.9)
    plastic = create_material("Plastic", (0.1, 0.1, 0.1, 1), metallic=0.0, roughness=0.3)
    return leather, metal, nylon, plastic

def export_model(name):
    # OBJ export works without numpy
    bpy.ops.wm.obj_export(filepath=os.path.join(equipment_path, f"{name}.obj"))
    print(f"✅ {name}.obj exported")

# 1. Shoulder Holster (Double) - Improved
clear_scene()
leather_mat, metal_mat, nylon_mat, plastic_mat = get_materials()
# Main harness straps
bpy.ops.mesh.primitive_torus_add(location=(0, 0, 1.4), major_radius=0.25, minor_radius=0.015)
strap1 = bpy.context.active_object
strap1.name = "ShoulderStrap_Left"

bpy.ops.mesh.primitive_torus_add(location=(0, 0, 1.4), major_radius=0.25, minor_radius=0.015)
strap2 = bpy.context.active_object
strap2.name = "ShoulderStrap_Right"
strap2.rotation_euler = (0, 0, math.pi)

# Chest connector
bpy.ops.mesh.primitive_cylinder_add(radius=0.02, depth=0.15, location=(0, 0.05, 1.45))
connector = bpy.context.active_object
connector.name = "ChestConnector"

# Holster bodies (two pistols)
for side, x_offset in [("Left", -0.2), ("Right", 0.2)]:
    bpy.ops.mesh.primitive_cube_add(size=1, location=(x_offset, -0.05, 0.9))
    holster = bpy.context.active_object
    holster.name = f"HolsterBody_{side}"
    holster.scale = (0.04, 0.08, 0.18)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Holster flap
    bpy.ops.mesh.primitive_cube_add(size=1, location=(x_offset, 0.05, 1.0))
    flap = bpy.context.active_object
    flap.name = f"HolsterFlap_{side}"
    flap.scale = (0.05, 0.01, 0.12)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Belt straps
for x_offset in [-0.2, 0.2]:
    bpy.ops.mesh.primitive_torus_add(location=(x_offset, 0, 0.7), major_radius=0.05, minor_radius=0.01)
    belt = bpy.context.active_object
    belt.name = f"BeltLoop_{x_offset}"
    belt.rotation_euler = (math.pi/2, 0, 0)

# Join all
bpy.ops.object.select_all(action='SELECT')
bpy.context.view_layer.objects.active = strap1
bpy.ops.object.join()
holster = bpy.context.active_object
holster.name = "ShoulderHolster_Double"

holster.data.materials.append(leather_mat)
holster.data.materials.append(metal_mat)
holster.data.materials.append(nylon_mat)

setup_uv_unwrap(holster)
export_model("ShoulderHolster_Double")

# 2. Hip Holster (TTI Race) - Improved
clear_scene()
leather_mat, metal_mat, nylon_mat, plastic_mat = get_materials()
leather_mat, metal_mat, nylon_mat, plastic_mat = get_materials()
# Main holster body
bpy.ops.mesh.primitive_cube_add(size=1, location=(0.15, 0, 0.85))
body = bpy.context.active_object
body.name = "HolsterBody"
body.scale = (0.045, 0.09, 0.2)
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Magazine carrier
bpy.ops.mesh.primitive_cube_add(size=1, location=(0.25, 0, 0.9))
mag = bpy.context.active_object
mag.name = "MagCarrier"
mag.scale = (0.03, 0.05, 0.12)
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Belt attachment
bpy.ops.mesh.primitive_torus_add(location=(0.15, 0, 1.05), major_radius=0.06, minor_radius=0.012)
belt = bpy.context.active_object
belt.name = "BeltLoop"
belt.rotation_euler = (math.pi/2, 0, 0)

# Retention strap
bpy.ops.mesh.primitive_cube_add(size=1, location=(0.15, 0.07, 0.95))
strap = bpy.context.active_object
strap.name = "RetentionStrap"
strap.scale = (0.06, 0.005, 0.1)
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Join
bpy.ops.object.select_all(action='SELECT')
bpy.context.view_layer.objects.active = body
bpy.ops.object.join()
hip = bpy.context.active_object
hip.name = "HipHolster_TTIRace"

hip.data.materials.append(leather_mat)
hip.data.materials.append(metal_mat)
hip.data.materials.append(nylon_mat)
setup_uv_unwrap(hip)
export_model("HipHolster_TTIRace")

# 3. Thigh Holster (Tactical) - Improved
clear_scene()
leather_mat, metal_mat, nylon_mat, plastic_mat = get_materials()
# Thigh strap
bpy.ops.mesh.primitive_torus_add(location=(0.25, 0, 0.55), major_radius=0.12, minor_radius=0.02)
thigh_strap = bpy.context.active_object
thigh_strap.name = "ThighStrap"

# Holster body on thigh
bpy.ops.mesh.primitive_cube_add(size=1, location=(0.25, -0.05, 0.55))
body = bpy.context.active_object
body.name = "HolsterBody"
body.scale = (0.04, 0.08, 0.18)
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Magazine pouch on thigh
bpy.ops.mesh.primitive_cube_add(size=1, location=(0.35, 0, 0.5))
mag = bpy.context.active_object
mag.name = "MagPouch"
mag.scale = (0.03, 0.05, 0.1)
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Drop leg strap connector
bpy.ops.mesh.primitive_cylinder_add(radius=0.015, depth=0.2, location=(0.25, 0.05, 0.65))
connector = bpy.context.active_object
connector.name = "DropConnector"

# Join
bpy.ops.object.select_all(action='SELECT')
bpy.context.view_layer.objects.active = thigh_strap
bpy.ops.object.join()
thigh = bpy.context.active_object
thigh.name = "ThighHolster_Tactical"

thigh.data.materials.append(nylon_mat)
thigh.data.materials.append(leather_mat)
thigh.data.materials.append(metal_mat)
setup_uv_unwrap(thigh)
export_model("ThighHolster_Tactical")

# 4. Ankle Holster (Concealed) - Improved
clear_scene()
leather_mat, metal_mat, nylon_mat, plastic_mat = get_materials()
# Ankle strap
bpy.ops.mesh.primitive_torus_add(location=(0.12, 0, 0.12), major_radius=0.06, minor_radius=0.015)
ankle = bpy.context.active_object
ankle.name = "AnkleStrap"

# Holster body
bpy.ops.mesh.primitive_cube_add(size=1, location=(0.12, 0.04, 0.12))
body = bpy.context.active_object
body.name = "HolsterBody"
body.scale = (0.035, 0.06, 0.12)
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Calf strap
bpy.ops.mesh.primitive_torus_add(location=(0.12, 0, 0.25), major_radius=0.07, minor_radius=0.012)
calf = bpy.context.active_object
calf.name = "CalfStrap"
calf.rotation_euler = (math.pi/2, 0, 0)

# Join
bpy.ops.object.select_all(action='SELECT')
bpy.context.view_layer.objects.active = ankle
bpy.ops.object.join()
ankle_holster = bpy.context.active_object
ankle_holster.name = "AnkleHolster_Concealed"

ankle_holster.data.materials.append(nylon_mat)
ankle_holster.data.materials.append(leather_mat)
setup_uv_unwrap(ankle_holster)
export_model("AnkleHolster_Concealed")

# 5. Chest Rig (Mag Carrier) - Improved
clear_scene()
leather_mat, metal_mat, nylon_mat, plastic_mat = get_materials()
# Main panel
bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0.05, 1.3))
panel = bpy.context.active_object
panel.name = "ChestPanel"
panel.scale = (0.22, 0.02, 0.18)
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Magazine pouches (3x)
for i in range(3):
    x = -0.12 + i * 0.12
    bpy.ops.mesh.primitive_cube_add(size=1, location=(x, 0.04, 1.3))
    pouch = bpy.context.active_object
    pouch.name = f"MagPouch_{i}"
    pouch.scale = (0.035, 0.045, 0.1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Flap
    bpy.ops.mesh.primitive_cube_add(size=1, location=(x, 0.07, 1.35))
    flap = bpy.context.active_object
    flap.name = f"PouchFlap_{i}"
    flap.scale = (0.04, 0.005, 0.08)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Shoulder straps
for x_offset in [-0.18, 0.18]:
    bpy.ops.mesh.primitive_cylinder_add(radius=0.012, depth=0.4, location=(x_offset, 0.08, 1.5))
    strap = bpy.context.active_object
    strap.name = f"ShoulderStrap_{x_offset}"
    strap.rotation_euler = (-0.3, 0, 0)

# Side straps
for x_offset in [-0.22, 0.22]:
    bpy.ops.mesh.primitive_cylinder_add(radius=0.01, depth=0.3, location=(x_offset, 0, 1.2))
    strap = bpy.context.active_object
    strap.name = f"SideStrap_{x_offset}"
    strap.rotation_euler = (0, 0, -0.2)

# Join
bpy.ops.object.select_all(action='SELECT')
bpy.context.view_layer.objects.active = panel
bpy.ops.object.join()
chest = bpy.context.active_object
chest.name = "ChestRig_MagCarrier"

chest.data.materials.append(nylon_mat)
chest.data.materials.append(leather_mat)
chest.data.materials.append(metal_mat)
setup_uv_unwrap(chest)
export_model("ChestRig_MagCarrier")

# 6. Belt Pouches (4x) - Improved
clear_scene()
leather_mat, metal_mat, nylon_mat, plastic_mat = get_materials()
# Belt base
bpy.ops.mesh.primitive_torus_add(location=(0, 0, 0.95), major_radius=0.35, minor_radius=0.015)
belt = bpy.context.active_object
belt.name = "BeltBase"

# 4 pouches
for i in range(4):
    angle = -0.6 + i * 0.4
    x = 0.35 * math.cos(angle)
    z = 0.95 + 0.35 * math.sin(angle)

    bpy.ops.mesh.primitive_cube_add(size=1, location=(x, 0.05, z))
    pouch = bpy.context.active_object
    pouch.name = f"Pouch_{i}"
    pouch.scale = (0.04, 0.06, 0.09)
    pouch.rotation_euler = (0, angle, 0)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Flap
    bpy.ops.mesh.primitive_cube_add(size=1, location=(x + 0.045*math.cos(angle), 0.085, z + 0.045*math.sin(angle)))
    flap = bpy.context.active_object
    flap.name = f"PouchFlap_{i}"
    flap.scale = (0.05, 0.005, 0.07)
    flap.rotation_euler = (0, angle, 0)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Join
bpy.ops.object.select_all(action='SELECT')
bpy.context.view_layer.objects.active = belt
bpy.ops.object.join()
belt_pouches = bpy.context.active_object
belt_pouches.name = "BeltPouches_4x"

belt_pouches.data.materials.append(nylon_mat)
belt_pouches.data.materials.append(leather_mat)
belt_pouches.data.materials.append(metal_mat)
setup_uv_unwrap(belt_pouches)
export_model("BeltPouches_4x")

# 7. Benelli M4 Shell Carrier - Improved
clear_scene()
leather_mat, metal_mat, nylon_mat, plastic_mat = get_materials()
# Belt loop
bpy.ops.mesh.primitive_torus_add(location=(0, 0, 0.9), major_radius=0.05, minor_radius=0.01)
belt = bpy.context.active_object
belt.name = "BeltLoop"
belt.rotation_euler = (math.pi/2, 0, 0)

# Shell tubes (8 shells)
for i in range(8):
    y = -0.07 + i * 0.02
    bpy.ops.mesh.primitive_cylinder_add(radius=0.01, depth=0.06, location=(0, y, 0.92))
    tube = bpy.context.active_object
    tube.name = f"ShellTube_{i}"

# Join
bpy.ops.object.select_all(action='SELECT')
bpy.context.view_layer.objects.active = belt
bpy.ops.object.join()
shell = bpy.context.active_object
shell.name = "BenelliM4_ShellCarrier"

shell.data.materials.append(nylon_mat)
shell.data.materials.append(metal_mat)
setup_uv_unwrap(shell)
export_model("BenelliM4_ShellCarrier")

# 8. KSG Dual Tube Carrier - Improved
clear_scene()
leather_mat, metal_mat, nylon_mat, plastic_mat = get_materials()
# Mount base
bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, 0.9))
base = bpy.context.active_object
base.name = "MountBase"
base.scale = (0.08, 0.04, 0.03)
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Dual tubes
for y_offset in [-0.03, 0.03]:
    for i in range(6):
        z = 0.9 + i * 0.025
        bpy.ops.mesh.primitive_cylinder_add(radius=0.012, depth=0.15, location=(0, y_offset, z))
        tube = bpy.context.active_object
        tube.name = f"Tube_{y_offset}_{i}"

# Join
bpy.ops.object.select_all(action='SELECT')
bpy.context.view_layer.objects.active = base
bpy.ops.object.join()
ksg = bpy.context.active_object
ksg.name = "KSG_DualTubeCarrier"

ksg.data.materials.append(nylon_mat)
ksg.data.materials.append(metal_mat)
setup_uv_unwrap(ksg)
export_model("KSG_DualTubeCarrier")

# 9. Knife Bandolier - Improved
clear_scene()
leather_mat, metal_mat, nylon_mat, plastic_mat = get_materials()
# Chest strap
bpy.ops.mesh.primitive_cylinder_add(radius=0.012, depth=0.5, location=(0, 0.05, 1.4))
strap = bpy.context.active_object
strap.name = "ChestStrap"
strap.rotation_euler = (0, 0, 0)

# Knife sheaths (5)
for i in range(5):
    y = -0.1 + i * 0.05
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, y, 1.4))
    sheath = bpy.context.active_object
    sheath.name = f"KnifeSheath_{i}"
    sheath.scale = (0.02, 0.04, 0.14)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Join
bpy.ops.object.select_all(action='SELECT')
bpy.context.view_layer.objects.active = strap
bpy.ops.object.join()
bandolier = bpy.context.active_object
bandolier.name = "KnifeBandolier"

bandolier.data.materials.append(leather_mat)
bandolier.data.materials.append(nylon_mat)
bandolier.data.materials.append(metal_mat)
setup_uv_unwrap(bandolier)
export_model("KnifeBandolier")

# 10. Pencil Case - Improved
clear_scene()
leather_mat, metal_mat, nylon_mat, plastic_mat = get_materials()
# Case body
bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, 1.1))
body = bpy.context.active_object
body.name = "CaseBody"
body.scale = (0.015, 0.04, 0.18)
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Lid
bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0.045, 1.1))
lid = bpy.context.active_object
lid.name = "CaseLid"
lid.scale = (0.018, 0.005, 0.19)
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

# Clip
bpy.ops.mesh.primitive_torus_add(location=(0, -0.05, 1.1), major_radius=0.01, minor_radius=0.003)
clip = bpy.context.active_object
clip.name = "PocketClip"
clip.rotation_euler = (math.pi/2, 0, 0)

# Join
bpy.ops.object.select_all(action='SELECT')
bpy.context.view_layer.objects.active = body
bpy.ops.object.join()
pencil = bpy.context.active_object
pencil.name = "PencilCase"

pencil.data.materials.append(plastic_mat)
pencil.data.materials.append(metal_mat)
setup_uv_unwrap(pencil)
export_model("PencilCase")

print("\n✅ All 11 equipment models generated as GLB + OBJ!")