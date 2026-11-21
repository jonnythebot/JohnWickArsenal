#!/usr/bin/env python3
"""
John Wick Arsenal - Procedural Weapon Generator
Generates all 10 weapons as FBX files using Blender Python API (bpy).
Run with: blender --background --python generate_weapons.py
"""

import bpy
import bmesh
import math
import os
import sys
from mathutils import Vector, Matrix, Euler

# ============================================================================
# CONFIGURATION
# ============================================================================

OUTPUT_DIR = "/home/wolfwolfwolf8586/JohnWickArsenal/UnityProject/Assets/JohnWickArsenal/Models"

WEAPON_SPECS = {
    # Pistols
    "HK_P30L": {
        "category": "Pistols",
        "type": "pistol",
        "length": 0.186,  # 186mm barrel + slide
        "height": 0.138,
        "width": 0.036,
        "barrel_length": 0.115,
        "slide_length": 0.175,
        "grip_length": 0.125,
        "compensator": True,
        "tactical_rail": True,
        "night_sights": True,
        "mag_capacity": 15,
        "caliber": "9x19mm",
        "details": {
            "slide_cuts": True,
            "slide_serrations": "rear",
            "trigger_guard": "standard",
            "hammer": "internal",
            "safety": "decocker",
        }
    },
    "Glock_34": {
        "category": "Pistols",
        "type": "pistol",
        "length": 0.221,  # Long slide
        "height": 0.138,
        "width": 0.033,
        "barrel_length": 0.135,
        "slide_length": 0.205,
        "grip_length": 0.125,
        "compensator": False,
        "tactical_rail": True,
        "mos_cut": True,
        "flared_magwell": True,
        "minus_connector": True,
        "mag_capacity": 17,
        "caliber": "9x19mm",
        "details": {
            "slide_cuts": True,
            "slide_serrations": "front_rear",
            "trigger_guard": "undercut",
            "hammer": "striker",
            "safety": "safe_action",
        }
    },
    "STI_2011_CombatMaster": {
        "category": "Pistols",
        "type": "pistol",
        "length": 0.216,
        "height": 0.145,
        "width": 0.038,
        "barrel_length": 0.127,
        "slide_length": 0.200,
        "grip_length": 0.135,
        "compensator": True,
        "integrated_comp": True,
        "rmr_cut": True,
        "gold_accents": True,
        "match_trigger": True,
        "extended_magwell": True,
        "mag_capacity": 20,
        "caliber": ".40 S&W",
        "details": {
            "slide_cuts": True,
            "slide_serrations": "front_rear_top",
            "trigger_guard": "undercut",
            "hammer": "external",
            "safety": "thumb",
            "dawson_sights": True,
        }
    },
    "TTI_PitViper": {
        "category": "Pistols",
        "type": "pistol",
        "length": 0.216,
        "height": 0.145,
        "width": 0.038,
        "barrel_length": 0.127,
        "slide_length": 0.200,
        "grip_length": 0.135,
        "compensator": True,
        "tti_comp": True,
        "rmrcc_mount": True,
        "gold_tin_finish": True,
        "viper_pattern": True,
        "custom_grip": True,
        "mag_capacity": 21,
        "caliber": "9mm Major",
        "details": {
            "slide_cuts": True,
            "slide_serrations": "front_rear_top",
            "trigger_guard": "undercut",
            "hammer": "external",
            "safety": "thumb",
            "match_trigger": True,
        }
    },

    # Shotguns
    "Benelli_M4": {
        "category": "Shotguns",
        "type": "shotgun",
        "length": 1.013,  # Extended stock
        "height": 0.220,
        "width": 0.060,
        "barrel_length": 0.470,
        "receiver_length": 0.280,
        "stock_length": 0.350,
        "ghost_ring_sights": True,
        "collapsible_stock": True,
        "pistol_grip": False,
        "shell_carrier": 6,
        "argo_gas_system": True,
        "mag_capacity": 7,
        "caliber": "12 Gauge",
        "details": {
            "vent_rib": True,
            "choke_tubes": True,
            "sling_mounts": True,
            "picatinny_top": True,
        }
    },
    "KelTec_KSG": {
        "category": "Shotguns",
        "type": "shotgun",
        "length": 0.686,  # Bullpup - shorter overall
        "height": 0.180,
        "width": 0.055,
        "barrel_length": 0.470,
        "receiver_length": 0.320,
        "dual_tube": True,
        "tube_selector": True,
        "bullpup": True,
        "forward_ejection": True,
        "picatinny_top": True,
        "forward_grip": True,
        "mag_capacity": 14,  # 7+7
        "caliber": "12 Gauge",
        "details": {
            "vent_rib": False,
            "choke_tubes": True,
            "sling_mounts": True,
            "magswitch": True,
        }
    },

    # Revolver
    "Remington_1875": {
        "category": "Revolvers",
        "type": "revolver",
        "length": 0.330,
        "height": 0.145,
        "width": 0.040,
        "barrel_length": 0.190,
        "cylinder_length": 0.055,
        "grip_length": 0.120,
        "single_action": True,
        "loading_gate": True,
        "ejector_rod": True,
        "case_hardened_frame": True,
        "walnut_grips": True,
        "octagonal_barrel": True,
        "mag_capacity": 6,
        "caliber": ".45 Colt",
        "details": {
            "hammer": "exposed",
            "front_sight": "blade",
            "rear_sight": "notch",
            "finish": "blued_casehardened",
        }
    },

    # Rifle
    "TTI_TR1_Ultralight": {
        "category": "Rifles",
        "type": "rifle",
        "length": 0.890,  # Collapsed stock
        "height": 0.200,
        "width": 0.070,
        "barrel_length": 0.406,  # 16"
        "receiver_length": 0.220,
        "handguard_length": 0.330,
        "stock_length": 0.260,
        "skeletonized_receiver": True,
        "carbon_fiber_handguard": True,
        "mro_optic": True,
        "compensator": True,
        "lightweight_bcg": True,
        "adjustable_gas_block": True,
        "mag_capacity": 30,
        "caliber": "5.56x45mm",
        "details": {
            "charging_handle": "ambidextrous",
            "safety": "ambidextrous",
            "bolt_catch": "extended",
            "mag_release": "extended",
            "trigger": "single_stage",
            "muzzle_device": "tti_comp",
        }
    },

    # Throwables
    "ThrowingKnife": {
        "category": "Throwables",
        "type": "knife",
        "length": 0.300,
        "blade_length": 0.180,
        "handle_length": 0.120,
        "blade_width": 0.025,
        "thickness": 0.006,
        "paracord_wrap": True,
        "balanced": True,
        "double_edged": True,
        "details": {
            "tip": "spear_point",
            "grind": "flat",
            "fuller": False,
            "guard": "none",
        }
    },
    "Pencil": {
        "category": "Throwables",
        "type": "pencil",
        "length": 0.190,
        "diameter": 0.0075,
        "tip_length": 0.015,
        "eraser_length": 0.012,
        "eraser_diameter": 0.009,
        "details": {
            "body_color": "yellow",
            "ferrule_color": "silver",
            "eraser_color": "pink",
            "tip_color": "graphite",
            "brand_text": "HB #2",
        }
    },
}

# ============================================================================
# UTILITY FUNCTIONS
# ============================================================================

def clear_scene():
    """Remove all objects from scene."""
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete()
    # Clear orphaned data
    for block in bpy.data.meshes:
        if block.users == 0:
            bpy.data.meshes.remove(block)
    for block in bpy.data.materials:
        if block.users == 0:
            bpy.data.materials.remove(block)
    for block in bpy.data.textures:
        if block.users == 0:
            bpy.data.textures.remove(block)
    for block in bpy.data.images:
        if block.users == 0:
            bpy.data.images.remove(block)

def create_material(name, color, metallic=0.0, roughness=0.5, emission=None, emission_strength=0.0, specular=0.5):
    """Create a PBR material."""
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    links = mat.node_tree.links

    # Clear default nodes
    for node in nodes:
        nodes.remove(node)

    # Output
    output = nodes.new('ShaderNodeOutputMaterial')
    output.location = (400, 0)

    # Principled BSDF
    principled = nodes.new('ShaderNodeBsdfPrincipled')
    principled.location = (0, 0)
    principled.inputs['Base Color'].default_value = (*color, 1.0)
    principled.inputs['Metallic'].default_value = metallic
    principled.inputs['Roughness'].default_value = roughness

    # Handle different Blender versions - Specular may be called "Specular IOR Level" or not exist
    if 'Specular' in principled.inputs:
        principled.inputs['Specular'].default_value = specular
    elif 'Specular IOR Level' in principled.inputs:
        principled.inputs['Specular IOR Level'].default_value = specular

    if emission:
        principled.inputs['Emission Color'].default_value = (*emission, 1.0)
        principled.inputs['Emission Strength'].default_value = emission_strength

    links.new(principled.outputs['BSDF'], output.inputs['Surface'])

    return mat

def add_bevel(obj, width=0.001, segments=3):
    """Add bevel modifier for edge wear."""
    mod = obj.modifiers.new(name="Bevel", type='BEVEL')
    mod.width = width
    mod.segments = segments
    mod.limit_method = 'ANGLE'
    mod.angle_limit = math.radians(60)
    return mod

def add_subdivision(obj, levels=1):
    """Add subdivision surface modifier."""
    mod = obj.modifiers.new(name="Subdivision", type='SUBSURF')
    mod.levels = levels
    mod.render_levels = levels
    return mod

def apply_transform(obj):
    """Apply location, rotation, scale."""
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

def create_collection(name):
    """Create or get collection."""
    if name in bpy.data.collections:
        return bpy.data.collections[name]
    col = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(col)
    return col

def link_to_collection(obj, collection_name):
    """Link object to collection."""
    col = create_collection(collection_name)
    # Unlink from current collections
    for c in obj.users_collection:
        c.objects.unlink(obj)
    col.objects.link(obj)

# ============================================================================
# PRIMITIVE CREATION HELPERS
# ============================================================================

