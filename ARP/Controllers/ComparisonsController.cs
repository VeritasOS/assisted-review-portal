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
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace garb.Controllers
{
	[Authorize]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class ComparisonsController : Controller
	{
		private UnitOfWork _unitOfWork;
		private GenericRepository<Build> _buildRepo;
		private GenericRepository<ScreenInBuild> _screenInBuildRepo;
		private GenericRepository<Comparison> _comparisonRepo;
		private readonly ILogger<ComparisonsController> _logger;

		public ComparisonsController(ILogger<ComparisonsController> logger, GarbContext context)
		{
			_logger = logger;
			_unitOfWork = new UnitOfWork(context);
			_buildRepo = _unitOfWork.BuildRepository;
			_screenInBuildRepo = _unitOfWork.ScreenInBuildRepository;
			_comparisonRepo = _unitOfWork.ComparisonRepository;
		}

		// GET api/values/5
		[HttpGet("{project}")]
		[ProducesResponseType(typeof(ComparisonDto), 200)]
		[ProducesResponseType(404)]
		[ProducesResponseType(500)]
		public async Task<IActionResult> Get(string project, [FromQuery]Guid screen1Id, [FromQuery]Guid screen2Id)
		{
			//Build buildFrom = await _buildRepo.GetByIDAsync(screen1Id);
			//Build buildTo = await _buildRepo.GetByIDAsync(screen2Id);
			_logger.LogInformation(string.Format("screen1Id:{0},screen2Id:{1}", screen1Id.ToString(), screen2Id.ToString()));

			//if (buildFrom == null || buildTo == null)
			//    return NotFound($"Invalid build [{screen1Id}] or [{screen2Id}]");

			ScreenInBuild screenFrom = (await _screenInBuildRepo.GetAsync(s => s.ScreenInBuildId == screen1Id, includeProperties: "Build")).FirstOrDefault();
			ScreenInBuild screenTo = (await _screenInBuildRepo.GetAsync(s => s.ScreenInBuildId == screen2Id, includeProperties: "Build")).FirstOrDefault();

			if (screenFrom == null || screenTo == null)
			{
				return NotFound($"Cannot find build screen [{screen1Id}] or [{screen2Id}]!");
			}
			else if (screenFrom.ScreenInBuildId == screenTo.ScreenInBuildId)
			{
				return Ok(new ComparisonDto() { Difference = 0, DiffImagePath = null, SourceScreenInBuildId = screenFrom.ScreenInBuildId, TargetScreenInBuildId = screenTo.ScreenInBuildId, SourceScreenName = screenFrom.ScreenName, TargetScreenName = screenTo.ScreenName });
			}
			else
			{
				Comparison comparison = (await _comparisonRepo.GetAsync(c => c.SourceScreenInBuildId == screenFrom.ScreenInBuildId && c.TargetScreenInBuildId == screenTo.ScreenInBuildId, includeProperties: "SourceScreenInBuild,TargetScreenInBuild")).FirstOrDefault();

				string diffImagePath = StorageHelper.GetDiffImagePath(screenFrom.ScreenInBuildId, screenTo.ScreenInBuildId);
				string diffAbsImagePath = StorageHelper.GetScreenAbsPath(diffImagePath);
				bool diffImageExists = ImageHelper.CheckImage(diffAbsImagePath);

				// reverse comparison paths
				string revDiffAbsImagePath = StorageHelper.GetScreenAbsPath(StorageHelper.GetDiffImagePath(screenTo.ScreenInBuildId, screenFrom.ScreenInBuildId));

				if (comparison != null && diffImageExists)
				{
					return Ok(new ComparisonDto(comparison));
				}
				else
				{
					Comparison newComparison = new Comparison();
					newComparison.SourceScreenInBuildId = screenFrom.ScreenInBuildId;
					newComparison.TargetScreenInBuildId = screenTo.ScreenInBuildId;

					string screenAbsFromPath = StorageHelper.GetScreenAbsPath(StorageHelper.GetScreenPath(project, screenFrom.LocaleCode, screenFrom.Build.BuildName, screenFrom.ScreenName));
					string screenAbsToPath = StorageHelper.GetScreenAbsPath(StorageHelper.GetScreenPath(project, screenTo.LocaleCode, screenTo.Build.BuildName, screenTo.ScreenName));
					bool diffImageFolderCreated = StorageHelper.GenerateDiffImageFolder(Path.GetDirectoryName(diffAbsImagePath));
					bool srcImageExists = ImageHelper.CheckImage(screenAbsFromPath) && ImageHelper.CheckImage(screenAbsToPath);

					if (diffImageFolderCreated && srcImageExists)
					{
						newComparison.Difference = ImageHelper.CompareImages(screenAbsFromPath, screenAbsToPath, diffAbsImagePath);

						//copy diff image for reverse comparison
						if (System.IO.File.Exists(diffAbsImagePath))
							System.IO.File.Copy(diffAbsImagePath, revDiffAbsImagePath);

						if (comparison == null)
						{
							_comparisonRepo.Insert(newComparison);

							//add reverse comparison to the database
							Comparison revComparison = new Comparison() { Difference = newComparison.Difference, SourceScreenInBuildId = newComparison.TargetScreenInBuildId, TargetScreenInBuildId = newComparison.SourceScreenInBuildId };
							_comparisonRepo.Insert(revComparison);
						}
						else
						{
							comparison.Difference = newComparison.Difference;
						}
						await _unitOfWork.SaveAsync();
						return Ok(new ComparisonDto(newComparison));
					}
					else
					{
						string errorMessage = "Error comparing images!";

						if (!srcImageExists)
							errorMessage += "\nscreenFromPath or screenToPath doesn't exists.";

						if (!diffImageFolderCreated)
							errorMessage += "\nCreate diff image folder failure.";

						_logger.LogError(errorMessage);
						return StatusCode((int)HttpStatusCode.InternalServerError, errorMessage);
					}
				}
			}
		}


		[HttpGet("{project}/List")]
		[ProducesResponseType(typeof(IEnumerable<ComparisonDto>), 200)]
		[ProducesResponseType(404)]
		[ProducesResponseType(500)]
		public async Task<IActionResult> Get(string project, [FromQuery]Guid build1Id, [FromQuery]string locale1, [FromQuery]Guid build2Id, [FromQuery]string locale2)
		{
			//string.IsNullOrWhiteSpace(locale1)
			//build1Id == null

			List<ComparisonDto> comparisonsList = new List<ComparisonDto>();

			GenericRepository<ScreenInBuild> screenInBuildRepo = _unitOfWork.ScreenInBuildRepository;

			List<ScreenInBuild> sourceScreens = (await screenInBuildRepo.GetAsync(s => s.ProjectName.Equals(project) &&
			s.LocaleCode.Equals(locale1) &&
			s.BuildId == build1Id,
			includeProperties: "Build")).ToList();

			if (build1Id.Equals(build2Id) && locale1.Equals(locale2))
			{
				foreach (ScreenInBuild sourceScreen in sourceScreens) {
					comparisonsList.Add(new ComparisonDto() {
						SourceScreenInBuildId = sourceScreen.ScreenInBuildId,
						SourceScreenName = sourceScreen.ScreenName,
						TargetScreenInBuildId = sourceScreen.ScreenInBuildId,
						TargetScreenName = sourceScreen.ScreenName,
						Difference = 0,
						DiffImagePath = null
					});
				}

				return Ok(comparisonsList);
			}


			List<ScreenInBuild> targetScreens = (await screenInBuildRepo.GetAsync(s => s.ProjectName.Equals(project) &&
			s.LocaleCode.Equals(locale2) &&
			s.BuildId == build2Id,
			includeProperties: "Build")).ToList();

			HashSet<Guid> sourceBuildIds = new HashSet<Guid>(sourceScreens.Select(s => s.ScreenInBuildId).ToList());

			Dictionary<Guid, List<Comparison>> comparisonsDict = (await _comparisonRepo.GetAsync(c => sourceBuildIds.Contains(c.SourceScreenInBuildId), includeProperties: "SourceScreenInBuild,TargetScreenInBuild")).GroupBy(c => c.SourceScreenInBuildId).ToDictionary(c => c.Key, c => c.ToList());

			foreach (ScreenInBuild sourceScreen in sourceScreens)
			{
				ScreenInBuild targetScreen = targetScreens.FirstOrDefault(s => s.ScreenName.Equals(sourceScreen.ScreenName));

				if (targetScreen != null)
				{
					//Comparison comparison = (await _comparisonRepo.GetAsync(c => c.SourceScreenInBuildId == sourceScreen.ScreenInBuildId && c.TargetScreenInBuildId == targetScreen.ScreenInBuildId, includeProperties: "SourceScreenInBuild,TargetScreenInBuild")).FirstOrDefault();

					Comparison comparison = null;

					if (comparisonsDict.ContainsKey(sourceScreen.ScreenInBuildId))
						comparison = comparisonsDict[sourceScreen.ScreenInBuildId].FirstOrDefault(c => c.TargetScreenInBuildId == targetScreen.ScreenInBuildId);

					string diffImagePath = StorageHelper.GetDiffImagePath(sourceScreen.ScreenInBuildId, targetScreen.ScreenInBuildId);
					string diffAbsImagePath = StorageHelper.GetScreenAbsPath(diffImagePath);
					bool diffImageExists = ImageHelper.CheckImage(diffAbsImagePath);

					// reverse comparison paths
					string revDiffAbsImagePath = StorageHelper.GetScreenAbsPath(StorageHelper.GetDiffImagePath(targetScreen.ScreenInBuildId, sourceScreen.ScreenInBuildId));

					if (comparison != null)
					{
						comparisonsList.Add(new ComparisonDto(comparison));
					}
					else
					{
						Comparison newComparison = new Comparison();
						newComparison.SourceScreenInBuildId = sourceScreen.ScreenInBuildId;
						newComparison.TargetScreenInBuildId = targetScreen.ScreenInBuildId;

						string screenAbsFromPath = StorageHelper.GetScreenAbsPath(StorageHelper.GetScreenPath(project, sourceScreen.LocaleCode, sourceScreen.Build.BuildName, sourceScreen.ScreenName));
						string screenAbsToPath = StorageHelper.GetScreenAbsPath(StorageHelper.GetScreenPath(project, targetScreen.LocaleCode, targetScreen.Build.BuildName, targetScreen.ScreenName));
						bool diffImageFolderCreated = StorageHelper.GenerateDiffImageFolder(Path.GetDirectoryName(diffAbsImagePath));
						bool srcImageExists = ImageHelper.CheckImage(screenAbsFromPath) && ImageHelper.CheckImage(screenAbsToPath);

						if (diffImageFolderCreated && srcImageExists)
						{
							newComparison.Difference = ImageHelper.CompareImages(screenAbsFromPath, screenAbsToPath, diffAbsImagePath);

							_comparisonRepo.Insert(newComparison);

							//copy diff image for reverse comparison
							if (System.IO.File.Exists(diffAbsImagePath))
								System.IO.File.Copy(diffAbsImagePath, revDiffAbsImagePath);

							//add reverse comparison to the database
							Comparison revComparison = new Comparison() { Difference = newComparison.Difference, SourceScreenInBuildId = newComparison.TargetScreenInBuildId, TargetScreenInBuildId = newComparison.SourceScreenInBuildId };
							_comparisonRepo.Insert(revComparison);

							await _unitOfWork.SaveAsync();

							comparisonsList.Add(
								new ComparisonDto()
								{
									SourceScreenInBuildId = sourceScreen.ScreenInBuildId,
									SourceScreenName = sourceScreen.ScreenName,
									TargetScreenInBuildId = targetScreen.ScreenInBuildId,
									TargetScreenName = targetScreen.ScreenName,
									Difference = newComparison.Difference,
									DiffImagePath = StorageHelper.GetDiffImagePath(sourceScreen.ScreenInBuildId, targetScreen.ScreenInBuildId, '/')
								});

						}
						else
						{
							string errorMessage = "Error comparing images!";

							if (!srcImageExists)
								errorMessage += "\nscreenFromPath or screenToPath doesn't exists.";

							if (!diffImageFolderCreated)
								errorMessage += "\nCreate diff image folder failure.";

							_logger.LogError(errorMessage);
						}
					}

				}
				else
				{
					comparisonsList.Add(new ComparisonDto()
					{
						SourceScreenInBuildId = sourceScreen.ScreenInBuildId,
						SourceScreenName = sourceScreen.ScreenName,
						Difference = 1
					});

				}
			}

			return Ok(comparisonsList);
		}
	}
}