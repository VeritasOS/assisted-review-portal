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

using garb.Dto;
using garb.Data;
using garb.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using garb.Helpers;

namespace garb.Controllers
{
	/// <summary>
	/// Controller for managing Issues
	/// </summary>
	[Authorize]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class IssuesController : Controller
	{
		private UnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		/// <summary>
		/// Constructor for issues controller
		/// </summary>
		/// <param name="context"></param>
		/// <param name="mapper"></param>
		public IssuesController(GarbContext context, IMapper mapper)
		{
			_unitOfWork = new UnitOfWork(context);
			_mapper = mapper;
		}

		/// <summary>
		/// Get issue details
		/// </summary>
		/// <param name="project">Project name</param>
		/// <param name="id">Issue identifier</param>
		/// <returns>JSON with issue details</returns>
		[HttpGet("{project}/{id}", Name = "GetIssueRoute")]
		[ProducesResponseType(typeof(IssueDto), 200)]
		public async Task<IActionResult> Get(string project, Guid id)
		{
			if (!ModelState.IsValid || string.IsNullOrWhiteSpace(project) || id != null)
			{
				return BadRequest("Invalid or missing parameters");
			}

			GenericRepository<Issue> issueRepo = _unitOfWork.IssueRepository;

			Issue issue = await issueRepo.GetByIDAsync(new[] { id });

			IssueDto issueDto = new IssueDto();
			_mapper.Map<Issue, IssueDto>(issue, issueDto);

			return Ok(issueDto);
		}

		/// <summary>
		/// Get all issues assigned to selected screen
		/// </summary>
		/// <param name="project">Project name</param>
		/// <param name="name">Screen name</param>
		/// <param name="locale">Locale code</param>
		/// <param name="onlyactive">true: Return issues only in active state (default), false: all issues</param>
		/// <returns>List of issues</returns>
		[HttpGet("{project}/{name}/{locale}")]
		[ProducesResponseType(typeof(IEnumerable<IssueDto>), 200)]
		public async Task<IActionResult> Get(string project, string name, string locale, [FromQuery]bool onlyactive = true)
		{
			if (!ModelState.IsValid || string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(locale)) {
				return BadRequest("Invalid or missing parameters");
			}

			GenericRepository<Build> buildRepo = _unitOfWork.BuildRepository;

			Build build = await BuildHelper.GetLatestBuild(buildRepo, project);

			Guid buildId = (build == null ? new Guid() : build.Id);
			

			GenericRepository<Issue> issueRepo = _unitOfWork.IssueRepository;

			List<IssueDto> issues = new List<IssueDto>();

			foreach (Issue issue in await issueRepo.GetAsync(i =>
				i.ProjectName.Equals(project) &&
				i.LocaleCode.Equals(locale) &&
				i.ScreenName.Equals(name) &&
				i.ModifiedInBuildId == buildId && 
				(!onlyactive || i.IssueStatus == IssueStatus.Active),
				includeProperties: "Build")) {
				IssueDto issueDto = new IssueDto();
				_mapper.Map<Issue, IssueDto>(issue, issueDto);

				issues.Add(issueDto);
			}

			return Ok(issues);
		}


		/// <summary>
		/// Update Issue details based on issue identifier
		/// </summary>
		/// <param name="project">Project name</param>
		/// <param name="id">Issue id</param>
		/// <param name="issueData">JSON containg updated issue</param>
		/// <returns>Paht to issue details</returns>
		[HttpPut("{project}/{id}")]
		[ProducesResponseType(typeof(IssueDto), 200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> Put(string project, Guid id, [FromBody]IssueDto issueData)
		{

			Guid _buildId;

			Build build;

			GenericRepository<Build> buildRepo = _unitOfWork.BuildRepository;


			if (Guid.TryParse(issueData.BuildModified, out _buildId))
			{
				build = await buildRepo.GetByIDAsync(_buildId);
			}
			else
			{
				build = (await buildRepo.GetAsync(b => b.BuildName.Equals(issueData.BuildModified))).FirstOrDefault();
			}

			string currentUser = User.Identity.Name;

			if (!ModelState.IsValid || build.Id == null || issueData == null || id != issueData.Id)
			{
				return BadRequest("Invalid or missing parameters");
			}


			GenericRepository<Issue> issueRepo = _unitOfWork.IssueRepository;

			Issue issue = (await issueRepo.GetAsync(i => i.IssueId == issueData.Id, includeProperties: "Build")).FirstOrDefault();

			if (issue == null)
			{
				return BadRequest($"Issue with ID {build.Id} doesn't exist!");
			}

			_mapper.Map<IssueDto, Issue>(issueData, issue);

			issue.Build = build;
			issue.ModifiedInBuildId = build.Id;

			await _unitOfWork.SaveAsync(currentUser);

			IssueDto updatedIssueDto = new IssueDto();
			_mapper.Map<Issue, IssueDto>(issue, updatedIssueDto);

			return CreatedAtRoute(routeName: "GetIssueRoute", routeValues: new { project = project, id = updatedIssueDto.Id }, value: updatedIssueDto);

		}

		/// <summary>
		/// Add issue to screen
		/// </summary>
		/// <param name="project">Project name</param>
		/// <param name="name">Screen name</param>
		/// <param name="locale">Locale code</param>
		/// <param name="build">Build name or identifier</param>
		/// <param name="issueData">Issue to add (JSON)</param>
		/// <returns>Path to issue details</returns>
		[HttpPost("{project}/{name}/{locale}/{build}")]
		[ProducesResponseType(typeof(IssueDto), 201)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> Post(string project, string name, string locale, string build, [FromBody]CreateIssueDto issueData)
		{
			//return StatusCode((int)HttpStatusCode.Conflict);

			Guid buildId;

			if (!Guid.TryParse(build, out buildId))
			{
				Build buildEntity = null;

				GenericRepository<Build> buildRepo = _unitOfWork.BuildRepository;
				buildEntity = (await buildRepo.GetAsync(b => b.BuildName.Equals(build))).FirstOrDefault();

				if (buildEntity != null)
					buildId = buildEntity.Id;
			}

			string currentUser = User.Identity.Name;


			if (!ModelState.IsValid || buildId == null || string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(locale) || issueData == null)
			{
				return BadRequest("Invalid or missing parameters");
			}

			GenericRepository<Issue> issueRepo = _unitOfWork.IssueRepository;

			Issue existingIssue = IssueHelper.GetExistingIssue(issueRepo, project, name, locale, issueData);

			IssueDto createdIssue = new IssueDto();

			if (existingIssue != null)
			{

				if (existingIssue.IssueStatus != issueData.Status ||
				existingIssue.IssueSeverity != issueData.Severity ||
				existingIssue.Value != issueData.Text ||
				existingIssue.Height != issueData.Height ||
				existingIssue.Width != issueData.Width ||
				existingIssue.X != issueData.X ||
				existingIssue.Y != issueData.Y)
				{
					existingIssue.IssueStatus = issueData.Status;
					existingIssue.IssueSeverity = issueData.Severity;
					existingIssue.Value = issueData.Text;
					existingIssue.Height = issueData.Height;
					existingIssue.Width = issueData.Width;
					existingIssue.X = issueData.X;
					existingIssue.Y = issueData.Y;
					existingIssue.ModifiedInBuildId = buildId;
					issueRepo.Update(existingIssue);
					await _unitOfWork.SaveAsync(currentUser);
					_mapper.Map<Issue, IssueDto>(existingIssue, createdIssue);
				}
				else
				{
					return StatusCode((int)HttpStatusCode.NoContent);
				}

			}
			else
			{
				Issue newIssue = new Issue()
				{
					ProjectName = project,
					ScreenName = name,
					ModifiedInBuildId = buildId,
					LocaleCode = locale,
					IssueType = issueData.Type,
					IssueSeverity = issueData.Severity,
					IssueStatus = issueData.Status,
					Identifier = issueData.Identifier,
					Value = issueData.Text,
					X = issueData.X,
					Y = issueData.Y,
					Height = issueData.Height,
					Width = issueData.Width
				};

				try
				{

					issueRepo.Insert(newIssue);
					await _unitOfWork.SaveAsync(currentUser);
				}
				catch (Exception ex)
				{
					string msg = ex.Message;
				}

				_mapper.Map<Issue, IssueDto>(newIssue, createdIssue);
			}

			return CreatedAtRoute(routeName: "GetIssueRoute", routeValues: new { project = project, id = createdIssue.Id }, value: createdIssue);
		}


		/// <summary>
		/// Update or create an issue
		/// </summary>
		/// <param name="project">Project name</param>
		/// <param name="name">Screen name</param>
		/// <param name="locale">Locale code</param>
		/// <param name="build">Build name or identifier</param>
		/// <param name="issueData">Issue to add or update (JSON)</param>
		/// <returns>Path to issue details</returns>
		[HttpPut("{project}/{name}/{locale}/{build}")]
		[ProducesResponseType(typeof(IssueDto), 201)]
		[ProducesResponseType(409)]
		public async Task<IActionResult> Put(string project, string name, string locale, string build, [FromBody]CreateIssueDto issueData)
		{
			Guid buildId;

			if (!Guid.TryParse(build, out buildId))
			{
				GenericRepository<Build> buildRepo = _unitOfWork.BuildRepository;
				buildId = (await buildRepo.GetAsync(b => b.BuildName.Equals(build))).FirstOrDefault().Id;
			}

			string currentUser = User.Identity.Name;

			if (!ModelState.IsValid || buildId == null || string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(locale) || issueData == null)
			{
				return BadRequest("Invalid or missing parameters");
			}

			GenericRepository<Issue> issueRepo = _unitOfWork.IssueRepository;

			Issue existingIssue = IssueHelper.GetExistingIssue(issueRepo, project, name, locale, issueData);

			IssueDto createdIssue = new IssueDto();

			if (existingIssue != null)
			{
				existingIssue.X = issueData.X;
				existingIssue.Y = issueData.Y;
				existingIssue.Width = issueData.Width;
				existingIssue.Height = issueData.Height;
				existingIssue.IssueStatus = issueData.Status;
				existingIssue.IssueSeverity = issueData.Severity;
				existingIssue.Value = issueData.Text;

				await _unitOfWork.SaveAsync(currentUser);
				_mapper.Map<Issue, IssueDto>(existingIssue, createdIssue);
			}
			else
			{
				Issue newIssue = new Issue()
				{
					ProjectName = project,
					ScreenName = name,
					ModifiedInBuildId = buildId,
					LocaleCode = locale,
					IssueType = issueData.Type,
					IssueSeverity = issueData.Severity,
					Identifier = issueData.Identifier,
					Value = issueData.Text,
					X = issueData.X,
					Y = issueData.Y,
					Height = issueData.Height,
					Width = issueData.Width
				};

				issueRepo.Insert(newIssue);

				await _unitOfWork.SaveAsync(currentUser);
				_mapper.Map<Issue, IssueDto>(newIssue, createdIssue);
			}



			return CreatedAtRoute(routeName: "GetIssueRoute", routeValues: new { project = project, id = createdIssue.Id }, value: createdIssue);
		}

	}
}
