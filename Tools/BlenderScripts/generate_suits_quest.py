# Generate Quest-optimized procedural suit models (low poly)
import bpy
import math
import os

def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete()
    for block in bpy.data.meshes:
        if block.users == 0:
            bpy.data.meshes.remove(block)
    for block in bpy.data.materials:
        if block.users == 0:
            bpy.data.materials.remove(block)

def create_material(name, base_color, metallic=0.0, roughness=0.5):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    bsdf = nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs['Base Color'].default_value = base_color
        bsdf.inputs['Metallic'].default_value = metallic
        bsdf.inputs['Roughness'].default_value = roughness
    return mat

def get_materials(suit_type):
    if suit_type == "tactical":
        fabric = create_material("TacticalFabric", (0.05, 0.05, 0.05, 1), metallic=0.0, roughness=0.85)
        armor = create_material("ArmorPlate", (0.1, 0.1, 0.12, 1), metallic=0.3, roughness=0.4)
        strap = create_material("NylonStrap", (0.03, 0.03, 0.03, 1), metallic=0.0, roughness=0.9)
        metal = create_material("MetalHardware", (0.25, 0.25, 0.28, 1), metallic=0.9, roughness=0.2)
    elif suit_type == "formal":
        fabric = create_material("WoolFabric", (0.02, 0.02, 0.02, 1), metallic=0.0, roughness=0.3)
        armor = create_material("Dummy", (0.1, 0.1, 0.12, 1), metallic=0.0, roughness=0.9)
        strap = create_material("LeatherTrim", (0.08, 0.04, 0.02, 1), metallic=0.0, roughness=0.7)
        metal = create_material("MetalButtons", (0.4, 0.35, 0.25, 1), metallic=0.9, roughness=0.1)
    elif suit_type == "armored":
        fabric = create_material("BallisticNylon", (0.06, 0.06, 0.07, 1), metallic=0.0, roughness=0.7)
        armor = create_material("CeramicPlate", (0.15, 0.15, 0.18, 1), metallic=0.2, roughness=0.3)
        strap = create_material("HeavyWebbing", (0.04, 0.04, 0.04, 1), metallic=0.0, roughness=0.85)
        metal = create_material("MetalHardware", (0.2, 0.2, 0.22, 1), metallic=0.85, roughness=0.25)
    elif suit_type == "elite":
        fabric = create_material("AdvancedComposite", (0.03, 0.03, 0.04, 1), metallic=0.05, roughness=0.4)
        armor = create_material("TitaniumPlate", (0.22, 0.22, 0.25, 1), metallic=0.7, roughness=0.2)
        strap = create_material("CarbonWebbing", (0.02, 0.02, 0.02, 1), metallic=0.1, roughness=0.3)
        metal = create_material("TitaniumHardware", (0.35, 0.35, 0.38, 1), metallic=0.95, roughness=0.1)
    else:  # classic
        fabric = create_material("WoolFabric", (0.05, 0.05, 0.05, 1), metallic=0.0, roughness=0.5)
        armor = create_material("Dummy", (0.1, 0.1, 0.12, 1), metallic=0.0, roughness=0.9)
        strap = create_material("LeatherTrim", (0.08, 0.04, 0.02, 1), metallic=0.0, roughness=0.7)
        metal = create_material("MetalButtons", (0.3, 0.25, 0.2, 1), metallic=0.8, roughness=0.15)
    return fabric, armor, strap, metal

def setup_uv_unwrap(obj):
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=66, island_margin=0.02)
    bpy.ops.object.mode_set(mode='OBJECT')

def export_model(name):
    bpy.ops.wm.obj_export(filepath=os.path.join(suits_path, f"{name}.obj"))
    print(f"✅ {name}.obj exported")

suits_path = "/home/wolfwolfwolf8586/JohnWickArsenal/UnityProject/Assets/JohnWickArsenal/Models/Suits/"

