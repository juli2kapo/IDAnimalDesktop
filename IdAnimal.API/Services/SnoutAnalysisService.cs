using Akira.Core.Enum;
using OpenCvSharp;
using OpenCvSharp.Features2D;
using Sumat.Animal.Functions;
using Sumat.Animal.Inference;
using Sumat.Classes;
using Sumat.Enums;
using System.Text.Json;

namespace IdAnimal.API.Services;

public interface ISnoutAnalysisService
{
    /// <summary>
    /// Detects a snout in the image bytes, crops it, and extracts SIFT descriptors.
    /// Returns null if no snout is detected.
    /// </summary>
    SnoutAnalysisResult? Analyze(byte[] imageBytes);

    /// <summary>
    /// Compares two lists of descriptors and returns the number of good matches using FLANN and Lowe's ratio test.
    /// </summary>
    SnoutMatchResult Compare(List<List<float>> descriptors1, List<List<float>> descriptors2);
}

public class SnoutAnalysisResult
{
    public string KeypointsJson { get; set; } = string.Empty;
    public string DescriptorsJson { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public int[] BBox { get; set; } = Array.Empty<int>();
}

public class SnoutMatchResult
{
    public int GoodMatchCount { get; set; }
    public List<object> Matches { get; set; } = new();
}

public class SnoutAnalysisService : ISnoutAnalysisService
{
    private readonly SnoutDetection _detector;
    // FLANN matcher is expensive to recreate, so we keep an instance or create per call. 
    // Creating per call is safer for thread-safety if the library isn't strictly thread-safe.
    
    public SnoutAnalysisService()
    {
        // Initialize detector (Intel CPU, 0.5f threshold as per your TestAnimal logic)
        _detector = new SnoutDetection(AcceleratorType.INTEL_CPU, 0.5f);
    }

    public SnoutAnalysisResult? Analyze(byte[] imageBytes)
    {
        using var srcImage = Cv2.ImDecode(imageBytes, ImreadModes.Color);
        if (srcImage.Empty()) return null;

        // 1. Detect Snout
        var results = _detector.Detect(srcImage);
        if (results.Count == 0) return null;

        var bestDetection = results[0];

        // 2. Crop Snout (Using Sumat Functions)
        using var croppedSnout = FunSnout.SnoutCrop(srcImage, bestDetection.Rect, 1.0f);

        // 3. Extract Features
        return ExtractSiftFeatures(croppedSnout, bestDetection);
    }

    public SnoutMatchResult Compare(List<List<float>> descriptors1, List<List<float>> descriptors2)
    {
        if (descriptors1 == null || descriptors1.Count == 0 || 
            descriptors2 == null || descriptors2.Count == 0)
        {
            return new SnoutMatchResult { GoodMatchCount = 0 };
        }

        // Convert lists to OpenCV Mats
        using var mat1 = ListToMat(descriptors1);
        using var mat2 = ListToMat(descriptors2);

        using var flann = new FlannBasedMatcher();
        
        // k=2 for Ratio Test
        var knnMatches = flann.KnnMatch(mat1, mat2, 2);

        var goodMatches = new List<object>();

        // Apply Lowe's Ratio Test (0.7)
        foreach (var match in knnMatches)
        {
            if (match.Length >= 2)
            {
                var m = match[0];
                var n = match[1];
                if (m.Distance < 0.7 * n.Distance)
                {
                    goodMatches.Add(new 
                    { 
                        queryIdx = m.QueryIdx, 
                        trainIdx = m.TrainIdx, 
                        distance = m.Distance 
                    });
                }
            }
        }

        return new SnoutMatchResult
        {
            GoodMatchCount = goodMatches.Count,
            Matches = goodMatches
        };
    }

    // --- Helpers ---

    private SnoutAnalysisResult ExtractSiftFeatures(Mat image, BBox detection)
    {
        using var sift = SIFT.Create();
        using var descriptors = new Mat();

        sift.DetectAndCompute(image, null, out KeyPoint[] keypoints, descriptors);

        // Serialize Keypoints
        var kpSerial = keypoints.Select(kp => new
        {
            pt = new[] { kp.Pt.X, kp.Pt.Y },
            kp.Size,
            kp.Angle,
            kp.Response,
            kp.Octave,
            kp.ClassId
        }).ToList();

        // Serialize Descriptors
        var desList = new List<List<float>>();
        if (!descriptors.Empty())
        {
            for (int i = 0; i < descriptors.Rows; i++)
            {
                var row = new List<float>();
                // Descriptors are typically CV_32F (float) for SIFT
                for (int j = 0; j < descriptors.Cols; j++)
                {
                    row.Add(descriptors.At<float>(i, j));
                }
                desList.Add(row);
            }
        }

        return new SnoutAnalysisResult
        {
            KeypointsJson = JsonSerializer.Serialize(kpSerial),
            DescriptorsJson = JsonSerializer.Serialize(desList),
            Confidence = detection.Confidence,
            BBox = new[] { (int)detection.RectBox.X, (int)detection.RectBox.Y, (int)detection.RectBox.Width, (int)detection.RectBox.Height }
        };
    }

    private Mat ListToMat(List<List<float>> list)
    {
        int rows = list.Count;
        int cols = list[0].Count;
        var mat = new Mat(rows, cols, MatType.CV_32F);

        // unsafe/direct memory copy is faster, but simple loop is safer for now
        // Using Parallel.For for speed on larger sets
        Parallel.For(0, rows, i =>
        {
            for (int j = 0; j < cols; j++)
            {
                mat.Set(i, j, list[i][j]);
            }
        });
        return mat;
    }
}