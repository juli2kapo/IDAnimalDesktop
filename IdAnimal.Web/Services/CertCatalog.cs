namespace IdAnimal.Web.Services;

/// <summary>
/// Catálogo fijo de certificaciones disponibles. Se persisten como CSV de keys
/// en CattleDto.Certifications (ej. "organic,brc,welfare,senasa").
/// </summary>
public static class CertCatalog
{
    public sealed record Cert(string Key, string Label, string ShortLabel, string Color);

    public static readonly IReadOnlyList<Cert> All = new[]
    {
        new Cert("senasa",  "SENASA — Libre de Brucelosis",        "SENASA",   "#1E5C8A"),
        new Cert("organic", "OIA — Orgánico Certificado",          "ORGÁNICO", "#4A6741"),
        new Cert("brc",     "BRC Certification Body",              "BRC",      "#6B2D8A"),
        new Cert("welfare", "IRAM — Bienestar Animal",             "BIENESTAR","#2C1F0A"),
        new Cert("ipcva",   "IPCVA — Carne Argentina",             "IPCVA",    "#A02020"),
        new Cert("kosher",  "Kosher",                              "KOSHER",   "#1A1A1A"),
    };

    public static IEnumerable<Cert> Parse(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) yield break;
        var keys = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var k in keys)
        {
            var match = All.FirstOrDefault(c => c.Key.Equals(k, StringComparison.OrdinalIgnoreCase));
            if (match is not null) yield return match;
        }
    }

    public static string Toggle(string? csv, string key, bool include)
    {
        var current = (csv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(k => !k.Equals(key, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (include) current.Add(key);
        return string.Join(",", current);
    }

    public static bool Contains(string? csv, string key)
        => !string.IsNullOrWhiteSpace(csv)
           && csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                 .Any(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
}
