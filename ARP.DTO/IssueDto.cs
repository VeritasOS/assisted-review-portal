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

using garb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace garb.Dto
{
	/// <summary>
	/// DTO for creating issues
	/// </summary>
	[Serializable]
	public class CreateIssueDto
	{
		/// <summary>
		/// Severity of an issue
		/// </summary>	
		public IssueSeverity Severity { get; set; }
		/// <summary>
		/// Type of an issue
		/// </summary>
		public IssueType Type { get; set; }
		/// <summary>
		/// Status of an issue
		/// </summary>
		public IssueStatus Status { get; set; }
		/// <summary>
		/// Issue identifier
		/// </summary>
		public string Identifier { get; set; }
		/// <summary>
		/// Text of an issue
		/// </summary>
		public string Text { get; set; }
		/// <summary>
		/// X position of an issue on the screen
		/// </summary>
		public int X { get; set; }
		/// <summary>
		/// Y position of an issue on the screen
		/// </summary>
		public int Y { get; set; }
		/// <summary>
		/// Width of an issue rectangle
		/// </summary>
		public int Width { get; set; }
		/// <summary>
		/// Height of an issue rectangle
		/// </summary>
		public int Height { get; set; }
	}

	/// <summary>
	/// DTO for issues
	/// </summary>
	[Serializable]
	public class IssueDto : CreateIssueDto
	{
		//public IssueDto(Issue issue)
		//{
		//	Id = issue.IssueId;
		//	ScreenName = issue.ScreenName;
		//	ProjectName = issue.ProjectName;
		//	LocaleCode = issue.LocaleCode;

		//	DateReported = issue.ModificationTime;
		//	BuildReported = issue.Build.BuildName;
		//	DateModified = issue.ModificationTime;
		//	BuildModified = issue.Build.BuildName;
		//	Status = issue.IssueStatus;
			
		//	Severity = issue.IssueSeverity;
		//	Type = issue.IssueType;
		//	Identifier = issue.Identifier;
		//	Text = issue.Value;
		//	X = issue.X;
		//	Y = issue.Y;
		//	Width = issue.Width;
		//	Height = issue.Height;
		//}

		/// <summary>
		/// Issue identifier
		/// </summary>
		public Guid Id { get; set; }
		/// <summary>
		/// Name of the screen where issue appears
		/// </summary>
		public string ScreenName { get; set; }
		/// <summary>
		/// Name of the project where issue appears
		/// </summary>
		public string ProjectName { get; set; }
		/// <summary>
		/// Locale code in which issue appears
		/// </summary>
		public string LocaleCode { get; set; }
		/// <summary>
		/// Date when issue was first reported
		/// </summary>
		public DateTime DateReported { get; set; }
		/// <summary>
		/// Build in which issues was reported
		/// </summary>
		public string BuildReported { get; set; }
		/// <summary>
		/// Date when issue was modified
		/// </summary>
		public DateTime DateModified { get; set; }
		/// <summary>
		/// Build in which issues was modified
		/// </summary>
		public string BuildModified { get; set; }
	}
}
