import * as fs from "fs";
import * as path from 'path';

function walkSync(dir, filelist) {
  let files = fs.readdirSync(dir);
  filelist = filelist || [];
  files.forEach(function (file) {
    if (fs.statSync(dir + path.sep + file).isDirectory()) {
      filelist = walkSync(dir + path.sep + file, filelist);
    }
    else {
      filelist.push(dir + path.sep + file);
    }
  });
  return filelist;
};

function godotImportImagePlugin() {
  return {
    name: 'godot-image-plugin',
    writeBundle(outputOptions, bundle) {
      let path = outputOptions.dir;
      let files = walkSync(path).filter(k =>
        k.endsWith(".png") ||
        k.endsWith(".jpg") ||
        k.endsWith(".svg")
      );
      files.forEach(file => {
        fs.writeFileSync(file + ".import", '[remap]\nimporter="keep"');
      });
    },
  };
}

export default godotImportImagePlugin;