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

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, of } from "rxjs";
import { catchError, map, tap } from 'rxjs/operators';
import { MessageService } from './message.service';

export interface Project {
	projectName: string;
}

export interface Build {
	id: string;
	buildName: string;
	projectName: string;
	buildStatus: number;
}

export interface Locale {
	localeCode: string;
	localeName: string;
}

export interface ScreenshotPath {
	screenshotpath: string;
}

export interface Screen {
	id: string;
	name: string;
	project: string;
	build: string;
	locale: string;
	timeStamp: string;
	path: string;
}

export interface Comparison {
	sourceScreenInBuildId: string;
	targetScreenInBuildId: string;
	sourceScreenName: string;
	targetScreenName: string;
	difference: number;
	diffImagePath: string;
}

export interface Issue {
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

export enum IssueType { Linguistic = 0, Hardcode = 1, Overlapping = 2, CharacterCorruption = 3 }
export enum IssueStatus { Active = 0, Resolved = 1, FalsePositive = 2 }
export enum IssueSeverity { Info = 0, Warning = 1, Error = 2 }

const httpOptions = {
	headers: new HttpHeaders({ 'Content-Type': 'application/json' })
};

@Injectable()
export class GarbService {

	private apiUrl: string;

	constructor(
		private http: HttpClient,
		private messageService: MessageService,
		@Inject('API_URL') private originUrl: string) {
		this.apiUrl = originUrl + "/api/v1.0";
	}

	/** GET Projects from the server */
	getProjects(): Observable<Array<Project>> {
		return this.http.get<Array<Project>>(`${this.apiUrl}/Projects`)
			.pipe(
				tap(projects => this.log(`fetched projects`)),
				catchError(this.handleError('getProjects', []))
			);
	}

	/** GET Builds from the server */
	getBuilds(projectName: string): Observable<Array<Build>> {
		return this.http.get<Array<Build>>(`${this.apiUrl}/Builds/${projectName}`).pipe(
			tap(builds => this.log(`fetched builds`)),
			catchError(this.handleError('getBuilds', []))
		);
	}

	/** GET Languages from the server */
	getLanguages(projectName: string, buildId: string): Observable<Array<Locale>> {
		return this.http.get<Array<Locale>>(`${this.apiUrl}/Locales/?project=${projectName}&build=${buildId}`).pipe(
			tap(languages => this.log(`fetched languages`)),
			catchError(this.handleError('getLanguages', []))
		);
	}

	/** GET Screens from the server */
	getScreens(projectName: string, locale: string, buildId: string): Observable<Array<Screen>> {
		return this.http.get<Array<Screen>>(`${this.apiUrl}/Screens/${projectName}?locale=${locale}&build=${buildId}`).pipe(
			tap(screens => this.log(`fetched screens`)),
			catchError(this.handleError('getScreens', []))
		);
	}

	/** GET Comparison from the server */
	getComparison(projectName: string, screen1Id: string, screen2Id: string): Observable<Comparison> {

		return this.http.get<Comparison>(`${this.apiUrl}/Comparisons/${projectName}?screen1Id=${screen1Id}&screen2Id=${screen2Id}`).pipe(
			tap(_ => this.log(`fetched comparison id1=${screen1Id} id2=${screen2Id}`)),
			catchError(this.handleError<Comparison>(`getComparison project=${projectName} id1=${screen1Id} id1=${screen2Id}`))
		);
	}

	/** GET Comparisons from the server */
	getComparisons(projectName: string, build1Id: string, locale1: string, build2Id: string, locale2: string): Observable<Array<Comparison>> {
		return this.http.get<Array<Comparison>>(`${this.apiUrl}/Comparisons/${projectName}/List?build1id=${build1Id}&locale1=${locale1}&build2id=${build2Id}&locale2=${locale2}`).pipe(
			tap(comparisons => this.log(`fetched comparisons`)),
			catchError(this.handleError('getComparisons', []))
		);
	}

	getDownload(projectName: string, build1Id: string, locale1: string, build2Id: string, locale2: string, threshold: string, sliderIsDisabled: boolean) {
		//let url = 'http://localhost:57786/api/v1.0/download';
		let url = this.apiUrl + "/download";
		return this.http.get(`${url}/?projectName=${projectName}&build1id=${build1Id}&locale1=${locale1}&build2id=${build2Id}&locale2=${locale2}&threshold=${threshold}&sliderIsDisabled=${sliderIsDisabled}`, { responseType: 'blob' });
	}

	/** GET Issues from the server */
	getIssues(projectName: string, screenName: string, locale: string): Observable<Array<Issue>> {

		// let issues: Issue[] = [
		// 	{
		// 		issueType: 'comment',
		// 		status: 'open',
		// 		text: 'This need to be fixed!!!!'

		// 	},
		// 	{
		// 		issueType: 'overlap',
		// 		status: 'open',
		// 		text: 'Overlaping controls'
		// 	},
		// 	{
		// 		issueType: 'comment',
		// 		status: 'closed',
		// 		text: 'Another comment'
		// 	}
		// ];

		// return of(issues);

		return this.http.get<Array<Issue>>(`${this.apiUrl}/Issues/${projectName}/${screenName}/${locale}?onlyactive=false`).pipe(
			tap(_ => this.log(`fetched issues project ${projectName}, screen ${screenName}, locale ${locale}`)),
			catchError(this.handleError('getIssues', []))
		);
	}


	/**
	   * Handle Http operation that failed.
	   * Let the app continue.
	   * @param operation - name of the operation that failed
	   * @param result - optional value to return as the observable result
	   */
	private handleError<T>(operation = 'operation', result?: T) {
		return (error: any): Observable<T> => {

			// TODO: send the error to remote logging infrastructure
			console.error(error); // log to console instead

			// TODO: better job of transforming error for user consumption
			this.log(`${operation} failed: ${error.message}`);

			// Let the app keep running by returning an empty result.
			return of(result as T);
		};
	}

	/** Log a GarbService message with the MessageService */
	private log(message: string) {
		this.messageService.add('GarbService: ' + message);
	}
}
