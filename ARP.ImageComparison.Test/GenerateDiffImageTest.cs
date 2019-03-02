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

using garb.ImageComparison;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace garb.ImageComparison.Test
{
	[TestClass]
	public class GenerateDiffImageTest
	{
		[TestInitialize]
		public void Initialize()
		{

		}

		[TestMethod, Timeout(1000)]
		public async Task CanGenerateDiff()
		{
			string image1Path = "TestFiles/en1.png";
			string image2Path = "TestFiles/ja1.png";
			string diffImagePath12 = "TestFiles/diff12.png";
			string diffImagePath21 = "TestFiles/diff21.png";

			double diff11 = ImageCompareSharp.GenerateDiffImageDirect(image1Path, image1Path);

			double diff12 = ImageCompareSharp.GenerateDiffImageDirect(image1Path, image2Path, diffImagePath12);

			double diff21 = ImageCompareSharp.GenerateDiffImageDirect(image2Path, image1Path, diffImagePath21);

			Assert.AreEqual(diff12, diff21, 0.01);

		}

		[TestMethod, Timeout(1000)]
		public async Task CanGenerateDiffFast()
		{
			string image1Path = "TestFiles/en1.png";
			string image2Path = "TestFiles/ja1.png";
			string diffImagePath12 = "TestFiles/diff12.png";
			string diffImagePath21 = "TestFiles/diff21.png";

			double diff11 = ImageCompareSharp.GenerateDiffImageFast(image1Path, image1Path);

			double diff12 = ImageCompareSharp.GenerateDiffImageFast(image1Path, image2Path, diffImagePath12);

			double diff21 = ImageCompareSharp.GenerateDiffImageFast(image2Path, image1Path, diffImagePath21);

			Assert.AreEqual(diff12, diff21, 0.01);

		}

		[TestMethod, Timeout(1000)]
		public async Task CanGenerateDiffFastest()
		{
			string image1Path = "TestFiles/en1.png";
			string image2Path = "TestFiles/ja1.png";
			string diffImagePath12 = "TestFiles/diff12.png";
			string diffImagePath21 = "TestFiles/diff21.png";

			double diff11 = ImageCompareSharp.GenerateDiffImageFaster(image1Path, image1Path);

			double diff12 = ImageCompareSharp.GenerateDiffImageFaster(image1Path, image2Path, diffImagePath12);

			double diff21 = ImageCompareSharp.GenerateDiffImageFaster(image2Path, image1Path, diffImagePath21);

			Assert.AreEqual(diff12, diff21, 0.01);

		}

		[TestMethod, Timeout(1000)]
		public async Task CanGenerateDiffRowSpan()
		{
			string image1Path = "TestFiles/en1.png";
			string image2Path = "TestFiles/ja1.png";
			string diffImagePath12 = "TestFiles/diff12.png";
			string diffImagePath21 = "TestFiles/diff21.png";

			double diff11 = ImageCompareSharp.GenerateDiffImage(image1Path, image1Path);

			double diff12 = ImageCompareSharp.GenerateDiffImage(image1Path, image2Path, diffImagePath12);

			double diff21 = ImageCompareSharp.GenerateDiffImage(image2Path, image1Path, diffImagePath21);

			Assert.AreEqual(diff12, diff21, 0.01);

		}
	}
}