def create_box(name, size, location=(0,0,0), rotation=(0,0,0)):
    """Create a box mesh."""
    bpy.ops.mesh.primitive_cube_add(size=1, location=location, rotation=rotation)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = size
    apply_transform(obj)
    return obj

def create_cylinder(name, radius, depth, location=(0,0,0), rotation=(0,0,0), vertices=32):
    """Create a cylinder mesh."""
    bpy.ops.mesh.primitive_cylinder_add(radius=radius, depth=depth, vertices=vertices, location=location, rotation=rotation)
    obj = bpy.context.active_object
    obj.name = name
    apply_transform(obj)
    return obj

def create_cone(name, radius1, radius2, depth, location=(0,0,0), rotation=(0,0,0), vertices=32):
    """Create a cone mesh."""
    bpy.ops.mesh.primitive_cone_add(radius1=radius1, radius2=radius2, depth=depth, vertices=vertices, location=location, rotation=rotation)
    obj = bpy.context.active_object
    obj.name = name
    apply_transform(obj)
    return obj

def create_sphere(name, radius, location=(0,0,0)):
    """Create a sphere mesh."""
    bpy.ops.mesh.primitive_uv_sphere_add(radius=radius, location=location, segments=16, ring_count=8)
    obj = bpy.context.active_object
    obj.name = name
    apply_transform(obj)
    return obj

def create_torus(name, major_radius, minor_radius, location=(0,0,0), rotation=(0,0,0)):
    """Create a torus mesh."""
    bpy.ops.mesh.primitive_torus_add(major_radius=major_radius, minor_radius=minor_radius, location=location, rotation=rotation)
    obj = bpy.context.active_object
    obj.name = name
    apply_transform(obj)
    return obj

def boolean_union(obj_a, obj_b):
    """Boolean union of two objects."""
    mod = obj_a.modifiers.new(name="BoolUnion", type='BOOLEAN')
    mod.operation = 'UNION'
    mod.object = obj_b
    bpy.context.view_layer.objects.active = obj_a
    bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.data.objects.remove(obj_b, do_unlink=True)

def boolean_difference(obj_a, obj_b):
    """Boolean difference (A - B)."""
    mod = obj_a.modifiers.new(name="BoolDiff", type='BOOLEAN')
    mod.operation = 'DIFFERENCE'
    mod.object = obj_b
    bpy.context.view_layer.objects.active = obj_a
    bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.data.objects.remove(obj_b, do_unlink=True)

def boolean_intersect(obj_a, obj_b):
    """Boolean intersection."""
    mod = obj_a.modifiers.new(name="BoolIntersect", type='BOOLEAN')
    mod.operation = 'INTERSECT'
    mod.object = obj_b
    bpy.context.view_layer.objects.active = obj_a
    bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.data.objects.remove(obj_b, do_unlink=True)

# ============================================================================
# WEAPON GENERATORS
# ============================================================================

