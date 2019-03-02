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
using garb.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace garb.Controllers
{
	[Authorize]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class LocalesController : Controller
    {
        private UnitOfWork _unitOfWork;
        private GenericRepository<Locale> _localeRepo;
        private GenericRepository<ScreenInBuild> _screenInBuildRepo;

        public LocalesController(GarbContext context)
        {
            _unitOfWork = new UnitOfWork(context);
            _localeRepo = _unitOfWork.LocaleRepository;
            _screenInBuildRepo = _unitOfWork.ScreenInBuildRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LocaleDto>), 200)]
        public async Task<IActionResult> Get(string project = null, string build = null)
        {      
          if (project == null && build == null)
          {
            return Ok((await _localeRepo.GetAsync()).Select(l => new LocaleDto(l)).ToList());
          }
          else if (project != null)
          {
              var predicate = PredicateBuilder.True<ScreenInBuild>();
              predicate = predicate.And(s => s.ProjectName == project);
              Guid buildId = Guid.Empty;
              Guid.TryParse(build, out buildId);
              if (buildId != Guid.Empty)
              {
                predicate = predicate.And(s => s.BuildId == buildId);
              }
              else if (build != null)
              {
                predicate = predicate.And(s => s.Build.BuildName == build);
              }
              var projectLocaleCodes = (await _screenInBuildRepo.GetAsync(predicate)).Select(s => s.LocaleCode).Distinct().ToList();
              var projectLocales = (await _localeRepo.GetAsync(l => projectLocaleCodes.Contains(l.LocaleCode))).Select(l => new LocaleDto(l)).ToList();
              return Ok(projectLocales);
          }
          else
          {
            return BadRequest();
          }        
        }

        // GET api/values/5
        [HttpGet("{code}", Name = "GetLocaleRoute")]
        [ProducesResponseType(typeof(LocaleDto), 200)]
        public async Task<IActionResult> Get(string code)
        {
          Locale locale = await _localeRepo.GetByIDAsync(code);

          if (locale == null)
            return NotFound();
          else
            return Ok(new LocaleDto(locale));
        }       

        // POST api/values
        [ProducesResponseType(typeof(LocaleDto), 201)]
        [ProducesResponseType(409)]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]LocaleDto locale)
        {
            if (!ModelState.IsValid || locale == null || string.IsNullOrWhiteSpace(locale.LocaleCode))
            {
                return BadRequest();
            }

            if (_localeRepo.Get(p => p.LocaleCode.Equals(locale.LocaleCode)).Count() != 0)
            {
                return StatusCode((int)HttpStatusCode.Conflict);
            }

            Locale newLocale = new Locale {  LocaleCode = locale.LocaleCode, LocaleName = locale.LocaleName };

            _localeRepo.Insert(newLocale);

            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException.Message);
            }

            return CreatedAtRoute(routeName: "GetLocaleRoute", routeValues: new { code = locale.LocaleCode }, value: new LocaleDto(newLocale));
        }
    }

}
