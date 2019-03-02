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
using Microsoft.EntityFrameworkCore;
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
	public class ProjectsController : Controller
    {
        private UnitOfWork _unitOfWork;
        private GenericRepository<Project> _projectRepo;

        public ProjectsController(GarbContext context)
        {
            _unitOfWork = new UnitOfWork(context);
            _projectRepo = _unitOfWork.ProjectRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProjectDto>), 200)]
        public async Task<IActionResult> Get()
        {
            return Ok((await _projectRepo.GetAsync()).Select(p => new ProjectDto(p)).ToList());
        }

        // GET api/values/5
        [HttpGet("{name}", Name = "GetProjectRoute")]
        [ProducesResponseType(typeof(ProjectDto), 200)]
        public async Task<IActionResult> Get(string name)
        {
            Project project = await _projectRepo.GetByIDAsync(name);

            if (project == null)
                return NotFound();
            else
                return Ok(new ProjectDto(project));
        }

        // POST api/values
        [ProducesResponseType(typeof(ProjectDto), 201)]
        [ProducesResponseType(409)]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]ProjectDto project)
        {
            if (!ModelState.IsValid || project == null || string.IsNullOrWhiteSpace(project.ProjectName))
            {
                return BadRequest();
            }

            if (_projectRepo.Get(p => p.ProjectName.Equals(project.ProjectName)).Count() != 0)
            {
                return StatusCode((int)HttpStatusCode.Conflict);
            }

            Project newProject = new Project { ProjectName = project.ProjectName };

            _projectRepo.Insert(newProject);

            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException.Message);
            }

            return CreatedAtRoute(routeName: "GetProjectRoute", routeValues: new { name = project.ProjectName }, value: new ProjectDto() { ProjectName = project.ProjectName } );
        }
    }

}