def generate_pistol(name, spec):
    """Generate a pistol model."""
    print(f"Generating pistol: {name}")

    parts = []
    mats = {}

    # Materials
    mats['slide'] = create_material(f"{name}_Slide", (0.12, 0.12, 0.14), metallic=0.9, roughness=0.15)
    mats['frame'] = create_material(f"{name}_Frame", (0.10, 0.10, 0.12), metallic=0.1, roughness=0.8)
    mats['barrel'] = create_material(f"{name}_Barrel", (0.08, 0.08, 0.10), metallic=1.0, roughness=0.05)
    mats['grip'] = create_material(f"{name}_Grip", (0.08, 0.08, 0.09), metallic=0.0, roughness=0.9)
    mats['sights'] = create_material(f"{name}_Sights", (0.05, 0.05, 0.05), metallic=0.5, roughness=0.3)
    mats['gold'] = create_material(f"{name}_Gold", (0.85, 0.65, 0.12), metallic=1.0, roughness=0.1)
    mats['compensator'] = create_material(f"{name}_Comp", (0.15, 0.15, 0.16), metallic=0.95, roughness=0.1)
    mats['trigger'] = create_material(f"{name}_Trigger", (0.15, 0.15, 0.16), metallic=0.8, roughness=0.2)
    mats['controls'] = create_material(f"{name}_Controls", (0.12, 0.12, 0.13), metallic=0.7, roughness=0.25)

    # Main slide
    slide = create_box(f"{name}_Slide",
        (spec['width'] * 0.95, spec['height'] * 0.45, spec['slide_length']),
        (0, spec['height'] * 0.25, 0))
    slide.data.materials.append(mats['slide'])
    parts.append(slide)

    # Slide top serrations
    if spec['details'].get('slide_serrations') in ['rear', 'front_rear', 'front_rear_top']:
        for i in range(8):
            y_offset = spec['slide_length'] * 0.5 - 0.003 - i * 0.004
            serration = create_box(f"{name}_SlideSerr_R_{i}",
                (spec['width'] * 0.8, 0.001, 0.002),
                (0, spec['height'] * 0.45, y_offset))
            serration.data.materials.append(mats['slide'])
            parts.append(serration)

    if spec['details'].get('slide_serrations') in ['front_rear', 'front_rear_top']:
        for i in range(6):
            y_offset = -spec['slide_length'] * 0.5 + 0.003 + i * 0.004
            serration = create_box(f"{name}_SlideSerr_F_{i}",
                (spec['width'] * 0.8, 0.001, 0.002),
                (0, spec['height'] * 0.45, y_offset))
            serration.data.materials.append(mats['slide'])
            parts.append(serration)

    # Top serrations for race guns
    if spec['details'].get('slide_serrations') == 'front_rear_top':
        for i in range(10):
            x_offset = -spec['width'] * 0.35 + i * 0.007
            serration = create_box(f"{name}_SlideSerr_T_{i}",
                (0.0015, 0.001, spec['slide_length'] * 0.7),
                (x_offset, spec['height'] * 0.46, 0))
            serration.data.materials.append(mats['slide'])
            parts.append(serration)

    # Ejection port
    ejection_port = create_box(f"{name}_EjectionPort",
        (spec['width'] * 0.6, spec['height'] * 0.25, 0.045),
        (0, spec['height'] * 0.35, -spec['slide_length'] * 0.25))
    boolean_difference(slide, ejection_port)

    # Barrel (visible through ejection port and muzzle)
    barrel = create_cylinder(f"{name}_Barrel",
        spec['width'] * 0.25, spec['barrel_length'] + 0.01,
        (0, spec['height'] * 0.25, spec['slide_length'] * 0.5 + 0.005),
        (math.radians(90), 0, 0), vertices=16)
    barrel.data.materials.append(mats['barrel'])
    parts.append(barrel)

    # Compensator
    if spec.get('compensator') or spec.get('integrated_comp') or spec.get('tti_comp'):
        comp = create_cylinder(f"{name}_Compensator",
            spec['width'] * 0.35, 0.035,
            (0, spec['height'] * 0.25, spec['barrel_length'] + 0.02),
            (math.radians(90), 0, 0), vertices=16)
        comp.data.materials.append(mats['compensator' if not spec.get('gold_accents') else 'gold'])
        parts.append(comp)

        # Comp ports
        for i in range(3):
            angle = i * 2 * math.pi / 3
            for j in range(2):
                port = create_cylinder(f"{name}_CompPort_{i}_{j}",
                    0.003, 0.04,
                    (math.sin(angle) * 0.008, spec['height'] * 0.25 + math.cos(angle) * 0.008, spec['barrel_length'] + 0.015 + j * 0.015),
                    (math.radians(90), 0, angle + math.radians(90)), vertices=8)
                port.data.materials.append(mats['compensator'])
                parts.append(port)

    # Frame
    frame = create_box(f"{name}_Frame",
        (spec['width'], spec['height'] * 0.7, spec['grip_length'] + 0.03),
        (0, -spec['height'] * 0.1, -spec['grip_length'] * 0.5))
    frame.data.materials.append(mats['frame'])
    parts.append(frame)

    # Trigger guard
    guard_shape = spec['details'].get('trigger_guard', 'standard')
    if guard_shape == 'undercut':
        # Undercut trigger guard
        tg = create_torus(f"{name}_TriggerGuard",
            spec['width'] * 0.35, spec['width'] * 0.12,
            (0, -spec['height'] * 0.15, -0.01),
            (math.radians(90), 0, 0))
        tg.data.materials.append(mats['frame'])
        parts.append(tg)
    else:
        tg = create_box(f"{name}_TriggerGuard",
            (spec['width'] * 0.7, spec['height'] * 0.35, 0.025),
            (0, -spec['height'] * 0.2, 0.01))
        tg.data.materials.append(mats['frame'])
        parts.append(tg)

    # Trigger
    trigger = create_box(f"{name}_Trigger",
        (spec['width'] * 0.25, 0.012, 0.025),
        (0, -spec['height'] * 0.15, 0.015),
        (math.radians(-15), 0, 0))
    trigger.data.materials.append(mats['trigger'])
    parts.append(trigger)

    # Magazine well
    magwell = create_box(f"{name}_Magwell",
        (spec['width'] * 0.85, spec['height'] * 0.6, 0.05),
        (0, -spec['height'] * 0.15, -spec['grip_length'] * 0.5 - 0.025))
    if spec.get('flared_magwell') or spec.get('extended_magwell'):
        magwell.scale.y *= 1.3
        magwell.scale.z *= 1.2
        apply_transform(magwell)
    magwell.data.materials.append(mats['frame'])
    parts.append(magwell)

    # Magazine release
    mag_release = create_box(f"{name}_MagRelease",
        (spec['width'] * 0.6, 0.008, 0.015),
        (spec['width'] * 0.48, -spec['height'] * 0.05, -0.02))
    mag_release.data.materials.append(mats['controls'])
    parts.append(mag_release)

    # Slide release
    slide_release = create_box(f"{name}_SlideRelease",
        (spec['width'] * 0.5, 0.006, 0.02),
        (-spec['width'] * 0.45, spec['height'] * 0.2, -0.01))
    slide_release.data.materials.append(mats['controls'])
    parts.append(slide_release)

    # Safety/Decocker
    if spec['details'].get('safety') in ['decocker', 'thumb', 'safe_action']:
        safety = create_box(f"{name}_Safety",
            (spec['width'] * 0.4, 0.006, 0.015),
            (-spec['width'] * 0.4, spec['height'] * 0.25, -0.02))
        safety.data.materials.append(mats['controls'])
        parts.append(safety)

    # Grip panels
    grip_left = create_box(f"{name}_Grip_L",
        (0.006, spec['height'] * 0.65, spec['grip_length']),
        (-spec['width'] * 0.5 - 0.003, -spec['height'] * 0.1, -spec['grip_length'] * 0.5))
    grip_left.data.materials.append(mats['grip'])
    parts.append(grip_left)

    grip_right = create_box(f"{name}_Grip_R",
        (0.006, spec['height'] * 0.65, spec['grip_length']),
        (spec['width'] * 0.5 + 0.003, -spec['height'] * 0.1, -spec['grip_length'] * 0.5))
    grip_right.data.materials.append(mats['grip'])
    parts.append(grip_right)

    # Backstrap
    backstrap = create_box(f"{name}_Backstrap",
        (spec['width'] * 0.9, 0.008, spec['grip_length']),
        (0, -spec['height'] * 0.42, -spec['grip_length'] * 0.5))
    backstrap.data.materials.append(mats['grip'])
    parts.append(backstrap)

    # Sights
    # Front sight
    fs_height = spec['height'] * 0.15 if not spec.get('dawson_sights') else spec['height'] * 0.18
    front_sight = create_box(f"{name}_FrontSight",
        (spec['width'] * 0.12, fs_height, 0.008),
        (0, spec['height'] * 0.45 + fs_height * 0.5, spec['slide_length'] * 0.45))
    front_sight.data.materials.append(mats['sights'])
    parts.append(front_sight)

    # Night sight tritium insert
    if spec.get('night_sights'):
        tritium = create_box(f"{name}_Tritium_Front",
            (spec['width'] * 0.06, 0.004, 0.004),
            (0, spec['height'] * 0.45 + fs_height * 0.5, spec['slide_length'] * 0.45 + 0.006))
        tritium.data.materials.append(create_material(f"{name}_Tritium", (0.2, 0.9, 0.3), emission=(0.2, 0.9, 0.3), emission_strength=5.0))
        parts.append(tritium)

    # Rear sight
    rs_height = spec['height'] * 0.1
    rear_sight = create_box(f"{name}_RearSight",
        (spec['width'] * 0.25, rs_height, 0.01),
        (0, spec['height'] * 0.45 + rs_height * 0.5, -spec['slide_length'] * 0.4))
    rear_sight.data.materials.append(mats['sights'])
    parts.append(rear_sight)

    # Rear sight notch
    notch = create_box(f"{name}_RearNotch",
        (spec['width'] * 0.12, rs_height * 0.6, 0.012),
        (0, spec['height'] * 0.45 + rs_height * 0.5, -spec['slide_length'] * 0.4 - 0.001))
    boolean_difference(rear_sight, notch)

    # Night sight tritium inserts (two dots)
    if spec.get('night_sights'):
        for side in [-1, 1]:
            tritium = create_box(f"{name}_Tritium_Rear_{side}",
                (0.004, 0.004, 0.004),
                (side * spec['width'] * 0.08, spec['height'] * 0.45 + rs_height * 0.5, -spec['slide_length'] * 0.4 + 0.006))
            tritium.data.materials.append(create_material(f"{name}_Tritium", (0.2, 0.9, 0.3), emission=(0.2, 0.9, 0.3), emission_strength=5.0))
            parts.append(tritium)

    # MOS cut / RMR cut
    if spec.get('mos_cut') or spec.get('rmr_cut') or spec.get('rmrcc_mount'):
        cut_width = spec['width'] * 0.6
        cut_length = 0.032
        cut_depth = spec['height'] * 0.15
        rmr_cut = create_box(f"{name}_RMCCut",
            (cut_width, cut_depth, cut_length),
            (0, spec['height'] * 0.45 + cut_depth * 0.5, 0))
        boolean_difference(slide, rmr_cut)

        # Mounting screws
        for x in [-cut_width*0.35, cut_width*0.35]:
            for z in [-cut_length*0.35, cut_length*0.35]:
                screw = create_cylinder(f"{name}_RMCScrew_{x}_{z}",
                    0.002, 0.006,
                    (x, spec['height'] * 0.45 + cut_depth + 0.003, z),
                    (math.radians(90), 0, 0), vertices=8)
                screw.data.materials.append(mats['gold' if spec.get('gold_accents') else 'controls'])
                parts.append(screw)

    # Gold accents for race guns
    if spec.get('gold_accents'):
        # Trigger
        trigger.data.materials[0] = mats['gold']
        # Controls
        for p in parts:
            if 'Trigger' in p.name or 'Safety' in p.name or 'Release' in p.name:
                p.data.materials[0] = mats['gold']
        # Barrel hood
        barrel.data.materials[0] = mats['gold']

    # Viper pattern (custom texture would be better, but we add geometric pattern)
    if spec.get('viper_pattern'):
        for i in range(12):
            angle = i * 2 * math.pi / 12
            scale = 0.0015
            pattern = create_box(f"{name}_ViperPattern_{i}",
                (scale, scale * 5, spec['slide_length'] * 0.6),
                (math.sin(angle) * spec['width'] * 0.55, spec['height'] * 0.45 + math.cos(angle) * spec['width'] * 0.55 * 0.5, 0))
            pattern.data.materials.append(mats['gold'])
            parts.append(pattern)

    # Tactical rail
    if spec.get('tactical_rail'):
        rail = create_box(f"{name}_TacticalRail",
            (spec['width'] * 0.7, 0.012, 0.05),
            (0, -spec['height'] * 0.1, spec['grip_length'] * 0.5 + 0.02))
        rail.data.materials.append(mats['frame'])
        parts.append(rail)

        # Rail cross slots
        for i in range(4):
            slot = create_box(f"{name}_RailSlot_{i}",
                (spec['width'] * 0.5, 0.003, 0.005),
                (0, -spec['height'] * 0.105, spec['grip_length'] * 0.5 + 0.015 + i * 0.012))
            slot.data.materials.append(mats['frame'])
            parts.append(slot)

    # Magazine
    mag = create_box(f"{name}_Magazine",
        (spec['width'] * 0.8, spec['height'] * 1.1, 0.04),
        (0, -spec['height'] * 0.7, -spec['grip_length'] * 0.5 - 0.02))
    mag.data.materials.append(create_material(f"{name}_Mag", (0.12, 0.12, 0.13), metallic=0.7, roughness=0.2))
    parts.append(mag)

    # Magazine base plate
    base = create_box(f"{name}_MagBase",
        (spec['width'] * 0.85, 0.015, 0.045),
        (0, -spec['height'] * 1.25, -spec['grip_length'] * 0.5 - 0.02))
    base.data.materials.append(mats['gold' if spec.get('gold_accents') else 'controls'])
    parts.append(base)

    # Magazine follower
    follower = create_box(f"{name}_Follower",
        (spec['width'] * 0.7, 0.01, 0.03),
        (0, -spec['height'] * 0.3, -spec['grip_length'] * 0.5 - 0.015))
    follower.data.materials.append(create_material(f"{name}_Follower", (0.6, 0.4, 0.2), metallic=0.0, roughness=0.5))
    parts.append(follower)

    # Hammer (for external hammer pistols)
    if spec['details'].get('hammer') == 'external':
        hammer = create_box(f"{name}_Hammer",
            (spec['width'] * 0.3, 0.012, 0.025),
            (0, spec['height'] * 0.3, -spec['grip_length'] * 0.3))
        hammer.data.materials.append(mats['controls'])
        parts.append(hammer)

    # Dawson Precision sights for STI
    if spec.get('dawson_sights'):
        # Fiber optic front sight
        fiber = create_cylinder(f"{name}_FiberOptic",
            0.0015, 0.006,
            (0, spec['height'] * 0.45 + fs_height + 0.003, spec['slide_length'] * 0.45),
            (math.radians(90), 0, 0), vertices=6)
        fiber.data.materials.append(create_material(f"{name}_Fiber", (1.0, 0.2, 0.1), emission=(1.0, 0.2, 0.1), emission_strength=10.0))
        parts.append(fiber)

    # Join all parts
    main_obj = parts[0]
    for part in parts[1:]:
        try:
            boolean_union(main_obj, part)
        except:
            pass  # Some parts may not union cleanly

    main_obj.name = name

    # Create required transforms for Unity
    create_unity_transforms(main_obj, spec, 'pistol')

    return main_obj

