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

import * as fs from "fs";
import * as path from "path";
import * as rm from "typed-rest-client/RestClient";
import * as garbInterface from "./garb-interface";
import * as helpers from "./helpers";

let baseUrl: string;
let restc: rm.RestClient;

export async function process(host: string,
  api: string,
  token: string,
  product: string,
  build: string,
  language: string,
  folder: string): Promise<boolean> {

  baseUrl = host + api;
  restc = new rm.RestClient("arp-uploader");
  let requestOptions: rm.IRequestOptions = <rm.IRequestOptions>{};

  requestOptions.acceptHeader = "application/json";
  requestOptions.additionalHeaders = {
    "Authorization": "Bearer " + token,
    "Content-Type": "application/json"
  };

  try {
    //
    // get Resource: strong typing of resource(s) via generics.
    // in this case httpbin.org has a response structure
    // response.result carries the resource(s)
    //

    let projectExists: boolean = await checkProject(baseUrl, requestOptions, product);
    let newProject: boolean = false;

    if (!projectExists) {
      newProject = await createProject(baseUrl, requestOptions, product);
      if (newProject) {
        projectExists = true;
      } else {
        console.error("Project is not valid!");
        return false;
      }
    }

    let buildId: string | null = null;

    if (!newProject) {
      buildId = await checkBuild(baseUrl, requestOptions, product, build);
    }

    if (buildId == null) {
      let newBuildId: string | null = await createBuild(baseUrl, requestOptions, product, build);
      if (newBuildId != null) {
        buildId = newBuildId;
      } else {
        console.error("Build is not valid!");
        return false;
      }
    }

    if (projectExists && buildId != null) {
      console.log("Reading content of: " + folder);

      folder = path.resolve(path.normalize(folder));
      console.log("Root language directory: " + folder);

      let screenProcessingTasks: Array<Promise<boolean>> = new Array<Promise<boolean>>();
      let issuesProcessingTasks: Array<Promise<boolean>> = new Array<Promise<boolean>>();

      let files: string[] = getAllFiles(folder);

      for (let file of files) {

        let fullFilePath: string = file;
        let screenName: string = helpers.getScreenName(folder, file);
        let extension: string = helpers.getExtension(file);

        if (extension === "png") {
          console.log("Processing " + file);
          let requestPath: string = "/Screens/" + product + "/" + screenName + "/" + language + "/" + buildId;
          screenProcessingTasks.push(uploadScreen(baseUrl, requestPath, requestOptions, fullFilePath));
        }
      }

      let overallStatus: boolean = true;

      console.log("Waiting for screen upload tasks to complete...");

      await Promise.all(screenProcessingTasks);

      for (let file of files) {

        let fullFilePath: string = file;
        let screenName: string = helpers.getScreenName(folder, file);
        let extension: string = helpers.getExtension(file);

        console.log("screen name: " + screenName);

        if (extension === "json") {
          console.log("Processing " + file);
          let requestPath: string = "/Issues/" + product + "/" + screenName + "/" + language + "/" + buildId;
          let fileBuffer: Buffer = fs.readFileSync(fullFilePath);
          let issues: garbInterface.ICreateIssue[] = JSON.parse(fileBuffer.toString());

          for (let issue of issues) {
            issuesProcessingTasks.push(uploadIssues(baseUrl, requestPath, requestOptions, issue));
          }
        }
      }


      console.log("Waiting for incident upload tasks to complete...");
      await Promise.all(issuesProcessingTasks);

      console.log("All upload tasks completed!");

      for (let result of screenProcessingTasks) {
        if (await result === false) {
          overallStatus = false;
        }
      }

      for (let result of issuesProcessingTasks) {
        if (await result === false) {
          overallStatus = false;
        }
      }

      if (overallStatus) {
        console.log("Status is good!");
        return true;
      } else {
        console.error("Some tasks failed!");
        return false;
      }
    }
  } catch (err) {
    console.error("Failed on: " + err.message);
  }
  return false;
}

/// check Project
async function checkProject(baseUrl: string, requestOptions: rm.IRequestOptions, product: string): Promise<boolean> {
  let requestPath: string = baseUrl + "/Projects/" + product;
  console.log("Checking project " + product);

  try {
    let restRes: rm.IRestResponse<garbInterface.IProject> = await restc.get<garbInterface.IProject>(requestPath, requestOptions);
    console.log(restRes.statusCode, restRes.result);

    if (restRes.statusCode === 200) {
      console.log("Project already exists.");
      return true;
    }
  } catch {
    console.log("Project does not exists in the database.");
  }

  return false;
}

