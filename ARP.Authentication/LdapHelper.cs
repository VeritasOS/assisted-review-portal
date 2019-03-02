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
		public static bool ValidateDomainCredentials(UnitOfWork _unitOfWork, string username, string password)
		{
			string domainName = "community.veritas.com";
			string userDn = $"{username}@{domainName}";

			try
			{
				// Using Novell LdapConnection instead of unsupported System.Directory services
				using (var connection = new LdapConnection { SecureSocketLayer = false })
				{
					connection.Connect(domainName, LdapConnection.DEFAULT_PORT);
					connection.Bind(userDn, password);

					if (connection.Bound)
					{
						var userRepo = _unitOfWork.UserRepository;

						var user = userRepo.GetByID(username);

						if (user == null)
						{
							User newUser = new User() { UserName = username };
							userRepo.Insert(newUser);
							_unitOfWork.Save();
						}

						return true;
					}
				}
			}
			finally
			{
			}

			return false;
		}
	}
}