def generate_shotgun(name, spec):
    """Generate a shotgun model."""
    print(f"Generating shotgun: {name}")

    parts = []
    mats = {}

    # Materials
    mats['receiver'] = create_material(f"{name}_Receiver", (0.15, 0.15, 0.16), metallic=0.85, roughness=0.2)
    mats['barrel'] = create_material(f"{name}_Barrel", (0.12, 0.12, 0.13), metallic=1.0, roughness=0.08)
    mats['stock'] = create_material(f"{name}_Stock", (0.06, 0.05, 0.04), metallic=0.0, roughness=0.85)
    mats['polymer'] = create_material(f"{name}_Polymer", (0.08, 0.08, 0.09), metallic=0.0, roughness=0.7)
    mats['chrome'] = create_material(f"{name}_Chrome", (0.75, 0.75, 0.78), metallic=1.0, roughness=0.02)
    mats['sights'] = create_material(f"{name}_Sights", (0.05, 0.05, 0.05), metallic=0.5, roughness=0.3)
    mats['trigger'] = create_material(f"{name}_Trigger", (0.15, 0.15, 0.16), metallic=0.8, roughness=0.2)
    mats['controls'] = create_material(f"{name}_Controls", (0.15, 0.15, 0.16), metallic=0.7, roughness=0.25)

    if spec.get('dual_tube'):
        mats['tube'] = create_material(f"{name}_Tube", (0.18, 0.18, 0.19), metallic=0.8, roughness=0.15)

    is_bullpup = spec.get('bullpup', False)

    if is_bullpup:
        # KSG - Bullpup design
        # Main receiver/body
        receiver = create_box(f"{name}_Receiver",
            (spec['width'] * 1.2, spec['height'] * 0.8, spec['receiver_length']),
            (0, spec['height'] * 0.1, 0))
        receiver.data.materials.append(mats['receiver'])
        parts.append(receiver)

        # Dual magazine tubes (top)
        tube_radius = spec['width'] * 0.35
        tube_length = spec['barrel_length'] * 0.9
        for side in [-1, 1]:
            tube = create_cylinder(f"{name}_MagTube_{side}",
                tube_radius, tube_length,
                (side * tube_radius * 1.1, spec['height'] * 0.6, tube_length * 0.5 - spec['receiver_length'] * 0.5),
                (math.radians(90), 0, 0), vertices=16)
            tube.data.materials.append(mats['tube'])
            parts.append(tube)

        # Tube selector switch
        selector = create_box(f"{name}_TubeSelector",
            (spec['width'] * 0.3, 0.015, 0.025),
            (0, spec['height'] * 0.85, -spec['receiver_length'] * 0.3))
        selector.data.materials.append(mats['controls'])
        parts.append(selector)

        # Barrel (bottom)
        barrel = create_cylinder(f"{name}_Barrel",
            spec['width'] * 0.28, spec['barrel_length'],
            (0, spec['height'] * 0.15, spec['barrel_length'] * 0.5),
            (math.radians(90), 0, 0), vertices=16)
        barrel.data.materials.append(mats['barrel'])
        parts.append(barrel)

        # Forward grip
        if spec.get('forward_grip'):
            fgrip = create_box(f"{name}_ForwardGrip",
                (spec['width'] * 0.4, spec['height'] * 0.35, 0.08),
                (0, spec['height'] * 0.1, spec['receiver_length'] * 0.5))
            fgrip.data.materials.append(mats['polymer'])
            parts.append(fgrip)

        # Pistol grip (rear)
        pgrip = create_box(f"{name}_PistolGrip",
            (spec['width'] * 0.35, spec['height'] * 0.5, 0.09),
            (0, -spec['height'] * 0.15, -spec['receiver_length'] * 0.55))
        pgrip.data.materials.append(mats['polymer'])
        parts.append(pgrip)

        # Trigger
        trigger = create_box(f"{name}_Trigger",
            (spec['width'] * 0.25, 0.01, 0.03),
            (0, -spec['height'] * 0.05, -spec['receiver_length'] * 0.3))
        trigger.data.materials.append(mats['trigger'])
        parts.append(trigger)

        # Picatinny rail (top)
        rail = create_box(f"{name}_PicatinnyRail",
            (spec['width'] * 0.7, 0.01, spec['receiver_length'] * 0.9),
            (0, spec['height'] * 0.55, 0))
        rail.data.materials.append(mats['receiver'])
        parts.append(rail)

        # Rail slots
        for i in range(8):
            slot = create_box(f"{name}_RailSlot_{i}",
                (spec['width'] * 0.5, 0.003, 0.006),
                (0, spec['height'] * 0.56, -spec['receiver_length'] * 0.4 + i * 0.045))
            slot.data.materials.append(mats['receiver'])
            parts.append(slot)

        # Charging handle (left side)
        ch = create_box(f"{name}_ChargingHandle",
            (0.015, spec['height'] * 0.15, 0.04),
            (-spec['width'] * 0.7, spec['height'] * 0.35, -spec['receiver_length'] * 0.2))
        ch.data.materials.append(mats['controls'])
        parts.append(ch)

        # Sights on rail
        fs = create_box(f"{name}_FrontSight",
            (spec['width'] * 0.1, spec['height'] * 0.12, 0.008),
            (0, spec['height'] * 0.68, spec['receiver_length'] * 0.4))
        fs.data.materials.append(mats['sights'])
        parts.append(fs)

        rs = create_box(f"{name}_RearSight",
            (spec['width'] * 0.2, spec['height'] * 0.08, 0.008),
            (0, spec['height'] * 0.64, -spec['receiver_length'] * 0.4))
        rs.data.materials.append(mats['sights'])
        parts.append(rs)

        notch = create_box(f"{name}_RearNotch",
            (spec['width'] * 0.1, spec['height'] * 0.05, 0.01),
            (0, spec['height'] * 0.64, -spec['receiver_length'] * 0.4 - 0.001))
        boolean_difference(rs, notch)

    else:
        # Benelli M4 - Traditional layout
        # Receiver
        receiver = create_box(f"{name}_Receiver",
            (spec['width'], spec['height'] * 0.7, spec['receiver_length']),
            (0, spec['height'] * 0.1, 0))
        receiver.data.materials.append(mats['receiver'])
        parts.append(receiver)

        # Barrel
        barrel = create_cylinder(f"{name}_Barrel",
            spec['width'] * 0.3, spec['barrel_length'],
            (0, spec['height'] * 0.1, spec['receiver_length'] * 0.5 + spec['barrel_length'] * 0.5),
            (math.radians(90), 0, 0), vertices=16)
        barrel.data.materials.append(mats['chrome' if 'Benelli' in name else 'barrel'])
        parts.append(barrel)

        # Vent rib
        if spec.get('vent_rib'):
            rib = create_box(f"{name}_VentRib",
                (spec['width'] * 0.12, 0.006, spec['barrel_length'] * 0.95),
                (0, spec['height'] * 0.42, spec['receiver_length'] * 0.5 + spec['barrel_length'] * 0.5))
            rib.data.materials.append(mats['barrel'])
            parts.append(rib)

        # Ghost ring sights
        if spec.get('ghost_ring_sights'):
            # Rear ghost ring
            ring = create_torus(f"{name}_GhostRing",
                spec['width'] * 0.25, 0.004,
                (0, spec['height'] * 0.35, -spec['receiver_length'] * 0.4),
                (math.radians(90), 0, 0))
            ring.data.materials.append(mats['sights'])
            parts.append(ring)

            # Front sight post
            fs = create_box(f"{name}_FrontSight",
                (spec['width'] * 0.08, spec['height'] * 0.12, 0.006),
                (0, spec['height'] * 0.45, spec['receiver_length'] * 0.5 + spec['barrel_length'] * 0.9))
            fs.data.materials.append(mats['sights'])
            parts.append(fs)

        # ARGO Gas System (visual)
        if spec.get('argo_gas_system'):
            gas_block = create_box(f"{name}_GasBlock",
                (spec['width'] * 0.5, spec['height'] * 0.15, 0.04),
                (0, spec['height'] * 0.25, spec['receiver_length'] * 0.5 + 0.08))
            gas_block.data.materials.append(mats['receiver'])
            parts.append(gas_block)

            # Gas piston housing
            piston = create_cylinder(f"{name}_GasPiston",
                spec['width'] * 0.12, 0.06,
                (0, spec['height'] * 0.1, spec['receiver_length'] * 0.5 + 0.08),
                (math.radians(90), 0, 0), vertices=12)
            piston.data.materials.append(mats['chrome'])
            parts.append(piston)

        # Collapsible stock
        if spec.get('collapsible_stock'):
            # Stock tube
            tube = create_cylinder(f"{name}_StockTube",
                spec['width'] * 0.2, spec['stock_length'],
                (0, spec['height'] * 0.05, -spec['stock_length'] * 0.5),
                (0, 0, 0), vertices=12)
            tube.data.materials.append(mats['receiver'])
            parts.append(tube)

            # Stock butt
            butt = create_box(f"{name}_StockButt",
                (spec['width'] * 0.6, spec['height'] * 0.4, 0.03),
                (0, spec['height'] * 0.05, -spec['stock_length'] - 0.015))
            butt.data.materials.append(mats['polymer'])
            parts.append(butt)

            # Adjustment lever
            lever = create_box(f"{name}_StockLever",
                (0.015, 0.03, 0.04),
                (-spec['width'] * 0.3, spec['height'] * 0.15, -spec['stock_length'] * 0.3))
            lever.data.materials.append(mats['controls'])
            parts.append(lever)

        # Shell carrier
        if spec.get('shell_carrier'):
            carrier = create_box(f"{name}_ShellCarrier",
                (spec['width'] * 0.7, 0.015, spec['shell_carrier'] * 0.025),
                (-spec['width'] * 0.7, spec['height'] * 0.4, 0))
            carrier.data.materials.append(mats['receiver'])
            parts.append(carrier)

            # Shells
            for i in range(spec['shell_carrier']):
                shell = create_cylinder(f"{name}_Shell_{i}",
                    0.01, 0.025,
                    (-spec['width'] * 0.85, spec['height'] * 0.42, -0.05 + i * 0.025),
                    (math.radians(90), 0, 0), vertices=8)
                shell.data.materials.append(create_material(f"{name}_Shell", (0.7, 0.5, 0.2), metallic=0.2, roughness=0.4))
                parts.append(shell)

        # Trigger group
        trigger = create_box(f"{name}_Trigger",
            (spec['width'] * 0.25, 0.01, 0.03),
            (0, -spec['height'] * 0.15, -spec['receiver_length'] * 0.3))
        trigger.data.materials.append(mats['trigger'])
        parts.append(trigger)

        # Trigger guard
        tg = create_torus(f"{name}_TriggerGuard",
            spec['width'] * 0.3, spec['width'] * 0.1,
            (0, -spec['height'] * 0.25, -spec['receiver_length'] * 0.2),
            (math.radians(90), 0, 0))
        tg.data.materials.append(mats['receiver'])
        parts.append(tg)

        # Safety
        safety = create_box(f"{name}_Safety",
            (spec['width'] * 0.35, 0.006, 0.02),
            (-spec['width'] * 0.4, spec['height'] * 0.2, -spec['receiver_length'] * 0.1))
        safety.data.materials.append(mats['controls'])
        parts.append(safety)

        # Bolt handle (right side)
        bolt = create_box(f"{name}_BoltHandle",
            (0.015, 0.012, 0.05),
            (spec['width'] * 0.55, spec['height'] * 0.4, 0))
        bolt.data.materials.append(mats['controls'])
        parts.append(bolt)

        # Magazine tube
        tube = create_cylinder(f"{name}_MagTube",
            spec['width'] * 0.25, spec['receiver_length'] + spec['barrel_length'] * 0.7,
            (0, spec['height'] * 0.1, spec['receiver_length'] * 0.2 + spec['barrel_length'] * 0.35),
            (math.radians(90), 0, 0), vertices=12)
        tube.data.materials.append(mats['barrel'])
        parts.append(tube)

        # Foregrip
        foregrip = create_box(f"{name}_Foregrip",
            (spec['width'] * 0.5, spec['height'] * 0.25, 0.06),
            (0, -spec['height'] * 0.05, spec['receiver_length'] * 0.5 + 0.15))
        foregrip.data.materials.append(mats['polymer'])
        parts.append(foregrip)

    # Join parts
    main_obj = parts[0]
    for part in parts[1:]:
        try:
            boolean_union(main_obj, part)
        except:
            pass

    main_obj.name = name
    create_unity_transforms(main_obj, spec, 'shotgun')

    return main_obj

