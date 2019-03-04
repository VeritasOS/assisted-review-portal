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

using garb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace garb.Data
{
    public class UnitOfWork : IDisposable
    {
        private GarbContext _context;
        private GenericRepository<Project> _projectRepository;
        private GenericRepository<Build> _buildRepository;
        private GenericRepository<ScreenInBuild> _screenInBuildRepository;
        private GenericRepository<Screen> _screenRepository;
        private GenericRepository<User> _userRepository;
        private GenericRepository<Locale> _localeRepository;
        private GenericRepository<Comparison> _comparisonRepository;
		private GenericRepository<Issue> _issueRepository;

		public bool AutoSaveChanges { get; set; }
        //public UnitOfWork()
        //{
        //    _context = new GarbContext();
        //    AutoSaveChanges = true;
        //}

        public UnitOfWork(GarbContext context)
        {
            _context = context;
            AutoSaveChanges = true;
        }

        public GenericRepository<Project> ProjectRepository
        {
            get
            {

                if (_projectRepository == null)
                {
                    _projectRepository = new GenericRepository<Project>(_context);
                }
                return _projectRepository;
            }
        }

        public GenericRepository<Build> BuildRepository
        {
            get
            {

                if (_buildRepository == null)
                {
                    _buildRepository = new GenericRepository<Build>(_context);
                }
                return _buildRepository;
            }
        }

        public GenericRepository<ScreenInBuild> ScreenInBuildRepository
        {
            get
            {
                if (_screenInBuildRepository == null)
                {
                    _screenInBuildRepository = new GenericRepository<ScreenInBuild>(_context);
                }
                return _screenInBuildRepository;
            }
        }

        public GenericRepository<Screen> ScreenRepository
        {
            get
            {
                if (_screenRepository == null)
                {
                    _screenRepository = new GenericRepository<Screen>(_context);
                }
                return _screenRepository;
            }
        }

        public GenericRepository<User> UserRepository
        {
            get
            {

                if (_userRepository == null)
                {
                    _userRepository = new GenericRepository<User>(_context);
                }
                return _userRepository;
            }
        }

        public GenericRepository<Locale> LocaleRepository
        {
            get
            {
                _localeRepository = _localeRepository ?? new GenericRepository<Locale>(_context);
                return _localeRepository;
            }
        }
        public GenericRepository<Comparison> ComparisonRepository
        {
            get
            {
                _comparisonRepository = _comparisonRepository ?? new GenericRepository<Comparison>(_context);
                return _comparisonRepository;
            }
        }
		public GenericRepository<Issue> IssueRepository
		{
			get
			{
				_issueRepository = _issueRepository ?? new GenericRepository<Issue>(_context);
				return _issueRepository;
			}
		}

		public int Save(string userName = null)
        {
            return _context.SaveChanges(userName);
        }

        public async Task<int> SaveAsync(string userName = null)
        {
            return await (AutoSaveChanges ? _context.SaveChangesAsync(userName) : Task.FromResult(0));
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
