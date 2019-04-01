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
using garb.Helpers;
using garb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace garb.Controllers
{
	[Authorize]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class ScreensController : Controller
    {
        private UnitOfWork _unitOfWork;
        private StorageHelper _storageHelper;
		private readonly ILogger<ScreensController> _logger;

		public ScreensController(GarbContext context, IOptions<StorageHelper> storageHelper, ILogger<ScreensController> logger)
        {
            _unitOfWork = new UnitOfWork(context);
            _storageHelper = storageHelper.Value;
			_logger = logger;
        }

        [HttpGet("{project}")]
        [ProducesResponseType(typeof(IEnumerable<ScreenDto>), 200)]
        public async Task<IActionResult> Get(string project, string locale, string name, Guid? build)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(project))
            {
                return BadRequest("Invalid or missing parameters");
            }

            GenericRepository<ScreenInBuild> screenInBuildRepo = _unitOfWork.ScreenInBuildRepository;

            List<ScreenDto> screens = new List<ScreenDto>();

            foreach (ScreenInBuild sib in await screenInBuildRepo.GetAsync(s => s.ProjectName.Equals(project) &&
            (string.IsNullOrWhiteSpace(locale) || s.LocaleCode.Equals(locale)) &&
            (string.IsNullOrWhiteSpace(name) || s.ScreenName.Equals(name)) &&
            (build == null || s.BuildId == build), includeProperties: "Build"))
            {
                screens.Add(new ScreenDto(sib));
            }

            return Ok(screens);
        }

        // GET api/values/5
        [HttpGet("{project}/{id}", Name = "GetScreenRoute")]
        [ProducesResponseType(typeof(ScreenDto), 200)]
        public async Task<IActionResult> Get(string project, Guid id)
        {
            if (!ModelState.IsValid || id == null || string.IsNullOrWhiteSpace(project))
            {
                return BadRequest("Invalid or missing parameters");
            }

            GenericRepository<ScreenInBuild> screenInBuildRepo = _unitOfWork.ScreenInBuildRepository;

            ScreenInBuild sib = (await screenInBuildRepo.GetAsync(s => s.ScreenInBuildId == id && s.ProjectName.Equals(project), includeProperties: "Build")).FirstOrDefault();

            if (sib == null)
            {
                return NotFound();
            }

            ScreenDto screen = new ScreenDto(sib);

            return Ok(screen);
        }

        // POST api/values
        [HttpPost("{project}/{name}/{locale}/{build}")]
        [ProducesResponseType(typeof(ScreenDto), 201)]
		[ProducesResponseType(204)]
		[ProducesResponseType(409)]
        public async Task<IActionResult> Post(string project, string name, string locale, string build, [FromBody]CreateScreenDto screenData)
        {
			GenericRepository<Build> buildRepo = _unitOfWork.BuildRepository;
			Build buildEntity;

			string currentUser = User.Identity.Name;

            // Workaround for local readonly user
            if (!Authentication.LdapHelper.CanWrite(currentUser))
            {
                return Unauthorized();
            }

            try
			{
				Guid buildId;

				if (!Guid.TryParse(build, out buildId))
				{
					buildEntity = (await buildRepo.GetAsync(b => b.BuildName.Equals(build))).FirstOrDefault();
				}
				else
				{
					buildEntity = buildRepo.GetByID(buildId);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in screens controller");
				return BadRequest();
			}


			if (buildEntity == null)
				return BadRequest($"Build with id {build} doesn't exist!");


			if (!ModelState.IsValid || buildEntity == null || string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(locale) || screenData == null)
            {
                return BadRequest("Invalid or missing parameters");
            }

            GenericRepository<Project> projectRepo = _unitOfWork.ProjectRepository;
            Project projectEntity = projectRepo.GetByID(project);

            if (projectEntity == null)
                return BadRequest($"Project {project} doesn't exist!");

            GenericRepository<Locale> localeRepo = _unitOfWork.LocaleRepository;

            if (localeRepo.GetByID(locale) == null)
                return BadRequest($"Unknown locale {locale}!");

            GenericRepository<Screen> screenRepo = _unitOfWork.ScreenRepository;
            if (screenRepo.GetByID(name, project) == null)
            {
                Screen screen = new Screen() { Project = projectEntity, ScreenName = name };
                screenRepo.Insert(screen);
                await _unitOfWork.SaveAsync(currentUser);
            }

            GenericRepository<ScreenInBuild> screenInBuildRepo = _unitOfWork.ScreenInBuildRepository;

			var existingScreen = screenInBuildRepo.Get(sr => sr.BuildId == buildEntity.Id && sr.ProjectName.Equals(project) && sr.LocaleCode.Equals(locale) && sr.ScreenName.Equals(name)).FirstOrDefault();

			bool missingFile = false;

			if (existingScreen != null)
			{
				string existingScreenLocalPath = StorageHelper.GetScreenAbsPath(StorageHelper.GetScreenPath(project, locale, buildEntity.BuildName, name));

				if (System.IO.File.Exists(existingScreenLocalPath))
				{

					byte[] existingContent = System.IO.File.ReadAllBytes(existingScreenLocalPath);

					if (!screenData.Content.SequenceEqual(existingContent))
						return StatusCode((int)HttpStatusCode.Conflict);
					else
						return StatusCode((int)HttpStatusCode.NoContent);
				}
				else
				{
					missingFile = true;
				}
			}

			string screenLocalPath = _storageHelper.StoreScreen(project, locale, buildEntity.BuildName, name, screenData.Content);

			ScreenDto newScreen;

			if (!missingFile)
			{
				ScreenInBuild newScreenInBuild = new ScreenInBuild() { ProjectName = project, ScreenName = name, BuildId = buildEntity.Id, LocaleCode = locale };
				screenInBuildRepo.Insert(newScreenInBuild);
				await _unitOfWork.SaveAsync(currentUser);

	            newScreen = new ScreenDto(newScreenInBuild);
			}
			else
			{
				newScreen = new ScreenDto(existingScreen);
			}

			return CreatedAtRoute(routeName: "GetScreenRoute", routeValues: new { project = project, id = newScreen.Id }, value: newScreen);
        }

    }
}