def generate_revolver(name, spec):
    """Generate a revolver model."""
    print(f"Generating revolver: {name}")

    parts = []
    mats = {}

    # Materials
    mats['frame'] = create_material(f"{name}_Frame", (0.12, 0.12, 0.13), metallic=0.9, roughness=0.15)
    mats['barrel'] = create_material(f"{name}_Barrel", (0.08, 0.08, 0.10), metallic=1.0, roughness=0.05)
    mats['cylinder'] = create_material(f"{name}_Cylinder", (0.15, 0.12, 0.10), metallic=0.95, roughness=0.12)
    mats['grip'] = create_material(f"{name}_Grip", (0.15, 0.08, 0.04), metallic=0.0, roughness=0.85)
    mats['hammer'] = create_material(f"{name}_Hammer", (0.15, 0.15, 0.16), metallic=0.8, roughness=0.2)
    mats['trigger'] = create_material(f"{name}_Trigger", (0.15, 0.15, 0.16), metallic=0.8, roughness=0.2)
    mats['sights'] = create_material(f"{name}_Sights", (0.05, 0.05, 0.05), metallic=0.5, roughness=0.3)
    mats['ejector'] = create_material(f"{name}_Ejector", (0.15, 0.15, 0.16), metallic=0.9, roughness=0.1)
    mats['brass'] = create_material(f"{name}_Brass", (0.8, 0.6, 0.2), metallic=1.0, roughness=0.1)
    mats['controls'] = create_material(f"{name}_Controls", (0.15, 0.15, 0.16), metallic=0.7, roughness=0.25)

    # Frame
    frame = create_box(f"{name}_Frame",
        (spec['width'], spec['height'] * 0.6, spec['grip_length'] + 0.04),
        (0, -spec['height'] * 0.05, -spec['grip_length'] * 0.5))
    frame.data.materials.append(mats['frame'])
    parts.append(frame)

    # Barrel (octagonal if specified)
    if spec.get('octagonal_barrel'):
        # Create octagonal barrel using cylinder with 8 vertices
        barrel = create_cylinder(f"{name}_Barrel",
            spec['width'] * 0.3, spec['barrel_length'],
            (0, spec['height'] * 0.15, spec['barrel_length'] * 0.5),
            (math.radians(90), 0, math.radians(22.5)), vertices=8)
    else:
        barrel = create_cylinder(f"{name}_Barrel",
            spec['width'] * 0.3, spec['barrel_length'],
            (0, spec['height'] * 0.15, spec['barrel_length'] * 0.5),
            (math.radians(90), 0, 0), vertices=16)
    barrel.data.materials.append(mats['barrel'])
    parts.append(barrel)

    # Cylinder
    cylinder = create_cylinder(f"{name}_Cylinder",
        spec['width'] * 0.55, spec['cylinder_length'],
        (0, spec['height'] * 0.15, -spec['cylinder_length'] * 0.5 - 0.01),
        (math.radians(90), 0, 0), vertices=16)
    cylinder.data.materials.append(mats['cylinder'])
    parts.append(cylinder)

    # Cylinder flutes
    for i in range(6):
        angle = i * 2 * math.pi / 6
        flute = create_cylinder(f"{name}_CylinderFlute_{i}",
            0.006, spec['cylinder_length'] + 0.002,
            (math.sin(angle) * spec['width'] * 0.45, spec['height'] * 0.15 + math.cos(angle) * spec['width'] * 0.45, -spec['cylinder_length'] * 0.5 - 0.01),
            (math.radians(90), 0, angle), vertices=8)
        flute.data.materials.append(mats['cylinder'])
        parts.append(flute)

    # Cylinder chambers
    for i in range(6):
        angle = i * 2 * math.pi / 6
        chamber = create_cylinder(f"{name}_Chamber_{i}",
            spec['width'] * 0.15, spec['cylinder_length'] * 0.9,
            (math.sin(angle) * spec['width'] * 0.25, spec['height'] * 0.15 + math.cos(angle) * spec['width'] * 0.25, -spec['cylinder_length'] * 0.5),
            (math.radians(90), 0, angle), vertices=8)
        boolean_difference(cylinder, chamber)

    # Hammer
    if spec.get('single_action'):
        hammer = create_box(f"{name}_Hammer",
            (spec['width'] * 0.35, 0.012, 0.035),
            (0, spec['height'] * 0.4, -spec['cylinder_length'] * 0.5 - 0.03))
        hammer.data.materials.append(mats['hammer'])
        parts.append(hammer)

        # Hammer spur
        spur = create_box(f"{name}_HammerSpur",
            (0.008, 0.025, 0.012),
            (0, spec['height'] * 0.45, -spec['cylinder_length'] * 0.5 - 0.03))
        spur.data.materials.append(mats['hammer'])
        parts.append(spur)

    # Trigger
    trigger = create_box(f"{name}_Trigger",
        (spec['width'] * 0.2, 0.01, 0.025),
        (0, -spec['height'] * 0.2, -spec['cylinder_length'] * 0.5 + 0.02))
    trigger.data.materials.append(mats['trigger'])
    parts.append(trigger)

    # Trigger guard
    tg = create_torus(f"{name}_TriggerGuard",
        spec['width'] * 0.3, spec['width'] * 0.1,
        (0, -spec['height'] * 0.3, -spec['cylinder_length'] * 0.5 + 0.03),
        (math.radians(90), 0, 0))
    tg.data.materials.append(mats['frame'])
    parts.append(tg)

    # Grips (walnut)
    grip_left = create_box(f"{name}_Grip_L",
        (0.008, spec['height'] * 0.55, spec['grip_length']),
        (-spec['width'] * 0.5 - 0.004, -spec['height'] * 0.1, -spec['grip_length'] * 0.5))
    grip_left.data.materials.append(mats['grip'])
    parts.append(grip_left)

    grip_right = create_box(f"{name}_Grip_R",
        (0.008, spec['height'] * 0.55, spec['grip_length']),
        (spec['width'] * 0.5 + 0.004, -spec['height'] * 0.1, -spec['grip_length'] * 0.5))
    grip_right.data.materials.append(mats['grip'])
    parts.append(grip_right)

    # Grip medallions
    for side in [-1, 1]:
        medallion = create_cylinder(f"{name}_GripMedallion_{side}",
            0.012, 0.004,
            (side * (spec['width'] * 0.5 + 0.01), -spec['height'] * 0.1, -spec['grip_length'] * 0.2),
            (0, math.radians(90), 0), vertices=12)
        medallion.data.materials.append(mats['brass'])
        parts.append(medallion)

    # Loading gate (right side)
    if spec.get('loading_gate'):
        gate = create_box(f"{name}_LoadingGate",
            (0.004, spec['height'] * 0.3, spec['cylinder_length'] * 1.2),
            (spec['width'] * 0.5 + 0.002, spec['height'] * 0.1, -spec['cylinder_length'] * 0.5 - 0.01))
        gate.data.materials.append(mats['frame'])
        parts.append(gate)

        # Gate latch
        latch = create_box(f"{name}_GateLatch",
            (0.01, 0.006, 0.008),
            (spec['width'] * 0.5 + 0.006, spec['height'] * 0.25, -spec['cylinder_length'] * 0.5 - 0.01))
        latch.data.materials.append(mats['controls'])
        parts.append(latch)

    # Ejector rod
    if spec.get('ejector_rod'):
        rod = create_cylinder(f"{name}_EjectorRod",
            0.003, spec['barrel_length'] + 0.02,
            (0, spec['height'] * 0.15 - 0.005, spec['barrel_length'] * 0.5 + 0.01),
            (math.radians(90), 0, 0), vertices=8)
        rod.data.materials.append(mats['ejector'])
        parts.append(rod)

        # Ejector rod head
        head = create_cylinder(f"{name}_EjectorHead",
            0.008, 0.006,
            (0, spec['height'] * 0.15 - 0.005, -spec['cylinder_length'] * 0.5 - 0.02),
            (math.radians(90), 0, 0), vertices=12)
        head.data.materials.append(mats['ejector'])
        parts.append(head)

    # Front sight
    fs = create_box(f"{name}_FrontSight",
        (spec['width'] * 0.06, spec['height'] * 0.1, 0.005),
        (0, spec['height'] * 0.35, spec['barrel_length'] * 0.9))
    fs.data.materials.append(mats['sights'])
    parts.append(fs)

    # Rear sight (notch in frame)
    rs = create_box(f"{name}_RearSight",
        (spec['width'] * 0.15, spec['height'] * 0.06, 0.005),
        (0, spec['height'] * 0.3, -spec['cylinder_length'] * 0.5 - 0.02))
    rs.data.materials.append(mats['sights'])
    parts.append(rs)

    notch = create_box(f"{name}_RearNotch",
        (spec['width'] * 0.08, spec['height'] * 0.04, 0.007),
        (0, spec['height'] * 0.3, -spec['cylinder_length'] * 0.5 - 0.025))
    boolean_difference(rs, notch)

    # Case-hardened frame coloring (simulated with material)
    if spec.get('case_hardened_frame'):
        frame.data.materials[0] = create_material(f"{name}_CaseHardened",
            (0.25, 0.18, 0.12), metallic=0.7, roughness=0.3)

    # Join parts
    main_obj = parts[0]
    for part in parts[1:]:
        try:
            boolean_union(main_obj, part)
        except:
            pass

    main_obj.name = name
    create_unity_transforms(main_obj, spec, 'revolver')

    return main_obj

