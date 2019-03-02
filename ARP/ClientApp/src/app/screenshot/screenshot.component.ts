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

import { PercentPipe } from '@angular/common';
import { ChangeDetectorRef, Component, Directive, ElementRef, HostListener, Inject, OnInit, ViewChild } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { saveAs } from 'file-saver';
import { Observable } from "rxjs";
import { VdlIconRegistry } from 'vdl-angular';
import { Build, Comparison, GarbService, Locale, Project, Screen, ScreenshotPath } from '../garb.service';
import { IconListService } from '../icon.service';
import { MessageService } from '../message.service';

@Component({
	selector: 'app-screenshot',
	templateUrl: './screenshot.component.html',
	styleUrls: ['./screenshot.component.css'],
	providers: [IconListService]
})

export class ScreenshotComponent implements OnInit {

	public leftImageToShow: any;
	public rightImageToShow: any;
	private storeUrl: string;

	public projects: Project[];
	public builds: Build[];
	public languagesLeft: Locale[];
	public languagesRight: Locale[];
	public allComparisons: Comparison[];
	public comparisonImagePath: string;
	public leftImagePath: string;
	public rightImagePath: string;
	public sourceImagePath: string;

	public sliderLoadingShow: boolean = false;

	// variable for screenshot difference rate
	screenshotDiff: number = 0;
	diffPercent: number;

	sliderControler: boolean = true;
	sliderIsDisabled: boolean = false;
	percentSliderIsDisabled: boolean = false;
	sliderToggleDisabled: boolean = false;
	lastSelectedState = this.sliderControler;

	//variables for left and right image
	leftImagesList: Screen[];
	rightImagesList: Screen[];
	sourceImagesList: Screen[];

	screenNames: string[];
	public currentScreenName: string = null;
	public currentLocaleLeft: string = null;
	public currentLocaleRight: string = null;
	currentBuildIdLeft: string = null;
	currentBuildIdRight: string = null;
	scale: number = 100;
	fitTheScreen = true;

	public showOverylay: boolean = true;
	public displayEnglish: boolean = false;

	currentComparison: Comparison;
	public currentProjectName: string;
	currentScreenLeft: Screen;
	currentScreenRight: Screen;
	currentSourceScreen: Screen;

	noPicturePath = null;

	public secondLine: boolean = true;
	public actions: boolean = true;

	public leftImageWidth: number = 0;
	public leftImageHeight: number = 0;
	public rightImageWidth: number = 0;
	public rightImageHeight: number = 0;
	public overlayImageHeight: number = 0;
	public overlayImageWidth: number = 0;
	public sourceImageWidth: number = 0;
	public sourceImageHeight: number = 0;

	public totalScreenNo: number = 0;
	public filteredScreenNo: number = 0;

	innerHeight: any;
	innerWidth: any;
	comparerWidth: any;

	public icons: Array<string>;

	constructor(private messageService: MessageService,
		@Inject('API_URL') private originUrl: string,
		private garbService: GarbService,
		private cdRef: ChangeDetectorRef,
		vdlIconRegistry: VdlIconRegistry,
		sanitizer: DomSanitizer,
		iconListService: IconListService
	) {
		this.storeUrl = originUrl + "/store/";
		this.diffPercent = 0;
		this.getProjects();
		this.icons = iconListService.getIconList();
		vdlIconRegistry
			.addSvgIcon(
				'no-picture',
				sanitizer.bypassSecurityTrustResourceUrl(
					'assets/img/no-picture.svg'
				)
			);
	}

	@ViewChild('comparer') private comparerContent: ElementRef;



	ngOnInit() {
		this.innerHeight = window.innerHeight;
		this.innerWidth = window.innerWidth;
		this.comparerWidth = this.comparerContent.nativeElement.offsetWidth;
		this.log("Screen size (init): " + this.innerHeight + "x" + this.innerWidth);
	}

	@HostListener('window:resize', ['$event'])
	onResize(event) {
		this.innerHeight = window.innerHeight;
		this.innerWidth = window.innerWidth;
		this.comparerWidth = this.comparerContent.nativeElement.offsetWidth;
		this.log("Screen size (resize): " + this.innerHeight + "x" + this.innerWidth);
		this.recalculateScale();
	}

