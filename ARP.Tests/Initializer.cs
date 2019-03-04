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
using garb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace garbUnitTest
{
	public static class TestInitializer
    {
		public static string UserName = "admin";

		public static ControllerContext GetContext()
		{
			return new ControllerContext
			{
				HttpContext = new DefaultHttpContext
				{
					User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
						{
				new Claim(ClaimTypes.Name, TestInitializer.UserName)
						}, "someAuthTypeName"))
				}
			};
		}

		public static GarbContext GetSeededContext(IMapper mapper)
		{
			string project1Name = "Velocity";
			string project2Name = "Test2";
            string project1Version0 = "0.1";
            string project1Version1 = "1.0";
			string project2Version1 = "2.2";
            string project2Version2 = "2.3";
            string userName = TestInitializer.UserName;


			var options = new DbContextOptionsBuilder<GarbContext>()
				.UseInMemoryDatabase(databaseName: $"GarbTestDb-{Guid.NewGuid()}")
				.Options;

			var context = new GarbContext(options, mapper);

			var users = new User [] {
					new User{ UserName = userName}
				};

			foreach (User u in users)
			{
				context.Users.Add(u);
			}
			context.SaveChanges();


			var projects = new Project[] {
					new Project{ProjectName=project1Name},
					new Project{ProjectName=project2Name}
				};

			foreach (Project p in projects)
			{
				context.Projects.Add(p);
			}
			context.SaveChanges();


			var oldBuilds = new Build[] {
                    new Build{ProjectName=project1Name, BuildName = project1Version0},
					new Build{ProjectName=project2Name, BuildName = project2Version1},
                };

			foreach (Build b in oldBuilds)
			{
				context.Builds.Add(b);
			}
			context.SaveChanges(userName);

            Thread.Sleep(10); // slight delay, so the new builds have new timestamps

            var newBuilds = new Build[] {
                    new Build{ProjectName=project1Name, BuildName = project1Version1},
                    new Build{ProjectName=project2Name, BuildName = project2Version2}
                };

            foreach (Build b in newBuilds)
            {
                context.Builds.Add(b);
            }
            context.SaveChanges(userName);

            var locales = new Locale[] {
					new Locale{ LocaleCode ="en-US", LocaleName = "English (United States)" },
					new Locale{LocaleCode="pl-PL", LocaleName = "Polish" }
				};

			foreach (Locale l in locales)
			{
				context.Locales.Add(l);
			}
			context.SaveChanges();

			string screen1Name = "Install";
			string screen2Name = "Uninstall";

			var screens = new Screen[] {
				new Screen{ ProjectName = project1Name, ScreenName = screen1Name },
				new Screen{ ProjectName = project1Name, ScreenName = screen2Name },
				};

			foreach (Screen s in screens)
			{
				context.Screens.Add(s);
			}
			context.SaveChanges();

            var previousBuildId = context.Builds.FirstOrDefault(b => b.BuildName == project1Version0).Id;

            var currentBuildId = context.Builds.FirstOrDefault(b => b.BuildName == project1Version1).Id;

			var screensInBuild = new ScreenInBuild[]
			{
                new ScreenInBuild{ ProjectName =project1Name, ScreenName = screen1Name, BuildId = previousBuildId, LocaleCode = "pl-PL" },
                new ScreenInBuild{ ProjectName =project1Name, ScreenName = screen1Name, BuildId = previousBuildId, LocaleCode = "en-US" },
                new ScreenInBuild{ ProjectName =project1Name, ScreenName = screen1Name, BuildId = currentBuildId, LocaleCode = "pl-PL" },
				new ScreenInBuild{ ProjectName =project1Name, ScreenName = screen1Name, BuildId = currentBuildId, LocaleCode = "en-US" },
				};

			foreach (ScreenInBuild sb in screensInBuild)
			{
				context.ScreensInBuilds.Add(sb);
			}
			context.SaveChanges(userName);

			var issues = new Issue[]{
                new Issue{ ProjectName = project1Name, ScreenName = screen1Name, LocaleCode = "en-US", IssueType = IssueType.Hardcode, Identifier = "1", Value = "Hardcode", ModifiedInBuildId = currentBuildId, IssueStatus = IssueStatus.Active },
                new Issue{ ProjectName = project1Name, ScreenName = screen1Name, LocaleCode = "pl-PL", IssueType = IssueType.Hardcode, Identifier = "1", Value = "Hardcode", ModifiedInBuildId = currentBuildId, IssueStatus = IssueStatus.Active },
                new Issue{ ProjectName = project1Name, ScreenName = screen1Name, LocaleCode = "pl-PL", IssueType = IssueType.Linguistic, Identifier = "2", Value = "Test1", ModifiedInBuildId = currentBuildId, IssueStatus = IssueStatus.FalsePositive },
                new Issue{ ProjectName = project1Name, ScreenName = screen1Name, LocaleCode = "pl-PL", IssueType = IssueType.Overlapping, Identifier = "3", Value = "", ModifiedInBuildId = currentBuildId, IssueStatus = IssueStatus.Active, X = 0, Y = 0, Width = 10, Height = 10 },
                new Issue{ ProjectName = project1Name, ScreenName = screen1Name, LocaleCode = "pl-PL", IssueType = IssueType.CharacterCorruption, Identifier = "7", Value = "C0rrpu4Ed!", ModifiedInBuildId = previousBuildId, IssueStatus = IssueStatus.Active },
                };

			foreach (Issue i in issues)
			{
				context.Issues.Add(i);
			}
			context.SaveChanges(userName);





			return context;
		}
    }
}
