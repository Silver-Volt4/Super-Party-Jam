import {execSync} from "child_process";
import * as fs from 'fs';

execSync("npm run build-games")
let temp = fs.mkdtempSync("spj-build")
fs.renameSync("../game/assets/controller/games", temp + "/games")
execSync("npm run build-controller")
fs.renameSync(temp + "/games", "../game/assets/controller/games")
fs.rmdirSync(temp)