	public getComparison(projectName: string, screen1Id: string, screen2Id: string) {
		//this.sliderLoadingShow =true;
		this.garbService.getComparison(projectName, screen1Id, screen2Id).subscribe(result => {
			//this.sliderLoadingShow = false;
			this.currentComparison = result as Comparison;
			if (this.currentComparison == null || this.currentComparison.diffImagePath == null)
				this.comparisonImagePath = null;
			else
				this.comparisonImagePath = this.storeUrl + this.currentComparison.diffImagePath;
		});
	}

	public getComparisonsList(projectName: string, build1Id: string, locale1: string, build2Id: string, locale2: string) {
		if (locale1 === locale2) {
			this.sliderLoadingShow = true;
			this.allComparisons = null;
			this.garbService.getComparisons(projectName, build1Id, locale1, build2Id, locale2).subscribe(result => {
				this.sliderLoadingShow = false;
				this.allComparisons = result as Comparison[];
				this.log("Comparison no: " + this.allComparisons.length);
				this.getFilteredImageNamesList(this.screenshotDiff);
			});
		}
	}

	onDownload() {
		this.sliderLoadingShow = true;
		this.garbService.getDownload(this.currentProjectName, this.currentBuildIdLeft, this.currentLocaleLeft,
			this.currentBuildIdRight, this.currentLocaleRight, String(this.diffPercent), this.sliderIsDisabled)
			.subscribe(data => {
				this.sliderLoadingShow = false;
				saveAs(data, 'screenshots.zip');
			});
	}

	onDiffChange() {
		this.diffPercent = Math.ceil(this.screenshotDiff * 1000 - 0.1) / 10;
		this.log("New diff setting: " + this.screenshotDiff);
		this.getFilteredImageNamesList(this.screenshotDiff);
	}

	onDiffPercentChange() {
		this.screenshotDiff = this.diffPercent / 100;
		this.log("New diff setting: " + this.screenshotDiff);
		this.getFilteredImageNamesList(this.screenshotDiff);
	}

	onThresholdEnableChange($event: any) {
		this.sliderIsDisabled = !$event.checked;
		this.lastSelectedState = this.sliderControler;
		if (!this.sliderIsDisabled) {
			this.getComparisonsList(this.currentProjectName, this.currentBuildIdLeft, this.currentLocaleLeft, this.currentBuildIdRight, this.currentLocaleRight);
			this.sliderIsDisabled = false;
		}
		else {
			this.sliderIsDisabled = true;
			this.percentSliderIsDisabled = true;
			this.getImageNamesList();
			this.filteredScreenNo = this.totalScreenNo;
		}
	}

	getProjects() {
		this.garbService.getProjects().subscribe(result => {
			this.projects = result as Project[];
			if (this.projects.length > 0) {
				this.currentProjectName = this.projects[0].projectName;
				this.getBuilds(this.currentProjectName);
			}

		});
	}

	getBuilds(projectName) {
		if (projectName != null) {
			this.currentProjectName = projectName;

			this.screenNames = null;
			this.currentScreenName = null;
			this.updateImages(null);

			this.garbService.getBuilds(projectName).subscribe(build => {
				this.builds = build as Build[];
				if (this.builds.length > 0) {
					this.currentBuildIdLeft = this.builds[0].id;
					this.currentBuildIdRight = this.builds[0].id;
					this.getLocalesLeft(this.currentProjectName, this.currentBuildIdLeft);
					this.getLocalesRight(this.currentProjectName, this.currentBuildIdRight);
				}
				else {
					this.currentBuildIdLeft = null;
					this.currentBuildIdRight = null;
					this.currentLocaleLeft = null;
					this.currentLocaleRight = null;
				}
			});
		}
	}


	getLocalesLeft(projectName, buildId) {
		if (projectName != null && buildId != null) {
			this.garbService.getLanguages(projectName, buildId).subscribe(language => {
				this.languagesLeft = language as Locale[];
				if (this.languagesLeft.length > 0) {
					this.currentLocaleLeft = this.languagesLeft[0].localeCode;
					this.getImages(this.currentProjectName, this.currentLocaleLeft, this.currentBuildIdLeft, 'LEFT');
				}
				else {
					this.currentLocaleLeft = null;
					this.leftImagesList = null;
				}
				//this.language = language as Locales[];
			});
		}
	}

