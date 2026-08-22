namespace IdAnimal.Web.Services;

/// <summary>
/// Utilidades para construir URLs servibles desde la API. El backend devuelve
/// rutas relativas (ej. "/static/images/..."); el navegador las cargaría desde
/// el host del web app, no del API. Esta clase prefija la URL base.
/// </summary>
public static class ApiUrl
{
    public const string BaseUrl = "https://api.idanimal.tech";

    /// <summary>Convierte rutas relativas en absolutas. Devuelve null si la entrada es null/vacía.</summary>
    public static string? Resolve(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }
        // Rutas relativas: garantizar que empiecen con "/"
        return path.StartsWith("/") ? $"{BaseUrl}{path}" : $"{BaseUrl}/{path}";
    }
}
