namespace BGLib.Inspections.Editor.Common {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using Core;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEngine;
    using UnityExtension.Editor;

    public abstract class AddressablesInspection : SimpleInspection {

        private const string kAssetGroupsFolder = "AssetGroups";
        private const string kSchemasFolder = "Schemas";
        private const string kPackedAssetGroupTemplate = "PackedAssetGroupTemplate";

        private const string kAddressableAssetSettingsNotFound = "AddressableAssetSettings object is not defined";

        protected enum AddressFormat {
            Ignore,
            NameOnly,
            FullPath
        }

        protected override InspectionResult InspectAndFix(bool inspectOnly) =>
            InspectAndFixComposite(inspectOnly).FirstFailed();

        protected abstract IEnumerable<InspectionResult> InspectAndFixComposite(bool inspectOnly);

        protected abstract string logPrefix { get; }

        protected InspectionResult InspectAndFix_AssetSettings(
            bool inspectOnly,
            out AddressableAssetSettings settings
        ) {

            settings = FindUnityObjectsHelper.FindAllObjectsByType<AddressableAssetSettings>()
                .FirstOrDefault(settings => !settings.BuildRemoteCatalog) ?? AddressableAssetSettingsDefaultObject.Settings;

            return settings == null
                ? InspectionResult.NonFixable(kAddressableAssetSettingsNotFound)
                : InspectionResult.Ok;
        }

        protected InspectionResult InspectAndFix_AssetGroup(
            bool inspectOnly,
            AddressableAssetSettings settings,
            string groupName,
            out AddressableAssetGroup group
        ) {

            group = settings.FindGroup(groupName);
            if (group != null) {
                return InspectionResult.Ok;
            }

            if (inspectOnly) {
                return InspectionResult.Fixable($"Group '{groupName}' not found");
            }

            var assetGroupTemplate =
                FindUnityObjectsHelper.FindObjectWithUniqueName<AddressableAssetGroupTemplate>(
                    kPackedAssetGroupTemplate
                );
            group = settings.CreateGroup(
                groupName,
                setAsDefaultGroup: false,
                readOnly: false,
                postEvent: true,
                schemasToCopy: assetGroupTemplate.SchemaObjects
            );

            AssetDatabaseExtensions.SaveAsset(settings);

            Log($"Created addressables group '{groupName}'");
            return InspectionResult.Ok;
        }

        protected InspectionResult InspectAndFix_Asset(
            bool inspectOnly,
            AddressableAssetSettings settings,
            AddressableAssetGroup group,
            string assetPath,
            string? label = null,
            AddressFormat addressFormat = AddressFormat.NameOnly
        ) {

            var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            var assetName = Path.GetFileNameWithoutExtension(assetPath);

            var assetEntry = settings.FindAssetEntry(assetGuid);
            if (assetEntry == null || assetEntry.parentGroup != group) {
                if (inspectOnly) {
                    return assetEntry == null
                        ? InspectionResult.Fixable($"'{assetName}' is not an addressable")
                        : InspectionResult.Fixable($"'{assetName}' has wrong group {assetEntry.parentGroup.name}");
                }

                assetEntry = settings.CreateOrMoveEntry(assetGuid, group, readOnly: false, postEvent: true);

                AssetDatabaseExtensions.SaveAsset(group);
                Log($"Addressable entry '{assetName}' assigned to the '{group.name}' group");
            }

            if (addressFormat != AddressFormat.Ignore) {
                var expectedAddress = addressFormat switch {
                    AddressFormat.NameOnly => assetName,
                    AddressFormat.FullPath => assetPath,
                    _ => throw new ArgumentOutOfRangeException(nameof(addressFormat), addressFormat, null)
                };
                if (expectedAddress != assetEntry.address) {
                    if (inspectOnly) {
                        return InspectionResult.Fixable($"Address '{assetEntry.address}' doesn't match '{expectedAddress}'");
                    }
                    assetEntry.SetAddress(expectedAddress);

                    AssetDatabaseExtensions.SaveAsset(group);
                    Log($"Addressable entry renamed to '{expectedAddress}'");
                }
            }

            if (string.IsNullOrEmpty(label)) {
                return InspectionResult.Ok;
            }

            if (!assetEntry.labels.Contains(label)) {
                if (inspectOnly) {
                    return InspectionResult.Fixable($"Label {label} not set");
                }

                assetEntry.SetLabel(label, enable: true, force: true);

                AssetDatabaseExtensions.SaveAsset(group);
                Log($"Assign label {label} to addressable '{assetName}'");
            }

            return InspectionResult.Ok;
        }

        protected InspectionResult InspectAndFix_AssetFolders(
            bool inspectOnly,
            AddressableAssetGroup group,
            string packageRootFolder
        ) {

            var groupRoot = $"{packageRootFolder}/{kAssetGroupsFolder}";
            if (!AssetDatabase.IsValidFolder(groupRoot)) {
                if (inspectOnly) {
                    return InspectionResult.Fixable("AssetGroups folder does not exit");
                }

                AssetDatabase.CreateFolder(packageRootFolder, kAssetGroupsFolder);
                Log($"Created {groupRoot} folder");
            }

            var schemasRoot = $"{groupRoot}/{kSchemasFolder}";
            if (!AssetDatabase.IsValidFolder(schemasRoot)) {
                if (inspectOnly) {
                    return InspectionResult.Fixable("Schemas folder does not exit");
                }

                AssetDatabase.CreateFolder(groupRoot, kSchemasFolder);
                Log($"Created {schemasRoot} folder");
            }

            var groupPath = $"{groupRoot}/{group.name}.asset";
            var oldGroupPath = AssetDatabase.GetAssetPath(group);
            if (groupPath != oldGroupPath) {
                if (inspectOnly) {
                    return InspectionResult.Fixable("Group not in package");
                }

                AssetDatabase.MoveAsset(oldGroupPath, groupPath);
                Log($"Group {oldGroupPath} moved to {groupPath}");
            }

            foreach (var schema in group.Schemas) {
                var schemaPath = $"{schemasRoot}/{schema.name}.asset";
                var oldSchemaPath = AssetDatabase.GetAssetPath(schema);
                if (schemaPath != oldSchemaPath) {
                    if (inspectOnly) {
                        return InspectionResult.Fixable("Schemas not in package");
                    }

                    AssetDatabase.MoveAsset(oldSchemaPath, schemaPath);
                    Log($"Schema {oldSchemaPath} moved to {schemaPath}");
                }
            }

            return InspectionResult.Ok;
        }

        private void Log(string message) {

            Debug.Log($"[{logPrefix}] {message}");
        }
    }
}
