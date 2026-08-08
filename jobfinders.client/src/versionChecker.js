import { ref } from "vue";

const currentVersion = ref(null);

async function loadVersion() {
  console.log("Before Fetch");
  const res = await fetch("../dist/version.json", { cache: "no-store" });
  const json = await res.json();
  console.log("Fetch Result: " + json);
  return json.version;
}

async function startChecking(onUpdateDetected) {
  currentVersion.value = await loadVersion();
  console.log("Current Version: " + currentVersion.value);
  setInterval(async () => {
    const serverVersion = await loadVersion();
    console.log("Server Version:" + serverVersion);
    if (serverVersion !== currentVersion.value) {
      onUpdateDetected();
    }
  }, 30000);
}

export const versionChecker = {
  startChecking,
};
