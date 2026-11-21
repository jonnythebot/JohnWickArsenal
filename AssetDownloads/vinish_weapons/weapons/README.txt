Weapons: free GLB models by vinish.dev
============================================================

Page: https://vinish.dev/free-3d-models/weapons/
Licence: CC BY 4.0 (see LICENSE.txt). Credit lines: CREDITS.txt.

What is inside
--------------
- 10 GLB files (binary glTF 2.0), one model each, plus a preview image of each in previews/.
- Sizes are in metres (width x depth x height below). +Y is up, the front faces +Z, the origin is on the ground.
- The textures (colour and normal maps) are packed inside each GLB.
- Moving parts are separate nodes with their pivots at the joints or hinges. Their names are listed below.

Models
------
pistol.glb  Pistol (Handgun): 4,480 triangles, 262 KB, 0.03 x 0.20 x 0.14 m
  A polymer-frame semi-auto pistol: steel slide with serrations and three-dot sights, an accessory rail and a stippled grip. The Slide and Magazine are their own nodes.
  Nodes: Frame, Slide, Magazine

revolver.glb  Revolver (Handgun): 6,050 triangles, 235 KB, 0.04 x 0.24 x 0.15 m
  A stainless six-shot revolver with a 4 inch barrel, full under-lug, red-insert front sight and walnut grip. The Cylinder (loaded) and Hammer turn on their own pivots.
  Nodes: Frame, Cylinder, Hammer

assault-rifle.glb  Assault Rifle (Rifle): 13,447 triangles, 616 KB, 0.05 x 0.88 x 0.33 m
  A modern carbine: full-length top rail, slotted handguard, muzzle brake, adjustable stock and a red dot sight. The curved Magazine and Charging_Handle are their own nodes.
  Nodes: Rifle, Magazine, Charging_Handle

submachine-gun.glb  Submachine Gun (Rifle): 6,532 triangles, 356 KB, 0.05 x 0.70 x 0.25 m
  A compact SMG with a suppressor, folding stock, vertical fore grip and reflex sight. The Magazine and Charging_Handle are their own nodes.
  Nodes: SMG, Magazine, Charging_Handle

pump-shotgun.glb  Pump Shotgun (Shotgun): 3,726 triangles, 231 KB, 0.05 x 1.03 x 0.15 m
  A pump-action shotgun with a blued receiver, vent rib with a brass bead, tube magazine and walnut stock. The grooved walnut Pump slides on its own node.
  Nodes: Shotgun, Pump

sniper-rifle.glb  Sniper Rifle (Rifle): 8,564 triangles, 483 KB, 0.09 x 1.33 x 0.28 m
  A bolt-action rifle with a tan stock, fluted brake, long scope on rings and a folded bipod. The Bolt, Scope and Bipod are their own nodes.
  Nodes: Rifle, Bolt, Scope, Bipod

rocket-launcher.glb  Rocket Launcher (Launcher): 3,674 triangles, 272 KB, 0.13 x 1.33 x 0.26 m
  A shoulder-fired launcher tube in olive paint with grips, a shoulder pad and an optic, with a rocket in the muzzle as its own Rocket node to fire.
  Nodes: Launcher, Rocket

hand-grenade.glb  Hand Grenade (Explosive): 2,764 triangles, 136 KB, 0.07 x 0.06 x 0.10 m
  A smooth-body hand grenade in olive paint with a steel fuze, a spoon Lever and a pull-ring Pin as their own nodes.
  Nodes: Grenade, Lever, Pin

combat-knife.glb  Combat Knife (Blade): 1,094 triangles, 119 KB, 0.02 x 0.33 x 0.05 m
  A clip-point combat knife: coated blade with a bright ground edge and spine teeth, a steel guard, a ringed rubber grip and a pommel.

sci-fi-blaster.glb  Sci-Fi Blaster (Sci-fi): 4,232 triangles, 183 KB, 0.07 x 0.29 x 0.15 m
  A futuristic energy pistol: white shell, glowing cyan cells and barrel rings, a dark grip with light strips and a Power_Cell node in the grip.
  Nodes: Blaster, Power_Cell


How to use
----------
three.js: GLTFLoader. Unity: the glTFast package (com.unity.cloud.gltfast). Godot 4: drop the .glb in the
FileSystem dock. Blender: File > Import > glTF 2.0, and export to FBX, OBJ or STL from there if you need to.

More free models: https://vinish.dev/free-3d-models/