/// check Build
async function checkBuild(baseUrl: string, requestOptions: rm.IRequestOptions, product: string, build: string): Promise<string | null> {
  let requestPath: string = baseUrl + "/Builds/" + product;
  console.log("Checking build " + build);

  try {
    let restRes: rm.IRestResponse<garbInterface.IBuild[]> = await restc.get<garbInterface.IBuild[]>(requestPath, requestOptions);
    console.log(restRes.statusCode, restRes.result);

    if (restRes.statusCode === 200 && restRes.result != null) {
      let bld: garbInterface.IBuild = restRes.result.find(b => b.buildName === build);

      if (bld != null) {
        console.log("Build already exists.");
        return bld.id;
      }
    }
  } catch {
    console.log("Build does not exists in the database.");
  }

  return null;
}


/// create project
async function createProject(baseUrl: string, requestOptions: rm.IRequestOptions, product: string): Promise<boolean> {
  // this function to wait the rest to respond back

  console.log("Creating project " + product);

  let requestPath: string = baseUrl + "/Projects";

  let body: garbInterface.IProject = <garbInterface.IProject>{ projectName: product };

  let hres: rm.IRestResponse<garbInterface.IProject> = await restc.create<garbInterface.IProject>(requestPath, body, requestOptions);

  if (hres.statusCode === 201) {
    console.log("Project created.");
    return true;
  }

  console.error("Project create failed!");
  return false;
}


/// create build
async function createBuild(baseUrl: string, requestOptions: rm.IRequestOptions, product: string, build: string): Promise<string | null> {
  // this function to wait the rest to respond back

  console.log("Creating build " + build);

  let requestPath: string = baseUrl + "/Builds/" + product;

  let body: garbInterface.IBuild = <garbInterface.IBuild>{ projectName: product, buildName: build };

  let hres: rm.IRestResponse<garbInterface.IBuild> = await restc.create<garbInterface.IBuild>(requestPath, body, requestOptions);


  if (hres.statusCode === 201) {
    let bld: garbInterface.IBuild | null = hres.result;

    if (bld != null) {
      console.log("Build created.");
      return bld.id;
    }
  }

  console.error("Build create failed!");
  return null;
}

async function uploadScreen(baseUrl: string,
  requestPath: string,
  requestOptions: rm.IRequestOptions,
  fileFullPath: string): Promise<boolean> {

  console.log("Uploading screenshot from " + fileFullPath + " to " + requestPath);

  let resourcePath: string = baseUrl + "/" + requestPath;

  let fileBuffer: Buffer = fs.readFileSync(fileFullPath);

  let base64Encoded: string = fileBuffer.toString("base64");

  let body: garbInterface.ICreateScreen = <garbInterface.ICreateScreen>{ content: base64Encoded };

  try {
    let hres: rm.IRestResponse<garbInterface.IBuild> = await restc.create<garbInterface.IBuild>(resourcePath, body, requestOptions);

    if (hres.statusCode === 201) {
      console.log("Screen [" + fileFullPath + "] upload complete succesfully.");
      return true;
    } else if (hres.statusCode === 204) {
      console.log("Screen [" + fileFullPath + "] already uploaded.");
      return true;
    } else if (hres.statusCode === 409) {
      console.error("Conflicd - different screen already uploaded!");
    } else {
      console.error("Unexpected code returned: " + hres.statusCode);
    }
  } catch (err) {
    console.error(err);
  }
  console.error("Screen [" + fileFullPath + "] upload failed.");

  return false;
}

async function uploadIssues(baseUrl: string,
  requestPath: string,
  requestOptions: rm.IRequestOptions,
  issue: garbInterface.ICreateIssue): Promise<boolean> {

  console.log("Uploading issue [" + issue.identifier + "] to " + requestPath);

  let resourcePath: string = baseUrl + "/" + requestPath;

  console.log("calling:" + resourcePath);
  try {
    let hres: rm.IRestResponse<garbInterface.IIssue> = await restc.create<garbInterface.IIssue>(resourcePath, issue, requestOptions);

    if (hres.statusCode === 201) {
      console.log("Issue [" + issue.identifier + "] upload completed succesfully.");
      return true;
    }
    if (hres.statusCode === 204) {
      console.log("Issue [" + issue.identifier + "] already on the server.");
      return true;
    }
  } catch (err) {
    console.error(err);
  }

  console.error("Issue [" + issue.identifier + "] upload failed.");
  return false;
}


// recursive synchronous 'walk' through a folder structure, with the given base path
function getAllFiles(baseFolder: string, fileList: string[] = []): string[] {

  let contents: string[] = fs.readdirSync(baseFolder);
  contents.forEach(content => {

    if (fs.statSync(path.join(baseFolder, content)).isDirectory()) {
      getAllFiles(path.join(baseFolder, content), fileList);
    } else {
      fileList.push(path.join(baseFolder, content));
    }
  });
  return fileList;
}