def generate_rifle(name, spec):
    """Generate a rifle model."""
    print(f"Generating rifle: {name}")

    parts = []
    mats = {}

    # Materials
    mats['receiver'] = create_material(f"{name}_Receiver", (0.12, 0.12, 0.13), metallic=0.85, roughness=0.2)
    mats['barrel'] = create_material(f"{name}_Barrel", (0.10, 0.10, 0.11), metallic=1.0, roughness=0.08)
    mats['handguard'] = create_material(f"{name}_Handguard", (0.08, 0.08, 0.09), metallic=0.1, roughness=0.7)
    mats['carbon'] = create_material(f"{name}_CarbonFiber", (0.05, 0.05, 0.05), metallic=0.3, roughness=0.4)
    mats['stock'] = create_material(f"{name}_Stock", (0.10, 0.10, 0.11), metallic=0.2, roughness=0.6)
    mats['trigger'] = create_material(f"{name}_Trigger", (0.15, 0.15, 0.16), metallic=0.8, roughness=0.2)
    mats['controls'] = create_material(f"{name}_Controls", (0.12, 0.12, 0.13), metallic=0.7, roughness=0.25)
    mats['optic'] = create_material(f"{name}_Optic", (0.08, 0.08, 0.09), metallic=0.8, roughness=0.15)
    mats['glass'] = create_material(f"{name}_Glass", (0.1, 0.1, 0.15), metallic=0.0, roughness=0.0, emission=(0.0, 0.0, 0.0))
    mats['gold'] = create_material(f"{name}_Gold", (0.85, 0.65, 0.12), metallic=1.0, roughness=0.1)
    mats['sights'] = create_material(f"{name}_Sights", (0.05, 0.05, 0.05), metallic=0.5, roughness=0.3)

    # Upper receiver
    upper = create_box(f"{name}_UpperReceiver",
        (spec['width'], spec['height'] * 0.5, spec['receiver_length']),
        (0, spec['height'] * 0.1, 0))
    upper.data.materials.append(mats['receiver'])
    parts.append(upper)

    # Skeletonized cuts in upper
    if spec.get('skeletonized_receiver'):
        for i in range(5):
            y_offset = -spec['receiver_length'] * 0.35 + i * 0.035
            cut = create_box(f"{name}_SkeletonCut_{i}",
                (spec['width'] * 0.6, spec['height'] * 0.35, 0.015),
                (0, spec['height'] * 0.15, y_offset))
            boolean_difference(upper, cut)

    # Lower receiver
    lower = create_box(f"{name}_LowerReceiver",
        (spec['width'] * 0.9, spec['height'] * 0.7, spec['receiver_length'] * 0.8),
        (0, -spec['height'] * 0.1, -spec['receiver_length'] * 0.1))
    lower.data.materials.append(mats['receiver'])
    parts.append(lower)

    # Magwell
    magwell = create_box(f"{name}_Magwell",
        (spec['width'] * 0.7, spec['height'] * 1.0, 0.06),
        (0, -spec['height'] * 0.6, -spec['receiver_length'] * 0.45))
    magwell.data.materials.append(mats['receiver'])
    parts.append(magwell)

    # Magazine
    mag = create_box(f"{name}_Magazine",
        (spec['width'] * 0.65, spec['height'] * 1.8, 0.04),
        (0, -spec['height'] * 1.3, -spec['receiver_length'] * 0.45))
    mag.data.materials.append(create_material(f"{name}_Mag", (0.12, 0.12, 0.13), metallic=0.7, roughness=0.2))
    parts.append(mag)

    # Barrel
    barrel = create_cylinder(f"{name}_Barrel",
        spec['width'] * 0.28, spec['barrel_length'],
        (0, spec['height'] * 0.1, spec['receiver_length'] * 0.5 + spec['barrel_length'] * 0.5),
        (math.radians(90), 0, 0), vertices=16)
    barrel.data.materials.append(mats['barrel'])
    parts.append(barrel)

    # Gas block (adjustable)
    if spec.get('adjustable_gas_block'):
        gb = create_box(f"{name}_GasBlock",
            (spec['width'] * 0.6, spec['height'] * 0.2, 0.035),
            (0, spec['height'] * 0.25, spec['receiver_length'] * 0.5 + 0.1))
        gb.data.materials.append(mats['receiver'])
        parts.append(gb)

        # Gas adjustment knob
        knob = create_cylinder(f"{name}_GasKnob",
            0.012, 0.015,
            (spec['width'] * 0.4, spec['height'] * 0.25, spec['receiver_length'] * 0.5 + 0.1),
            (0, 0, 0), vertices=12)
        knob.data.materials.append(mats['controls'])
        parts.append(knob)

    # Compensator
    if spec.get('compensator'):
        comp = create_cylinder(f"{name}_Compensator",
            spec['width'] * 0.4, 0.055,
            (0, spec['height'] * 0.1, spec['receiver_length'] * 0.5 + spec['barrel_length'] + 0.02),
            (math.radians(90), 0, 0), vertices=16)
        comp.data.materials.append(mats['gold' if 'TTI' in name else 'receiver'])
        parts.append(comp)

        # Comp ports
        for i in range(4):
            angle = i * math.pi / 2
            for j in range(3):
                port = create_cylinder(f"{name}_CompPort_{i}_{j}",
                    0.0035, 0.055,
                    (math.sin(angle) * 0.012, spec['height'] * 0.1 + math.cos(angle) * 0.012, spec['receiver_length'] * 0.5 + spec['barrel_length'] + 0.005 + j * 0.015),
                    (math.radians(90), 0, angle), vertices=8)
                port.data.materials.append(mats['receiver'])
                parts.append(port)

    # Handguard
    if spec.get('carbon_fiber_handguard'):
        hg = create_cylinder(f"{name}_Handguard",
            spec['width'] * 0.42, spec['handguard_length'],
            (0, spec['height'] * 0.1, spec['receiver_length'] * 0.5 + spec['handguard_length'] * 0.5),
            (math.radians(90), 0, 0), vertices=16)
        hg.data.materials.append(mats['carbon'])
        parts.append(hg)

        # M-LOK slots
        for i in range(7):
            z_pos = -spec['handguard_length'] * 0.45 + i * 0.045
            for angle in [0, math.pi/2, math.pi, 3*math.pi/2]:
                if angle in [0, math.pi]:  # Top and bottom only for M-LOK
                    slot = create_box(f"{name}_MLOK_{i}_{angle}",
                        (0.006, 0.002, 0.03),
                        (math.sin(angle) * 0.035, spec['height'] * 0.1 + math.cos(angle) * 0.035, spec['receiver_length'] * 0.5 + z_pos))
                    slot.data.materials.append(mats['carbon'])
                    parts.append(slot)

        # Top rail section
        rail = create_box(f"{name}_HandguardRail",
            (spec['width'] * 0.5, 0.01, spec['handguard_length']),
            (0, spec['height'] * 0.33, spec['receiver_length'] * 0.5))
        rail.data.materials.append(mats['carbon'])
        parts.append(rail)
    else:
        hg = create_box(f"{name}_Handguard",
            (spec['width'] * 0.7, spec['height'] * 0.35, spec['handguard_length']),
            (0, spec['height'] * 0.1, spec['receiver_length'] * 0.5))
        hg.data.materials.append(mats['handguard'])
        parts.append(hg)

    # Stock
    stock = create_box(f"{name}_Stock",
        (spec['width'] * 0.7, spec['height'] * 0.5, spec['stock_length']),
        (0, spec['height'] * 0.05, -spec['receiver_length'] * 0.2 - spec['stock_length'] * 0.5))
    stock.data.materials.append(mats['stock'])
    parts.append(stock)

    # Buffer tube
    bt = create_cylinder(f"{name}_BufferTube",
        spec['width'] * 0.22, spec['stock_length'] + 0.05,
        (0, spec['height'] * 0.05, -spec['receiver_length'] * 0.2 - spec['stock_length'] * 0.5 - 0.025),
        (0, 0, 0), vertices=12)
    bt.data.materials.append(mats['receiver'])
    parts.append(bt)

    # Stock adjustment lever
    lever = create_box(f"{name}_StockLever",
        (0.02, 0.03, 0.04),
        (-spec['width'] * 0.35, spec['height'] * 0.15, -spec['receiver_length'] * 0.2 - 0.05))
    lever.data.materials.append(mats['controls'])
    parts.append(lever)

    # Trigger group
    trigger = create_box(f"{name}_Trigger",
        (spec['width'] * 0.25, 0.01, 0.03),
        (0, -spec['height'] * 0.25, -spec['receiver_length'] * 0.3))
    trigger.data.materials.append(mats['trigger'])
    parts.append(trigger)

    # Trigger guard
    tg = create_torus(f"{name}_TriggerGuard",
        spec['width'] * 0.35, spec['width'] * 0.12,
        (0, -spec['height'] * 0.35, -spec['receiver_length'] * 0.2),
        (math.radians(90), 0, 0))
    tg.data.materials.append(mats['receiver'])
    parts.append(tg)

    # Safety selector (ambidextrous)
    for side in [-1, 1]:
        safety = create_box(f"{name}_Safety_{side}",
            (spec['width'] * 0.45, 0.006, 0.02),
            (side * spec['width'] * 0.5, spec['height'] * 0.15, -spec['receiver_length'] * 0.1))
        safety.data.materials.append(mats['controls'])
        parts.append(safety)

    # Magazine release (ambidextrous)
    for side in [-1, 1]:
        mag_rel = create_box(f"{name}_MagRelease_{side}",
            (spec['width'] * 0.4, 0.006, 0.015),
            (side * spec['width'] * 0.5, -spec['height'] * 0.3, -spec['receiver_length'] * 0.35))
        mag_rel.data.materials.append(mats['controls'])
        parts.append(mag_rel)

    # Bolt catch (extended)
    bolt_catch = create_box(f"{name}_BoltCatch",
        (spec['width'] * 0.3, 0.006, 0.025),
        (-spec['width'] * 0.5, spec['height'] * 0.2, -spec['receiver_length'] * 0.05))
    bolt_catch.data.materials.append(mats['controls'])
    parts.append(bolt_catch)

    # Charging handle (ambidextrous)
    ch = create_box(f"{name}_ChargingHandle",
        (spec['width'] * 0.6, 0.015, 0.05),
        (0, spec['height'] * 0.35, -spec['receiver_length'] * 0.2))
    ch.data.materials.append(mats['controls'])
    parts.append(ch)

    # Latch
    latch = create_box(f"{name}_CH_Latch",
        (0.01, 0.02, 0.01),
        (spec['width'] * 0.25, spec['height'] * 0.35, -spec['receiver_length'] * 0.2))
    latch.data.materials.append(mats['controls'])
    parts.append(latch)

    # Forward assist
    fa = create_box(f"{name}_ForwardAssist",
        (0.02, 0.02, 0.02),
        (spec['width'] * 0.55, spec['height'] * 0.25, 0.02))
    fa.data.materials.append(mats['controls'])
    parts.append(fa)

    # Ejection port cover
    cover = create_box(f"{name}_EjectCover",
        (spec['width'] * 0.5, 0.003, 0.04),
        (0, spec['height'] * 0.35, 0.03))
    cover.data.materials.append(mats['receiver'])
    parts.append(cover)

    # MRO Optic
    if spec.get('mro_optic'):
        # Optic body
        optic = create_cylinder(f"{name}_OpticBody",
            0.025, 0.05,
            (0, spec['height'] * 0.5, -spec['receiver_length'] * 0.15),
            (0, 0, 0), vertices=16)
        optic.data.materials.append(mats['optic'])
        parts.append(optic)

        # Objective lens
        lens = create_cylinder(f"{name}_OpticLens",
            0.022, 0.003,
            (0, spec['height'] * 0.5, -spec['receiver_length'] * 0.15 + 0.025),
            (0, 0, 0), vertices=16)
        lens.data.materials.append(mats['glass'])
        parts.append(lens)

        # Mount
        mount = create_box(f"{name}_OpticMount",
            (spec['width'] * 0.5, 0.015, 0.04),
            (0, spec['height'] * 0.45, -spec['receiver_length'] * 0.15))
        mount.data.materials.append(mats['receiver'])
        parts.append(mount)

        # Red dot (emissive)
        dot = create_cylinder(f"{name}_RedDot",
            0.0015, 0.001,
            (0, spec['height'] * 0.5, -spec['receiver_length'] * 0.15 + 0.0255),
            (0, 0, 0), vertices=8)
        dot.data.materials.append(create_material(f"{name}_Dot", (1.0, 0.0, 0.0), emission=(1.0, 0.0, 0.0), emission_strength=20.0))
        parts.append(dot)

    # Sights (backup iron sights on handguard rail)
    fs = create_box(f"{name}_BUIS_Front",
        (spec['width'] * 0.1, spec['height'] * 0.15, 0.008),
        (0, spec['height'] * 0.45, spec['receiver_length'] * 0.5 + spec['handguard_length'] * 0.4))
    fs.data.materials.append(mats['sights'])
    parts.append(fs)

    rs = create_box(f"{name}_BUIS_Rear",
        (spec['width'] * 0.2, spec['height'] * 0.1, 0.008),
        (0, spec['height'] * 0.45, -spec['receiver_length'] * 0.35))
    rs.data.materials.append(mats['sights'])
    parts.append(rs)

    notch = create_box(f"{name}_BUIS_Notch",
        (spec['width'] * 0.1, spec['height'] * 0.06, 0.01),
        (0, spec['height'] * 0.45, -spec['receiver_length'] * 0.35 - 0.001))
    boolean_difference(rs, notch)

    # Pistol grip
    pgrip = create_box(f"{name}_PistolGrip",
        (spec['width'] * 0.3, spec['height'] * 0.55, 0.08),
        (0, -spec['height'] * 0.35, -spec['receiver_length'] * 0.35))
    pgrip.data.materials.append(mats['stock'])
    parts.append(pgrip)

    # Join parts
    main_obj = parts[0]
    for part in parts[1:]:
        try:
            boolean_union(main_obj, part)
        except:
            pass

    main_obj.name = name
    create_unity_transforms(main_obj, spec, 'rifle')

    return main_obj

