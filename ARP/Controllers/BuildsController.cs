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
    public class BuildsController : Controller
    {
        private UnitOfWork _unitOfWork;
        private GenericRepository<Build> _buildRepo;

        public BuildsController(GarbContext context)
        {
            _unitOfWork = new UnitOfWork(context);
            _buildRepo = _unitOfWork.BuildRepository;
        }

        [HttpGet("{project}")]
        [ProducesResponseType(typeof(ICollection<BuildDto>), 200)]
        public async Task<IActionResult> Get(string project)
        {
            return Ok((await _buildRepo.GetAsync(b => b.ProjectName.Equals(project))).Select(b => new BuildDto(b)).ToList());
        }

        // GET api/values/5
        [HttpGet("{project}/{id}", Name = "GetBuildRoute")]
        [ProducesResponseType(typeof(BuildDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(string project, Guid id)
        {
            Build build = await _buildRepo.GetByIDAsync(id);

            if (build == null || !build.ProjectName.Equals(project))
                return NotFound();
            else
                return Ok(new BuildDto(build));
        }

        // POST api/values
        [HttpPost("{project}")]
        [ProducesResponseType(typeof(BuildDto), 201)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Post(string project, [FromBody]CreateBuildDto build)
        {
			string currentUser = User.Identity.Name;

			if (!ModelState.IsValid || string.IsNullOrWhiteSpace(project) || build == null)
            {
                return BadRequest();
            }

            if (_buildRepo.Get(b => b.BuildName.Equals(build.BuildName) && b.ProjectName.Equals(project)).Count() != 0)
            {
                return StatusCode((int)HttpStatusCode.Conflict);
            }

            Build newBuild = new Build { ProjectName = project, BuildName = build.BuildName, Status = BuildStatus.Unknown };

            _buildRepo.Insert(newBuild);

            try
            {
                await _unitOfWork.SaveAsync(currentUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException.Message);
            }

            return CreatedAtRoute(routeName: "GetBuildRoute", routeValues: new { project = project, id = newBuild.Id }, value: new BuildDto(newBuild));
        }

    }
}
