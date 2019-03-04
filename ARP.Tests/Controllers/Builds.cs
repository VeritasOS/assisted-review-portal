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
    public class BuildsControllerTest
    {
        private GarbContext _context;
        private UnitOfWork _unitOfWork;
        private BuildsController _controller;
        private IMapper _mapper;


        [TestInitialize]
        public void InitializeDb()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<GarbProfile>(); });
            _mapper = config.CreateMapper();

            _context = TestInitializer.GetSeededContext(_mapper);

            _unitOfWork = new UnitOfWork(_context);

        }

        [TestMethod]
        public async Task CanGetBuilds()
        {
            _controller = new BuildsController(_context);

            string _projectName = "Velocity";

            //var result = await _controller.Get(_projectName) as OkObjectResult;
            var OkObjectTask = _controller.Get(_projectName);

            OkObjectTask.Wait();

            var result = OkObjectTask.Result as OkObjectResult;

            Assert.IsNotNull(result);

            var builds = result.Value as ICollection<BuildDto>;

            Assert.IsNotNull(builds);

            Assert.IsTrue(builds.Count > 0);
        }


        [TestMethod]
        public async Task CanGetLatestBuild()
        {
            GenericRepository<Build> buildRepo = _unitOfWork.BuildRepository;

            string project1Name = "Velocity";
            string project2Name = "Test2";
            string project1Version1 = "1.0";
            string project2Version2 = "2.3";

            Build build1 = await BuildHelper.GetLatestBuild(buildRepo, project1Name);

            Assert.IsNotNull(build1);

            Assert.AreEqual(project1Version1, build1.BuildName);

            Build build2 = await BuildHelper.GetLatestBuild(buildRepo, project2Name);

            Assert.IsNotNull(build2);

            Assert.AreEqual(project2Version2, build2.BuildName);

            Build build3 = await BuildHelper.GetLatestBuild(buildRepo, project2Name+project1Name);

            Assert.IsNull(build3);
        }
    }
    }
