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

export interface IProject {
	projectName: string;
}

export interface IBuild {
	id: string;
	buildName: string;
	projectName: string;
	buildStatus: number;
}

export interface ILocale {
	localeCode: string;
	localeName: string;
}

export interface IScreenshotPath {
	screenshotpath: string;
}

export interface IScreen {
	id: string;
	name: string;
	project: string;
	build: string;
	locale: string;
	timeStamp: string;
	path: string;
}

export interface ICreateScreen {
	content: string;
}

export interface IComparison {
	sourceScreenInBuildId: string;
	targetScreenInBuildId: string;
	sourceScreenName: string;
	targetScreenName: string;
	difference: number;
	diffImagePath: string;
}

export interface IIssue {
	severity?: IssueSeverity;
	type?: IssueType;
	status?: IssueStatus;
	identifier?: string;
	text?: string;
	x?: number;
	y?: number;
	width?: number;
	height?: number;
	id?: string;
	screenName?: string;
	projectName?: string;
	localeCode?: string;
	dateReported?: string;
	buildReported?: string;
	dateModified?: string;
	buildModified?: string;
}

export interface ICreateIssue {
	severity?: IssueSeverity;
	type?: IssueType;
	status?: IssueStatus;
	identifier?: string;
	text?: string;
	x?: number;
	y?: number;
	width?: number;
	height?: number;
}


export enum IssueType { Linguistic = 0, Hardcode = 1, DateTime = 2, Overlap = 3, Corruption = 4, Clipped = 5 }
export enum IssueStatus { Active = 0, Resolved = 1, FalsePositive = 2 }
export enum IssueSeverity { Info = 0, Warning = 1, Error = 2 }
