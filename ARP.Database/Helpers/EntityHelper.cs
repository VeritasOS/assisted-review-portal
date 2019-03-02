﻿/*
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

//using System.Web;


namespace garb.Helpers
{
	/// <summary>
	/// Helper for working with entities
	/// </summary>
	public static class EntityHelper
	{
		/// <summary>
		/// Gets the values entity had befor the change
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="context"></param>
		/// <param name="updatedEntity"></param>
		/// <returns></returns>	
		public static TEntity GetOriginal<TEntity>(this DbContext context, TEntity updatedEntity) where TEntity : class
		{
			Func<PropertyValues, Type, object> getOriginal = null;
			getOriginal = (originalValues, type) =>
			{
				object original = Activator.CreateInstance(type, true);
				foreach (var property in originalValues.Properties)
				{
					var propertyInfo = property.PropertyInfo;
					var propertyName = propertyInfo.Name;

					object value = originalValues[propertyName];
					if (value is PropertyValues) //nested complex object
					{
						propertyInfo.SetValue(original, getOriginal(value as PropertyValues, propertyInfo.PropertyType));
					}
					else
					{
						propertyInfo.SetValue(original, value);
					}
				}
				return original;
			};
			return (TEntity)getOriginal(context.Entry(updatedEntity).OriginalValues, typeof(TEntity));
		}
	}
}