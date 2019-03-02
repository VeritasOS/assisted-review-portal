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

using AutoMapper;
using garb.Data;
using garb.Dto;
using garb.Helpers;
using garb.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace garbUnitTest.Revisions
{
    [TestClass]
    public class IssuesRevisionsTest
    {
        private GarbContext _context;
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;

        [TestInitialize]
        public void InitializeDb()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<GarbProfile>(); cfg.CreateMap<Issue, Issue>(); });
            _mapper = config.CreateMapper();
            _context = TestInitializer.GetSeededContext(_mapper);
            _unitOfWork = new UnitOfWork(_context);
        }

        [TestMethod]
        public void CanCreateIssueRevision()
        {

            GenericRepository<Issue> issueRepo = _unitOfWork.IssueRepository;
            GenericRepository<Build> buildRepo = _unitOfWork.BuildRepository;

            int initialIssueNo = _context.Issues.Count();
            int initialIssueRevNo = _context.IssueRevision.Count();

            string _projectName = "Velocity";
            string _screenName = "Install";
            string _locale = "pl-PL";
            string _build = "1.0";
            string initialValue = "Test issue";

            Build build = buildRepo.Get(b => b.BuildName.Equals(_build)).FirstOrDefault();

            Issue newIssue = new Issue { Height = 10, Width = 10, X = 5, Y = 6, Value = initialValue, IssueSeverity = IssueSeverity.Error, IssueType = IssueType.Linguistic, ProjectName = _projectName, ScreenName = _screenName, LocaleCode = _locale, Build = build, IssueStatus = IssueStatus.Active };

            issueRepo.Insert(newIssue);

            _unitOfWork.Save(TestInitializer.UserName);

            Guid issueId = newIssue.IssueId;

            Assert.AreEqual(initialIssueNo + 1, _context.Issues.Count());
            Assert.AreEqual(initialIssueRevNo, _context.IssueRevision.Count());

            Issue selectedIssue = issueRepo.Get(i => i.IssueId == issueId).FirstOrDefault();

            Assert.AreEqual(initialValue, selectedIssue.Value);

            string modifiedValue = "New value";

            newIssue.Value = modifiedValue;

            _unitOfWork.Save(TestInitializer.UserName);

            Assert.AreEqual(initialIssueNo + 1, _context.Issues.Count());
            Assert.AreEqual(initialIssueRevNo + 1, _context.IssueRevision.Count());

            _unitOfWork.Save(TestInitializer.UserName);

            Assert.AreEqual(initialIssueNo + 1, _context.Issues.Count());
            Assert.AreEqual(initialIssueRevNo + 1, _context.IssueRevision.Count());

            Issue selectedUpdatedIssue = issueRepo.Get(i => i.IssueId == issueId).FirstOrDefault();
            IssueRevision selectedIssueRevision = _context.IssueRevision.FirstOrDefault(i => i.IssueId == issueId);

            Assert.AreEqual(initialValue, selectedIssueRevision.Value);
            Assert.AreEqual(modifiedValue, selectedUpdatedIssue.Value);

            string anotherModifiedValue = initialValue;

            newIssue.Value = anotherModifiedValue;

            _unitOfWork.Save(TestInitializer.UserName);

            Assert.AreEqual(initialIssueNo + 1, _context.Issues.Count());
            Assert.AreEqual(initialIssueRevNo + 2, _context.IssueRevision.Count());

            Issue selectedUpdatedAgainIssue = issueRepo.Get(i => i.IssueId == issueId).FirstOrDefault();
            IssueRevision selectedIssueRevision1 = _context.IssueRevision.FirstOrDefault(i => i.IssueId == issueId && i.RevisionNo == 1);
            IssueRevision selectedIssueRevision2 = _context.IssueRevision.FirstOrDefault(i => i.IssueId == issueId && i.RevisionNo == 2);

            Assert.AreEqual(initialValue, selectedIssueRevision1.Value);
            Assert.AreEqual(modifiedValue, selectedIssueRevision2.Value);
            Assert.AreEqual(anotherModifiedValue, selectedUpdatedAgainIssue.Value);

        }

        [TestMethod]
        public void CanDetectExistingIssues()
        {
            GenericRepository<Issue> issueRepo = _unitOfWork.IssueRepository;
            GenericRepository<Build> buildRepo = _unitOfWork.BuildRepository;

            int initialIssueNo = _context.Issues.Count();
            int initialIssueRevNo = _context.IssueRevision.Count();

            string _projectName = "Velocity";
            string _screenName = "Install";
            string _locale = "pl-PL";
            string _build = "1.0";

            Build build = buildRepo.Get(b => b.BuildName.Equals(_build)).FirstOrDefault();

            //new Issue { ProjectName = project1Name, ScreenName = screen1Name, LocaleCode = "en-US", IssueType = IssueType.Hardcode, Identifier = "1", Value = "Hardcode", ModifiedInBuildId = buildId, IssueStatus = IssueStatus.Active },
            //new Issue { ProjectName = project1Name, ScreenName = screen1Name, LocaleCode = "pl-PL", IssueType = IssueType.Hardcode, Identifier = "1", Value = "Hardcode", ModifiedInBuildId = buildId, IssueStatus = IssueStatus.Active },
            //new Issue { ProjectName = project1Name, ScreenName = screen1Name, LocaleCode = "pl-PL", IssueType = IssueType.Linguistic, Identifier = "2", Value = "Test1", ModifiedInBuildId = buildId, IssueStatus = IssueStatus.FalsePositive },
            //new Issue { ProjectName = project1Name, ScreenName = screen1Name, LocaleCode = "pl-PL", IssueType = IssueType.Overlapping, Identifier = "3", Value = "", ModifiedInBuildId = buildId, IssueStatus = IssueStatus.Active, X = 0, Y = 0, Width = 10, Height = 10 }


            IssueDto sameIssue1 = new IssueDto() { Identifier = "1", Type = IssueType.Hardcode, Text = "New hardcode" };    // same Id different text
            IssueDto sameIssue2 = new IssueDto() { Identifier = "0", Type = IssueType.Hardcode, Text = "Hardcode" };    // differetn Id same text
            IssueDto sameIssue3 = new IssueDto() { Identifier = "0", Type = IssueType.Overlapping, Text = "", X = 0, Y = 0, Width = 10, Height = 10 }; // different Id, empty text, same coordinates
            IssueDto sameIssue4 = new IssueDto() { Identifier = "1", Type = IssueType.Hardcode, Text = "Hardcode" };  // same 1
            IssueDto sameIssue5 = new IssueDto() { Identifier = "2", Type = IssueType.Linguistic, Text = "Test1" };  // same 2
            IssueDto differentTypeIssue = new IssueDto() { Identifier = "1", Type = IssueType.Linguistic, Text = "Hardcode" };  // same, with different type



            Assert.IsNotNull(IssueHelper.GetExistingIssue(issueRepo, _projectName, _screenName, _locale, sameIssue1));
            Assert.IsNotNull(IssueHelper.GetExistingIssue(issueRepo, _projectName, _screenName, _locale, sameIssue2));
            Assert.IsNotNull(IssueHelper.GetExistingIssue(issueRepo, _projectName, _screenName, _locale, sameIssue3));
            Assert.IsNotNull(IssueHelper.GetExistingIssue(issueRepo, _projectName, _screenName, _locale, sameIssue4));
            Assert.IsNull(IssueHelper.GetExistingIssue(issueRepo, _projectName+"1", _screenName, _locale, sameIssue4));
            Assert.IsNull(IssueHelper.GetExistingIssue(issueRepo, _projectName, _screenName+"1", _locale, sameIssue4));
            Assert.IsNotNull(IssueHelper.GetExistingIssue(issueRepo, _projectName, _screenName, "en-US", sameIssue4));
            Assert.IsNull(IssueHelper.GetExistingIssue(issueRepo, _projectName, _screenName, "en-US", sameIssue5));
            Assert.IsNull(IssueHelper.GetExistingIssue(issueRepo, _projectName, _screenName, _locale, differentTypeIssue));
        }
    }
}
