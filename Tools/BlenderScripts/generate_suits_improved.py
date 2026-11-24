# Generate improved procedural suit models with better geometry
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

def get_materials(suit_type):
    """Create materials fresh for each model"""
    # Default materials
    fabric = create_material("WoolFabric", (0.05, 0.05, 0.05, 1), metallic=0.0, roughness=0.5)
    armor = create_material("ArmorPlate", (0.1, 0.1, 0.12, 1), metallic=0.3, roughness=0.4)
    strap = create_material("NylonStrap", (0.03, 0.03, 0.03, 1), metallic=0.0, roughness=0.9)
    metal = create_material("MetalHardware", (0.25, 0.25, 0.28, 1), metallic=0.9, roughness=0.2)
    silk = create_material("SilkLining", (0.04, 0.03, 0.02, 1), metallic=0.0, roughness=0.2)

    if suit_type == "tactical":
        fabric = create_material("TacticalFabric", (0.05, 0.05, 0.05, 1), metallic=0.0, roughness=0.85)
        armor = create_material("ArmorPlate", (0.1, 0.1, 0.12, 1), metallic=0.3, roughness=0.4)
        strap = create_material("NylonStrap", (0.03, 0.03, 0.03, 1), metallic=0.0, roughness=0.9)
        metal = create_material("MetalHardware", (0.25, 0.25, 0.28, 1), metallic=0.9, roughness=0.2)
    elif suit_type == "formal":
        fabric = create_material("WoolFabric", (0.02, 0.02, 0.02, 1), metallic=0.0, roughness=0.3)
        silk = create_material("SilkLining", (0.05, 0.03, 0.02, 1), metallic=0.0, roughness=0.15)
        metal = create_material("MetalButtons", (0.4, 0.35, 0.25, 1), metallic=0.9, roughness=0.1)
        strap = create_material("LeatherTrim", (0.08, 0.04, 0.02, 1), metallic=0.0, roughness=0.7)
        armor = create_material("DummyArmor", (0.1, 0.1, 0.12, 1), metallic=0.0, roughness=0.9)  # unused
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
    # classic uses defaults
    return fabric, armor, strap, metal, silk

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

# ========================================================================
# SUIT MODELS
# ========================================================================

def create_suit_base(name, suit_type):
    """Create a base suit model with proper human proportions"""
    clear_scene()
    fabric, armor, strap, metal, silk = get_materials(suit_type)

    # Create torso (jacket)
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, 1.15))
    torso = bpy.context.active_object
    torso.name = f"{name}_Torso"
    torso.scale = (0.48, 0.32, 0.55)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Add subdivision for smoother shape (lower for Quest optimization)
    mod = torso.modifiers.new(name="Subdiv", type='SUBSURF')
    mod.levels = 1
    mod.render_levels = 1

    # Create legs (pants)
    bpy.ops.mesh.primitive_cylinder_add(radius=0.09, depth=0.95, location=(0.16, 0, 0.55))
    leg_r = bpy.context.active_object
    leg_r.name = f"{name}_Leg_R"

    bpy.ops.mesh.primitive_cylinder_add(radius=0.09, depth=0.95, location=(-0.16, 0, 0.55))
    leg_l = bpy.context.active_object
    leg_l.name = f"{name}_Leg_L"

    # Create arms (sleeves)
    bpy.ops.mesh.primitive_cylinder_add(radius=0.07, depth=0.7, location=(0.52, 0, 1.3))
    arm_r = bpy.context.active_object
    arm_r.name = f"{name}_Arm_R"
    arm_r.rotation_euler = (0, 0, -0.3)

    bpy.ops.mesh.primitive_cylinder_add(radius=0.07, depth=0.7, location=(-0.52, 0, 1.3))
    arm_l = bpy.context.active_object
    arm_l.name = f"{name}_Arm_L"
    arm_l.rotation_euler = (0, 0, 0.3)

    # Create head placeholder (collar area)
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.13, location=(0, 0, 1.78))
    head = bpy.context.active_object
    head.name = f"{name}_Head"
    head.scale = (1.1, 1, 1.3)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Join all parts
    bpy.ops.object.select_all(action='DESELECT')
    for obj in [torso, leg_r, leg_l, arm_r, arm_l, head]:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = torso
    bpy.ops.object.join()

    suit = bpy.context.active_object
    suit.name = name
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Apply decimation for Quest optimization (target ~15k tris)
    decimate = suit.modifiers.new(name="Decimate", type='DECIMATE')
    decimate.ratio = 0.15
    decimate.use_collapse_triangulate = True
    bpy.ops.object.modifier_apply(modifier="Decimate")

    return suit, fabric, armor, strap, metal, silk

