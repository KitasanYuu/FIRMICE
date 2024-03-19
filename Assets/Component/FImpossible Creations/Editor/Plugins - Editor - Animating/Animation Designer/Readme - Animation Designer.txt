__________________________________________________________________________________________

Package "Animation Designer"
Version 1.2.4.2

Made by FImpossible Creations - Filip Moeglich
https://www.FilipMoeglich.pl
FImpossibleCreations@Gmail.com or FImpossibleGames@Gmail.com or Filip.Moeglich@Gmail.com

__________________________________________________________________________________________

Youtube: https://www.youtube.com/channel/UCDvDWSr6MAu1Qy9vX4w8jkw
Facebook: https://www.facebook.com/FImpossibleCreations
Twitter (@FimpossibleC): https://twitter.com/FImpossibleC

___________________________________________________


Package Contests:

User Manual: Check the .pdf file

DEMO - Animation Designer.unitypackage
Package contains scene with few example designed animations with Animation Designer and comparisons.

Animation Designer - Assembly Definitions.unitypackage (Supported since Unity 2017)
Assembly Definition files to speed up compilation time of your project (Fimpossible Directories will have no influence on compilation time of whole project)


__________________________________________________________________________________________
Description:

Innovative Animation Tweaking Tool For Unity Engine 

Upgrade Animation Clips with additional procedural motion and bake it!
Tweak Animation Clip pose and save it in new AnimationClip file.
Set additional motion to some bones and save new AnimationClip for use as animation variant.

NO TOYING WITH KEYFRAMES OR TIMELINE!
Meet new workflow for tweaking animation!
All you need to do is using few sliders and curve window.

You don't need to be an Animation Artist to work with this tool.
If you're an Animation Artist, this tool will give you new possibilities!

Keep original animation intact!
Save modified animation as separated AnimationClip file.
Store designer animation settings for multiple animation clips, in single preset file, so you can go back for tweaking animations design, in any time!

Prepare base animation without full polishing, 
let do it the procedural motion algorithm. Add some flappyness to small limbs and bake it.
Adjust Unity Humanoid animation retargeting errors inside editor.


MAIN FEATURES:
• Editor Animation Tweaking Tool
• Use already animated clips and save modified versions in new AnimationClip files
• Store Animation Modifiers Settings inside Presets
• Intuitive GUI
• Automatic Limbs Setup for Humanoid Rigs
• Supporting Generic Rigs
• Single Bone Modificators
• Limbs Modificators - Procedural Elasticness
• Limbs IK - For Hands, Bone Chains and Legs (Humanoid and Generic Rigs Supported)
• Legs IK Analyzer for adjusting Legs Motion Animation
• Supporting Root Motion Animations

__________________________________________________________________________________________

Version 1.2.4.2
- Morphs now allows to blend root motion of the morphed clip
- Root motion loop fix (last frame being 0 frame)
- Clip duration multiplier limits now can be set bigger/smaller than before

Version 1.2.4.1
- Added possibility to remove extra version of animation clip setup (green ++ button)

Version 1.2.4
- Save+ Button which will try to remember last saved directory for next save operations
- Rename (no prefix) option for clip rename (under green ++ button)
- Look At Modifier - override rotation mode
- New Module: Sudden Rotation Steps - initially made for insect like fast head rotations, but can find other usages.
- New Module: Vibrate - custom pattern rotation over the clip - set rate high for vibrating bones, for example monster jaw during roaring
- New Module: Add Event - Simply to add animation events with some name in the clip
- Arms Poser Module rotation ranges can be bigger/smaller (exceeding 1), select ADM-Humanoid_Arm Poser file and you will find 'Sliders Ranges' parameter in the inspector view

Version 1.2.3
- Project animation clips gather button now will categorize collected animation clips into better categories
- Added fields in the limb menu to add child bones of the bone chain usin single drag & drop
- Pelvis Elasticity Blend can be controlled with the curve now
- Updated IK scripts to by in sync with Legs Animator plugin
- Added animation clip verification feature for baking-process armature
may be done automatically in the future update, right now it's under setup top foldout "Verify Armature Using Animation Clip Of The Model".

