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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace garb.Models
{
    public class IssueRevision : ITrackedEntityRevision
    {
        public Guid IssueId { get; set; }
        public int RevisionNo { get; set; }
        [StringLength(260)]
        public string ScreenName { get; set; }
        [StringLength(100)]
        public string ProjectName { get; set; }
        [StringLength(32)]
        public string LocaleCode { get; set; }
		public Guid ModifiedInBuildId { get; set; }
		public DateTime ModificationTime { get; set; } = DateTime.UtcNow;
        [StringLength(100)]
        public string ModifiedByUser { get; set; }
		public IssueType IssueType { get; set; }
		public IssueSeverity IssueSeverity { get; set; }
		public IssueStatus IssueStatus { get; set; }
		public string Identifier { get; set; }
		public string Value { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		[ForeignKey("IssueId")]
        public virtual Issue Issue { get; set; }
    }
}
