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
using garb.Controllers;
using garb.Data;
using garb.Dto;
using garb.Helpers;
using garb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace garbUnitTest.Controllers
{
    [TestClass]
    public class IssuesControllerTest
    {
        private GarbContext _context;
        private IssuesController _controller;
        private IMapper _mapper;

        [TestInitialize]
        public void InitializeDb()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<GarbProfile>(); cfg.CreateMap<IssueDto, IssueDto>(); });
            _mapper = config.CreateMapper();
            _context = TestInitializer.GetSeededContext(_mapper);
        }

        [TestMethod]
        public async Task CanGetIssues()
        {
            _controller = new IssuesController(_context, _mapper);

            string _projectName = "Velocity";
            string _screenName = "Install";
            string _locale = "pl-PL";

            //var result = await _controller.Get(_projectName) as OkObjectResult;
            var OkObjectTask = _controller.Get(_projectName, _screenName, _locale);

            OkObjectTask.Wait();

            var result = OkObjectTask.Result as OkObjectResult;

            Assert.IsNotNull(result);

            var issues = result.Value as ICollection<IssueDto>;

            Assert.IsNotNull(issues);

            Assert.IsTrue(issues.Count == 2);

            var OkObjectTaskAll = _controller.Get(_projectName, _screenName, _locale, false);

            OkObjectTaskAll.Wait();

            var resultAll = OkObjectTaskAll.Result as OkObjectResult;

            Assert.IsNotNull(resultAll);

            var issuesAll = resultAll.Value as ICollection<IssueDto>;

            Assert.IsNotNull(issuesAll);

            Assert.IsTrue(issuesAll.Count == 3);

        }

        [TestMethod]
        public async Task CanCreateIssue()
        {

            _controller = new IssuesController(_context, _mapper);

            _controller.ControllerContext = TestInitializer.GetContext();


            string _projectName = "Velocity";
            string _screenName = "Install";
            string _locale = "pl-PL";
            string _build = "1.0";

            CreateIssueDto newIssue = new CreateIssueDto { Height = 10, Width = 10, X = 5, Y = 6, Text = "Test issue", Severity = IssueSeverity.Error, Type = IssueType.Linguistic };

            var OkObjectTask = _controller.Post(_projectName, _screenName, _locale, _build, newIssue);

            OkObjectTask.Wait();

            var result = OkObjectTask.Result as CreatedAtRouteResult;

            Assert.IsNotNull(result);

            var issue = result.Value as IssueDto;

            Assert.IsNotNull(issue);

            Assert.AreEqual(_projectName, issue.ProjectName);
            Assert.AreEqual(_screenName, issue.ScreenName);
            Assert.AreEqual(_locale, issue.LocaleCode);
            Assert.AreEqual(_build, issue.BuildModified);
            Assert.AreEqual(newIssue.Height, issue.Height);
            Assert.AreEqual(newIssue.Width, issue.Width);
            Assert.AreEqual(newIssue.X, issue.X);
            Assert.AreEqual(newIssue.Y, issue.Y);
            Assert.AreEqual(newIssue.Type, issue.Type);
            Assert.AreEqual(newIssue.Severity, issue.Severity);
            Assert.AreEqual(newIssue.Identifier, issue.Identifier);
            Assert.AreEqual(newIssue.Text, issue.Text);

        }


        [TestMethod]
        public async Task CanUpdateIssueById()
        {
            _controller = new IssuesController(_context, _mapper);
            _controller.ControllerContext = TestInitializer.GetContext();

            string _projectName = "Velocity";
            string _screenName = "Install";
            string _locale = "pl-PL";
            string _build = "1.0";

            CreateIssueDto newIssue = new CreateIssueDto { Height = 10, Width = 10, X = 5, Y = 6, Text = "Test issue", Severity = IssueSeverity.Error, Type = IssueType.Linguistic };

            var OkObjectTask = _controller.Post(_projectName, _screenName, _locale, _build, newIssue);

            OkObjectTask.Wait();

            var result = OkObjectTask.Result as CreatedAtRouteResult;

            Assert.IsNotNull(result);

            var issue = result.Value as IssueDto;

            Assert.IsNotNull(issue);

            Assert.AreEqual(_projectName, issue.ProjectName);
            Assert.AreEqual(_screenName, issue.ScreenName);
            Assert.AreEqual(_locale, issue.LocaleCode);
            Assert.AreEqual(_build, issue.BuildModified);
            Assert.AreEqual(newIssue.Height, issue.Height);
            Assert.AreEqual(newIssue.Width, issue.Width);
            Assert.AreEqual(newIssue.X, issue.X);
            Assert.AreEqual(newIssue.Y, issue.Y);
            Assert.AreEqual(newIssue.Type, issue.Type);
            Assert.AreEqual(newIssue.Severity, issue.Severity);
            Assert.AreEqual(newIssue.Identifier, issue.Identifier);
            Assert.AreEqual(newIssue.Text, issue.Text);


            IssueDto updatedIssue = new IssueDto();

            //_mapper.CreateMap<IssueDto, IssueDto>();
            _mapper.Map<IssueDto, IssueDto>(issue, updatedIssue);

            updatedIssue.Text = "New text";


            var UpdatedOkObjectTask = _controller.Put(_projectName, updatedIssue.Id, updatedIssue);

            UpdatedOkObjectTask.Wait();

            var result2 = UpdatedOkObjectTask.Result as CreatedAtRouteResult;

            Assert.IsNotNull(result2);

            var issue2 = result2.Value as IssueDto;

            Assert.AreEqual(_projectName, issue2.ProjectName);
            Assert.AreEqual(_screenName, issue2.ScreenName);
            Assert.AreEqual(_locale, issue2.LocaleCode);
            Assert.AreEqual(_build, issue2.BuildModified);
            Assert.AreEqual(newIssue.Height, issue2.Height);
            Assert.AreEqual(newIssue.Width, issue2.Width);
            Assert.AreEqual(newIssue.X, issue2.X);
            Assert.AreEqual(newIssue.Y, issue2.Y);
            Assert.AreEqual(newIssue.Type, issue2.Type);
            Assert.AreEqual(newIssue.Severity, issue2.Severity);
            Assert.AreEqual(newIssue.Identifier, issue2.Identifier);
            Assert.AreEqual(updatedIssue.Text, issue2.Text);

        }


        [TestMethod]
        public async Task CanUpdateIssue()
        {
            _controller = new IssuesController(_context, _mapper);
            _controller.ControllerContext = TestInitializer.GetContext();

            string _projectName = "Velocity";
            string _screenName = "Install";
            string _locale = "pl-PL";
            string _build = "1.0";

            CreateIssueDto newIssue = new CreateIssueDto { Height = 10, Width = 10, X = 5, Y = 6, Text = "Test issue", Identifier = "ID1", Severity = IssueSeverity.Error, Type = IssueType.Linguistic, Status = IssueStatus.Active };

            var OkObjectTask = _controller.Put(_projectName, _screenName, _locale, _build, newIssue);

            OkObjectTask.Wait();

            var result = OkObjectTask.Result as CreatedAtRouteResult;

            Assert.IsNotNull(result);

            var issue = result.Value as IssueDto;

            Assert.IsNotNull(issue);

            Assert.AreEqual(_projectName, issue.ProjectName);
            Assert.AreEqual(_screenName, issue.ScreenName);
            Assert.AreEqual(_locale, issue.LocaleCode);
            Assert.AreEqual(_build, issue.BuildModified);
            Assert.AreEqual(newIssue.Height, issue.Height);
            Assert.AreEqual(newIssue.Width, issue.Width);
            Assert.AreEqual(newIssue.X, issue.X);
            Assert.AreEqual(newIssue.Y, issue.Y);
            Assert.AreEqual(newIssue.Type, issue.Type);
            Assert.AreEqual(newIssue.Severity, issue.Severity);
            Assert.AreEqual(newIssue.Identifier, issue.Identifier);
            Assert.AreEqual(newIssue.Text, issue.Text);


            IssueDto updatedIssue = new IssueDto();

            //_mapper.CreateMap<IssueDto, IssueDto>();
            _mapper.Map<IssueDto, IssueDto>(issue, updatedIssue);

            updatedIssue.Text = "New text";
            updatedIssue.X = issue.X + 5;
            updatedIssue.Y = issue.Y + 5;
            updatedIssue.Height = issue.Height + 5;
            updatedIssue.Width = issue.Width + 5;
            updatedIssue.Severity = IssueSeverity.Info;
            updatedIssue.Status = IssueStatus.Resolved;
            updatedIssue.Width = issue.Width + 5;


            var UpdatedOkObjectTask = _controller.Put(_projectName, _screenName, _locale, _build, updatedIssue);

            UpdatedOkObjectTask.Wait();

            var result2 = UpdatedOkObjectTask.Result as CreatedAtRouteResult;

            Assert.IsNotNull(result2);

            var issue2 = result2.Value as IssueDto;

            Assert.AreEqual(_projectName, issue2.ProjectName);
            Assert.AreEqual(_screenName, issue2.ScreenName);
            Assert.AreEqual(_locale, issue2.LocaleCode);
            Assert.AreEqual(_build, issue2.BuildModified);
            Assert.AreEqual(updatedIssue.Height, issue2.Height);
            Assert.AreEqual(updatedIssue.Width, issue2.Width);
            Assert.AreEqual(updatedIssue.X, issue2.X);
            Assert.AreEqual(updatedIssue.Y, issue2.Y);
            Assert.AreEqual(newIssue.Type, issue2.Type);
            Assert.AreEqual(updatedIssue.Severity, issue2.Severity);
            Assert.AreEqual(newIssue.Identifier, issue2.Identifier);
            Assert.AreEqual(updatedIssue.Text, issue2.Text);

            IssueDto addedIssue = new IssueDto();

            //_mapper.CreateMap<IssueDto, IssueDto>();
            _mapper.Map<IssueDto, IssueDto>(issue, addedIssue);

            addedIssue.Text = "New text!!!";
            addedIssue.X = issue.X + 5;
            addedIssue.Y = issue.Y + 5;
            addedIssue.Height = issue.Height + 5;
            addedIssue.Width = issue.Width + 5;
            addedIssue.Severity = IssueSeverity.Info;
            addedIssue.Status = IssueStatus.Resolved;
            addedIssue.Width = issue.Width + 5;
            addedIssue.Identifier = "ID4";
            addedIssue.Type = IssueType.Overlapping;


            var AddedOkObjectTask = _controller.Put(_projectName, _screenName, _locale, _build, addedIssue);

            AddedOkObjectTask.Wait();

            var result3 = AddedOkObjectTask.Result as CreatedAtRouteResult;

            Assert.IsNotNull(result3);

            var issue3 = result3.Value as IssueDto;

            Assert.AreNotEqual(issue.Id, issue3.Id);
            Assert.AreEqual(_projectName, issue3.ProjectName);
            Assert.AreEqual(_screenName, issue3.ScreenName);
            Assert.AreEqual(_locale, issue3.LocaleCode);
            Assert.AreEqual(_build, issue3.BuildModified);
            Assert.AreEqual(addedIssue.Height, issue3.Height);
            Assert.AreEqual(addedIssue.Width, issue3.Width);
            Assert.AreEqual(addedIssue.X, issue3.X);
            Assert.AreEqual(addedIssue.Y, issue3.Y);
            Assert.AreEqual(addedIssue.Type, issue3.Type);
            Assert.AreEqual(addedIssue.Severity, issue3.Severity);
            Assert.AreEqual(addedIssue.Identifier, issue3.Identifier);
            Assert.AreEqual(addedIssue.Text, issue3.Text);
        }


        [TestMethod]
        public async Task CanBringBackTheIssue()
        {
            _controller = new IssuesController(_context, _mapper);
            _controller.ControllerContext = TestInitializer.GetContext();

            string _projectName = "Velocity";
            string _screenName = "Install";
            string _locale = "pl-PL";

            //var result = await _controller.Get(_projectName) as OkObjectResult;
            var OkObjectTask = _controller.Get(_projectName, _screenName, _locale);

            OkObjectTask.Wait();

            var result = OkObjectTask.Result as OkObjectResult;

            Assert.IsNotNull(result);

            var issues = result.Value as ICollection<IssueDto>;

            Assert.IsNotNull(issues);

            Assert.IsTrue(issues.Count == 2);


            // update old issue

            string _build = "1.0";

            CreateIssueDto newIssue = new CreateIssueDto { Text = "C0rrpu4Ed!", Severity = IssueSeverity.Error, Type = IssueType.CharacterCorruption };

            var OkObjectTask2 = _controller.Post(_projectName, _screenName, _locale, _build, newIssue);

            OkObjectTask2.Wait();

            var result2 = OkObjectTask2.Result as CreatedAtRouteResult;

            Assert.IsNotNull(result2);



            // check if it is not visible

            var OkObjectTask3 = _controller.Get(_projectName, _screenName, _locale);

            OkObjectTask3.Wait();

            var result3 = OkObjectTask3.Result as OkObjectResult;

            Assert.IsNotNull(result3);

            var issues3 = result3.Value as ICollection<IssueDto>;

            Assert.IsNotNull(issues3);

            Assert.IsTrue(issues3.Count == 3);


        }
    }
}
