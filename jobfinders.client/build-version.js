import crypto from "crypto";
import fs from "fs";
import path from "path";

const dir = "dist/assets";
const files = fs.readdirSync(dir).filter((f) => f.endsWith(".js"));

// pick the largest JS file (usually the main bundle)
const mainBundle = files.sort(
  (a, b) =>
    fs.statSync(path.join(dir, b)).size - fs.statSync(path.join(dir, a)).size,
)[0];

const content = fs.readFileSync(path.join(dir, mainBundle));
const hash = crypto.createHash("sha256").update(content).digest("hex");

fs.writeFileSync("dist/version.json", JSON.stringify({ version: hash }));
