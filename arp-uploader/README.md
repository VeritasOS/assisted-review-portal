# Assisted Review Portal Uploader

[![npm version](https://badge.fury.io/js/arp-uploader.svg)](https://badge.fury.io/js/arp-uploader)

## Overview

This tool allows you to upload screenshots (PNG) and issues (JSON) detected by the [Globalization Automation Library](https://www.npmjs.com/package/glob-auto-library) to the [Assisted Review Portal](https://github.com/VeritasOS/assisted-review-portal).

## Usage

Install the package via npm and save it as a dev dependency:

`npm install arp-uploader -D`

This will update the existing package.json or generate a new one for you.

### Upload using command line

```bash
set TOKEN=***your token***
set SCREENFOLDER=c:\screens\en-US
npm run upload -- -p myProject -b 0.43.445 -l en-US -t %TOKEN% -f %SCREENFOLDER%
```

### Or from npm script in package.json

```json
{
  "config": {
    "product": "MyProduct",
    "language": "ja-JP",
    "build": "1.0.3",
    "restapi": "https://my-arp-rest-api.com"
  },
  "scripts": {
    "upload": "cross-var arp-upload -p $npm_package_config_product -b $npm_package_config_build -l $npm_package_config_language -t $ARPTOKEN -r $npm_package_config_restapi -f ./screenshots/$npm_package_config_language",
  }
}
```
