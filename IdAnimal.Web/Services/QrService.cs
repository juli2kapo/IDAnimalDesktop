using System.Text;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace IdAnimal.Web.Services;

/// <summary>
/// Genera el QR de la ficha pública de un animal.
///
/// Devuelve SVG (no PNG) a propósito: el QR se imprime y se pega en el corral o
/// en el catálogo, y el vectorial se mantiene nítido a cualquier tamaño. Además
/// evita depender de System.Drawing / ImageSharp para rasterizar.
/// </summary>
public class QrService
{
    /// <summary>Módulos de silencio alrededor del código. El estándar pide 4.</summary>
    private const int QuietZone = 4;

    /// <summary>URL pública de un animal, la que codifica el QR.</summary>
    public static string PublicUrl(string globalId) => $"https://app.idanimal.tech/a/{globalId}";

    /// <summary>
    /// SVG del QR de <paramref name="content"/>. <paramref name="pixelSize"/> es el
    /// lado en píxeles del viewBox; el SVG escala solo.
    /// </summary>
    public string ToSvg(string content, int pixelSize = 512)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("El contenido del QR no puede estar vacío.", nameof(content));

        var writer = new QRCodeWriter();
        var hints = new Dictionary<EncodeHintType, object>
        {
            // M tolera ~15% de daño: suficiente para una etiqueta que se ensucia
            // en el corral, sin agrandar demasiado la grilla.
            [EncodeHintType.ERROR_CORRECTION] = ErrorCorrectionLevel.M,
            [EncodeHintType.CHARACTER_SET] = "UTF-8",
            [EncodeHintType.MARGIN] = QuietZone,
        };

        // Pedimos la matriz cruda (1x1 por módulo) y armamos el SVG nosotros:
        // así el tamaño final lo decide el viewBox y no hay reescalado borroso.
        var matrix = writer.encode(content, ZXing.BarcodeFormat.QR_CODE, 0, 0, hints);
        int w = matrix.Width, h = matrix.Height;

        var path = new StringBuilder();
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (matrix[x, y])
                {
                    // Un rect por módulo, como comandos de path: pesa bastante
                    // menos que <rect> sueltos cuando el QR tiene cientos.
                    path.Append("M").Append(x).Append(' ').Append(y).Append("h1v1h-1z");
                }
            }
        }

        return $"""
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {w} {h}" width="{pixelSize}" height="{pixelSize}" shape-rendering="crispEdges" role="img" aria-label="Código QR de la ficha del animal">
            <rect width="{w}" height="{h}" fill="#ffffff"/>
            <path d="{path}" fill="#000000"/>
            </svg>
            """;
    }

    /// <summary>SVG del QR listo para embeber en un <c>src</c> de <c>&lt;img&gt;</c>.</summary>
    public string ToDataUri(string content, int pixelSize = 512)
    {
        var svg = ToSvg(content, pixelSize);
        var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
        return $"data:image/svg+xml;base64,{b64}";
    }
}
