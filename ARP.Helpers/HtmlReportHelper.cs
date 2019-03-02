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

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace garb.Helpers
{
	public class HtmlReportHelper
	{
		public static void CreateHtmlFile(List<string> leftScreens, string projectName, string leftBuild, string rightBuild, string leftLocale, string rightLocale, string tempDir)
		{
			HtmlDocument htmlDoc = new HtmlDocument();
			var html = HtmlNode.CreateNode(HtmlBase(projectName, leftBuild, rightBuild, leftLocale));
			htmlDoc.DocumentNode.AppendChild(html);

			var headerTable = htmlDoc.DocumentNode.SelectSingleNode("/html/body/table[1]");
			var contentTable = htmlDoc.DocumentNode.SelectSingleNode("/html/body/table[2]");
			var tableHeader = HtmlNode.CreateNode("<tr><td colspan='2'><h2><b>Veritas </b>Assisted Review Portal</h2></td></tr>");
			var tableProjectHeader = HtmlNode.CreateNode("<tr><td colspan='2'><b>Project: </b>" + projectName + "</td></tr>");

			var tableBuildLocaleHeader = HtmlNode.CreateNode("<tr><td id='leftImage'><b>Left Build: </b>" + leftBuild +
				" - <b>Left Locale: </b>" + leftLocale + "</td><td id='rightImage'><b>Right Build: </b>" + rightBuild +
				" - <b>Right Locale: </b>" + rightLocale + "</td></tr>");

			var tableCheckboxes = HtmlNode.CreateNode("<tr><td><input type='checkbox' id='flip' onclick='flipEnglish()'><b> Flip English</b></td>" +
				"<td><input type='checkbox' id='overlay' onclick='diffOverlay()'><b> Overlay</b></td></tr>");

			headerTable.AppendChild(tableHeader);
			headerTable.AppendChild(tableProjectHeader);
			headerTable.AppendChild(tableBuildLocaleHeader);
			headerTable.AppendChild(tableCheckboxes);

			int screenNumber = 1;

			foreach (string leftScreen in leftScreens)
			{
				string screenName = Path.GetFileNameWithoutExtension(leftScreen);
				var screenNames = HtmlNode.CreateNode("<tr><td>" + screenNumber.ToString() + " - " + screenName +
					"</td><td>" + screenNumber.ToString() + " - " + screenName + "</td></tr>");

				string leftRelPath = Path.Combine(projectName, leftLocale, leftBuild, screenName + ".png");
				string rightRelPath = Path.Combine(projectName, rightLocale, rightBuild, screenName + ".png");
				var screenshots = HtmlNode.CreateNode("<tr><td>" +
					"<a href='" + leftRelPath + "' target=\"_blank\">" +
					"<img width='650' height='500' class='leftImage' src='" + leftRelPath + "'></a><td>" +
					"<a href='" + rightRelPath + "' target=\"_blank\">" +
					"<img width='650' height='500' class='rightImage' src='" + rightRelPath + "'></a>" +
					"<img class='overlayImage' width='650' height='500' hidden>" +
					"</td></tr>");

				contentTable.AppendChild(screenNames);
				contentTable.AppendChild(screenshots);

				screenNumber++;
			}

			using (StreamWriter sw = System.IO.File.CreateText(Path.Combine(tempDir, "screenshots.html")))
			{
				htmlDoc.Save(sw);
			}
		}

		static string HtmlBase(string project, string leftBuild, string rightBuild, string locale)
		{
			var html = "<html>" +
				"<head><title>ARP Screenshots</title></head>" +
				"<style> " +
				"body { margin: 0; font-family: Arial, Helvetica, sans-serif; }" +
				"table { padding: 1px 10px; }" +
				".header { background: #555; color: #f1f1f1; position: fixed; top: 0; width: 100%; z-index:1000; }" +
				".content { padding-top: 170px; }" +
				".overlayImage { position: absolute; }" +
				"</style>" +
				"<script>" +
				"function flipEnglish() {" +
				"var leftSide = document.getElementById('leftImage');" +
				"var baseLocale = '" + locale + "';" +
				"var leftBuild = '" + leftBuild + "';" +
				"var rightBuild = '" + rightBuild + "';" +
				"var index = 0;" +
				"for (el of document.querySelectorAll('img[src]')) {" +
				"var classname = el.getAttribute('class');" +
				"if (classname == 'leftImage') {" +
				"var fullPath = el.src.split('/'); " +
				"var c = fullPath.length;" +
				"var fileName = fullPath[c - 1];" +
				"var imageLink = el.parentElement;" +
				"if(flip.checked == true) {" +
				"el.src = './" + project + "/en-US/" + rightBuild + "/' + fileName;" +
				"imageLink.href = el.src;" +
				"leftSide.innerHTML = '<b>Left Build: </b>' + rightBuild + ' - <b>Left Locale: </b>en-US';" +
				"} else { el.src = './" + project + "/" + locale + "/" + leftBuild + "/' + fileName;" +
				"imageLink.href = el.src;" +
				"leftSide.innerHTML = '<b>Left Build: </b>' + leftBuild + ' - <b>Left Locale: </b>' + baseLocale; }" +
				"} index++;" +
				"}}" +
				"</script><script>" +
				"function diffOverlay() {" +
				"var index = 0;" +
				"for (el of document.querySelectorAll('img')) {" +
				"var classname = el.getAttribute('class'); var rect;" +
				"if (classname == 'rightImage') {" +
				"var fullPath = el.src.split('/'); var c = fullPath.length;" +
				"var fileName = fullPath[c - 1]; rect = el.getBoundingClientRect();" +
				"}" +
				"if (classname == 'overlayImage') {" +
				"el.style.position = 'absolute'; el.style.left = rect.left;" +
				"if(overlay.checked == true) {" +
				"el.src = '" + project + "/DIFF/' + fileName; el.hidden = false;" +
				"} else {  el.src = ''; el.hidden = true;" +
				"}} index++;" +
				"}}" +
				"</script>" +
				"<body>" +
				"<table class='header' style='width: 100%'" +
				"</table>" +
				"<table class='content' style='width: 100%'" +
				"</table>" +
				"</body>" +
				"</html>";

			return html;
		}

	}
}