def generate_throwing_knife(name, spec):
    """Generate a throwing knife model."""
    print(f"Generating throwing knife: {name}")

    parts = []
    mats = {}

    mats['blade'] = create_material(f"{name}_Blade", (0.2, 0.2, 0.22), metallic=1.0, roughness=0.02)
    mats['paracord'] = create_material(f"{name}_Paracord", (0.1, 0.05, 0.03), metallic=0.0, roughness=0.9)
    mats['guard'] = create_material(f"{name}_Guard", (0.15, 0.15, 0.16), metallic=0.8, roughness=0.2)

    # Blade (double-edged, spear point)
    blade_len = spec['blade_length']
    blade_w = spec['blade_width']
    thick = spec['thickness']

    # Main blade body
    blade = create_box(f"{name}_Blade",
        (thick, blade_w, blade_len * 0.7),
        (0, 0, blade_len * 0.35))
    blade.data.materials.append(mats['blade'])
    parts.append(blade)

    # Spear point tip
    tip = create_cone(f"{name}_Tip",
        blade_w * 0.5, 0, blade_len * 0.3,
        (0, 0, blade_len * 0.85),
        (math.radians(90), 0, 0), vertices=8)
    tip.data.materials.append(mats['blade'])
    parts.append(tip)

    # Full tang handle
    tang = create_box(f"{name}_Tang",
        (thick * 1.5, blade_w * 0.6, spec['handle_length']),
        (0, 0, -spec['handle_length'] * 0.5))
    tang.data.materials.append(mats['blade'])
    parts.append(tang)

    # Paracord wrap
    for i in range(int(spec['handle_length'] / 0.006)):
        z = -spec['handle_length'] * 0.5 + i * 0.006
        wrap = create_cylinder(f"{name}_Paracord_{i}",
            thick * 1.2, 0.005,
            (0, 0, z), (0, 0, 0), vertices=8)
        wrap.data.materials.append(mats['paracord'])
        parts.append(wrap)

    # Lanyard hole
    hole = create_cylinder(f"{name}_LanyardHole",
        0.004, thick * 2,
        (0, 0, -spec['handle_length'] * 0.5 - 0.005),
        (math.radians(90), 0, 0), vertices=8)
    boolean_difference(tang, hole)

    # Balance point marker (visual)
    bp = create_box(f"{name}_BalancePoint",
        (thick * 3, 0.001, blade_w * 0.3),
        (0, blade_w * 0.5 + 0.0005, spec['length'] * 0.45))
    bp.data.materials.append(create_material(f"{name}_BP", (1.0, 0.8, 0.0), emission=(1.0, 0.8, 0.0), emission_strength=2.0))
    parts.append(bp)

    # Join parts
    main_obj = parts[0]
    for part in parts[1:]:
        try:
            boolean_union(main_obj, part)
        except:
            pass

    main_obj.name = name
    create_unity_transforms(main_obj, spec, 'knife')

    return main_obj

def generate_pencil(name, spec):
    """Generate the legendary pencil model."""
    print(f"Generating pencil: {name}")

    parts = []
    mats = {}

    mats['body'] = create_material(f"{name}_Body", (1.0, 0.85, 0.0), metallic=0.0, roughness=0.7)
    mats['ferrule'] = create_material(f"{name}_Ferrule", (0.75, 0.75, 0.78), metallic=1.0, roughness=0.05)
    mats['eraser'] = create_material(f"{name}_Eraser", (1.0, 0.6, 0.7), metallic=0.0, roughness=0.9)
    mats['graphite'] = create_material(f"{name}_Graphite", (0.08, 0.08, 0.09), metallic=0.2, roughness=0.6)
    mats['text'] = create_material(f"{name}_Text", (0.1, 0.1, 0.1), metallic=0.0, roughness=0.5)

    # Main body (hexagonal)
    body = create_cylinder(f"{name}_Body",
        spec['diameter'] * 0.5, spec['length'] - spec['tip_length'] - spec['eraser_length'],
        (0, 0, (spec['length'] - spec['tip_length'] - spec['eraser_length']) * 0.5 - spec['eraser_length']),
        (0, 0, 0), vertices=6)
    body.data.materials.append(mats['body'])
    parts.append(body)

    # Ferrule (metal band)
    ferrule = create_cylinder(f"{name}_Ferrule",
        spec['diameter'] * 0.52, spec['eraser_length'],
        (0, 0, -spec['eraser_length'] * 0.5),
        (0, 0, 0), vertices=16)
    ferrule.data.materials.append(mats['ferrule'])
    parts.append(ferrule)

    # Eraser
    eraser = create_cylinder(f"{name}_Eraser",
        spec['eraser_diameter'] * 0.5, spec['eraser_length'] * 0.8,
        (0, 0, -spec['eraser_length'] - spec['eraser_length'] * 0.4),
        (0, 0, 0), vertices=16)
    eraser.data.materials.append(mats['eraser'])
    parts.append(eraser)

    # Sharpened tip
    tip = create_cone(f"{name}_Tip",
        spec['diameter'] * 0.5, 0, spec['tip_length'],
        (0, 0, spec['length'] - spec['tip_length'] * 0.5 - spec['eraser_length']),
        (0, 0, 0), vertices=6)
    tip.data.materials.append(mats['graphite'])
    parts.append(tip)

    # Graphite core (visible at tip)
    core = create_cylinder(f"{name}_Core",
        0.001, spec['tip_length'] * 0.5,
        (0, 0, spec['length'] - spec['tip_length'] * 0.75 - spec['eraser_length']),
        (0, 0, 0), vertices=6)
    core.data.materials.append(mats['graphite'])
    parts.append(core)

    # "HB #2" text imprint (simulated as groove)
    text_groove = create_box(f"{name}_Text",
        (spec['diameter'] * 2, 0.0005, 0.03),
        (0, spec['diameter'] * 0.5 + 0.0003, -spec['eraser_length'] - 0.02))
    text_groove.data.materials.append(mats['text'])
    parts.append(text_groove)

    # Lacquer bands (visual)
    for i in range(3):
        z = -spec['eraser_length'] + (spec['length'] - spec['tip_length'] - spec['eraser_length']) * (i + 1) / 4
        band = create_cylinder(f"{name}_Band_{i}",
            spec['diameter'] * 0.51, 0.001,
            (0, 0, z), (0, 0, 0), vertices=16)
        band.data.materials.append(create_material(f"{name}_Band", (0.9, 0.7, 0.0), metallic=0.0, roughness=0.3))
        parts.append(band)

    # Join parts
    main_obj = parts[0]
    for part in parts[1:]:
        try:
            boolean_union(main_obj, part)
        except:
            pass

    main_obj.name = name
    create_unity_transforms(main_obj, spec, 'pencil')

    return main_obj

