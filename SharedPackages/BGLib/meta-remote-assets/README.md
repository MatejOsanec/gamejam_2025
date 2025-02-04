# Meta Remote Assets Manager
Core functionality for loading assets remotely using Meta infra-structure

## Getting started

### Setting up
The meta remote assets manager configures the request to the Meta servers and fetches authentication token differently depending on
the platform the client is requesting from.

Given that purpose, to set it up it is required to bind the given auth token provider interface
```csharp
        [Inject] IAuthenticationTokenProvider _authenticationTokenProvider;
```

For the Platform model inheritors, it can be bind it as in the example:
```csharp
#if IS_PLAYSTATION_APPLICATION_SPECIFIC_DIRECTIVE
        Container.Bind<string>().WithId(MetaRemoteAssetsManager.kPlatformInjectId)
            .FromInstance(SupportedPlatforms.Playstation);
        ...
#else
        Container.Bind<string>().WithId(MetaRemoteAssetsManager.kPlatformInjectId)
            .FromInstance(SupportedPlatforms.Android);
```

Whereas authentication provider comes from the application's implementation. Once both are bind the 
MetaRemoteAssetsManager is ready to be injected as well:

```csharp
        Container.Bind<MetaRemoteAssetsManager>()...
```

Also make sure to disable automatic catalog update on startup, instead initialize MetaRemoteAssetsManager and it will update
the catalogs once authenticated to meta servers. This can be done by enabling this box under top level addressables settings
(Window > Asset Management > Addressables > Settings):

![addressables-disable-catalog-update-on-startup.png](addressables-disable-catalog-update-on-startup.png)

### Building
Remote Assets Manager is configured to fetch from Meta servers assets that has load paths starting with `{BGLib.MetaRemoteAssetsManager.RuntimeConfig.MetaServer}`.
Make sure that this is the case and that PlayMode script is set to "use existing build", otherwise it will fetch from local data base. Check Unity docs [here](https://docs.unity3d.com/Packages/com.unity.addressables@1.19/manual/RemoteContentDistribution.html).

### Usage

Initialize MetaRemoteAssetsManager

```csharp
    await _metaRemoteAssetsManager.WaitInitAsync();
```

Use Addressables interface `Addressables.LoadAssetAsync`

## Known issues

This package does not block the developer from using `Addressables.LoadAssetAsync` on any asset, once it's imported. Which means that
this method can be called in a Meta server remote addressable before the catalog is properly updated. Therefore be mindful that if an asset
is loaded before RemoteAssetManager is initialized, it can use an older catalog thus lead to unexpected behavior.
