using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Online;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Sdcb.PaddleOCR.Tests;

public class OnlineModelsTest
{
    private readonly ITestOutputHelper _console;

    public OnlineModelsTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public async Task FastCheckOCR()
    {
        FullOcrModel model = await OnlineFullModels.EnglishV3.DownloadAsync();

        byte[] sampleImageData = File.ReadAllBytes(@"./samples/vsext.png");

        using (PaddleOcrAll all = new(model)
        {
            AllowRotateDetection = true, /* 允许识别有角度的文字 */
            Enable180Classification = false, /* 允许识别旋转角度大于90度的文字 */
        })
        {
            // Load local file by following code:
            // using (Mat src2 = Cv2.ImRead(@"C:\test.jpg"))
            using (Mat src = Cv2.ImDecode(sampleImageData, ImreadModes.Color))
            {
                PaddleOcrResult result = all.Run(src);
                _console.WriteLine("Detected all texts: \n" + result.Text);
                foreach (PaddleOcrResultRegion region in result.Regions)
                {
                    _console.WriteLine($"Text: {region.Text}, Score: {region.Score}, RectCenter: {region.Rect.Center}, RectSize:    {region.Rect.Size}, Angle: {region.Rect.Angle}");
                }
            }
        }
    }

    [Fact]
    public async Task V4FastCheckOCR()
    {
        OnlineFullModels onlineModels = new OnlineFullModels(
            OnlineDetectionModel.ChineseV4, null, LocalDictOnlineRecognizationModel.EnglishV4);
        FullOcrModel model = await onlineModels.DownloadAsync();

        byte[] sampleImageData = File.ReadAllBytes(@"./samples/vsext.png");

        using (PaddleOcrAll all = new(model, PaddleDevice.Openblas(), PaddleDevice.Mkldnn(), PaddleDevice.Openblas())
        {
            AllowRotateDetection = true, /* 允许识别有角度的文字 */
            Enable180Classification = false, /* 允许识别旋转角度大于90度的文字 */
        })
        {
            // Load local file by following code:
            // using (Mat src2 = Cv2.ImRead(@"C:\test.jpg"))
            using (Mat src = Cv2.ImDecode(sampleImageData, ImreadModes.Color))
            {
                PaddleOcrResult result = null!;
                int count = 1;
                double total = 0;
                for (int i = 0; i < 5; ++i)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    result = all.Run(src);
                    total += sw.Elapsed.TotalMilliseconds;
                }

                _console.WriteLine($"average elapsed={total / count:N2}ms");
                _console.WriteLine("Detected all texts: \n" + result.Text);
            }
        }
    }
}