	getLocalesRight(projectName, buildId) {
		if (projectName != null && buildId != null) {
			this.garbService.getLanguages(projectName, buildId).subscribe(language => {
				this.languagesRight = language as Locale[];
				if (this.languagesRight.length > 0) {
					this.currentLocaleRight = this.languagesRight[0].localeCode;
					this.getImages(this.currentProjectName, this.currentLocaleRight, this.currentBuildIdRight, 'RIGHT');
					this.getImages(this.currentProjectName, 'en-US', this.currentBuildIdRight, 'SOURCE');
				}
				else {
					this.currentLocaleRight = null;
					this.rightImagesList = null;
				}
				//this.language = language as Locales[];
			});
		}
	}

	getImages(projectName, locale, buildId, type) {
		if (projectName != null && buildId != null && locale != null) {
			this.garbService.getScreens(projectName, locale, buildId).subscribe(screens => {

				if (this.currentLocaleLeft != this.currentLocaleRight) {
					this.sliderIsDisabled = true;
					this.sliderToggleDisabled = true;
					this.sliderControler = false;
				} else {
					this.sliderControler = this.lastSelectedState;
					if (this.sliderControler === false) {
						this.sliderIsDisabled = true;
						this.sliderToggleDisabled = this.lastSelectedState;
					}
					else {
						this.sliderIsDisabled = false;
						this.sliderToggleDisabled = false;
					}
				}

				let sortedImages = screens.sort((a, b) => a.name !== b.name ? a.name < b.name ? -1 : 1 : 0);

				if (type === 'LEFT') {
					this.leftImagesList = sortedImages;
				}
				if (type === 'RIGHT') {
					this.rightImagesList = sortedImages;
					this.totalScreenNo = this.rightImagesList.length;
					this.filteredScreenNo = this.totalScreenNo;
				}
				if (type === 'SOURCE') {
					this.sourceImagesList = sortedImages;
				}

				this.getImageNamesList();
			});
		}
	}

	getImageNamesList() {
		let newScreenList: string[] = [];
		if (this.leftImagesList != null) {
			for (let scrl of this.leftImagesList) {
				newScreenList.push(scrl.name);
			}
		}
		if (this.rightImagesList != null) {
			for (let scrr of this.rightImagesList) {
				if (!newScreenList.includes(scrr.name)) {
					newScreenList.push(scrr.name);
				}
			}
		}

		this.screenNames = newScreenList.sort();

		if (this.screenNames.length > 0) {
			if (this.currentScreenName == null || !this.screenNames.includes(this.currentScreenName)) {
				this.currentScreenName = this.screenNames[0];
			}
			this.updateImages(this.currentScreenName);
		}
		else {
			this.updateImages(null);
		}

		if (!this.sliderIsDisabled) {
			this.getComparisonsList(this.currentProjectName, this.currentBuildIdLeft, this.currentLocaleLeft, this.currentBuildIdRight, this.currentLocaleRight);
		}
	}

	getFilteredImageNamesList(threshold) {
		let newScreenList: string[] = [];

		if (this.allComparisons != null) {
			for (let comparison of this.allComparisons) {
				if (comparison.difference > threshold && !newScreenList.includes(comparison.sourceScreenName)) {
					newScreenList.push(comparison.sourceScreenName);
				}
			}
			this.screenNames = newScreenList.sort();

			this.filteredScreenNo = this.screenNames.length;

			if (this.screenNames.length > 0) {
				if (this.currentScreenName == null || !this.screenNames.includes(this.currentScreenName)) {
					this.currentScreenName = this.screenNames[0];
				}
				this.updateImages(this.currentScreenName);
			}
			else {
				this.updateImages(null);
			}
		}
	}


