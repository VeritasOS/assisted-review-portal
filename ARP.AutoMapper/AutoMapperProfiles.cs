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
using garb.Dto;
using garb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace garb.Helpers
{
	/// <summary>
	/// Automapper profile
	/// </summary>
	public class GarbProfile : Profile
	{
		/// <summary>
		/// Automapper profile definition
		/// </summary>	
		public GarbProfile()
		{
			CreateMap<Issue, IssueDto>()
				.ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.IssueId))
				.ForMember(dest => dest.DateReported, opts => opts.MapFrom(src => src.ModificationTime))
				.ForMember(dest => dest.BuildReported, opts => opts.MapFrom(src => src.Build.BuildName))
				.ForMember(dest => dest.DateModified, opts => opts.MapFrom(src => src.ModificationTime))
				.ForMember(dest => dest.BuildModified, opts => opts.MapFrom(src => src.Build.BuildName))
				.ForMember(dest => dest.Status, opts => opts.MapFrom(src => src.IssueStatus))
				.ForMember(dest => dest.Severity, opts => opts.MapFrom(src => src.IssueSeverity))
				.ForMember(dest => dest.Type, opts => opts.MapFrom(src => src.IssueType))
				.ForMember(dest => dest.Text, opts => opts.MapFrom(src => src.Value));

			CreateMap<IssueDto, Issue>()
				.ForMember(dest => dest.IssueId, opts => opts.MapFrom(src => src.Id))
				.ForMember(dest => dest.ModificationTime, opts => opts.MapFrom(src => src.DateReported))
				.ForMember(dest => dest.IssueStatus, opts => opts.MapFrom(src => src.Status))
				.ForMember(dest => dest.IssueSeverity, opts => opts.MapFrom(src => src.Severity))
				.ForMember(dest => dest.IssueType, opts => opts.MapFrom(src => src.Type))
				.ForMember(dest => dest.Value, opts => opts.MapFrom(src => src.Text));

			CreateMap<Issue, IssueRevision>().ReverseMap();
		}
	}
}