def add_suit_details(suit, suit_type, fabric, armor, strap, metal, silk):
    """Add style-specific details like lapels, pockets, armor plates, etc."""
    bpy.context.view_layer.objects.active = suit
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='DESELECT')
    bpy.ops.object.mode_set(mode='OBJECT')

    # Add details based on suit type
    if suit_type == "tactical":
        # Armor plates on chest
        bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0.18, 1.25))
        chest_plate = bpy.context.active_object
        chest_plate.name = "ChestPlate"
        chest_plate.scale = (0.25, 0.02, 0.2)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        chest_plate.data.materials.append(armor)

        # Back plate
        bpy.ops.mesh.primitive_cube_add(size=1, location=(0, -0.16, 1.25))
        back_plate = bpy.context.active_object
        back_plate.name = "BackPlate"
        back_plate.scale = (0.25, 0.02, 0.2)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        back_plate.data.materials.append(armor)

        # Shoulder pads
        for x in [-0.45, 0.45]:
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, 0, 1.5))
            shoulder = bpy.context.active_object
            shoulder.name = f"ShoulderPad_{x}"
            shoulder.scale = (0.12, 0.1, 0.08)
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            shoulder.data.materials.append(armor)

        # Tactical pouches on chest
        for i, (x, z) in enumerate([(-0.25, 1.35), (0.25, 1.35), (-0.25, 1.15), (0.25, 1.15)]):
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, 0.19, z))
            pouch = bpy.context.active_object
            pouch.name = f"TacticalPouch_{i}"
            pouch.scale = (0.06, 0.04, 0.08)
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            pouch.data.materials.append(strap)

    elif suit_type == "formal":
        # Lapels
        for x in [-0.18, 0.18]:
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, 0.17, 1.45))
            lapel = bpy.context.active_object
            lapel.name = f"Lapel_{x}"
            lapel.scale = (0.06, 0.015, 0.25)
            lapel.rotation_euler = (0, 0, -0.2 if x > 0 else 0.2)
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            lapel.data.materials.append(fabric)

        # Vest
        bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0.16, 1.1))
        vest = bpy.context.active_object
        vest.name = "Vest"
        vest.scale = (0.35, 0.015, 0.45)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        vest.data.materials.append(silk)

        # Buttons
        for i, z in enumerate([1.5, 1.3, 1.1, 0.9]):
            bpy.ops.mesh.primitive_cylinder_add(radius=0.008, depth=0.01, location=(0.08, 0.18, z))
            btn = bpy.context.active_object
            btn.name = f"Button_{i}"
            btn.data.materials.append(metal)

        # Pocket squares
        bpy.ops.mesh.primitive_cube_add(size=1, location=(0.3, 0.17, 1.3))
        pocket = bpy.context.active_object
        pocket.name = "ChestPocket"
        pocket.scale = (0.05, 0.005, 0.06)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        pocket.data.materials.append(silk)

    elif suit_type == "armored":
        # Heavy armor plates
        plates = [
            (0, 0.2, 1.35, 0.28, 0.03, 0.22),  # Chest
            (0, -0.18, 1.35, 0.28, 0.03, 0.22),  # Back
            (-0.42, 0, 1.55, 0.14, 0.04, 0.1),  # L Shoulder
            (0.42, 0, 1.55, 0.14, 0.04, 0.1),   # R Shoulder
            (-0.35, 0, 1.0, 0.08, 0.03, 0.15),  # L Thigh
            (0.35, 0, 1.0, 0.08, 0.03, 0.15),   # R Thigh
            (-0.18, 0, 0.65, 0.06, 0.03, 0.12), # L Shin
            (0.18, 0, 0.65, 0.06, 0.03, 0.12),  # R Shin
        ]
        for i, (x, y, z, sx, sy, sz) in enumerate(plates):
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, y, z))
            plate = bpy.context.active_object
            plate.name = f"ArmorPlate_{i}"
            plate.scale = (sx, sy, sz)
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            plate.data.materials.append(armor)

        # Tactical gear attachment points
        for x in [-0.3, 0.3]:
            bpy.ops.mesh.primitive_torus_add(location=(x, 0.18, 1.2), major_radius=0.025, minor_radius=0.008)
            ring = bpy.context.active_object
            ring.name = f"AttachmentRing_{x}"
            ring.rotation_euler = (math.pi/2, 0, 0)
            ring.data.materials.append(metal)

    elif suit_type == "elite":
        # High-tech armor plates with glowing edges
        plates = [
            (0, 0.19, 1.3, 0.26, 0.025, 0.2),   # Chest
            (0, -0.17, 1.3, 0.26, 0.025, 0.2),  # Back
            (-0.4, 0, 1.5, 0.12, 0.035, 0.09),  # L Shoulder
            (0.4, 0, 1.5, 0.12, 0.035, 0.09),   # R Shoulder
            (-0.38, 0, 1.05, 0.09, 0.03, 0.16), # L Thigh
            (0.38, 0, 1.05, 0.09, 0.03, 0.16),  # R Thigh
            (-0.2, 0, 0.7, 0.07, 0.025, 0.13),  # L Shin
            (0.2, 0, 0.7, 0.07, 0.025, 0.13),   # R Shin
            (-0.35, 0, 1.4, 0.06, 0.02, 0.06),  # L Upper Arm
            (0.35, 0, 1.4, 0.06, 0.02, 0.06),   # R Upper Arm
        ]
        for i, (x, y, z, sx, sy, sz) in enumerate(plates):
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, y, z))
            plate = bpy.context.active_object
            plate.name = f"ElitePlate_{i}"
            plate.scale = (sx, sy, sz)
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            plate.data.materials.append(armor)

        # Energy cores
        for x, z in [(-0.15, 1.35), (0.15, 1.35)]:
            bpy.ops.mesh.primitive_uv_sphere_add(radius=0.025, location=(x, 0.2, z))
            core = bpy.context.active_object
            core.name = f"EnergyCore_{x}"
            core.data.materials.append(create_material("EnergyCore", (0.0, 0.8, 1.0, 1), metallic=0.0, roughness=0.0, emission=(0.0, 0.8, 1.0, 1)))

        # Advanced attachment system
        for x in [-0.28, 0.28, -0.28, 0.28]:
            z = 1.15 if x < 0 else 1.25
            bpy.ops.mesh.primitive_cylinder_add(radius=0.015, depth=0.03, location=(x, 0.19, z))
            mount = bpy.context.active_object
            mount.name = f"QuickMount_{x}"
            mount.rotation_euler = (math.pi/2, 0, 0)
            mount.data.materials.append(metal)

    else:  # classic
        # Lapels
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
            bpy.ops.mesh.primitive_cylinder_add(radius=0.007, depth=0.008, location=(0.06, 0.17, z))
            btn = bpy.context.active_object
            btn.name = f"Button_{i}"
            btn.data.materials.append(metal)

        # Pockets
        for x, z in [(-0.25, 1.15), (0.25, 1.15), (-0.25, 0.85), (0.25, 0.85)]:
            bpy.ops.mesh.primitive_cube_add(size=1, location=(x, 0.165, z))
            pocket = bpy.context.active_object
            pocket.name = f"Pocket_{x}_{z}"
            pocket.scale = (0.05, 0.008, 0.07)
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            pocket.data.materials.append(fabric)

        # Inner pocket
        bpy.ops.mesh.primitive_cube_add(size=1, location=(-0.15, 0.1, 1.25))
        inner = bpy.context.active_object
        inner.name = "InnerPocket"
        inner.scale = (0.06, 0.04, 0.08)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        inner.data.materials.append(silk)

    # Join all detail objects with main suit
    bpy.ops.object.select_all(action='SELECT')
    bpy.context.view_layer.objects.active = suit
    bpy.ops.object.join()

    return suit

