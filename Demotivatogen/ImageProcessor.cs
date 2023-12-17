using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Demotivatogen;

public static class ImageProcessor
{
    private static readonly Point ImageSize = new(1024, 1024);

    private const int BorderThickness = 5;
    private const int MaxFontSize = 80;
    private const int MaxSecondFontSize = 60;

    private static readonly Point BorderMarginX = new(20, 20);
    private static readonly Point BorderMarginY = new(20, ImageSize.X / 4);

    private static readonly Point Border1 = BorderMarginX;
    private static readonly Point Border2 = new(ImageSize.X - BorderMarginX.X, ImageSize.Y - BorderMarginY.Y);

    private const int ImagePadding = 15;

    private static readonly PointF[] BorderBox =
    [
        Border1, new Point(Border2.X, Border1.Y),
        Border2, new Point(Border1.X, Border2.Y),
    ];

    private static readonly Point ImageBorder1 = new(Border1.X + ImagePadding, Border1.Y + ImagePadding);
    private static readonly Point ImageBorder2 = new(Border2.X - ImagePadding, Border2.Y - ImagePadding);

    private static readonly Size CentralImageSize =
        new(ImageBorder2.X - ImageBorder1.X, ImageBorder2.Y - ImageBorder1.Y);

    private static readonly Point TextBorder1 = new(BorderMarginX.X, Border2.Y + ImagePadding);
    private static readonly Point TextBorder2 = new(ImageSize.X - BorderMarginX.X, ImageSize.Y - ImagePadding);

    private static readonly Size TextSize = new(TextBorder2.X - TextBorder1.X, TextBorder2.Y - TextBorder1.Y);
    private static readonly Size TextSizeHalf = new(TextSize.Width, TextSize.Height / 2);

    private static readonly Point TextPositionCenter =
        new((TextBorder1.X + TextBorder2.X) / 2, (TextBorder1.Y + TextBorder2.Y) / 2);

    private static readonly Point TextPositionFirst =
        new(TextPositionCenter.X, TextPositionCenter.Y - TextSize.Height / 4);

    private static readonly Point TextPositionSecond =
        new(TextPositionCenter.X, TextPositionCenter.Y + TextSize.Height / 4);

    private static readonly Font MainFont = SystemFonts.CreateFont("Times New Roman", MaxFontSize, FontStyle.Bold);

    private static readonly Font SecondFont =
        SystemFonts.CreateFont("Times New Roman", MaxSecondFontSize, FontStyle.Bold);
    
    public static async Task CreateImageAsync(Stream centralImage, Stream output, string mainText, string? subText)
    {
        using Image image = new Image<Rgba32>(ImageSize.X, ImageSize.Y, Color.Black);

        image.Mutate(ctx =>
        {
            using var central = Image.Load(centralImage);
            ctx.DrawPolygon(Color.White, BorderThickness, BorderBox);
            central.Mutate(c => c.Resize(CentralImageSize));
            ctx.DrawImage(central, ImageBorder1, 1);

            var mainTextPosition = subText is null ? TextPositionCenter : TextPositionFirst;
            var mainTextSize = subText is null ? TextSize : TextSizeHalf;

            var textOptions = new RichTextOptions(MainFont)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TabWidth = 2,
                Origin = mainTextPosition,
            };
            var size = TextMeasurer.MeasureBounds(mainText, textOptions);
            var scale = Math.Min(mainTextSize.Width / size.Width, mainTextSize.Height / size.Height);
            textOptions.Font = new Font(MainFont, Math.Min(MaxFontSize, MainFont.Size * scale));
            ctx.DrawText(textOptions, mainText, Color.White);

            if (subText is null) return;
            var textOptions1 = new RichTextOptions(SecondFont)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TabWidth = 2,
                Origin = TextPositionSecond,
            };
            var size1 = TextMeasurer.MeasureBounds(subText, textOptions1);
            var scale1 = Math.Min(TextSizeHalf.Width / size1.Width, TextSizeHalf.Height / size1.Height);
            textOptions1.Font = new Font(SecondFont, Math.Min(MaxSecondFontSize, SecondFont.Size * scale1));
            ctx.DrawText(textOptions1, subText, Color.White);
        });

        await image.SaveAsPngAsync(output);
    }
}