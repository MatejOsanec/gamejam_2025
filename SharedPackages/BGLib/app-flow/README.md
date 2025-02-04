# Async Initialization

Enables loading async resources before the scene initialization

## Getting started

This packages is responsible for the core features to bootstrap the application and manage scene.

As BGLib it must not contain any Beat Saber specific function for loading

Core components:

1) **[AppInit](Runtime/Initialization/AppInit.cs):** This is the base class that you need to inherit from to create a
   new app
2) **[AsyncInitializationManager](Runtime/Initialization/AsyncInitializationManager.cs):** Component that must be added to AppInit Scene context in order to enable asyncronous
   loading of resources before the initialization of the App
3) **[GameScenesManager](Runtime/SceneManagement/GameScenesManager.cs):** Component that manages scene loading and unloading for scene transitions



## Known issues

TODO: Describe here common issues that users might face while using this package

## Roadmap

TODO: Write a roadmap of features for this tool and how can users collaborate 
