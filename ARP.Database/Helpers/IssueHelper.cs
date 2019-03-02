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

using garb.Data;
using garb.Dto;
using garb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace garb.Helpers {
	public static class IssueHelper {

		public static Issue GetExistingIssue(GenericRepository<Issue> issueRepo, string project, string name, string locale, CreateIssueDto issueData)
		{
			Issue existingIssue = issueRepo.Get(
				i => i.ProjectName.Equals(project) &&
				i.ScreenName.Equals(name) &&
				i.LocaleCode.Equals(locale) &&
				i.IssueType == issueData.Type &&
				((i.Identifier != null && i.Identifier.Equals(issueData.Identifier))	// same identifier
				|| (!string.IsNullOrEmpty(i.Value) && i.Value.Equals(issueData.Text))	// same not empty text
				|| (string.IsNullOrEmpty(i.Value) && i.Height == issueData.Height && i.Width == issueData.Width && i.X == issueData.X && i.Y == issueData.Y)) // empty text and same coordinates 
				).FirstOrDefault();

			return existingIssue;
		}
	}
}
