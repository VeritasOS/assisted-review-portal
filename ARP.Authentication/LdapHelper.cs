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
using garb.Data;
using Novell.Directory.Ldap;
using System;

namespace garb.Authentication
{
	public class LdapHelper
	{
		public static string LdapDomain;
        public static string LocalAdminName;
        public static string LocalAdminPwd;
        public static string LocalReadOnlyName;
        public static string LocalReadOnlyPwd;

        public static bool ValidateDomainCredentials(UnitOfWork _unitOfWork, string username, string password)
		{
            try
            {
                if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
                {
                    // Workaround for Local Admin
                    if (username.Equals(LocalAdminName) && password.Equals(LocalAdminPwd))
                    {
                        CreateRepro(_unitOfWork, username, password);
                        return true;
                    }

                    // Workaround for Local User (with read only access)
                    if (username.Equals(LocalReadOnlyName) && password.Equals(LocalReadOnlyPwd))
                    {
                        CreateRepro(_unitOfWork, username, password);
                        return true;
                    }

                    // Veritas AD User
                    string userDn = $"{username}@{LdapDomain}";

                    // Using Novell LdapConnection instead of unsupported System.Directory services
                    using (var connection = new LdapConnection { SecureSocketLayer = false })
                    {
                        connection.Connect(LdapDomain, LdapConnection.DEFAULT_PORT);
                        connection.Bind(userDn, password);

                        if (connection.Bound)
                        {
                            CreateRepro(_unitOfWork, username, password);
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                return false;
            }
        }

        public static void CreateRepro(UnitOfWork _unitOfWork, string username, string password)
        {
            var userRepo = _unitOfWork.UserRepository;

            var user = userRepo.GetByID(username);

            if (user == null)
            {
                User newUser = new User() { UserName = username };
                userRepo.Insert(newUser);
                _unitOfWork.Save();
            }
        }

        public static bool CanWrite(string username)
        {
            if (username != LocalReadOnlyName)
            {
                return true;
            }

            return false;
        }
    }
}
