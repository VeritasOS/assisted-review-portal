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

import * as optimist from "optimist";
import * as processor from "./utils/processor";

const defaultHost: string = "https://gal.globalization.veritas.com";

let host: string = defaultHost;
let api: string = "/api/v1.0";

let args: Array<string> = [];


let argv: any = optimist.usage("Usage: $0 --project [project] --build [build] --locale [locale] --token [token] --folder [folder]")
  .describe("p", "Project name")
  .describe("b", "Build name")
  .describe("l", "Locale")
  .describe("t", "Token")
  .describe("f", "Folder to process")
  .describe("r", "REST API Host")
  .alias("p", "project")
  .alias("b", "build")
  .alias("l", "locale")
  .alias("t", "token")
  .alias("f", "folder")
  .alias("r", "restapi")
  .demand(["p", "b", "l", "t", "f"])
  .argv;

console.log("Start processing.");

if (argv.restapi != null) {
  host = argv.restapi;
}

processor.process(host, api, argv.token, argv.project, argv.build, argv.locale, argv.folder)
  .then((result) => {
    console.log("Done");
    if (result === true) {
      process.exit(0);
    } else {
      process.exit(1);
    }

  });