	updateImages(imageName) {
		if (imageName != null) {
			if (this.leftImagesList !== undefined) {
				this.currentScreenLeft = this.leftImagesList.find(c => c.name == imageName);

				if (this.currentScreenLeft == null || this.currentScreenLeft.path == null) {
					this.leftImagePath = this.noPicturePath;
				}
				else {
					this.leftImagePath = this.storeUrl + this.currentScreenLeft.path;
				}
			}

			if (this.sourceImagesList !== undefined) {
				this.currentSourceScreen = this.sourceImagesList.find(c => c.name == imageName);

				if (this.currentSourceScreen == null || this.currentSourceScreen.path == null) {
					this.sourceImagePath = this.noPicturePath;
				}
				else {
					this.sourceImagePath = this.storeUrl + this.currentSourceScreen.path;
				}
			}

			if (this.rightImagesList !== undefined) {
				this.currentScreenRight = this.rightImagesList.find(c => c.name == imageName);
				if (this.currentScreenRight == null || this.currentScreenRight.path == null) {
					this.rightImagePath = this.noPicturePath;
				}
				else {
					this.rightImagePath = this.storeUrl + this.currentScreenRight.path;
				}
			}
			if (this.currentScreenRight != null && this.currentScreenLeft != null && this.currentProjectName != null) {
				this.getComparison(this.currentProjectName, this.currentScreenLeft.id, this.currentScreenRight.id);
			}
		}
		else {
			this.currentScreenName = null;
			this.leftImagePath = this.noPicturePath;
			this.rightImagePath = this.noPicturePath;
			this.comparisonImagePath = null;
		}
	}

	moveCurrentImage(delta: number) {
		if (this.screenNames.length > 0) {
			let screenCounter = this.screenNames.findIndex(s => s == this.currentScreenName);
			if (screenCounter < 0) {
				screenCounter = 0;
			}
			else {
				screenCounter += delta;
			}

			if (screenCounter < 0) {
				screenCounter = this.screenNames.length - 1;
			}
			else if (screenCounter >= this.screenNames.length) {
				screenCounter = 0;
			}

      this.comparisonImagePath = null;

			this.log("screenCounter " + screenCounter);

			this.currentScreenName = this.screenNames[screenCounter];
			this.updateImages(this.currentScreenName);
		}
	}

	imageSize(diff: number) {
		this.fitTheScreen = false;
		let roundTo = Math.abs(diff);
		if (diff < 0)
			this.scale = Math.ceil(this.scale / roundTo) * roundTo;
		else
			this.scale = Math.floor(this.scale / roundTo) * roundTo;

		this.scale += diff;


		if (this.scale < 10)
			this.scale = 10;
		if (this.scale > 400)
			this.scale = 400;
	}

	viewLargeImage(imagePath) {
		window.open(imagePath);
	}

	/** Log a ScreenshotComponent message with the MessageService */
	private log(message: string) {
		this.messageService.add('ScreenshotComponent: ' + message);
	}

	imageLoaded($event: any) {
		let target = $event.target || $event.srcElement || $event.currentTarget;
		let idAttr = target.attributes.id;
		let imageId = idAttr.nodeValue;

		let widthAttr = target.naturalWidth;
		let heightAttr = target.naturalHeight;
		this.log(imageId + " loaded, size: " + widthAttr + "x" + heightAttr);

		if (imageId === "leftImage") {
			this.leftImageHeight = heightAttr;
			this.leftImageWidth = widthAttr;
			if (this.fitTheScreen)
				this.recalculateScale();
		}
		else if (imageId === "rightImage") {
			this.rightImageHeight = heightAttr;
			this.rightImageWidth = widthAttr;
			if (this.fitTheScreen)
				this.recalculateScale();
		}
		else if (imageId === "sourceImage") {
			this.sourceImageHeight = heightAttr;
			this.sourceImageWidth = widthAttr;
			if (this.fitTheScreen)
				this.recalculateScale();
		}
	}


	resetZoom($event: any) {
		if ($event.checked) {
			this.recalculateScale();
		}
		this.fitTheScreen = $event.checked;
	}

	recalculateScale() {


		let currentLeftImageHeight: number = (this.displayEnglish ? this.sourceImageHeight : this.leftImageHeight);
		let currentLeftImageWidth: number = (this.displayEnglish ? this.sourceImageWidth : this.leftImageWidth);


		this.overlayImageHeight = Math.max(currentLeftImageHeight, this.rightImageHeight);
		this.overlayImageWidth = Math.max(currentLeftImageWidth, this.rightImageWidth);
		this.scale = Math.floor(100 * (this.comparerWidth / 2) / Math.max(currentLeftImageWidth, this.rightImageWidth));
	}

	overlayChange($event: any) {
		this.showOverylay = $event.checked;
	}

	flipEnglishDisplay($event: any) {
		this.displayEnglish = $event.checked;
		//this.updateImages(this.currentScreenName);
	}

}
