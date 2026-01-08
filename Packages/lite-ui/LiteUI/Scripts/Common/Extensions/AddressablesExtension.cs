using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
using UnityEngine.AddressableAssets;

namespace LiteUI.Common.Extensions
{
    public static class AddressablesExtension
    {
#if UNITY_EDITOR
        public static string? GetAddressablesAddress(this UnityEngine.Object asset)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long localId);
            if (string.IsNullOrEmpty(guid) || localId == 0) {
                return null;
            }
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetEntry? assetEntry = (AddressableAssetEntry?) settings.FindAssetEntry(guid);
            return assetEntry?.address;
        }

        public static string? GetAddressablesAddress(this AssetReference assetReference)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetEntry? assetEntry = (AddressableAssetEntry?) settings.FindAssetEntry(assetReference.AssetGUID);
            return assetEntry?.address;
        }

        public static string? GetAddressablesPath(this UnityEngine.Object asset)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long localId);
            if (string.IsNullOrEmpty(guid) || localId == 0) {
                return null;
            }
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetEntry? assetEntry = (AddressableAssetEntry?) settings.FindAssetEntry(guid);
            return assetEntry?.AssetPath;
        }

        public static AssetReference? GetAssetReferenceForAssetId(string assetId)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            foreach (AddressableAssetGroup group in settings.groups) {
                foreach (AddressableAssetEntry entry in group.entries) {
                    if (entry.address == assetId) {
                        return new AssetReference(entry.guid);
                    }
                }
            }
            return null;
        }

        public static AssetReferenceSprite? GetSpriteAssetReferenceForAssetId(string assetId)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            foreach (AddressableAssetGroup group in settings.groups) {
                foreach (AddressableAssetEntry entry in group.entries) {
                    if (entry.address == assetId) {
                        AssetReferenceSprite spriteAtlas = new(entry.guid);
                        AssetReferenceSprite sprite = new(entry.guid);
                        sprite.SetEditorSubObject(spriteAtlas.editorAsset);
                        return sprite;
                    }
                }
            }
            
            return null;
        }

        public static void AddToAddressables(this UnityEngine.Object asset, string address)
        {
            if (!string.IsNullOrEmpty(GetAddressablesAddress(asset))) {
                throw new Exception($"Already in addressables");
            }
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.DefaultGroup;
            string? assetPath = AssetDatabase.GetAssetPath(asset);
            string? guid = AssetDatabase.AssetPathToGUID(assetPath);

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = address;
            List<AddressableAssetEntry> entriesAdded = new() { entry };

            group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);
        }
#endif
    }
}
