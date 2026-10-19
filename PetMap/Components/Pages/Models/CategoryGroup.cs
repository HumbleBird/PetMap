using PetMap.Data.Utiles;

namespace PetMap.Components.Pages.Models;

public sealed record CategoryGroup(
    string Key,
    string Title,
    IReadOnlyList<AnimalCategory> Categories
);