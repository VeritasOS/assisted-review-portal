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
using garb.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace garb.Helpers
{
    /// <summary>
    /// Helper for comparison functionality
    /// </summary>
    public class ComparisonHelper
    {

        /// <summary>
        /// Get list of comparisons
        /// </summary>
        /// <param name="unitOfWork">Unit of work</param>
        /// <param name="project">Project name</param>
        /// <param name="build1Id">ID of the "from" build</param>
        /// <param name="locale1">Locale of the "from" build</param>
        /// <param name="build2Id">ID of the "to" build</param>
        /// <param name="locale2">Locale of the "to" build</param>
        /// <returns></returns>
        public async Task<IList<Comparison>> GetComparisons(UnitOfWork unitOfWork, string project, Guid build1Id, string locale1, Guid build2Id, string locale2)
        {
            IList<Comparison> comparisons = new List<Comparison>();

            GenericRepository<Build> buildRepo = unitOfWork.BuildRepository;
            GenericRepository<Comparison> comparisonRepo = unitOfWork.ComparisonRepository;
            GenericRepository<ScreenInBuild> screenInBuildRepo = unitOfWork.ScreenInBuildRepository;

            List<ScreenInBuild> sourceScreens = (await screenInBuildRepo.GetAsync(s => s.ProjectName.Equals(project) &&
            s.LocaleCode.Equals(locale1) &&
            s.BuildId == build1Id,
            includeProperties: "Build")).ToList();

            if (build1Id.Equals(build2Id) && locale1.Equals(locale2))
            {
                foreach (ScreenInBuild sourceScreen in sourceScreens)
                {
                    comparisons.Add(new Comparison()
                    {
                        SourceScreenInBuildId = sourceScreen.ScreenInBuildId,
                        TargetScreenInBuildId = sourceScreen.ScreenInBuildId,
                        Difference = 0
                    });
                }

                return comparisons;
            }


            List<ScreenInBuild> targetScreens = (await screenInBuildRepo.GetAsync(s => s.ProjectName.Equals(project) &&
            s.LocaleCode.Equals(locale2) &&
            s.BuildId == build2Id,
            includeProperties: "Build")).ToList();

			HashSet<Guid> sourceBuildIds = new HashSet<Guid>(sourceScreens.Select(s => s.ScreenInBuildId).ToList());

			Dictionary<Guid, List<Comparison>> comparisonsDict = (await comparisonRepo.GetAsync(c => sourceBuildIds.Contains(c.SourceScreenInBuildId), includeProperties: "SourceScreenInBuild,TargetScreenInBuild")).GroupBy(c => c.SourceScreenInBuildId).ToDictionary(c => c.Key, c => c.ToList());

			foreach (ScreenInBuild sourceScreen in sourceScreens)
            {
                ScreenInBuild targetScreen = targetScreens.FirstOrDefault(s => s.ScreenName.Equals(sourceScreen.ScreenName));

                if (targetScreen != null)
                {
					//Comparison comparison = (await comparisonRepo.GetAsync(c => c.SourceScreenInBuildId == sourceScreen.ScreenInBuildId && c.TargetScreenInBuildId == targetScreen.ScreenInBuildId, includeProperties: "SourceScreenInBuild,TargetScreenInBuild")).FirstOrDefault();

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
                        comparisons.Add(comparison);
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

                            comparisonRepo.Insert(newComparison);

                            //copy diff image for reverse comparison
                            if (System.IO.File.Exists(diffAbsImagePath))
                                System.IO.File.Copy(diffAbsImagePath, revDiffAbsImagePath);

                            //add reverse comparison to the database
                            Comparison revComparison = new Comparison() { Difference = newComparison.Difference, SourceScreenInBuildId = newComparison.TargetScreenInBuildId, TargetScreenInBuildId = newComparison.SourceScreenInBuildId };
                            comparisonRepo.Insert(revComparison);

                            await unitOfWork.SaveAsync();

                            comparisons.Add(
                                new Comparison()
                                {
                                    SourceScreenInBuildId = sourceScreen.ScreenInBuildId,
                                    TargetScreenInBuildId = targetScreen.ScreenInBuildId,
                                    Difference = newComparison.Difference
                                });

                        }
                        else
                        {
                            string errorMessage = "Error comparing images!";

                            if (!srcImageExists)
                                errorMessage += "\nscreenFromPath or screenToPath doesn't exists.";

                            if (!diffImageFolderCreated)
                                errorMessage += "\nCreate diff image folder failure.";

                            throw new Exception(errorMessage);
                        }
                    }

                }
                else
                {
                    comparisons.Add(new Comparison()
                    {
                        SourceScreenInBuildId = sourceScreen.ScreenInBuildId,
                        Difference = 1
                    });

                }
            }

            return comparisons;
        }
    }
}