JelloPhysics

a soft body library for XNA

Copyright (c) 2007 Walaber



RELEASE - version 0.5

NOTES:

- several interface changes, including addition of "Tags" to tag
  user objects to a body, to be referenced in collision callbacks, etc.

- changed velocity damping from a global value to a per-body value.
  this has changed the physics behavior slightly, some modification
  may be needed to maintain the same behavior of previous versions.

-------------------------------------------------------------------

RELEASE - version 0.4

NOTES:

- updated for XNA GS 2.0.


-------------------------------------------------------------------

RELEASE - version 0.3

NOTES:

- more optimizations, including a spacial partition that has improved
  performance on the 360 quite a bit.
- this is the version that was used on version 1.0 of JelloCar.


-------------------------------------------------------------------

RELEASE - version 0.2

NOTES:

- Addition of collision callback (delegate) system.
- Several optimizations.
- general code improvements
- addition of IsStatic and IsKinematic properties to Bodies, 
  to improve system performance.
- addition of material system, allowing custom friction and 
  elasticity between different materials.
  also allows ignoring collision between certain material types, 
  also greatly helping performance.


-------------------------------------------------------------------

Release - version 0.15

NOTES:

Optimization on many areas to improve performance on the Xbox360.

-------------------------------------------------------------------

Initial release - version 0.1


NOTES:

Solution includes projects for both Windows and XBOX 360.
Both included demos require an XBOX 360 controller.


TODO:
1. implement collision callback system (not yet implemented)
2. needs heavy optimization on X360, very slow at this point.



this library is released under the MIT license, which can be viewed 
in the LICENSE.txt file.


http://walaber.com