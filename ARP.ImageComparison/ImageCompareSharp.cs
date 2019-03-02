/*
Copyright (c) 2019 Veritas Technologies LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace garb.ImageComparison
{
	public class ImageCompareSharp
	{
		public static double GenerateDiffImageDirect(string image1Path, string image2Path, string diffImagePath = null)
		{
			// same image or two nulls - no diff
			if ((image1Path == null && image2Path == null) || image1Path.Equals(image2Path))
				return 0;

			// one image imissing - 100% diff
			if (image1Path == null || image2Path == null)
				return 1;

			Rgba32 diffColor = Rgba32.Red;
			Rgba32 equalColor = Rgba32.Transparent;

			Image<Rgba32> image1 = Image.Load<Rgba32>(image1Path);
			Image<Rgba32> image2 = Image.Load<Rgba32>(image2Path);

			int minWidth = Math.Min(image1.Width, image2.Width);
			int minHeight = Math.Min(image1.Height, image2.Height);
			int maxWidth = Math.Max(image1.Width, image2.Width);
			int maxHeight = Math.Max(image1.Height, image2.Height);

			Image<Rgba32> diffImage = new Image<Rgba32>(maxWidth, maxHeight);

			int allPixels = maxWidth * maxHeight;
			int diffPixels = 0;

			for (int x = 0; x < maxWidth; x++)
			{
				for (int y = 0; y < maxHeight; y++)
				{
					if (x < minWidth && y < minHeight)
					{
						if (!image1[x, y].Equals(image2[x, y]))
						{
							diffImage[x, y] = diffColor;
							++diffPixels;
						}
						else
						{
							diffImage[x, y] = equalColor;
						}
					}
					else
					{
						diffImage[x, y] = diffColor;
						++diffPixels;
					}
				}
			}

			if (diffImagePath != null)
			{
				diffImage.Save(diffImagePath);
			}

			return (double)diffPixels / allPixels;
		}

		public static double GenerateDiffImageFast(string image1Path, string image2Path, string diffImagePath = null)
		{
			// same image or two nulls - no diff
			if ((image1Path == null && image2Path == null) || image1Path.Equals(image2Path))
				return 0;

			// one image imissing - 100% diff
			if (image1Path == null || image2Path == null)
				return 1;

			Rgba32 diffColor = Rgba32.Red;
			Rgba32 equalColor = Rgba32.Transparent;

			Image<Rgba32> image1 = Image.Load<Rgba32>(image1Path);
			Image<Rgba32> image2 = Image.Load<Rgba32>(image2Path);

			int w1 = image1.Width;
			int h1 = image1.Height;

			int w2 = image2.Width;
			int h2 = image2.Height;

			int minWidth = Math.Min(w1, w2);
			int minHeight = Math.Min(h1, h2);
			int maxWidth = Math.Max(w1, w2);
			int maxHeight = Math.Max(h1, h2);

			//Image<Rgba32> diffImage = new Image<Rgba32>(maxWidth, maxHeight);

			int allPixels = maxWidth * maxHeight;
			int diffPixels = 0;


			Rgba32[] pixels1 = image1.GetPixelSpan().ToArray();
			Rgba32[] pixels2 = image1.GetPixelSpan().ToArray();

			Rgba32[] pixelsDiff = new Rgba32[maxWidth * maxHeight];

			for (int x = 0; x < maxWidth; x++)
			{
				for (int y = 0; y < maxHeight; y++)
				{
					int offset1 = y * w1 + x;
					int offset2 = y * w2 + x;
					int offsetDiff = y * maxWidth + x;

					if (x < minWidth && y < minHeight)
					{

						if (pixels1[offset1] == pixels2[offset2])
						{
							pixelsDiff[offsetDiff] = diffColor;
							++diffPixels;
						}
						else
						{
							pixelsDiff[offsetDiff] = equalColor;
						}
					}
					else
					{
						pixelsDiff[offsetDiff] = diffColor;
						++diffPixels;
					}
				}
			}

			if (diffImagePath != null)
			{
				Image<Rgba32> diffImage = Image.LoadPixelData(pixelsDiff, maxWidth, maxHeight);

				diffImage.Save(diffImagePath);
			}

			return (double)diffPixels / allPixels;
		}

		public static double GenerateDiffImageFaster(string image1Path, string image2Path, string diffImagePath = null)
		{
			// same image or two nulls - no diff
			if ((image1Path == null && image2Path == null) || image1Path.Equals(image2Path))
				return 0;

			// one image imissing - 100% diff
			if (image1Path == null || image2Path == null)
				return 1;


			Image<Rgba32> image1 = Image.Load<Rgba32>(image1Path);
			Image<Rgba32> image2 = Image.Load<Rgba32>(image2Path);

			int w1 = image1.Width;
			int h1 = image1.Height;

			int w2 = image2.Width;
			int h2 = image2.Height;

			int minWidth = Math.Min(w1, w2);
			int minHeight = Math.Min(h1, h2);
			int maxWidth = Math.Max(w1, w2);
			int maxHeight = Math.Max(h1, h2);

			//Image<Rgba32> diffImage = new Image<Rgba32>(maxWidth, maxHeight);

			int allPixels = maxWidth * maxHeight;
			int diffPixels = 0;

			byte[] rgbaBytes1 = MemoryMarshal.AsBytes(image1.GetPixelSpan()).ToArray();
			byte[] rgbaBytes2 = MemoryMarshal.AsBytes(image2.GetPixelSpan()).ToArray();
			byte[] rgbaBytesDiff = new byte[maxWidth * maxHeight * 4];


			//Rgba32[] pixels1 = image1.GetPixelSpan().ToArray();
			//Rgba32[] pixels2 = image1.GetPixelSpan().ToArray();

			//Rgba32[] pixelsDiff = new Rgba32[maxWidth * maxHeight];

			Rgba32 diffColor = Rgba32.Red;
			Rgba32 equalColor = Rgba32.Transparent;

			byte R = diffColor.R;
			byte G = diffColor.G;
			byte B = diffColor.B;
			byte A = equalColor.A;

			for (int x = 0; x < maxWidth; x++)
			{
				for (int y = 0; y < maxHeight; y++)
				{
					int offset1 = (y * w1 + x)<<2;
					int offset2 = (y * w2 + x)<<2;
					int offsetDiff = (y * maxWidth + x)<<2;

					if (x < minWidth && y < minHeight)
					{

						if (rgbaBytes1[offset1] == rgbaBytes2[offset2] && rgbaBytes1[offset1 + 1] == rgbaBytes2[offset2 + 1] && rgbaBytes1[offset1 + 2] == rgbaBytes2[offset2 + 2])
						{
							rgbaBytesDiff[offsetDiff] = R;
							rgbaBytesDiff[offsetDiff + 1] = G;
							rgbaBytesDiff[offsetDiff + 2] = B;
							++diffPixels;
						}
						else
						{
							rgbaBytesDiff[offsetDiff + 3] = A;
						}
					}
					else
					{
						rgbaBytesDiff[offsetDiff] = R;
						rgbaBytesDiff[offsetDiff + 1] = G;
						rgbaBytesDiff[offsetDiff + 2] = B;
						++diffPixels;
					}
				}
			}

			if (diffImagePath != null)
			{
				Image<Rgba32> diffImage = Image.LoadPixelData<Rgba32>(rgbaBytesDiff, maxWidth, maxHeight);

				diffImage.Save(diffImagePath);
			}

			return (double)diffPixels / allPixels;
		}


		public static double GenerateDiffImage(string image1Path, string image2Path, string diffImagePath = null)
		{
			// same image or two nulls - no diff
			if ((image1Path == null && image2Path == null) || image1Path.Equals(image2Path))
				return 0;

			// one image imissing - 100% diff
			if (image1Path == null || image2Path == null)
				return 1;

			Rgba32 diffColor = Rgba32.Red;
			Rgba32 equalColor = Rgba32.Transparent;

			Image<Rgba32> image1 = Image.Load<Rgba32>(image1Path);
			Image<Rgba32> image2 = Image.Load<Rgba32>(image2Path);

			int minWidth = Math.Min(image1.Width, image2.Width);
			int minHeight = Math.Min(image1.Height, image2.Height);
			int maxWidth = Math.Max(image1.Width, image2.Width);
			int maxHeight = Math.Max(image1.Height, image2.Height);

			Image<Rgba32> diffImage = new Image<Rgba32>(maxWidth, maxHeight);

			int allPixels = maxWidth * maxHeight;
			int diffPixels = 0;

			for (int y = 0; y < maxHeight; y++)
			{
				Span<Rgba32> pixelRowSpan1 = image1.GetPixelRowSpan(y);
				Span<Rgba32> pixelRowSpan2 = image2.GetPixelRowSpan(y);
				Span<Rgba32> pixelRowSpanDiff = diffImage.GetPixelRowSpan(y);

				for (int x = 0; x < maxWidth; x++)
				{
					if (x < minWidth && y < minHeight)
					{
						if (!pixelRowSpan1[x].Equals(pixelRowSpan2[x]))
						{
							pixelRowSpanDiff[x] = diffColor;
							++diffPixels;
						}
						else
						{
							pixelRowSpanDiff[x] = equalColor;
						}
					}
					else
					{
						pixelRowSpanDiff[x] = diffColor;
						++diffPixels;
					}
				}
			}

			if (diffImagePath != null)
			{
				diffImage.Save(diffImagePath);
			}

			return (double)diffPixels / allPixels;
		}
	}
}
