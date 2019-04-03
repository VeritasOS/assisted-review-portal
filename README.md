# Assisted Review Portal

## Overview

The Assisted Review Portal (ARP) is a web application that stores screenshots and associated incidents in a structured manner. It has been developed to conduct linguistic reviews using screenshots of the product's user interface. It enables users to see the translated strings in context to provide accurate translations. ARP also has an image comparison feature to identify any changes that have occurred between different versions of the product, which can reduce the manual effort in regression testing.

## Logging in to the Assisted Review Portal

A demo version of ARP is available on https://arp-demo.azurewebsites.net/

To login, use the following credentials:
Username: arpuser 
Password: User@123

## Reviewing Screenshots

After you login to ARP, you will be taken to the screenshot review page. Here you can select different projects, builds and Languages to review. 

Note: You may need to turn off the Diff Threshold option to see screenshots.

Navigate through screenshots by using the drop-down menu, or by using the Previous and Next buttons.

You can zoom in or zoom out of each screenshot by using the -/+ buttons. To reset the size of the screenshots so that they fit on the page, select the Fit checkbox. You can also click on a screenshot to view it in a separate tab.

When reviewing screenshots, different languages or builds should be selected for the left and right screens. 

For example:
1. Select the DEMO project
2. Select Build 1.0.0.2 for both the Left Build and Right Build
3. Select English (United States) for the Left Locale
4. Select Simplified Chinese for the Right Locale
5. Turn off (uncheck) the Overlay option

With these settings you can review the Simplified Chinese screens against the English ones.

If you are reviewing two non-English languages, you can quickly view the English screenshot by clicking the Flip English option. 

## Comparing Screenshots

The Assisted Review Portal will automatically detect differences in screenshots. This can be very useful when you quickly want to review any differences between builds.

To see an example of screenshot comparison:
1. Select the DEMO project
2. Select Build 1.0.0.1 for the Left Build
3. Select Simplified Chinese for the Left Locale
4. Select Build 1.0.0.2 for the Right Build
5. Also select Simplified Chinese for the Right Build
6. Turn on (check) the Overlay option
7. Navigate to the Home Page screenshot by using the drop down menu

Notice that the Home Page screenshot is different in Build 1.0.0.2. The differences are highlighted in red. Uncheck the Overlay option to turn off the red highlighting.

## Diff Threshold

With projects that have many screenshots, it can be time consuming to navigate through each screenshot to see what's different. Turning on the Diff Threshold option will filter screenshots, showing only the ones that are different.

For example:
1. Select the DEMO project
2. Select Build 1.0.0.1 for the Left Build
3. Select Simplified Chinese for the Left Locale
4. Select Build 1.0.0.2 for the Right Build
5. Also select Simplified Chinese for the Right Build
6. Turn on/off the Diff Threshold option

Notice when the Diff Threshold option is turned off, all screenshots are available for review (3). When the Diff Threshold option is turned on, only the screenshots that are different are available (2).

The Diff Threshold option can also be changed to only show bigger differences in screenshots. For example, using the above example, change the value in the Treshold textbox to 0.5%. Now only one screenshot is available for review (the Home Screen).

## Downloading Screenshots

Screenshots can be downloaded for offline use by clicking the Download button. Note that the current selected filter will apply.

An HTML file is also generated and contained within the screenshots zipped file. This file has some basic ARP features such as displaying screenshots side by side, highlighting differences and flipping the screen to English.

## Uploading Screenshots

For security purposes, uploading screenshots is not available using the arpuser account. To upload screens, please request an account.

To upload screenshots you will first need an ARP token:
1. Navigate to the ARP website you wish to upload to (e.g. https://arp-demo.azurewebsites.net)
2. Select Get Token from the menu
3. Copy this token to a local variable, for example:
```bash
set ARPTOKEN="eyJhbGciOiJIUzI1NiIs..."      // Windows
export ARPTOKEN=eyJhbGciOiJIUzI1NiIs...     // Mac
```

To upload screenshots, use the following command:
```bash
arp-upload -p [PRODUCT_NAME] -b [BUILD] -l [LANGUAGE] -t $ARPTOKEN -r [ARP_WEBSITE] -f [SCREENSHOT_FOLDER]
```

For example, to upload the English screens of Build 1.0.0.1 of a product called "Example", type the following:
```bash
arp-upload -p "Example" -b "1.0.0.1" -l "en-US" -t $ARPTOKEN -r "https://arp-demo.azurewebsites.net" -f "./screenshots/en-US"
```

### Uploading Screenshots using package.json

You can also set these variables in your package.json file. For example:
```json
{
  "name": "name-of-project",
  "config": {
    "product": "name-of-product",
    "language": "en-US",
    "build": "1.0.0.1",
    "restapi": "https://arp-demo.azurewebsites.net"
  },
  "version": "1.0.0",
  "description": "Short description of your project",
  "main": "config.js",
  "scripts": {
    "test": "protractor config.js",
    "upload": "arp-upload -p $npm_package_config_product -b $npm_package_config_build -l $npm_package_config_language -t $ARPTOKEN -r $npm_package_config_restapi -f ./screenshots/$npm_package_config_language",
    "webdriver-manager": "node ./node_modules/protractor/bin/webdriver-manager --ignore_ssl=true",
    "webdriver:start": "node ./node_modules/protractor/bin/webdriver-manager start --ignore_ssl=true",
    "webdriver:update": "node ./node_modules/protractor/bin/webdriver-manager update --ignore_ssl=true"
  },
  "author": "Your name",
  "license": "MIT",
  "devDependencies": {
    "glob-auto-library": "^1.1.0"
  }
}
```

In this example type the following command to upload:
```bash
npm run upload
```