def create_quest_suit(name, suit_type):
    """Create a low-poly suit optimized for Quest (<10k tris)"""
    clear_scene()
    fabric, armor, strap, metal = get_materials(suit_type)

    # Create simple torso (jacket) - low poly cube with minimal subdiv
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, 1.15))
    torso = bpy.context.active_object
    torso.name = f"{name}_Torso"
    torso.scale = (0.48, 0.32, 0.55)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Simple legs (pants) - cylinders with low segments
    bpy.ops.mesh.primitive_cylinder_add(radius=0.09, depth=0.95, location=(0.16, 0, 0.55), vertices=8)
    leg_r = bpy.context.active_object
    leg_r.name = f"{name}_Leg_R"

    bpy.ops.mesh.primitive_cylinder_add(radius=0.09, depth=0.95, location=(-0.16, 0, 0.55), vertices=8)
    leg_l = bpy.context.active_object
    leg_l.name = f"{name}_Leg_L"

    # Simple arms (sleeves) - low segments
    bpy.ops.mesh.primitive_cylinder_add(radius=0.07, depth=0.7, location=(0.52, 0, 1.3), vertices=8)
    arm_r = bpy.context.active_object
    arm_r.name = f"{name}_Arm_R"
    arm_r.rotation_euler = (0, 0, -0.3)

    bpy.ops.mesh.primitive_cylinder_add(radius=0.07, depth=0.7, location=(-0.52, 0, 1.3), vertices=8)
    arm_l = bpy.context.active_object
    arm_l.name = f"{name}_Arm_L"
    arm_l.rotation_euler = (0, 0, 0.3)

    # Simple head/collar
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.13, location=(0, 0, 1.78), segments=8, ring_count=6)
    head = bpy.context.active_object
    head.name = f"{name}_Head"
    head.scale = (1.1, 1, 1.3)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Join base parts
    bpy.ops.object.select_all(action='DESELECT')
    for obj in [torso, leg_r, leg_l, arm_r, arm_l, head]:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = torso
    bpy.ops.object.join()

    suit = bpy.context.active_object
    suit.name = name
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Add suit-specific details (low poly)
    if suit_type == "tactical":
        # Chest plate
        bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0.18, 1.25))
        plate = bpy.context.active_object
        plate.name = "ChestPlate"
        plate.scale = (0.25, 0.02, 0.2)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        plate.data.materials.append(armor)

        # Back plate
        bpy.ops.mesh.primitive_cube_add(size=1, location=(0, -0.16, 1.25))
        plate = bpy.context.active_object
        plate.name = "BackPlate"
        plate.scale = (0.25, 0.02, 0.2)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        plate.data.materials.append(armor)

        # Shoulder pads
        for x in [-0.45, 0.45]:
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, 0, 1.5))
            pad = bpy.context.active_object
            pad.name = f"ShoulderPad_{x}"
            pad.scale = (0.12, 0.1, 0.08)
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            pad.data.materials.append(armor)

    elif suit_type == "formal":
        # Lapels (simple)
        for x in [-0.18, 0.18]:
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, 0.17, 1.45))
            lapel = bpy.context.active_object
            lapel.name = f"Lapel_{x}"
            lapel.scale = (0.06, 0.015, 0.25)
            lapel.rotation_euler = (0, 0, -0.2 if x > 0 else 0.2)
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            lapel.data.materials.append(fabric)

        # Buttons
        for i, z in enumerate([1.5, 1.3, 1.1, 0.9]):
            bpy.ops.mesh.primitive_cylinder_add(radius=0.008, depth=0.01, location=(0.08, 0.18, z), vertices=6)
            btn = bpy.context.active_object
            btn.name = f"Button_{i}"
            btn.data.materials.append(metal)

        # Chest pocket
        bpy.ops.mesh.primitive_cube_add(size=1, location=(0.3, 0.17, 1.3))
        pocket = bpy.context.active_object
        pocket.name = "ChestPocket"
        pocket.scale = (0.05, 0.005, 0.06)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        pocket.data.materials.append(fabric)

    elif suit_type == "armored":
        # Fewer, larger armor plates
        plates = [
            (0, 0.2, 1.35, 0.28, 0.03, 0.22),  # Chest
            (0, -0.18, 1.35, 0.28, 0.03, 0.22), # Back
            (-0.42, 0, 1.55, 0.14, 0.04, 0.1),  # L Shoulder
            (0.42, 0, 1.55, 0.14, 0.04, 0.1),   # R Shoulder
        ]
        for i, (x, y, z, sx, sy, sz) in enumerate(plates):
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, y, z))
            plate = bpy.context.active_object
            plate.name = f"ArmorPlate_{i}"
            plate.scale = (sx, sy, sz)
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            plate.data.materials.append(armor)

    elif suit_type == "elite":
        # Minimal high-tech plates
        plates = [
            (0, 0.19, 1.3, 0.26, 0.025, 0.2),   # Chest
            (0, -0.17, 1.3, 0.26, 0.025, 0.2),  # Back
            (-0.4, 0, 1.5, 0.12, 0.035, 0.09),  # L Shoulder
            (0.4, 0, 1.5, 0.12, 0.035, 0.09),   # R Shoulder
        ]
        for i, (x, y, z, sx, sy, sz) in enumerate(plates):
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, y, z))
            plate = bpy.context.active_object
            plate.name = f"ElitePlate_{i}"
            plate.scale = (sx, sy, sz)
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            plate.data.materials.append(armor)

    else:  # classic
        # Simple lapels
        for x in [-0.16, 0.16]:
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, 0.165, 1.4))
            lapel = bpy.context.active_object
            lapel.name = f"Lapel_{x}"
            lapel.scale = (0.05, 0.012, 0.22)
            lapel.rotation_euler = (0, 0, -0.15 if x > 0 else 0.15)
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            lapel.data.materials.append(fabric)

        # Buttons
        for i, z in enumerate([1.45, 1.25, 1.05]):
            bpy.ops.mesh.primitive_cylinder_add(radius=0.007, depth=0.008, location=(0.06, 0.17, z), vertices=6)
            btn = bpy.context.active_object
            btn.name = f"Button_{i}"
            btn.data.materials.append(metal)

        # Simple pockets
        for x, z in [(-0.25, 1.15), (0.25, 1.15)]:
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, 0.165, z))
            pocket = bpy.context.active_object
            pocket.name = f"Pocket_{x}"
            pocket.scale = (0.05, 0.008, 0.07)
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            pocket.data.materials.append(fabric)

    # Join all details
    bpy.ops.object.select_all(action='SELECT')
    bpy.context.view_layer.objects.active = suit
    bpy.ops.object.join()

    # Apply materials
    suit.data.materials.append(fabric)
    if suit_type in ["tactical", "armored", "elite"]:
        suit.data.materials.append(armor)
    suit.data.materials.append(strap)
    suit.data.materials.append(metal)

    # UV unwrap
    setup_uv_unwrap(suit)

    return suit

# ========================================================================
# GENERATE ALL 5 SUITS (Quest Optimized)
# ========================================================================

suits = [
    ("ClassicBlackSuit", "classic"),
    ("BlackTacticalSuit", "tactical"),
    ("FormalThreePieceSuit", "formal"),
    ("ArmoredCombatSuit", "armored"),
    ("EliteHighProtectionSuit", "elite"),
]

for suit_name, suit_type in suits:
    suit = create_quest_suit(suit_name, suit_type)
    export_model(suit_name)
    print(f"✅ {suit_name} completed")

print("\n✅ All 5 Quest-optimized suit models generated as OBJ!")