def finalize_suit(suit, suit_type, fabric, armor, strap, metal, silk):
    """Apply final materials and UV unwrapping"""
    suit.data.materials.append(fabric)
    if suit_type in ["tactical", "armored", "elite"]:
        suit.data.materials.append(armor)
    suit.data.materials.append(strap)
    suit.data.materials.append(metal)
    if suit_type in ["classic", "formal"]:
        suit.data.materials.append(silk)

    setup_uv_unwrap(suit)
    return suit

# ========================================================================
# GENERATE ALL 5 SUITS
# ========================================================================

suits = [
    ("ClassicBlackSuit", "classic"),
    ("BlackTacticalSuit", "tactical"),
    ("FormalThreePieceSuit", "formal"),
    ("ArmoredCombatSuit", "armored"),
    ("EliteHighProtectionSuit", "elite"),
]

for suit_name, suit_type in suits:
    suit, fabric, armor, strap, metal, silk = create_suit_base(suit_name, suit_type)
    suit = add_suit_details(suit, suit_type, fabric, armor, strap, metal, silk)
    suit = finalize_suit(suit, suit_type, fabric, armor, strap, metal, silk)
    export_model(suit_name)
    print(f"✅ {suit_name} completed")

print("\n✅ All 5 suit models generated as OBJ!")