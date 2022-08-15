# Unity Save Play Mode Changes Tool.

![Save Play Mode Changes](./Documents/imgs/img_sample_save_play_mode_changes.png)

[中文](./README_CN.md)

## Supported Unity Version

Unity 2019.4 and later.

## How to use

Add `SavePlayModeChanges` component to any game object in scene, then drag the component you want to save to `SavePlayModeChanges.ComponentsToSave` list.

Enter play mode and modify any properties on the component you want to save, then exit play mode, all changes of **supported** properties will be saved.

## Limitations

Editor use only.

Support **most** of the fields in custom MonoBehaviours and **a few** properties in Unity's original Behaviours.
