#!/usr/bin/env python3
"""
John Wick Arsenal - Procedural Equipment Generator
Generates holsters, pouches, and suits as OBJ files using Blender Python API.
Run with: blender --background --python generate_equipment.py
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

EQUIPMENT_SPECS = {
    # Holsters
    "ShoulderHolster_Double": {
        "category": "Equipment",
        "type": "holster",
        "width": 0.25, "height": 0.35, "depth": 0.05,
        "pistol_slots": 2,
        "material": "leather",
    },
    "HipHolster_TTIRace": {
        "category": "Equipment",
        "type": "holster",
        "width": 0.18, "height": 0.25, "depth": 0.06,
        "pistol_slots": 1,
        "race_cut": True,
        "material": "kydex",
    },
    "ThighHolster_Tactical": {
        "category": "Equipment",
        "type": "holster",
        "width": 0.15, "height": 0.30, "depth": 0.06,
        "pistol_slots": 1,
        "strap": True,
        "material": "polymer",
    },
    "AnkleHolster_Concealed": {
        "category": "Equipment",
        "type": "holster",
        "width": 0.12, "height": 0.15, "depth": 0.04,
        "pistol_slots": 1,
        "material": "leather",
    },

    # Magazine Pouches
    "ChestRig_MagCarrier": {
        "category": "Equipment",
        "type": "pouch",
        "width": 0.30, "height": 0.25, "depth": 0.05,
        "mag_slots": 6,
        "rows": 2, "cols": 3,
        "material": "nylon",
    },
    "BeltPouches_4x": {
        "category": "Equipment",
        "type": "pouch",
        "width": 0.40, "height": 0.18, "depth": 0.05,
        "mag_slots": 4,
        "rows": 1, "cols": 4,
        "material": "nylon",
    },
    "BenelliM4_ShellCarrier": {
        "category": "Equipment",
        "type": "pouch",
        "width": 0.25, "height": 0.12, "depth": 0.04,
        "shell_slots": 6,
        "material": "polymer",
    },
    "KSG_DualTubeCarrier": {
        "category": "Equipment",
        "type": "pouch",
        "width": 0.35, "height": 0.15, "depth": 0.05,
        "shell_slots": 14,
        "material": "polymer",
    },

    # Throwable Carriers
    "KnifeBandolier": {
        "category": "Equipment",
        "type": "pouch",
        "width": 0.45, "height": 0.08, "depth": 0.02,
        "knife_slots": 8,
        "material": "leather",
    },
    "PencilCase": {
        "category": "Equipment",
        "type": "pouch",
        "width": 0.22, "height": 0.05, "depth": 0.02,
        "pencil_slots": 3,
        "material": "leather",
    },
}

SUIT_SPECS = {
    "ClassicBlackSuit": {
        "category": "Suits",
        "type": "suit",
        "armor_dr": 0.15,
        "weight": 2.5,
        "concealment": 0.70,
        "style": "formal",
        "jacket": True,
        "pants": True,
        "vest": False,
        "armor_plates": False,
    },
    "BlackTacticalSuit": {
        "category": "Suits",
        "type": "suit",
        "armor_dr": 0.25,
        "weight": 3.2,
        "concealment": 0.80,
        "style": "tactical",
        "jacket": True,
        "pants": True,
        "vest": True,
        "armor_plates": True,
    },
    "FormalThreePieceSuit": {
        "category": "Suits",
        "type": "suit",
        "armor_dr": 0.35,
        "weight": 4.0,
        "concealment": 0.60,
        "style": "formal",
        "jacket": True,
        "pants": True,
        "vest": True,
        "armor_plates": True,
    },
    "ArmoredCombatSuit": {
        "category": "Suits",
        "type": "suit",
        "armor_dr": 0.30,
        "weight": 3.5,
        "concealment": 0.75,
        "style": "tactical",
        "jacket": True,
        "pants": True,
        "vest": True,
        "armor_plates": True,
        "helmet_compatible": True,
    },
    "EliteHighProtectionSuit": {
        "category": "Suits",
        "type": "suit",
        "armor_dr": 0.45,
        "weight": 4.5,
        "concealment": 0.50,
        "style": "heavy",
        "jacket": True,
        "pants": True,
        "vest": True,
        "armor_plates": True,
        "full_coverage": True,
    },
}

# ============================================================================
# UTILITY FUNCTIONS
# ============================================================================

def clear_scene():
    """Remove all objects from scene."""
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete()
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

    for node in nodes:
        nodes.remove(node)

    output = nodes.new('ShaderNodeOutputMaterial')
    output.location = (400, 0)

    principled = nodes.new('ShaderNodeBsdfPrincipled')
    principled.location = (0, 0)
    principled.inputs['Base Color'].default_value = (*color, 1.0)
    principled.inputs['Metallic'].default_value = metallic
    principled.inputs['Roughness'].default_value = roughness

    if 'Specular' in principled.inputs:
        principled.inputs['Specular'].default_value = specular
    elif 'Specular IOR Level' in principled.inputs:
        principled.inputs['Specular IOR Level'].default_value = specular

    if emission:
        principled.inputs['Emission Color'].default_value = (*emission, 1.0)
        principled.inputs['Emission Strength'].default_value = emission_strength

    links.new(principled.outputs['BSDF'], output.inputs['Surface'])
    return mat

def create_box(name, size, location=(0,0,0), rotation=(0,0,0)):
    """Create a box mesh."""
    bpy.ops.mesh.primitive_cube_add(size=1, location=location, rotation=rotation)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = size
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return obj

def create_cylinder(name, radius, depth, location=(0,0,0), rotation=(0,0,0), vertices=16):
    """Create a cylinder mesh."""
    bpy.ops.mesh.primitive_cylinder_add(radius=radius, depth=depth, vertices=vertices, location=location, rotation=rotation)
    obj = bpy.context.active_object
    obj.name = name
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return obj

def create_cone(name, radius1, radius2, depth, location=(0,0,0), rotation=(0,0,0), vertices=16):
    """Create a cone mesh."""
    bpy.ops.mesh.primitive_cone_add(radius1=radius1, radius2=radius2, depth=depth, vertices=vertices, location=location, rotation=rotation)
    obj = bpy.context.active_object
    obj.name = name
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return obj

def create_torus(name, major_radius, minor_radius, location=(0,0,0), rotation=(0,0,0)):
    """Create a torus mesh."""
    bpy.ops.mesh.primitive_torus_add(major_radius=major_radius, minor_radius=minor_radius, location=location, rotation=rotation)
    obj = bpy.context.active_object
    obj.name = name
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
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

def export_obj(obj, filepath):
    """Export object as OBJ."""
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

# ============================================================================
# MATERIAL PRESETS
# ============================================================================

def get_material_preset(mat_type, name_prefix):
    """Get material for equipment type."""
    presets = {
        'leather': create_material(f"{name_prefix}_Leather", (0.12, 0.08, 0.05), metallic=0.0, roughness=0.85),
        'kydex': create_material(f"{name_prefix}_Kydex", (0.10, 0.10, 0.12), metallic=0.1, roughness=0.3),
        'polymer': create_material(f"{name_prefix}_Polymer", (0.08, 0.08, 0.09), metallic=0.0, roughness=0.7),
        'nylon': create_material(f"{name_prefix}_Nylon", (0.15, 0.15, 0.16), metallic=0.0, roughness=0.8),
        'metal': create_material(f"{name_prefix}_Metal", (0.4, 0.4, 0.42), metallic=1.0, roughness=0.2),
        'fabric': create_material(f"{name_prefix}_Fabric", (0.08, 0.08, 0.10), metallic=0.0, roughness=0.9),
        'kevlar': create_material(f"{name_prefix}_Kevlar", (0.15, 0.15, 0.16), metallic=0.2, roughness=0.6),
        'plate': create_material(f"{name_prefix}_Plate", (0.3, 0.3, 0.32), metallic=0.8, roughness=0.15),
        'gold': create_material(f"{name_prefix}_Gold", (0.85, 0.65, 0.12), metallic=1.0, roughness=0.1),
    }
    return presets.get(mat_type, presets['polymer'])

# ============================================================================
# EQUIPMENT GENERATORS
# ============================================================================

def generate_holster(name, spec):
    """Generate a holster model."""
    print(f"Generating holster: {name}")
    parts = []
    mats = {}

    mat_type = spec.get('material', 'polymer')
    mats['body'] = get_material_preset(mat_type, name)
    mats['strap'] = get_material_preset('nylon', name)
    mats['hardware'] = get_material_preset('metal', name)
    mats['retention'] = get_material_preset('kydex', name)

    w, h, d = spec['width'], spec['height'], spec['depth']
    slots = spec.get('pistol_slots', 1)

    # Main holster body
    body = create_box(f"{name}_Body", (w, h, d), (0, h/2, d/2))
    body.data.materials.append(mats['body'])
    parts.append(body)

    # Pistol cutout(s)
    for i in range(slots):
        y_offset = -h/2 + h/(slots+1) * (i+1)
        cut = create_box(f"{name}_Cutout_{i}", (w*0.7, h*0.8/slots, d*1.5), (0, y_offset, d/2))
        boolean_difference(body, cut)

    # Retention strap
    if spec.get('race_cut'):
        # Open top for race holster
        pass
    else:
        strap = create_box(f"{name}_RetentionStrap", (w*0.8, 0.01, d*0.5), (0, h*0.3, d*0.75))
        strap.data.materials.append(mats['retention'])
        parts.append(strap)

        # Snap button
        snap = create_cylinder(f"{name}_Snap", 0.008, 0.004, (0, h*0.3, d*0.75), (math.radians(90), 0, 0), vertices=8)
        snap.data.materials.append(mats['hardware'])
        parts.append(snap)

    # Belt loops / attachment
    if spec.get('strap'):
        # Thigh strap
        strap = create_box(f"{name}_ThighStrap", (0.04, h*1.2, 0.005), (-w/2 - 0.02, 0, 0))
        strap.data.materials.append(mats['strap'])
        parts.append(strap)
    else:
        # Belt loops
        for i in range(2):
            loop = create_torus(f"{name}_BeltLoop_{i}", 0.025, 0.006, (-w/2 + i*w, h*0.05, d/2), (math.radians(90), 0, 0))
            loop.data.materials.append(mats['strap'])
            parts.append(loop)

    # Cant adjustment hardware
    if not spec.get('strap'):
        for i in range(2):
            screw = create_cylinder(f"{name}_CantScrew_{i}", 0.004, 0.01, (-w/2 + i*w, h*0.2, d*0.5), (0, 0, 0), vertices=6)
            screw.data.materials.append(mats['hardware'])
            parts.append(screw)

    # Join parts
    main_obj = parts[0]
    for part in parts[1:]:
        try:
            boolean_union(main_obj, part)
        except:
            pass

    main_obj.name = name
    return main_obj

def generate_pouch(name, spec):
    """Generate a magazine/ammo pouch model."""
    print(f"Generating pouch: {name}")
    parts = []
    mats = {}

    mat_type = spec.get('material', 'nylon')
    mats['body'] = get_material_preset(mat_type, name)
    mats['flap'] = get_material_preset(mat_type, name)
    mats['strap'] = get_material_preset('nylon', name)
    mats['hardware'] = get_material_preset('metal', name)
    mats['elastic'] = create_material(f"{name}_Elastic", (0.05, 0.05, 0.05), metallic=0.0, roughness=0.9)

    w, h, d = spec['width'], spec['height'], spec['depth']

    # Main pouch body
    body = create_box(f"{name}_Body", (w, h, d), (0, h/2, d/2))
    body.data.materials.append(mats['body'])
    parts.append(body)

    # Mag cutouts
    mag_slots = spec.get('mag_slots', 0)
    shell_slots = spec.get('shell_slots', 0)
    knife_slots = spec.get('knife_slots', 0)
    pencil_slots = spec.get('pencil_slots', 0)
    rows = spec.get('rows', 1)
    cols = spec.get('cols', mag_slots)

    if mag_slots > 0:
        for r in range(rows):
            for c in range(cols):
                if r * cols + c >= mag_slots:
                    continue
                x = -w/2 + w/(cols+1) * (c+1)
                y = -h/2 + h/(rows+1) * (r+1)
                # Magazine well cutout
                cut = create_box(f"{name}_MagCut_{r}_{c}", (w/cols*0.7, h/rows*0.8, d*1.2), (x, y, d/2))
                boolean_difference(body, cut)

                # Elastic retention
                elastic = create_box(f"{name}_Elastic_{r}_{c}", (w/cols*0.6, 0.005, d*0.6), (x, y + h/rows*0.35, d*0.7))
                elastic.data.materials.append(mats['elastic'])
                parts.append(elastic)

    if shell_slots > 0:
        # Shell loops
        for i in range(shell_slots):
            x = -w/2 + w/(shell_slots+1) * (i+1)
            loop = create_torus(f"{name}_ShellLoop_{i}", 0.012, 0.004, (x, h/2, d/2), (math.radians(90), 0, 0))
            loop.data.materials.append(mats['elastic'])
            parts.append(loop)

    if knife_slots > 0:
        # Knife slots (vertical)
        for i in range(knife_slots):
            x = -w/2 + w/(knife_slots+1) * (i+1)
            slot = create_box(f"{name}_KnifeSlot_{i}", (0.02, h*0.9, 0.005), (x, 0, d/2))
            slot.data.materials.append(mats['elastic'])
            parts.append(slot)

    if pencil_slots > 0:
        # Pencil slots
        for i in range(pencil_slots):
            x = -w/2 + w/(pencil_slots+1) * (i+1)
            slot = create_cylinder(f"{name}_PencilSlot_{i}", 0.006, h*0.9, (x, 0, d/2), (math.radians(90), 0, 0), vertices=8)
            slot.data.materials.append(mats['elastic'])
            parts.append(slot)

    # Flap cover (if not open top)
    if mag_slots > 0 and not shell_slots > 0:
        flap = create_box(f"{name}_Flap", (w*1.05, h*0.3, 0.005), (0, h*0.65, d*0.5))
        flap.data.materials.append(mats['flap'])
        parts.append(flap)

        # Velcro / snap
        snap = create_cylinder(f"{name}_FlapSnap", 0.01, 0.004, (0, h*0.65, d*0.5), (math.radians(90), 0, 0), vertices=8)
        snap.data.materials.append(mats['hardware'])
        parts.append(snap)

    # MOLLE/PALS webbing on back
    for i in range(3):
        y = -h/2 + h/4 * (i+1)
        web = create_box(f"{name}_MOLLE_{i}", (w*0.8, 0.008, 0.003), (0, y, -d/2 - 0.001))
        web.data.materials.append(mats['strap'])
        parts.append(web)

    # Join parts
    main_obj = parts[0]
    for part in parts[1:]:
        try:
            boolean_union(main_obj, part)
        except:
            pass

    main_obj.name = name
    return main_obj

def generate_suit(name, spec):
    """Generate a suit model (simplified - placeholder for character rig)."""
    print(f"Generating suit: {name}")
    parts = []
    mats = {}

    style = spec.get('style', 'tactical')
    has_armor = spec.get('armor_plates', False)
    has_vest = spec.get('vest', False)
    has_helmet = spec.get('helmet_compatible', False)
    full_cov = spec.get('full_coverage', False)

    # Materials based on style
    if style == 'formal':
        mats['fabric'] = create_material(f"{name}_Wool", (0.05, 0.05, 0.06), metallic=0.0, roughness=0.85)
        mats['lining'] = create_material(f"{name}_Lining", (0.12, 0.10, 0.08), metallic=0.0, roughness=0.7)
        mats['buttons'] = create_material(f"{name}_Buttons", (0.1, 0.1, 0.1), metallic=0.5, roughness=0.3)
    elif style == 'tactical':
        mats['fabric'] = create_material(f"{name}_Nylon", (0.10, 0.10, 0.12), metallic=0.0, roughness=0.8)
        mats['lining'] = create_material(f"{name}_Mesh", (0.15, 0.15, 0.16), metallic=0.0, roughness=0.7)
        mats['buttons'] = create_material(f"{name}_Velcro", (0.08, 0.08, 0.09), metallic=0.0, roughness=0.9)
    else:  # heavy
        mats['fabric'] = create_material(f"{name}_Cordura", (0.08, 0.08, 0.10), metallic=0.0, roughness=0.7)
        mats['lining'] = create_material(f"{name}_Kevlar", (0.15, 0.15, 0.16), metallic=0.2, roughness=0.6)
        mats['buttons'] = create_material(f"{name}_Buckles", (0.3, 0.3, 0.32), metallic=1.0, roughness=0.2)

    if has_armor:
        mats['plate'] = get_material_preset('plate', name)

    # Torso/Jacket
    jacket_w, jacket_h, jacket_d = 0.45, 0.60, 0.03
    jacket = create_box(f"{name}_Jacket", (jacket_w, jacket_h, jacket_d), (0, jacket_h/2 - 0.1, jacket_d/2))
    jacket.data.materials.append(mats['fabric'])
    parts.append(jacket)

    # Armor plates
    if has_armor:
        # Front plate
        fplate = create_box(f"{name}_FrontPlate", (jacket_w*0.7, jacket_h*0.6, 0.01), (0, jacket_h*0.1, jacket_d + 0.005))
        fplate.data.materials.append(mats['plate'])
        parts.append(fplate)

        # Back plate
        bplate = create_box(f"{name}_BackPlate", (jacket_w*0.7, jacket_h*0.6, 0.01), (0, jacket_h*0.1, -0.005))
        bplate.data.materials.append(mats['plate'])
        parts.append(bplate)

        # Side plates
        for side in [-1, 1]:
            splate = create_box(f"{name}_SidePlate_{side}", (0.02, jacket_h*0.5, 0.01), (side*jacket_w*0.36, jacket_h*0.1, 0))
            splate.data.materials.append(mats['plate'])
            parts.append(splate)

    # Vest (over jacket or standalone)
    if has_vest:
        vest = create_box(f"{name}_Vest", (jacket_w*0.95, jacket_h*0.7, 0.02), (0, jacket_h*0.05, 0.02))
        vest.data.materials.append(mats['fabric'])
        parts.append(vest)

        # MOLLE on vest
        for i in range(4):
            y = -jacket_h*0.2 + jacket_h*0.5/4 * (i+1)
            web = create_box(f"{name}_VestMOLLE_{i}", (jacket_w*0.8, 0.008, 0.003), (0, y, 0.03))
            web.data.materials.append(mats['lining'])
            parts.append(web)

    # Pants
    pants_w, pants_h, pants_d = 0.35, 0.85, 0.025
    pants = create_box(f"{name}_Pants", (pants_w, pants_h, pants_d), (0, -pants_h/2 - 0.1, pants_d/2))
    pants.data.materials.append(mats['fabric'])
    parts.append(pants)

    # Knee pads (tactical/heavy)
    if style in ['tactical', 'heavy']:
        for side in [-1, 1]:
            knee = create_box(f"{name}_KneePad_{side}", (pants_w*0.6, 0.12, 0.015), (0, -pants_h*0.25, pants_d + 0.01))
            knee.data.materials.append(mats['plate'] if has_armor else mats['fabric'])
            parts.append(knee)

    # Helmet attachment points
    if has_helmet:
        for side in [-1, 1]:
            mount = create_box(f"{name}_HelmetMount_{side}", (0.03, 0.03, 0.01), (side*jacket_w*0.4, jacket_h*0.4, 0.02))
            mount.data.materials.append(mats['hardware'] if 'hardware' in mats else mats['buttons'])
            parts.append(mount)

    # Full coverage additions (neck, groin, etc.)
    if full_cov:
        # Neck guard
        neck = create_box(f"{name}_NeckGuard", (jacket_w*0.6, 0.08, 0.015), (0, jacket_h*0.45, 0.02))
        neck.data.materials.append(mats['plate'])
        parts.append(neck)

        # Groin guard
        groin = create_box(f"{name}_GroinGuard", (pants_w*0.5, 0.15, 0.015), (0, -pants_h*0.4, 0.01))
        groin.data.materials.append(mats['plate'])
        parts.append(groin)

    # Collar
    collar = create_box(f"{name}_Collar", (jacket_w*1.05, 0.08, 0.01), (0, jacket_h*0.4, 0.015))
    collar.data.materials.append(mats['fabric'])
    parts.append(collar)

    # Epaulets (tactical)
    if style == 'tactical':
        for side in [-1, 1]:
            ep = create_box(f"{name}_Epaulet_{side}", (0.08, 0.15, 0.005), (side*jacket_w*0.45, jacket_h*0.3, 0.015))
            ep.data.materials.append(mats['fabric'])
            parts.append(ep)

    # Join parts
    main_obj = parts[0]
    for part in parts[1:]:
        try:
            boolean_union(main_obj, part)
        except:
            pass

    main_obj.name = name
    return main_obj

# ============================================================================
# MAIN
# ============================================================================

def main():
    print("=" * 60)
    print("JOHN WICK ARSENAL - Procedural Equipment Generator")
    print("=" * 60)

    # Ensure output directories exist
    for category in ['Equipment', 'Suits']:
        os.makedirs(os.path.join(OUTPUT_DIR, category), exist_ok=True)

    generated = []

    # Generate equipment
    for name, spec in EQUIPMENT_SPECS.items():
        try:
            clear_scene()

            if spec['type'] == 'holster':
                obj = generate_holster(name, spec)
            elif spec['type'] == 'pouch':
                obj = generate_pouch(name, spec)
            else:
                continue

            out_path = os.path.join(OUTPUT_DIR, spec['category'], f"{name}.obj")
            export_obj(obj, out_path)
            generated.append((name, spec['category'], out_path))

        except Exception as e:
            print(f"ERROR generating {name}: {e}")
            import traceback
            traceback.print_exc()

    # Generate suits
    for name, spec in SUIT_SPECS.items():
        try:
            clear_scene()

            obj = generate_suit(name, spec)

            out_path = os.path.join(OUTPUT_DIR, spec['category'], f"{name}.obj")
            export_obj(obj, out_path)
            generated.append((name, spec['category'], out_path))

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