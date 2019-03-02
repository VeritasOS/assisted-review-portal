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
    public static class DbInitializer
    {
        public static void Initialize(GarbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Locales.Any())
            {
                return;   // DB has been seeded
            }

            //var projects = new Project[]
            //{
            //    new Project{ProjectName="Velocity"},
            //    new Project{ProjectName="Test2"},
            //    new Project{ProjectName="BE"},
            //};

            //foreach (Project p in projects)
            //{
            //    context.Projects.Add(p);
            //}
            //context.SaveChanges();


            //var builds = new Build[] {
            //    new Build{ProjectName="Velocity", BuildName = "1.0"},
            //    new Build{ProjectName="Test2", BuildName = "2.2"}
            //};

            //foreach (Build b in builds)
            //{
            //    context.Builds.Add(b);
            //}
            //context.SaveChanges();

            var locales = new Locale[] {
                new Locale{ LocaleCode ="en-US", LocaleName = "English (United States)" },
                new Locale{LocaleCode="pl-PL", LocaleName = "Polish" },
                new Locale{LocaleCode="ja-JP", LocaleName = "Japanese" },
                new Locale{LocaleCode="de-DE", LocaleName = "German" },
              	new Locale{LocaleCode="zh-CN", LocaleName = "Simplified Chinese"},
                new Locale{LocaleCode="zh-TW", LocaleName = "Traditional Chinese"}
            };

            foreach (Locale l in locales)
            {
                context.Locales.Add(l);
            }
            context.SaveChanges();
        }
    }
}
