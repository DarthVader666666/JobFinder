import crypto from "crypto";
import fs from "fs";

const guid = crypto.randomUUID();
fs.writeFileSync("dist/version.json", JSON.stringify({ version: guid }));
