About 

 Amplify Animation Pack (c) Amplify Creations, Lda. All rights reserved.

 Amplify Animation Pack is set of animations designed for Third-Person games, with
 a Third-Person Character Controller(work in progress) and Blender Source.
	 
 Redistribution of Amplify Animation Pack is frowned upon. If you want to share the 
 software, please refer others to the official product page:

 https://u3d.as/2HjE?aid=1011lPwI

Description

 Amplify Animation Pack is a full set of animations ranging from locomotion to boating,
 swimming, and varied interactions.  Whether you’re making a sandbox game, tactical espionage
 sim, or exploring uncharted ruins, the starter Third-person Character Controller will get you
 rolling faster than ever!
	
Features

    Blender Source
    600+ Animation Sequences
    Third-person Controller

Supported Platforms

  * All platforms 

Minimum Requirements

  Software

    Unity 2019.4.x
	
  Dependencies
  
    Cinemachine
    TextMesh
    Post Processing Stack (optional)


Using the included Animations

  1) Make sure your character is set to Humanoid. (Alternatively, use the rig in our
     Blender Source Files)
  2) Make any adjustments required as per-Unity documentation: 
      https://learn.unity.com/project/re-targeting-and-re-using-animation
      https://docs.unity3d.com/Manual/AnimationsImport.html
      https://blog.unity.com/technology/mecanim-humanoids
  3) Add our Animation Sequence to your Animator Controller.

Using the Third-Person Controller
  
  1) Change the FixedTimeStep value on the Time section of the Project Settings to 0.0166667. 
     At this point you can either open the pre-configured Demo Scene located under 
     "AmplifyAnimationPack\Scenes" or proceed to Step 2. 
  2) Import the Character prefab into your desired scene.
  3) Import the CharacterUIManager prefab into your scene.
  4) Import the CharacterCameraManager prefab into your scene.
  5) In the Character prefab you've imported, set the "FollowCam", "LockCam" and "UIManger"
     variables to the FollowCamManager, LockCamManager (both childs of the CharacterCameraManager) 
     and CharacterUIManager prefabs respectively.
  6) In the CharacterCameraManager prefab you've imported, on the FollowCamManager set the "Follow" 
     and "LookAt" variables to the "Root" and "HeadMarker" childs of the Character prefab you've imported.
  7) In the CharacterCameraManager prefab you've imported, on the LockCamManager set the "Follow" to
     the "Root" child of the Character prefab you've imported.
 
 Accessing Blender Source files

  1) Unpack the BlenderSource files in the BlenderSource Unity Package located under
     "AmplifyAnimationPack\BlenderSource".
  2) Move files to an external folder. (Optional, avoids a Unity re-import when making changes 
     to the Blender file)
  3) Open file in Blender, edit as required and export. Check the Official Wiki below for details.
  
Documentation

  Please refer to the following website for an up-to-date online manual:

    http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Animation_Pack/Manual
	
Feedback

  To file error reports, questions or suggestions, you may use 
  our feedback form online:
	
    http://amplify.pt/contact

  Or contact us directly:

    For general inquiries - info@amplify.pt
    For technical support - support@amplify.pt (customers only)