- Animation Clip Internal Crop fixes
- Animation Clip Internal Crop preview support 
uncorrectly exporting source root motion curves when editing internally cropped animation clips (still center of mass motion can be used)
(need to be handled in a different way for generics, differently for humanoids and differently for humanoid IK, it's not implemented fully yet)

- You can set constant animation clips export directory with the button on the right to export button
- Hit export button with right mouse button to save clip without Save File popup
- Updated and upgraded IK position handles display
- Reworked some parts of root motion export procedure
- Fixed some time cropping related calculations for animation previewing
- If root bone of the character is using keyframe animation, you can enable baking it under additional settings (export tab)
but you also need to remember about including it in the armature report

Version 1.2.2
- New Module: Basic Hips Stabilizer (automatic stable pose keeper when using Leg IKs)
- New Module: Hand Reach Helper (automatic spine rotation on stretched arm IKs)
- Few small fixes

Version 1.2.1
- IK Offset parameter now can be adjusted with the scene gizmos when animation playback is paused
- New Module for setting up Custom Animation Curves for the animation clip
- Few GUI improvements

Version 1.2.0
- New way for handling extra features for Animation Designer, introducing "Custom Modules" 
- New Module for Humanoid Fist Open/Close posing
- New Module for Humanoid hand fingers posing
- New Module for Humanoid arms posing
- New Module for Humanoid legs posing
- New Module for Humanoid spine posing
- New Module for Humanoid head and face posing
- New Module for Stylizing Animation with variable Framerate
- New Module for Designing Strafe animation (helps making 8 directional movement)

- Now IK 'Align With' will be remembered after loading setup again
(if it was assigned to some skeleton bone or bone was parent of target transform)
- Time Evaluation Curve ("Time Flow") switch for easier control over animation clip time modify
- Grounder Foot IK Start/End Alignment offset params
- Each morph contains now it's own Time Evaluation curve ("Time Flow")
- Changed "Modificators" display name as "Modifiers" and "Elasticness" as "Elasticity"
- Few small GUI Changes

version 1.1.0
- Copy/Paste button for copying all modificators in animation
- Few GUI reminding warnings
- Experimental Ctrl+Z undo implementation (enable it with the button on the top left part of the Animation Designer window)

Version 1.0.9
- Fixed leg ik grounding mode "Foot offset when grounded, ungrounded, constant offset" parameters
- Possibility to change IK Limbs execution order under IK Setup foldout ("Alternate execution order" button)
- Possibility to animate other character in sync with animation designer animation
Go to Setup Category, Additional Settings foldout, on the bottom select object from scene with animator and select clip to play on it

- New Auto-Hint IK modes for Arm IK and Leg IK, can be switched under "IK Setup" foldout, parameter "Auto Hint Mode"
- Rotation handle on the scene for 'Additive Rotation' modificators
- Fixed Axis Rotation mode (rotation button) for 'Additive Rotation' modificator 
- Not looped animations / toggled to force not looping will reset motion every time when reaching end of the animation clip in the preview 

Version 1.0.8
- Multi-Still-IK points now can overlap each other and will blend correctly
- Multi-Still-IK points now can use curves for fade-in-out animations, including curves to offset/rotate foot and offset pelvis in sync with transition
- Added Animation Clips Pack solution: 'Create -> Fimpossible Creations -> Utilities -> Animation Clips Pack' to create file in which you can keep multiple animation clips exported with animation designer (drag & drop through inspector window)
- Fixed Legacy/Generic Animation Export (when exported once, the humanoid export was working wrong)


version 1.0.7
- World foot ik now will be saved in animator's parent space if there is a parent transform
- Join root motion toggle to join original root motion with custom offsets (additional options in the setup tab)
- Humanoid IK toggle to enable precise mecanim foot ik on the preview and in the bake process (experimental - additional options in the setup tab)
So if your animation clip contains IK data, you can bake it just for your character skeleton target, so later you don't need to use IK Pass and Foot IK on the animation states for better precision on foot (less CPU usage)
- Multi-Still IK positions feature to snap ik to different position/rotation in clip time.
To enable it, hit the transform button on the still ik section (not visible for grounding legs ik)
It can be really helpful for fixing sliding foots issues, cleaning mocap files or for hand-attach climb animation


version 1.0.5.3
- The Morphing masks was not saving, now it's fixed!
- Added option to force exporting looped / not looped clip under additional export settings
- Added IK still position attach to world position toggle (requires character placed in zero position on the scene)

version 1.0.5.2
- Reset root position support for morphing clips
- Naming support for animation clip versions
- Added more options for curve menu

version 1.0.5.1
- Added "Cycle Offset" parameter for morphing
- Added new option for green "++" button which allows to copy all animation designer settings from one animation clip setup to another
- Fixed some 'Reset Root Position' export issue

version 1.0.5
- Implemented Morphing Feature! Available in the last bookmark, next to the IK bookmark.
Now you can blend selected limbs with other animation clips
- Added small button on the right of curve view which will display menu to do some helper operations on the curve

version 1.0.2.1.2
- Fixed baking additional bones positions in humanoid rigs
- (rare case fix) Added possibility to fix humanoid avatar export with choosing root bone for bake to use "Skeleton Root" instead of the animator transform
- Few small fixes for root motion and GUI

version 1.0.2.1.1
- Added experimental "Reverse Clip Time" button on the right of "Clip Duration Multiplier" field
- Added experimental "Clip Time Modify" curve
- Fixed export of not needed parameters for humanoid clips

version 1.0.2.1
- Added possibility to export Generic/Legacy animation clips out of Humanoid Rig
(Under "Additional Export Settings" when you work on humanoid rig)
- Additional options for helping out automatic grounding IK curve generating
- Added possibility to export grounding curve as animator parameter
version 1.0.2.0
-Original Clip Root motion disable options in the Setup tab under "Additional Settings For" foldout
-Experimental feature to enable multiple cycles, of the original animation motion
version 1.0.1.9
-Corrected effect of "Hips and legs precision boost" slider for humanoid rigs which will provide better non-sliding foots for idle animations
version 1.0.1.8
-Added foot glue tool which can be useful for fixing mocap sliding foots issue
-Using alt + left mouse button on some curve fields will result in keyframe added in current playback position
version 1.0.1.7
-Fixed root motion issue with humanoids with big scale offsets
-Grounding Foot IK "swings" and related parameters now should work correctly with the root motion
version 1.0.1.6
-Improved Looping algorithms
-Loops were lacking one last frame in the animation after exporting : fixed
-Fixed cleaning references after closing Designer Window (previously it resulted in bug of moving other object than edited one)
version 1.0.1.5
-Added support for exporting root motion of original clips (needs testing)
-Reworked humanoid root motion export
-Fixed issue with one-framed animation export
-Added "Additional Loop Frames" 3 algorithms for different loop purposes
-Few minor fixes

Version 1.0.1:
-Added support for baking root motion of generic rigs and improved humanoid root motion baking
-Fixed export legs quality boost slider effect
-Added experimental "stretching" slider for movement based elasticness
-After finishing export, character should behave much more calm
-Overwriting already existing animation clip files should work better now
-Modified looping algorithm to work more universally
-Improved "Motion Influence" parameter for elasticness to help out controll root motion
-Fixed "Calibrate" option freezing animated bones
-Few minor fixes

