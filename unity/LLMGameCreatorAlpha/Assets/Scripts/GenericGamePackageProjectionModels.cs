using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    [Serializable]
    public sealed class GenericGamePackageProjectionDocument
    {
        public GenericGamePackageManifest manifest = new GenericGamePackageManifest();
        public GenericGamePackageGame game = new GenericGamePackageGame();
    }

    [Serializable]
    public sealed class GenericGamePackageManifest
    {
        public string packageId = string.Empty;
        public string title = string.Empty;
        public string startMapId = string.Empty;
    }

    [Serializable]
    public sealed class GenericGamePackageGame
    {
        public GenericGamePackageTilePrototype[] tilePrototypes = Array.Empty<GenericGamePackageTilePrototype>();
        public GenericGamePackageEntityPrototype[] entityPrototypes = Array.Empty<GenericGamePackageEntityPrototype>();
        public GenericGamePackageMap[] maps = Array.Empty<GenericGamePackageMap>();
        public GenericGamePackageItem[] items = Array.Empty<GenericGamePackageItem>();
    }

    [Serializable]
    public sealed class GenericGamePackageTilePrototype
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public bool walkable = true;
    }

    [Serializable]
    public sealed class GenericGamePackageEntityPrototype
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public string assetId = string.Empty;
        public GenericGamePackageComponent[] components = Array.Empty<GenericGamePackageComponent>();
    }

    [Serializable]
    public sealed class GenericGamePackageComponent
    {
        public string type = string.Empty;
        public GenericGamePackageComponentArgs args = new GenericGamePackageComponentArgs();
    }

    [Serializable]
    public sealed class GenericGamePackageComponentArgs
    {
        public string dialogueId = string.Empty;
        public string text = string.Empty;
        public string interactionId = string.Empty;
        public string blocksMovement = string.Empty;
    }

    [Serializable]
    public sealed class GenericGamePackageMap
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public int width;
        public int height;
        public string defaultTileId = string.Empty;
        public GenericGamePackagePosition startPosition = new GenericGamePackagePosition();
        public GenericGamePackageTile[] tiles = Array.Empty<GenericGamePackageTile>();
        public GenericGamePackageEntity[] entities = Array.Empty<GenericGamePackageEntity>();
    }

    [Serializable]
    public sealed class GenericGamePackagePosition
    {
        public int x;
        public int y;
    }

    [Serializable]
    public sealed class GenericGamePackageTile
    {
        public int x;
        public int y;
        public string tileId = string.Empty;
    }

    [Serializable]
    public sealed class GenericGamePackageEntity
    {
        public string id = string.Empty;
        public string prototypeId = string.Empty;
        public GenericGamePackagePosition position = new GenericGamePackagePosition();
        public GenericGamePackageComponent[] components = Array.Empty<GenericGamePackageComponent>();
    }

    [Serializable]
    public sealed class GenericGamePackageItem
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public string kind = string.Empty;
    }

    public sealed class GenericGamePackageProjectionModel
    {
        public string SamplePackagePath = GenericGamePackageProjectionAdapter.SamplePackageRelativePath;
        public string PackageId = string.Empty;
        public string PackageTitle = string.Empty;
        public string StartMapId = string.Empty;
        public string MapId = string.Empty;
        public string MapName = string.Empty;
        public int MapWidth;
        public int MapHeight;
        public int StartX;
        public int StartY;
        public readonly List<GenericGamePackageProjectionTile> Tiles = new List<GenericGamePackageProjectionTile>();
        public readonly List<GenericGamePackageProjectionEntity> Entities = new List<GenericGamePackageProjectionEntity>();
        public readonly List<GenericGamePackageProjectionItem> Items = new List<GenericGamePackageProjectionItem>();
        public readonly List<string> Diagnostics = new List<string>();
    }

    public sealed class GenericGamePackageProjectionTile
    {
        public int X;
        public int Y;
        public string TileId = string.Empty;
        public string TileName = string.Empty;
        public string TileKind = string.Empty;
        public bool Explicit;
        public bool Walkable;
    }

    public sealed class GenericGamePackageProjectionEntity
    {
        public string EntityId = string.Empty;
        public string PrototypeId = string.Empty;
        public string PrototypeName = string.Empty;
        public int X;
        public int Y;
        public bool Interactable;
        public string InteractionId = string.Empty;
        public string DialogueId = string.Empty;
        public string InteractionText = string.Empty;
    }

    public sealed class GenericGamePackageProjectionItem
    {
        public string ItemId = string.Empty;
        public string Name = string.Empty;
        public string Kind = string.Empty;
    }

    public sealed class GenericGamePackageProjectionSmokeResult
    {
        public bool SectionPresent;
        public bool PackageIdentityPresent;
        public bool MapDimensionsPresent;
        public bool StartPlayerMarkerPresent;
        public bool TileMarkerPresent;
        public bool EntityMarkerPresent;
        public bool InteractionMarkerPresent;
        public bool ItemSummaryEntryPresent;
        public bool DescriptorPresent;
        public bool EventLogPresent;
        public bool ZeroFatalErrors;
        public string PackageId = string.Empty;
        public string PackageTitle = string.Empty;
        public string MapId = string.Empty;
        public int MapWidth;
        public int MapHeight;
        public int EntityCount;
        public int ItemCount;
        public string StatusLine = string.Empty;

        public bool Passed
        {
            get
            {
                return SectionPresent
                       && PackageIdentityPresent
                       && MapDimensionsPresent
                       && StartPlayerMarkerPresent
                       && TileMarkerPresent
                       && EntityMarkerPresent
                       && InteractionMarkerPresent
                       && ItemSummaryEntryPresent
                       && DescriptorPresent
                       && EventLogPresent
                       && ZeroFatalErrors;
            }
        }

        public string ToDiagnosticText()
        {
            return "passed=" + Passed
                   + "\nsectionPresent=" + SectionPresent
                   + "\npackageIdentityPresent=" + PackageIdentityPresent
                   + "\nmapDimensionsPresent=" + MapDimensionsPresent
                   + "\nstartPlayerMarkerPresent=" + StartPlayerMarkerPresent
                   + "\ntileMarkerPresent=" + TileMarkerPresent
                   + "\nentityMarkerPresent=" + EntityMarkerPresent
                   + "\ninteractionMarkerPresent=" + InteractionMarkerPresent
                   + "\nitemSummaryEntryPresent=" + ItemSummaryEntryPresent
                   + "\ndescriptorPresent=" + DescriptorPresent
                   + "\neventLogPresent=" + EventLogPresent
                   + "\nzeroFatalErrors=" + ZeroFatalErrors
                   + "\npackageId=" + PackageId
                   + "\npackageTitle=" + PackageTitle
                   + "\nmapId=" + MapId
                   + "\nmapWidth=" + MapWidth
                   + "\nmapHeight=" + MapHeight
                   + "\nentityCount=" + EntityCount
                   + "\nitemCount=" + ItemCount
                   + "\nstatusLine=" + StatusLine;
        }
    }
}
