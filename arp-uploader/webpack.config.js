const path = require('path');
const webpack = require('webpack');
const nodeExternals = require('webpack-node-externals');

module.exports = {
  devtool: 'source-map',
  entry: './src/arp/cli.ts',
  mode: 'development',
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: 'ts-loader',
        exclude: /node_modules/
      }
    ]
  },
  node: {
    console: false,
    fs: 'empty',
    net: 'empty',
    tls: 'empty'
  },
  output: {
    filename: 'arp/arp-cli.js',
    path: path.resolve(__dirname, 'built')
  },
  plugins: [
    new webpack.BannerPlugin({ banner: '#!/usr/bin/env node', raw: true })
  ],
  resolve: {
    extensions: ['.tsx', '.ts', '.js']
  },
  target: 'node', // in order to ignore built-in modules like path, fs, etc.
  externals: [nodeExternals()] // in order to ignore all modules in node_modules folder  
};