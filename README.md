# Passthrough with hands (no room)
Just open HandWithPassthrough.scene and you should be good to go. You will get hand tracking, hand models and passthrough, but nothing else.

Make sure to disable domain reload as it speeds up entering playmode significantly. **Note: This might cause some issues (see [known issues](#known-issues)).**

![Screenshot (4350)](https://ghe.oculus-rep.com/BeatGames/prototypes-phoenix/assets/7555/f3aa966f-bee7-4191-80b8-45ec429edbb6)

For faster iteration (because launching passthrough takes time) you can use this toggle to disable passthrough and develop in VR.

![image1](https://ghe.oculus-rep.com/BeatGames/prototypes-phoenix/assets/7555/b1937dd1-d7c1-422e-a6bc-27f3a0e10200)


# MR with full Room
I suggest to start with MixedReality.scene inside “Meta XR Core SDK” => “Mixed Reality Sample”
In order to use this you will need to setup room on Quest and allow spatial data over Link (see following screenshots)

![image2](https://ghe.oculus-rep.com/BeatGames/prototypes-phoenix/assets/7555/7c41448d-f49e-451d-b1b1-8d97743fd07c)
![image3](https://ghe.oculus-rep.com/BeatGames/prototypes-phoenix/assets/7555/aae634b3-3f5a-4f8b-8b38-1aa47f784490)


# Other
Check sample scenes inside “Meta XR Core SDK”, “Meta XR Interaction ​SDK” or “Meta XR Interaction SDK Essentials” and pick a base for your prototype based on your needs

# Known Issues

## Domain Reload Disabled

The whole "Meta XR Core SDK" is pretty broken with disabled domain reload. I tried to workaround it in many ways but in general if something behaves weirdly or does not work (especially in Meta Sample Scenes) and you have domain reload disabled, try enabling it again or reloading domain manually. Alternativelly try adding `DomainReloadFix.cs` component to your scene if not already present or even add more things into `DomainReloadFix.cs`.

## OVRCameraRig Exception

Reload Domain (change script and recompile for example) or enable domain reload.

Typically happens when switching between scenes with OVRCameraRig. A bug in OVRCameraRig I was unable to workaround.
![473752438_477379658499312_4233200344773755773_n](https://ghe.oculus-rep.com/BeatGames/prototypes-phoenix/assets/7555/3c19593b-1b91-40d1-872f-ab3be01724b4)


## OculusProjectConfig
Sometimes when opening the project following error is displayed, if that happens, your OculusProjectConfig.asset gets reset to default = no hand tracking etc. Enable it again in OculusProjectConfig.asset or simply revert the change in git.

![Screenshot (4352)](https://ghe.oculus-rep.com/BeatGames/prototypes-phoenix/assets/7555/623e3bfa-893d-4a5a-ae6a-3ecdf0f0ab6b)


![Screenshot (4351)](https://ghe.oculus-rep.com/BeatGames/prototypes-phoenix/assets/7555/feea0d21-2839-41aa-bbaa-9f6b07769cdd)

# Enhancements
- Followed a [Detecting Poses](https://developers.meta.com/horizon/documentation/unity/unity-isdk-building-hand-pose-recognizer) guide for a Left Hand THUMBS_UP
