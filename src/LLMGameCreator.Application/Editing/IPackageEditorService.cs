using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Editing;

public interface IPackageEditorService
{
    PackageEditorSnapshot GetSnapshot();
    void UpdateManifest(ManifestEditModel model);
    void AddTilePrototype(TilePrototypeEditModel model);
    void UpdateTilePrototype(TilePrototypeEditModel model);
    void RemoveTilePrototype(string id);
    void AddMap(MapEditModel model);
    void UpdateMap(MapEditModel model);
    void RemoveMap(string id);
    void AddEntityPrototype(EntityPrototypeEditModel model);
    void UpdateEntityPrototype(EntityPrototypeEditModel model);
    void RemoveEntityPrototype(string id);
    Task SaveAsync(CancellationToken cancellationToken);
    ValidationReport Validate();
}
