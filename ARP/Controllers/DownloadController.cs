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
using garb.Helpers;
using garb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace garb.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class DownloadController : Controller
    {
        private UnitOfWork _unitOfWork;
        //private GenericRepository<Project> _projectRepo;
		private StorageHelper _storageHelper;
		string leftBuild, rightBuild;

        public DownloadController(GarbContext context, IOptions<StorageHelper> storageHelper)
        {
            _unitOfWork = new UnitOfWork(context);
			_storageHelper = storageHelper.Value;
		}

		[ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string projectName, [FromQuery]Guid build1Id, [FromQuery]string locale1, [FromQuery]Guid build2Id, [FromQuery]string locale2, [FromQuery] string threshold, bool sliderIsDisabled)
        {
            ComparisonHelper comparisonHelper = new ComparisonHelper();
            IList<Comparison> comparisons = await comparisonHelper.GetComparisons(_unitOfWork, projectName, build1Id, locale1, build2Id, locale2);

            // Threshold is read as a percentage
            double dThreshold = Convert.ToDouble(threshold) / 100;

            List<ScreenInBuild> leftScreens = new List<ScreenInBuild>();
            List<ScreenInBuild> rightScreens = new List<ScreenInBuild>();
            //List<string> diffScreens = new List<string>();
            Dictionary<string, string> diffScreens = new Dictionary<string, string>();

            foreach (var screen in comparisons)
            {
                if (screen.SourceScreenInBuild != null || screen.TargetScreenInBuild != null)
                {
                    // If the slider is enabled, only add differences
                    if (!sliderIsDisabled)
                    {
                        if (dThreshold == 0)
                        {
                            if (screen.Difference > 0)
                            {
                                leftScreens.Add(screen.SourceScreenInBuild);
                                rightScreens.Add(screen.TargetScreenInBuild);
                                leftBuild = screen.SourceScreenInBuild.Build.BuildName;
                                rightBuild = screen.TargetScreenInBuild.Build.BuildName;

                                diffScreens.Add(StorageHelper.GetDiffImagePath(screen.SourceScreenInBuildId, screen.TargetScreenInBuildId), screen.SourceScreenInBuild.ScreenName);
                            }

                        }
                        else // Threshold is not 0
                        {
                            if (screen.Difference >= dThreshold)
                            {
                                leftScreens.Add(screen.SourceScreenInBuild);
                                rightScreens.Add(screen.TargetScreenInBuild);
                                leftBuild = screen.SourceScreenInBuild.Build.BuildName;
                                rightBuild = screen.TargetScreenInBuild.Build.BuildName;

                                diffScreens.Add(StorageHelper.GetDiffImagePath(screen.SourceScreenInBuildId, screen.TargetScreenInBuildId), screen.SourceScreenInBuild.ScreenName);
                            }
                        }
                    }
                    else // slider is disabled, so add everything
                    {
                        leftScreens.Add(screen.SourceScreenInBuild);
                        rightScreens.Add(screen.TargetScreenInBuild);
                        leftBuild = screen.SourceScreenInBuild.Build.BuildName;
                        rightBuild = screen.TargetScreenInBuild.Build.BuildName;

                        diffScreens.Add(StorageHelper.GetDiffImagePath(screen.SourceScreenInBuildId, screen.TargetScreenInBuildId), screen.SourceScreenInBuild.ScreenName);
                    }
                }
                else // There is no comparison, so screen.SourceScreenInBuild is null
                {
                    leftScreens = getAllScreens(projectName, locale1, build1Id);
                    rightScreens = getAllScreens(projectName, locale2, build2Id);
                    break;
                }
            }

            string tempFolder = createTempFolder();

            List<string> leftScreenPaths = copyScreens(leftScreens, tempFolder, false);
            List<string> rightScreenPaths = copyScreens(rightScreens, tempFolder, true);
            copyDiffImages(diffScreens, tempFolder, projectName);

            HtmlReportHelper.CreateHtmlFile(leftScreenPaths, projectName, leftBuild, rightBuild, locale1, locale2, tempFolder);

            string randomFileName = Path.GetRandomFileName();
            string zipFile = Path.Combine(Path.GetTempPath(), randomFileName + ".zip");
            if (!System.IO.File.Exists(zipFile))
            {
                ZipFile.CreateFromDirectory(tempFolder, zipFile, CompressionLevel.Optimal, false);
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(zipFile);
            string fileName = "screenshots.zip";

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        private List<ScreenInBuild> getAllScreens(string projectName, string locale, Guid buildID)
        {
            GenericRepository<ScreenInBuild> screenInBuildRepo = _unitOfWork.ScreenInBuildRepository;
            List<ScreenInBuild> screens = screenInBuildRepo.Get(s => s.ProjectName.Equals(projectName) &&
                s.LocaleCode.Equals(locale) &&
                s.BuildId == buildID, includeProperties: "Build").ToList();

            foreach (var screen in screens)
            {
                leftBuild = screen.Build.BuildName;
                rightBuild = leftBuild;
            }
                
            return screens;
        }

        private string createTempFolder()
        {
            // Create a random folder to store screens
            string tempFolder = Path.GetTempPath();
            string tempScreensFolder = Path.Combine(tempFolder, Path.GetRandomFileName());
            Directory.CreateDirectory(tempScreensFolder);

            return tempScreensFolder;
        }

        private List<string> copyScreens(List<ScreenInBuild> screens, string tempScreensFolder, bool copyEnglish)
        {
            List<string> screenPaths = new List<string>();

            foreach (ScreenInBuild sib in screens)
            {
                string path = StorageHelper.GetScreenPath(sib.ProjectName, sib.LocaleCode, sib.Build.BuildName, sib.ScreenName);
                string absPath = StorageHelper.GetScreenAbsPath(path);

                string targetPath = Path.Combine(tempScreensFolder, sib.ProjectName, sib.LocaleCode, sib.Build.BuildName);
                string targetFile = Path.Combine(targetPath, sib.ScreenName + ".png");

                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                if(copyEnglish)
                {
                    string engPath = StorageHelper.GetScreenPath(sib.ProjectName, "en-US", sib.Build.BuildName, sib.ScreenName);
                    string engAbsPath = StorageHelper.GetScreenAbsPath(engPath);
                    string engTargetPath = Path.Combine(tempScreensFolder, sib.ProjectName, "en-US", sib.Build.BuildName);
                    string engTargetFile = Path.Combine(engTargetPath, sib.ScreenName + ".png");

                    if (!Directory.Exists(engTargetPath)) {
                        Directory.CreateDirectory(engTargetPath);
                    }

                    if (!System.IO.File.Exists(engTargetFile)) {
                        System.IO.File.Copy(engAbsPath, engTargetFile, true);
                    }
                }
 
                System.IO.File.Copy(absPath, targetFile, true);

                screenPaths.Add(targetFile);
            }

            return screenPaths;
        }

        private void copyDiffImages(Dictionary<string, string> diffImages, string tempFolder, string projectName)
        {
            foreach (KeyValuePair<string, string> diffImage in diffImages) {
                string diffScreen = diffImage.Key;
                string correspondingScreen = diffImage.Value + ".png";

                string diffAbsImagePath = StorageHelper.GetScreenAbsPath(diffScreen);
                bool diffImageExists = ImageHelper.CheckImage(diffAbsImagePath);
                string targetPath = Path.Combine(tempFolder, projectName, "DIFF");

                if (diffImageExists)
                {
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }

                    System.IO.File.Copy(diffAbsImagePath, Path.Combine(targetPath, correspondingScreen));
                }

            }
        }


    }
}