def create_unity_transforms(obj, spec, weapon_type):
    """Create Unity-required transform hierarchy."""
    # Create empty parent for proper hierarchy
    root = bpy.data.objects.new(obj.name + "_Root", None)
    bpy.context.scene.collection.objects.link(root)
    obj.parent = root

    # Create required transforms
    transforms = {
        'MuzzlePoint': Vector((0, 0, 0)),
        'EjectionPort': Vector((0, 0, 0)),
        'Grip_L': Vector((0, 0, 0)),
        'Grip_R': Vector((0, 0, 0)),
    }

    if weapon_type in ['rifle', 'shotgun']:
        transforms['Sight_Front'] = Vector((0, 0, 0))
        transforms['Sight_Rear'] = Vector((0, 0, 0))

    if weapon_type in ['knife', 'pencil']:
        transforms['Tip'] = Vector((0, 0, 0))

    # Calculate positions based on weapon type and spec
    if weapon_type == 'pistol':
        transforms['MuzzlePoint'] = Vector((0, spec['height'] * 0.25, spec['barrel_length'] + (0.035 if spec.get('compensator') else 0)))
        transforms['EjectionPort'] = Vector((spec['width'] * 0.3, spec['height'] * 0.35, -spec['slide_length'] * 0.25))
        transforms['Grip_L'] = Vector((-spec['width'] * 0.5, -spec['height'] * 0.3, -spec['grip_length'] * 0.3))
        transforms['Grip_R'] = Vector((spec['width'] * 0.5, -spec['height'] * 0.3, -spec['grip_length'] * 0.3))
    elif weapon_type == 'shotgun':
        if spec.get('bullpup'):
            transforms['MuzzlePoint'] = Vector((0, spec['height'] * 0.15, spec['barrel_length']))
            transforms['EjectionPort'] = Vector((0.04, spec['height'] * 0.2, spec['receiver_length'] * 0.3))
            transforms['Grip_L'] = Vector((-spec['width'] * 0.3, spec['height'] * 0.05, -spec['receiver_length'] * 0.4))
            transforms['Grip_R'] = Vector((spec['width'] * 0.3, spec['height'] * 0.05, -spec['receiver_length'] * 0.4))
            transforms['Sight_Front'] = Vector((0, spec['height'] * 0.5, spec['receiver_length'] * 0.4))
            transforms['Sight_Rear'] = Vector((0, spec['height'] * 0.45, -spec['receiver_length'] * 0.4))
        else:
            transforms['MuzzlePoint'] = Vector((0, spec['height'] * 0.1, spec['receiver_length'] * 0.5 + spec['barrel_length']))
            transforms['EjectionPort'] = Vector((spec['width'] * 0.5, spec['height'] * 0.3, 0))
            transforms['Grip_L'] = Vector((-spec['width'] * 0.4, -spec['height'] * 0.15, -spec['receiver_length'] * 0.3))
            transforms['Grip_R'] = Vector((spec['width'] * 0.4, -spec['height'] * 0.15, -spec['receiver_length'] * 0.3))
            transforms['Sight_Front'] = Vector((0, spec['height'] * 0.4, spec['receiver_length'] * 0.5 + spec['barrel_length'] * 0.9))
            transforms['Sight_Rear'] = Vector((0, spec['height'] * 0.35, -spec['receiver_length'] * 0.4))
    elif weapon_type == 'revolver':
        transforms['MuzzlePoint'] = Vector((0, spec['height'] * 0.15, spec['barrel_length']))
        transforms['EjectionPort'] = Vector((0, spec['height'] * 0.15, -spec['cylinder_length'] * 0.5))
        transforms['Grip_L'] = Vector((-spec['width'] * 0.5, -spec['height'] * 0.2, -spec['grip_length'] * 0.3))
        transforms['Grip_R'] = Vector((spec['width'] * 0.5, -spec['height'] * 0.2, -spec['grip_length'] * 0.3))
    elif weapon_type == 'rifle':
        transforms['MuzzlePoint'] = Vector((0, spec['height'] * 0.1, spec['receiver_length'] * 0.5 + spec['barrel_length'] + (0.055 if spec.get('compensator') else 0)))
        transforms['EjectionPort'] = Vector((spec['width'] * 0.5, spec['height'] * 0.3, 0.03))
        transforms['Grip_L'] = Vector((-spec['width'] * 0.35, -spec['height'] * 0.3, -spec['receiver_length'] * 0.35))
        transforms['Grip_R'] = Vector((spec['width'] * 0.35, -spec['height'] * 0.3, -spec['receiver_length'] * 0.35))
        transforms['Sight_Front'] = Vector((0, spec['height'] * 0.35, spec['receiver_length'] * 0.5 + spec['handguard_length'] * 0.4))
        transforms['Sight_Rear'] = Vector((0, spec['height'] * 0.35, -spec['receiver_length'] * 0.35))
    elif weapon_type == 'knife':
        transforms['MuzzlePoint'] = Vector((0, 0, spec['length']))
        transforms['Tip'] = Vector((0, 0, spec['length']))
        transforms['Grip_L'] = Vector((0, 0, 0))
        transforms['Grip_R'] = Vector((0, 0, 0))
    elif weapon_type == 'pencil':
        transforms['MuzzlePoint'] = Vector((0, 0, spec['length']))
        transforms['Tip'] = Vector((0, 0, spec['length']))
        transforms['Grip_L'] = Vector((0, 0, -spec['eraser_length']))
        transforms['Grip_R'] = Vector((0, 0, -spec['eraser_length']))

    for name, pos in transforms.items():
        t = bpy.data.objects.new(name, None)
        t.empty_display_type = 'ARROWS'
        t.empty_display_size = 0.02
        t.parent = root
        t.location = pos
        bpy.context.scene.collection.objects.link(t)

    return root

def export_obj(obj, filepath):
    """Export object as OBJ."""
    # Select only the root and its children
    bpy.ops.object.select_all(action='DESELECT')
    root = obj
    while root.parent:
        root = root.parent

    def select_hierarchy(o):
        o.select_set(True)
        for child in o.children:
            select_hierarchy(child)

    select_hierarchy(root)
    bpy.context.view_layer.objects.active = root

    # Export as OBJ
    bpy.ops.wm.obj_export(
        filepath=filepath,
        export_selected_objects=True,
        apply_modifiers=True,
        export_materials=True,
        export_uv=True,
        export_normals=True,
        forward_axis='NEGATIVE_Z',
        up_axis='Y',
    )

    print(f"Exported: {filepath}")

def main():
    """Main generation function."""
    print("=" * 60)
    print("JOHN WICK ARSENAL - Procedural Weapon Generator")
    print("=" * 60)

    # Ensure output directories exist
    for category in ['Pistols', 'Shotguns', 'Revolvers', 'Rifles', 'Throwables']:
        os.makedirs(os.path.join(OUTPUT_DIR, category), exist_ok=True)

    generated = []

    for name, spec in WEAPON_SPECS.items():
        try:
            clear_scene()

            category = spec['category']
            wtype = spec['type']

            if wtype == 'pistol':
                obj = generate_pistol(name, spec)
            elif wtype == 'shotgun':
                obj = generate_shotgun(name, spec)
            elif wtype == 'revolver':
                obj = generate_revolver(name, spec)
            elif wtype == 'rifle':
                obj = generate_rifle(name, spec)
            elif wtype == 'knife':
                obj = generate_throwing_knife(name, spec)
            elif wtype == 'pencil':
                obj = generate_pencil(name, spec)
            else:
                print(f"Unknown weapon type: {wtype}")
                continue

            # Export
            out_path = os.path.join(OUTPUT_DIR, category, f"{name}.obj")
            export_obj(obj, out_path)
            generated.append((name, category, out_path))

        except Exception as e:
            print(f"ERROR generating {name}: {e}")
            import traceback
            traceback.print_exc()

    print("\n" + "=" * 60)
    print("GENERATION COMPLETE")
    print("=" * 60)
    for name, cat, path in generated:
        size = os.path.getsize(path) / 1024
        print(f"  {cat}/{name}.obj ({size:.1f} KB)")

if __name__ == "__main__":
    main()