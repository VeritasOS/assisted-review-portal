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

using garb.Helpers;
using garb.Models;
using System;

namespace garb.Dto
{
	public class ComparisonDto
    {
        public ComparisonDto() { }
        public ComparisonDto(Comparison comparison)
        {
            SourceScreenInBuildId = comparison.SourceScreenInBuildId;
            TargetScreenInBuildId = comparison.TargetScreenInBuildId;
            if (comparison.SourceScreenInBuild != null)
                SourceScreenName = comparison.SourceScreenInBuild.ScreenName;
            if (comparison.TargetScreenInBuild != null)
                TargetScreenName = comparison.TargetScreenInBuild.ScreenName;
            Difference = comparison.Difference;
            if (comparison.SourceScreenInBuild != null && comparison.TargetScreenInBuild != null && comparison.Difference != 0 && comparison.Difference != 1)
                DiffImagePath = StorageHelper.GetDiffImagePath(SourceScreenInBuildId, TargetScreenInBuildId, '/');
        }

        public Guid SourceScreenInBuildId { get; set; }
        public Guid TargetScreenInBuildId { get; set; }
		public string SourceScreenName { get; set; }
		public string TargetScreenName { get; set; }
		public double Difference { get; set; }
        public string DiffImagePath { get; set; }
    }
}
