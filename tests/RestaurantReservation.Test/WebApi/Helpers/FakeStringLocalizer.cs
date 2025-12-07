using Microsoft.Extensions.Localization;
using RestaurantReservation.WebApi.Localization;

namespace RestaurantReservation.Test.WebApi.Helpers;

// Nos testes, a gente não precisa de recurso de tradução de verdade.
// Criamos um mock que sempre devolve ResourceNotFound = true e, 
// assim, o ResultExtensions cai no fallback usando as mensagens de erro reais.
public class FakeStringLocalizer : IStringLocalizer<SharedResource>
{
    public LocalizedString this[string name] => new(name, name, resourceNotFound: true);

    public LocalizedString this[string name, params object[] arguments] => new(name, name, resourceNotFound: true);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}